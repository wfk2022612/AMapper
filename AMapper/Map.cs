using System;
using System.Collections.Generic;

namespace AMapper
{
    /// <summary>
    /// ʵ��ӳ�������
    /// </summary>
    public class Map
    {
        /// <summary>
        /// ʵ��ת������
        /// </summary>
        internal static readonly Dictionary<string, ITypeMap> TypeMaps = new Dictionary<string, ITypeMap>();

        /// <summary>
        /// ��������ӳ��
        /// </summary>
        /// <typeparam name="TS">Դ����</typeparam>
        /// <typeparam name="TD">������</typeparam>
        /// <returns>����ӳ���ϵʵ��</returns>
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