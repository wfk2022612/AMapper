using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AMapper
{
    public static class TypeUtils
    {
        /// <summary>
        /// 是否是基本类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsBaseType(Type type)
        {
            return BaseTypes.Contains(type);
        }

        /// <summary>
        /// 基本类型集合
        /// </summary>
        public static IEnumerable<Type> BaseTypes => new[]
        {
            typeof(byte),typeof(sbyte),
            typeof(short),typeof(int),typeof(long),
            typeof(ushort),typeof(uint),typeof(ulong),
            typeof(float),typeof(double),typeof(decimal),
            typeof(string),typeof(char),typeof(bool),typeof(DateTime)
        };
    }
}
