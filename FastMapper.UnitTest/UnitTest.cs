using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FastMapper.UnitTest
{
    [TestClass]
    public class UnitTest
    {
        private Func<User, People> _mapFunc;

        [TestInitialize]
        public void Init()
        {
            _mapFunc = FastMap.CreateMap<User, People>().Compile();
        }
        [TestMethod]
        [TestCategory("单个实例转换")]
        public void TestMethod1()
        {
            User user = new User();
            user.Name = "jack";
            user.Age = 13;

            var people = _mapFunc(user);
            Assert.IsTrue(people.Name == user.Name, "姓名不一致");
            Assert.IsTrue(people.Age == user.Age, "年龄不一致");
        }

        [TestMethod]
        [TestCategory("多个实例转换")]
        public void TestMethod2()
        {
            var user = new User();
            user.Name = "jack";
            user.Age = 13;

            People people = null;
            const int count = 10000000;
            for (int i = 0; i <= 10000000; i++)
            {
                user.Age = i;
                people = _mapFunc(user);
            }
            Assert.IsNotNull(people, "转换失败");
            Assert.IsTrue(people.Name == user.Name, "姓名不一致");
            Assert.IsTrue(people.Age == count, "年龄不一致");
        }
    }
}
