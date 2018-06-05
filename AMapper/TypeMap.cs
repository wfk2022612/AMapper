using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace AMapper
{
    /// <summary>
    /// 类型映射关系
    /// </summary>
    /// <typeparam name="TS">源类型</typeparam>
    /// <typeparam name="TD">新类型</typeparam>
    public class TypeMap<TS, TD> : ITypeMap
        where TS : class, new()
        where TD : class, new()
    {
        /// <summary>
        /// 属性关联的表达式集合
        /// </summary>
        readonly Dictionary<string, Expression> _propExps = new Dictionary<string, Expression>();

        /// <summary>
        /// 类型映射关系
        /// </summary>
        public TypeMap()
        {
            var tdProps = typeof(TD).GetProperties();
            var tsProps = typeof(TS).GetProperties();
            foreach (var prop in tdProps)
            {
                // 默认采用忽略大小写的属性名映射
                var tsProp = tsProps.FirstOrDefault(x => x.Name.Equals(prop.Name, StringComparison.OrdinalIgnoreCase));
                // 有对应的属性
                if (tsProp != null)
                {
                    // 生成属性表达式
                    ParameterExpression param = Expression.Parameter(typeof(TS), "ts");

                    Expression propExp = Expression.Property(param, tsProp);

                    var systemConvert = typeof(Convert).GetMethod("To" + prop.PropertyType.Name,
                        new Type[] { propExp.Type });
                    if (systemConvert != null)
                    {
                        propExp = Expression.Call(null, systemConvert, propExp); // 使用系统方法转换
                    }

                    var exp = Expression.Lambda(propExp, param);

                    _propExps[prop.Name] = exp;
                }
                else
                {
                    // 没有对应的属性,使用默认值
                    _propExps[prop.Name] = Expression.Default(prop.PropertyType);
                }
            }
        }
        /// <summary>
        /// 属性映射配置
        /// </summary>
        /// <param name="prop">新类型属性</param>
        /// <param name="tsProp">源类型属性或值表达式</param>
        /// <returns></returns>
        public TypeMap<TS, TD> ForMember(Expression<Func<TD, object>> prop, Expression<Func<TS, object>> tsProp)
        {
            string name = null;
            if (prop.Body.NodeType == ExpressionType.MemberAccess)
            {
                name = ((MemberExpression)prop.Body).Member.Name;
            }
            else if (prop.Body.NodeType == ExpressionType.Convert)
            {
                name = ((MemberExpression)((UnaryExpression)prop.Body).Operand).Member.Name;
            }

            _propExps[name] = tsProp;

            return this;
        }

        /// <summary>
        /// 创建实体转换类型
        /// </summary>
        private void CreateMapType()
        {
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("FastMapAsm"+DateTime.Now.Ticks), AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("FastMapAsmModule");
            var typeBuilder = moduleBuilder.DefineType("FastPropMap" + typeof(TS).Name + "_" + typeof(TD).Name, TypeAttributes.Class | TypeAttributes.Public);
            var bprops = typeof(TD).GetProperties();

            // 单个属性转换
            foreach (var propertyInfo in bprops)
            {
                var propMap = _propExps[propertyInfo.Name];

                ParameterExpression parameter = Expression.Parameter(typeof(TS), "a");
                ParameterExpression bparam = Expression.Parameter(typeof(TD), "b");

                Expression val = null;

                var valExp = (LambdaExpression)propMap;
                val = valExp.Body;
                Type convertType = null;// CreateMapType生成的转换类
                MethodInfo toArrayOrList = null;
                if (propertyInfo.PropertyType.GetInterface(typeof(IEnumerable).Name) != null && propertyInfo.PropertyType != typeof(string))
                {
                    // 处理集合属性
                    convertType = GetTypeConvert(GetElementType(val.Type), GetElementType(propertyInfo.PropertyType), true);
                    if (propertyInfo.PropertyType.Name != typeof(IEnumerable).Name && propertyInfo.PropertyType.Name != typeof(IEnumerable<>).Name)
                    {
                        toArrayOrList = typeof(Enumerable).GetMethod(propertyInfo.PropertyType.IsArray ? "ToArray" : "ToList");
                        if (toArrayOrList != null)
                            toArrayOrList = toArrayOrList.MakeGenericMethod(GetElementType(propertyInfo.PropertyType));
                    }
                }
                else
                {
                    // 获取已创建的映射转换类
                    convertType = GetTypeConvert(val.Type, propertyInfo.PropertyType);
                }

                if (convertType != null)
                {
                    var convertMethod = convertType.GetMethod("Convert");

                    val = Expression.Call(null, convertMethod, val);

                    // 将IEnumerable 转为目标类型
                    if (toArrayOrList != null)
                    {
                        val = Expression.Call(null, toArrayOrList, val);
                    }
                }
                else if (val.Type != propertyInfo.PropertyType)
                {
                    var systemConvert = typeof(Convert).GetMethod("To" + propertyInfo.PropertyType.Name,
                       new Type[] { val.Type });
                    if (systemConvert != null)
                    {
                        val = Expression.Call(null, systemConvert, val); // 使用系统方法转换
                    }
                    else
                    {
                        val = Expression.Convert(val, propertyInfo.PropertyType);// 强制转换
                    }
                }

                var call = Expression.Call(bparam, typeof(TD).GetProperty(propertyInfo.Name).GetSetMethod(), val);

                var callLambda = Expression.Lambda<Action<TS, TD, TS>>(call, parameter, bparam, valExp.Parameters[0]);

                var methodBuilder = typeBuilder.DefineMethod("Convert_" + propertyInfo.Name,
                                   MethodAttributes.Public | MethodAttributes.Static, propertyInfo.PropertyType,
                                   new[] { typeof(TS), typeof(TD), typeof(TS) });

                callLambda.CompileToMethod(methodBuilder);

            }
            var propConvertType = typeBuilder.CreateType();

            typeBuilder = moduleBuilder.DefineType("FastMap" + typeof(TS).Name + "_" + typeof(TD).Name, TypeAttributes.Class | TypeAttributes.Public);

            // 普通字段处理方法
            var convertMethodBuilder = typeBuilder.DefineMethod("Convert",
                MethodAttributes.Public | MethodAttributes.Static, typeof(TD),
                new Type[] { typeof(TS) });

            var il = convertMethodBuilder.GetILGenerator();
            il.DeclareLocal(typeof(TD));

            il.Emit(OpCodes.Newobj, typeof(TD).GetConstructor(new Type[0]));
            il.Emit(OpCodes.Stloc_0);

            var tdProps = typeof(TD).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).ToList();
            foreach (var tdProp in tdProps)
            {
                if (tdProp.CanWrite)
                {
                    var tdSet = tdProp.GetSetMethod();
                    if (tdSet != null)
                    {
                        var method = propConvertType.GetMethod("Convert_" + tdProp.Name);
                        if (method != null)
                        {
                            if (method.GetParameters().Length == 2)
                            {
                                //ts
                                il.Emit(OpCodes.Ldarg_0);
                                //td
                                il.Emit(OpCodes.Ldloc_0);
                                // call 
                                il.Emit(OpCodes.Call, method);
                            }
                            else if (method.GetParameters().Length == 3)
                            {
                                //ts
                                il.Emit(OpCodes.Ldarg_0);
                                //td
                                il.Emit(OpCodes.Ldloc_0);
                                //ts
                                il.Emit(OpCodes.Ldarg_0);
                                // call 
                                il.Emit(OpCodes.Call, method);
                            }
                        }

                    }

                }
            }
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);

            this.ConvertType = typeBuilder.CreateType();

            this.CollectConvertType = CreateCollectConvertType(moduleBuilder);
        }

        /// <summary>
        /// 获取集合类型的内部元素类型
        /// <para>主要用于数组、IEnumerable`1类型</para>
        /// </summary>
        /// <param name="type">属性类型</param>
        /// <returns>集合元素类型</returns>
        private Type GetElementType(Type type)
        {
            // 排除字符串
            if(type==typeof(string))
            {
                return type;
            }
            // 可枚举类型
            if (type.Name == typeof(IEnumerable<>).Name)
            {
                return type.GetGenericArguments()[0];
            }
            // 实现了可枚举接口
            if (type.GetInterface(typeof(IEnumerable<>).Name) != null)
            {
                return type.GetInterface(typeof(IEnumerable<>).Name).GetGenericArguments()[0];
            }
            return type;
        }
        /// <summary>
        /// 创建集合转换类
        /// </summary>
        /// <param name="moduleBuilder"></param>
        /// <returns></returns>
        private Type CreateCollectConvertType(ModuleBuilder moduleBuilder)
        {
            var typeBuilder = moduleBuilder.DefineType("FastMaps" + typeof(TS).Name + "_" + typeof(TD).Name, TypeAttributes.Class | TypeAttributes.Public);

            var convertMethodBuilder = typeBuilder.DefineMethod("Convert", MethodAttributes.Public | MethodAttributes.Static, typeof(IEnumerable<TD>),
                   new Type[] { typeof(IEnumerable<TS>) });

            var addMethod = typeof(List<TD>).GetMethod("Add");
            var getEnumerator = typeof(IEnumerable<TS>).GetMethod("GetEnumerator");
            var current = typeof(IEnumerator<TS>).GetProperty("Current").GetGetMethod();
            var moveNext = typeof(IEnumerator).GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.Public);
            var convert = this.ConvertType.GetMethod("Convert");

            var il = convertMethodBuilder.GetILGenerator();
            il.DeclareLocal(typeof(List<TD>));
            il.DeclareLocal(typeof(TD));
            il.DeclareLocal(typeof(IEnumerator<TS>));
            il.DeclareLocal(typeof(TS));


            il.Emit(OpCodes.Newobj, typeof(List<TD>).GetConstructor(Type.EmptyTypes));
            il.Emit(OpCodes.Stloc_0);

            // ienumerator
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Callvirt, getEnumerator);
            il.Emit(OpCodes.Stloc_2);
            // loop
            var label = il.DefineLabel();
            var ret = il.DefineLabel();

            il.MarkLabel(label);
            // movenext
            il.Emit(OpCodes.Ldloc_2);
            il.Emit(OpCodes.Callvirt, moveNext);
            il.Emit(OpCodes.Brfalse_S, ret);

            //getcurrent
            il.Emit(OpCodes.Ldloc_2);
            il.Emit(OpCodes.Callvirt, current);
            il.Emit(OpCodes.Stloc_3);

            // convert
            il.Emit(OpCodes.Ldloc_3);
            il.Emit(OpCodes.Call, convert);
            il.Emit(OpCodes.Stloc_1);

            // 添加到list中
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Callvirt, addMethod);

            // 循环
            il.Emit(OpCodes.Br, label);

            il.MarkLabel(ret);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);


            return typeBuilder.CreateType();
        }

        /// <summary>
        /// 单实体转换类
        /// </summary>
        public Type ConvertType { get; private set; }
        /// <summary>
        /// 多实体转换类
        /// </summary>
        public Type CollectConvertType { get; private set; }

        /// <summary>
        /// 获取类型映射
        /// </summary>
        /// <param name="tsType">源类型</param>
        /// <param name="tdType">新类型</param>
        /// <param name="collectType">是否是多实体转换</param>
        /// <returns>包含转换方法的类型</returns>
        private Type GetTypeConvert(Type tsType, Type tdType, bool collectType = false)
        {
            var mapKey = string.Format("{0},{1}", tsType.FullName, tdType.FullName);
            if (Map.TypeMaps.ContainsKey(mapKey))
            {
                var obj = Map.TypeMaps[mapKey];
                if (obj != null)
                {
                    var convertType = collectType ? obj.CollectConvertType : obj.ConvertType;
                    return convertType;
                }

            }
            return null;
        }
        /// <summary>
        /// 编译生成入口方法
        /// </summary>
        /// <returns></returns>
        public Func<TS, TD> Compile()
        {
            CreateMapType();

            var dm = new DynamicMethod("translate" + typeof(TD).Name + "To" + typeof(TS).Name, typeof(TD),
                new[] { typeof(TS) }, typeof(TD).Module);
            var il = dm.GetILGenerator();
            il.DeclareLocal(typeof(TD));


            il.Emit(OpCodes.Newobj, typeof(TD).GetConstructor(new Type[0]));
            il.Emit(OpCodes.Stloc_0);

            // 执行Convert方法
            var tdConvertMethod = this.ConvertType.GetMethod("Convert");
            //ts
            il.Emit(OpCodes.Ldarg_0);
            // call 
            il.Emit(OpCodes.Call, tdConvertMethod);
            il.Emit(OpCodes.Stloc_0);

            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);

            return (Func<TS, TD>)dm.CreateDelegate(typeof(Func<TS, TD>));

        }
    }
}
