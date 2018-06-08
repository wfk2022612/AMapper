using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace AMapper
{
    /// <summary>
    /// 常用集合转换类
    /// </summary>
    public static partial class EnumerableEx
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static IList<TSource> ToList<TSource>(IEnumerable<TSource> source)
        {
            return source.ToList();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static TSource[] ToArray<TSource>(IEnumerable<TSource> source)
        {
            return source.ToArray();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static IList<TSource> ToIList<TSource>(IEnumerable<TSource> source)
        {
            return source.ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static Stack<TSource> ToStack<TSource>(IEnumerable<TSource> source)
        {
            return new Stack<TSource>(source);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static Queue<TSource> ToQueue<TSource>(IEnumerable<TSource> source)
        {
            return new Queue<TSource>(source);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static HashSet<TSource> ToHashSet<TSource>(IEnumerable<TSource> source)
        {
            return new HashSet<TSource>(source);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static ISet<TSource> ToISet<TSource>(IEnumerable<TSource> source)
        {
            return new HashSet<TSource>(source);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static LinkedList<TSource> ToLinkedList<TSource>(IEnumerable<TSource> source)
        {
            return new LinkedList<TSource>(source);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static ICollection<TSource> ToICollection<TSource>(IEnumerable<TSource> source)
        {
            return new List<TSource>(source);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static Collection<TSource> ToCollection<TSource>(IEnumerable<TSource> source)
        {
            return new Collection<TSource>(source.ToList());
        }

    }
}
