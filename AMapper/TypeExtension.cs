using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AMapper
{
    public static class TypeExtension
    {
        /// <summary>
        /// 是否是一维数组或泛型集合
        /// </summary>
        /// <returns></returns>
        public static bool IsGenericCollectType(this Type type)
        {
            if (type.IsInterface)
            {
                // 必须满足泛型、实现IEnumerable或IEnumerable<>接口
                if (type.IsGenericType)
                {
                    return type.Name == typeof(IEnumerable<>).Name
                           || type.GetInterface(typeof(IEnumerable<>).Name) != null;
                }
            }
            else if (type.IsArray)
            {
                return type.GetArrayRank() == 1 && type.GetElementType().IsGenericCollectType() == false && (type.GetElementType() != typeof(object));
            }
            else if (type.IsClass)
            {
                if (type != typeof(string))
                {
                    return type.GetInterface(typeof(IEnumerable<>).Name) != null;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取集合类型的泛型参数类型或数组元素类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns>如果不能匹配则返回object类型，否则返回实际类型</returns>
        public static Type GetGenericCollectType(this Type type)
        {
            if (type.IsGenericCollectType())
            {
                if (type.IsGenericType && type.GetGenericArguments().Length == 1)
                {
                    return type.GetGenericArguments()[0];
                }
                else if (type.IsArray)
                {
                    return type.GetElementType();
                }
            }
            return typeof(object);
        }
    }
}
