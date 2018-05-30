using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MainProgram;

namespace FastMapper
{
    public class TypeMap<TS, TD>
        where TS : class, new()
        where TD : class, new()
    {
        readonly Dictionary<string, PropMap> _propMaps = new Dictionary<string, PropMap>();
        public TypeMap()
        {
            var props = typeof(TD).GetProperties();
            foreach (var prop in props)
            {
                _propMaps[prop.Name] = new PropMap();
            }
        }

        public TypeMap<TS, TD> ForMember(Expression<Func<TD, object>> prop, Expression<Action<PropMap>> map)
        {
            var name = "";
            if (prop.Body.NodeType == ExpressionType.MemberAccess)
            {
                name = ((MemberExpression)prop.Body).Member.Name;
            }
            if (!name.IsNullOrWhiteSpace() && _propMaps.ContainsKey(name))
            {
                map.Compile()(_propMaps[name]);
            }

            return this;
        }

        public TypeMap<TS, TD> ForMember(Expression<Func<TD, object>> prop, Expression<Func<TS, object>> tsProp)
        {
            string name = null, tsPropName = null;
            if (prop.Body.NodeType == ExpressionType.MemberAccess)
            {
                name = ((MemberExpression)prop.Body).Member.Name;
            }

            _propMaps[name].ValueExpression = tsProp;


            return this;
        }

        //public TypeMap<TS, TD> ForMemberExpression(Expression<Func<TD, object>> prop, Action<TD> tdExpression)
        //{
        //    string name = null;
        //    if (prop.Body.NodeType == ExpressionType.MemberAccess)
        //    {
        //        name = ((MemberExpression)prop.Body).Member.Name;
        //    }
        //    propMaps[name].Expression(tdExpression);

        //    return this;
        //}

        private Type CreateMapType()
        {
            AssemblyName aName = new AssemblyName("DynamicAssemblyExample");
            AssemblyBuilder assemblyBuilder =
                AppDomain.CurrentDomain.DefineDynamicAssembly(
                    aName,
                    AssemblyBuilderAccess.RunAndSave);

            // AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("FastMapAsm"), AssemblyBuilderAccess.RunAndSave);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("FastMapAsmModule");

            var typeBuilder = moduleBuilder.DefineType("FastMap" + typeof(TD).Name, TypeAttributes.Class | TypeAttributes.Public);

            var bprops = typeof(TD).GetProperties();
            var aprops = typeof(TS).GetProperties();

            foreach (var propertyInfo in bprops)
            {

                var propMap = _propMaps[propertyInfo.Name];
                ParameterExpression parameter = Expression.Parameter(typeof(TS), "a");
                ParameterExpression bparam = Expression.Parameter(typeof(TD), "b");

                if (propMap.ValueExpression != null)
                {
                    var valExp = (Expression<Func<TS, object>>)propMap.ValueExpression;

                    var call = Expression.Call(bparam, typeof(TD).GetProperty(propertyInfo.Name).GetSetMethod(), valExp.Body);
                    var callLambda = Expression.Lambda<Action<TS, TD, TS>>(call, parameter, bparam, valExp.Parameters[0]);

                    var exp = callLambda.Reduce();

                    var methodBuilder = typeBuilder.DefineMethod("Convert_" + propertyInfo.Name,
               MethodAttributes.Public | MethodAttributes.Static, propertyInfo.PropertyType,
               new[] { typeof(TS), typeof(TD), typeof(TS) });

                    callLambda.CompileToMethod(methodBuilder);

                    expAction += callLambda.Compile();
                }
                else if (aprops.Any(x => x.Name == propertyInfo.Name))
                {

                    var call = Expression.Call(bparam, typeof(TD).GetProperty(propertyInfo.Name).GetSetMethod(), Expression.Property(parameter, propertyInfo.Name));

                    var callLambda = Expression.Lambda<Action<TS, TD>>(call, parameter, bparam);

                    var exp = callLambda.Reduce();

                    var methodBuilder = typeBuilder.DefineMethod("Convert_" + propertyInfo.Name,
              MethodAttributes.Public | MethodAttributes.Static, propertyInfo.PropertyType,
              new[] { typeof(TS), typeof(TD) });

                    callLambda.CompileToMethod(methodBuilder);

                    ;

                    action += callLambda.Compile();
                }
            }
            var type = typeBuilder.CreateType();

            ;
            return type;
        }

        public Func<TS, TD> Compile()
        {
            var mapType = CreateMapType();

            var tdProps = typeof(TD).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).ToList();

            var dm = new DynamicMethod("translate" + typeof(TD).Name + "To" + typeof(TS).Name, typeof(TD),
                new[] { typeof(TS) }, typeof(TD).Module);
            var il = dm.GetILGenerator();
            il.DeclareLocal(typeof(TD));
            il.DeclareLocal(mapType);

            il.Emit(OpCodes.Newobj, typeof(TD).GetConstructor(new Type[0]));
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Newobj, mapType.GetConstructor(new Type[0]));
            il.Emit(OpCodes.Stloc_1);


            foreach (var tdProp in tdProps)
            {
                var propMap = _propMaps[tdProp.Name];
                if (propMap.IsIgnore)
                {
                    continue;
                }
                if (tdProp.CanWrite)
                {
                    var tdSet = tdProp.GetSetMethod();
                    if (tdSet != null)
                    {
                        // 获取实际值
                        var method = mapType.GetMethod("Convert_" + tdProp.Name);
                        if (method.GetParameters().Length == 2)
                        {
                            //ts
                            il.Emit(OpCodes.Ldloc_0);
                            //td
                            il.Emit(OpCodes.Ldloc_1);
                            // call 
                            il.Emit(OpCodes.Callvirt, method);
                        }
                        else if (method.GetParameters().Length == 3)
                        {
                            //ts
                            il.Emit(OpCodes.Ldloc_0);
                            //td
                            il.Emit(OpCodes.Ldloc_1);
                            // ts
                            il.Emit(OpCodes.Ldloc_0);
                            // call
                            il.Emit(OpCodes.Callvirt, method);
                        }
                    }

                }
            }
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);

            return (Func<TS, TD>)dm.CreateDelegate(typeof(Func<TS, TD>));

        }
        Action<TS, TD> action = null;
        private Action<TS, TD, TS> expAction = null;
        private TD Convert(TS tsObj)
        {
            TD tdObj = new TD();
            if (action != null)
                action(tsObj, tdObj);
            if (expAction != null)
            {
                expAction(tsObj, tdObj, tsObj);
            }
            return tdObj;
        }

        Dictionary<string, Func<TS, object>> delDic = new Dictionary<string, Func<TS, object>>();

        public object ExecDelegate(TS tsObj, string name)
        {
            if (delDic.ContainsKey(name))
            {
                return delDic[name](tsObj);
            }
            return null;
        }
    }
}
