using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;

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
                    var param = Expression.Parameter(typeof(TS), "ts");
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
                    var param = Expression.Parameter(typeof(TS), "ts");
                    _propExps[prop.Name] = Expression.Lambda(Expression.Default(prop.PropertyType), param);
                }
            }
        }

        /// <summary>
        /// 属性映射配置
        /// </summary>
        /// <param name="prop">新类型属性</param>
        /// <param name="tsProp">源类型属性或值表达式</param>
        /// <returns></returns>
        public TypeMap<TS, TD> ForMember<TDPropType, TsPropType>(Expression<Func<TD, TDPropType>> prop, Expression<Func<TS, TsPropType>> tsProp)
        {
            string name = null;
            if (prop.Body.NodeType == ExpressionType.MemberAccess)
            {
                name = ((MemberExpression)prop.Body).Member.Name;
            }

            _propExps[name] = tsProp;

            return this;
        }

        /// <summary>
        /// 创建实体转换类型
        /// </summary>
        private void CreateMapType()
        {
            AssemblyBuilder assemblyBuilder =
                AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("FastMapAsm" + Guid.NewGuid().ToString("N")),
                    AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("FastMapAsmModule");
            this.BaseCollectionConvertType = CreateBaseCollectionConvertType(moduleBuilder);

            var typeBuilder = moduleBuilder.DefineType("FastPropMap" + Guid.NewGuid().ToString("N"), TypeAttributes.Class | TypeAttributes.Public);
            var bprops = typeof(TD).GetProperties();

            // 单个属性转换
            foreach (var propertyInfo in bprops)
            {
                var propMap = _propExps[propertyInfo.Name];

                ParameterExpression tsParam = Expression.Parameter(typeof(TS), "ts");
                ParameterExpression tdParam = Expression.Parameter(typeof(TD), "td");

                var valExp = (LambdaExpression)propMap;
                Expression val = valExp.Body;

                MethodInfo toCollectionMethod = null;// 集合转换方法 ToArray ToList 等
                MethodInfo convertMethod = null;// 类型转换方法

                if (propertyInfo.PropertyType.GetInterface(typeof(IEnumerable).Name) != null
                    && propertyInfo.PropertyType != typeof(string)
                    && val.Type.GetInterface(typeof(IEnumerable).Name) != null
                    && val.Type != typeof(string))
                {
                    #region 处理集合属性

                    if (GetElementType(val.Type) != GetElementType(propertyInfo.PropertyType))
                    {
                        // 处理集合属性
                        convertMethod = GetElementConvertMethod(GetElementType(val.Type), GetElementType(propertyInfo.PropertyType));

                        if (convertMethod == null)
                        {
                            continue;
                        }
                    }


                    if (propertyInfo.PropertyType.Name != typeof(IEnumerable).Name &&
                        propertyInfo.PropertyType.Name != typeof(IEnumerable<>).Name)
                    {
                        // 只对非IEnumerable类型做处理

                        toCollectionMethod =
                            typeof(EnumerableEx).GetMethods()
                                .Select(m => m.MakeGenericMethod(GetElementType(propertyInfo.PropertyType)))
                                .FirstOrDefault(m => m.ReturnType.Name == propertyInfo.PropertyType.Name);
                    }

                    #endregion
                }

                if (convertMethod == null)
                {
                    //处理非集合属性
                    convertMethod = GetTypeMap(val.Type, propertyInfo.PropertyType)?.GetMethod("Convert"); ;
                }

                if (convertMethod != null)
                {
                    val = Expression.Call(null, convertMethod, val);
                }

                if (val.Type != propertyInfo.PropertyType)
                {
                    // 将IEnumerable 转为目标集合类型
                    if (toCollectionMethod != null)
                    {
                        // val= val!=default(T)?ToList(val):default(T)
                        val =
                            Expression.Condition(
                                Expression.ReferenceNotEqual(val, Expression.Default(propertyInfo.PropertyType)),
                                Expression.Call(null, toCollectionMethod, val),
                                Expression.Default(propertyInfo.PropertyType));
                    }
                    else
                    {
                        var systemConvert = typeof(Convert).GetMethod("To" + propertyInfo.PropertyType.Name,
                      new Type[] { val.Type });
                        if (systemConvert != null)
                        {
                            val = Expression.Call(null, systemConvert, val); // 使用系统方法转换
                        }
                        else if (propertyInfo.PropertyType.IsAssignableFrom(val.Type) || val.NodeType == ExpressionType.Constant)
                        {
                            val = Expression.Convert(val, propertyInfo.PropertyType); // 强制转换
                        }
                        else
                        {
                            continue;
                        }
                    }

                }

                var call = Expression.Call(tdParam, typeof(TD).GetProperty(propertyInfo.Name).GetSetMethod(), val);

                var callLambda = Expression.Lambda<Action<TS, TD, TS>>(call, tsParam, tdParam, valExp.Parameters[0]);

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
        /// 获取集合元素转换方法
        /// </summary>
        /// <param name="tsType">源类型</param>
        /// <param name="tdType">新类型</param>
        /// <returns></returns>
        private MethodInfo GetElementConvertMethod(Type tsType, Type tdType)
        {
            MethodInfo convertMethod = null;
            var convertType = GetTypeMap(tsType, tdType, true);

            if (convertType != null)
            {
                convertMethod = convertType.GetMethod("Convert");
            }
            else if (TypeUtils.IsBaseType(tsType) &&
                     TypeUtils.IsBaseType(tdType))
            {
                // 获取基本类型集合的转换方法
                convertMethod = GetBaseCollectionConvertMethod(tsType, tdType);
            }
            return convertMethod;
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
            if (type == typeof(string))
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

        private Type MakeIEnumerable(Type type)
        {
            return typeof(IEnumerable<>).MakeGenericType(type);
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
        /// 创建集合转换类
        /// </summary>
        /// <param name="moduleBuilder"></param>
        /// <returns></returns>
        private Type CreateBaseCollectionConvertType(ModuleBuilder moduleBuilder)
        {
            var typeBuilder = moduleBuilder.DefineType("BaseTypesConvert", TypeAttributes.Class | TypeAttributes.Public);

            var baseTypes = TypeUtils.BaseTypes.ToArray();

            for (var i = 0; i < baseTypes.Length; i++)
            {
                for (var j = 1; j < baseTypes.Length; j++)
                {
                    var tdType = baseTypes[i];
                    var tsType = baseTypes[j];

                    if (tdType == tsType)
                    {
                        continue;
                    }

                    var tdEnumerableTyp = typeof(IEnumerable<>).MakeGenericType(tdType);
                    var tsEnumerableTyp = typeof(IEnumerable<>).MakeGenericType(tsType);
                    var tsEnumeratorType = typeof(IEnumerator<>).MakeGenericType(tsType);
                    var tdListType = typeof(List<>).MakeGenericType(tdType);


                    var convertMethod = typeof(Convert).GetMethod("To" + tdType.Name, new Type[] { tsType });

                    if (convertMethod != null)
                    {
                        var convertMethodBuilder = typeBuilder.DefineMethod("To" + tdType.Name, MethodAttributes.Public | MethodAttributes.Static, tdEnumerableTyp,
                          new Type[] { tsEnumerableTyp });

                        var addMethod = tdListType.GetMethod("Add");
                        var getEnumerator = tsEnumerableTyp.GetMethod("GetEnumerator");
                        var current = tsEnumeratorType.GetProperty("Current").GetGetMethod();
                        var moveNext = typeof(IEnumerator).GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.Public);

                        var il = convertMethodBuilder.GetILGenerator();
                        il.DeclareLocal(tdListType);
                        il.DeclareLocal(tdType);
                        il.DeclareLocal(tsEnumeratorType);
                        il.DeclareLocal(tsType);


                        il.Emit(OpCodes.Newobj, tdListType.GetConstructor(Type.EmptyTypes));
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
                        il.Emit(OpCodes.Call, convertMethod);
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

                    }
                }
            }
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
        /// 基本数据的集合类型转换类
        /// </summary>
        public Type BaseCollectionConvertType { get; private set; }

        /// <summary>
        /// 获取类型映射
        /// </summary>
        /// <param name="tsType">源类型</param>
        /// <param name="tdType">新类型</param>
        /// <param name="collectType">是否是多实体转换</param>
        /// <returns>包含转换方法的类型</returns>
        private Type GetTypeMap(Type tsType, Type tdType, bool collectType = false)
        {
            if (tsType != typeof(object) && tdType != typeof(object))
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
            }
            return null;
        }

        /// <summary>
        /// 获取基本类型集合的转换方法
        /// </summary>
        /// <param name="tsType">源类型</param>
        /// <param name="tdType">新类型</param>
        /// <returns></returns>
        private MethodInfo GetBaseCollectionConvertMethod(Type tdType, Type tsType)
        {
            var tsColType = typeof(IEnumerable<>).MakeGenericType(tsType);
            var tdColType = typeof(IEnumerable<>).MakeGenericType(tdType);
            return this.BaseCollectionConvertType.GetMethods().FirstOrDefault(m => m.ReturnType == tsColType && m.GetParameters()[0].ParameterType == tdColType);
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
