using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AMapper.UnitTest
{
    [TestClass]
    public class TypeExtensionTest
    {
        [TestMethod, TestCategory("IsGenericCollectType")]
        public void IsGenericCollectType()
        {
            Assert.IsFalse(typeof(Array).IsGenericCollectType(), typeof(Array).Name);
            Assert.IsFalse(typeof(object[]).IsGenericCollectType(), typeof(object[]).Name);
            Assert.IsTrue(typeof(string[]).IsGenericCollectType(), typeof(string[]).Name);

            Assert.IsFalse(typeof(Stack).IsGenericCollectType(), typeof(Stack).Name);
            Assert.IsFalse(typeof(Queue).IsGenericCollectType(), typeof(Queue).Name);
            Assert.IsFalse(typeof(IEnumerable).IsGenericCollectType(), typeof(IEnumerable).Name);

            Assert.IsFalse(typeof(IDictionary).IsGenericCollectType(), typeof(IDictionary).Name);
            Assert.IsFalse(typeof(string).IsGenericCollectType(), typeof(string).Name);

            Assert.IsFalse(typeof(SortedList).IsGenericCollectType(), typeof(SortedList).Name);
            Assert.IsFalse(typeof(ArrayList).IsGenericCollectType(), typeof(ArrayList).Name);

            Assert.IsFalse(typeof(IList).IsGenericCollectType(), typeof(IList).Name);

            Assert.IsTrue(typeof(IEnumerable<string>).IsGenericCollectType(), typeof(IEnumerable<string>).Name);
            Assert.IsTrue(typeof(List<>).IsGenericCollectType(), typeof(List<>).Name);
            Assert.IsTrue(typeof(HashSet<>).IsGenericCollectType(), typeof(HashSet<>).Name);
            Assert.IsTrue(typeof(IList<>).IsGenericCollectType(), typeof(IList<>).Name);
            Assert.IsTrue(typeof(IReadOnlyCollection<>).IsGenericCollectType(), typeof(IReadOnlyCollection<>).Name);
            Assert.IsTrue(typeof(IReadOnlyList<>).IsGenericCollectType(), typeof(IReadOnlyList<>).Name);
            Assert.IsTrue(typeof(ICollection<>).IsGenericCollectType(), typeof(ICollection<>).Name);

        }

        [TestMethod, TestCategory("GetGenericCollectType")]
        public void GetGenericCollectType()
        {
            Assert.IsTrue(typeof(Array).GetGenericCollectType() == typeof(object), typeof(Array).Name);
            Assert.IsTrue(typeof(object[]).GetGenericCollectType() == typeof(object), typeof(object[]).Name);
            Assert.IsTrue(typeof(string[]).GetGenericCollectType() == typeof(string), typeof(string[]).Name);

            Assert.IsTrue(typeof(Stack).GetGenericCollectType() == typeof(object), typeof(Stack).Name);
            Assert.IsTrue(typeof(Queue).GetGenericCollectType() == typeof(object), typeof(Queue).Name);
            Assert.IsTrue(typeof(IEnumerable).GetGenericCollectType() == typeof(object), typeof(IEnumerable).Name);

            Assert.IsTrue(typeof(IDictionary).GetGenericCollectType() == typeof(object), typeof(IDictionary).Name);
            Assert.IsTrue(typeof(string).GetGenericCollectType() == typeof(object), typeof(string).Name);

            Assert.IsTrue(typeof(SortedList).GetGenericCollectType() == typeof(object), typeof(SortedList).Name);
            Assert.IsTrue(typeof(ArrayList).GetGenericCollectType() == typeof(object), typeof(ArrayList).Name);

            Assert.IsTrue(typeof(IList).GetGenericCollectType() == typeof(object), typeof(IList).Name);

            Assert.IsTrue(typeof(IEnumerable<string>).GetGenericCollectType() == typeof(string), typeof(IEnumerable<string>).FullName);
            Assert.IsTrue(typeof(List<string>).GetGenericCollectType() == typeof(string), typeof(List<string>).Name);
            Assert.IsTrue(typeof(HashSet<string>).GetGenericCollectType() == typeof(string), typeof(HashSet<string>).Name);
            Assert.IsTrue(typeof(IList<string>).GetGenericCollectType() == typeof(string), typeof(IList<string>).Name);
            Assert.IsTrue(typeof(IReadOnlyCollection<string>).GetGenericCollectType() == typeof(string), typeof(IReadOnlyCollection<string>).Name);
            Assert.IsTrue(typeof(IReadOnlyList<string>).GetGenericCollectType() == typeof(string), typeof(IReadOnlyList<string>).Name);
            Assert.IsTrue(typeof(ICollection<string>).GetGenericCollectType() == typeof(string), typeof(ICollection<string>).Name);

            Assert.IsTrue(typeof(int[,]).GetGenericCollectType() == typeof(object), typeof(IEnumerable<int[,]>).Name);
            Assert.IsTrue(typeof(int[][]).GetGenericCollectType() == typeof(object), typeof(IEnumerable<int[][]>).Name);
        }
    }
}
