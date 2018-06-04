using System;
using System.Collections.Generic;

namespace AMapper
{
    /// <summary>
    /// 实体映射帮助类
    /// </summary>
    public class Map
    {
        /// <summary>
        /// 实体转换缓存
        /// </summary>
        internal static readonly Dictionary<string, ITypeMap> TypeMaps = new Dictionary<string, ITypeMap>();

        /// <summary>
        /// 创建类型映射
        /// </summary>
        /// <typeparam name="TS">源类型</typeparam>
        /// <typeparam name="TD">新类型</typeparam>
        /// <returns>类型映射关系实例</returns>
        public static TypeMap<TS, TD> Create<TS, TD>()
            where TS : class, new()
            where TD : class, new()
        {
            var typeMap = new TypeMap<TS, TD>();
            TypeMaps[$"{typeof(TS).FullName},{typeof(TD).FullName}"] = typeMap;
            return typeMap;
        }
    }
}