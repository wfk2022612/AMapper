using System;
using System.Collections.Generic;

namespace FastMapper
{
    /// <summary>
    /// ʵ��ӳ�������
    /// </summary>
    public class FastMap
    {
        /// <summary>
        /// ʵ��ת������
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