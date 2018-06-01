using System;
using System.Collections.Generic;

namespace FastMapper
{
    /// <summary>
    /// 实体映射帮助类
    /// </summary>
    public class FastMap
    {
        /// <summary>
        /// 实体转换缓存
        /// </summary>
        internal static readonly Dictionary<string, ITypeMap> TypeMaps = new Dictionary<string, ITypeMap>();

        public static TypeMap<TS, TD> CreateMap<TS, TD>()
            where TS : class, new()
            where TD : class, new()
        {
            var typeMap = new TypeMap<TS, TD>();
            TypeMaps[$"{typeof(TS).FullName},{typeof(TD).FullName}"] = typeMap;
            return typeMap;
        }
    }
}