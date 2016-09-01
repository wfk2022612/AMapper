using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace FastMapper
{
    public class TypeMap<TS, TD>
        where TS : new()
        where TD : new()
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

            if (tsProp.Body.NodeType == ExpressionType.MemberAccess)
            {
                tsPropName = ((MemberExpression)tsProp.Body).Member.Name;
            }

            if (!name.IsNullOrWhiteSpace() && _propMaps.ContainsKey(name) && !name.IsNullOrWhiteSpace())
            {
                _propMaps[name].MapProp = tsPropName;
            }

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

        public Func<TS, TD> Compile()
        {
            var tsProps = typeof(TS).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).ToList();
            var tdProps = typeof(TD).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).ToList();
            //var toString = typeof(object).GetMethod("ToString");
            var dm = new DynamicMethod("translate" + typeof(TD).Name + "To" + typeof(TS).Name, typeof(TD),
                new[] { typeof(TS) }, typeof(TD).Module);
            var il = dm.GetILGenerator();
            il.DeclareLocal(typeof(TD));
            il.DeclareLocal(typeof(bool));

            il.Emit(OpCodes.Newobj, typeof(TD).GetConstructor(new Type[0]));
            il.Emit(OpCodes.Stloc_0);
            foreach (var tdProp in tdProps)
            {
                var propMap = _propMaps[tdProp.Name];
                if (propMap.IsIgnore)
                {
                    continue;
                }
                if (tdProp.CanWrite)
                {
                    var tsProp = tsProps.FirstOrDefault(p => p.Name == (propMap.MapProp ?? tdProp.Name));
                    if (tsProp != null && tsProp.CanRead)
                    {
                        var tsGet = tsProp.GetGetMethod();
                        var tdSet = tdProp.GetSetMethod();
                        if (tsGet != null && tdSet != null)
                        {
                            if (tsGet.IsStatic)
                            {
                                il.Emit(OpCodes.Call, tsGet);
                            }
                            else
                            {
                                il.Emit(OpCodes.Ldloc_0);
                                il.Emit(OpCodes.Ldarg_0);
                                il.Emit(OpCodes.Callvirt, tsGet);
                            }
                            il.Emit(tdSet.IsStatic ? OpCodes.Call : OpCodes.Callvirt, tdSet);
                        }

                    }
                }
            }
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);
            return (Func<TS, TD>)dm.CreateDelegate(typeof(Func<TS, TD>));
        }
    }
}
