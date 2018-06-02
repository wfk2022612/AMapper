using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AMapper.UnitTest
{
    [TestClass]
    public class UnitTest
    {
        private Func<UserPoco, UserEntity> _userMapFunc;
        private Func<HobbyPoco, HobbyEntity> _hobbyMapFunc;

        [TestInitialize]
        public void Init()
        {
            _hobbyMapFunc = Map.Create<HobbyPoco, HobbyEntity>()
                .Compile();
            _userMapFunc = Map.Create<UserPoco, UserEntity>()
                .ForMember(user => user.Hobbies, data =>(data.Hobbies??new HobbyPoco[0]).Select(x=>x.Name).ToArray())
                .Compile();
        }
        [TestMethod]
        [TestCategory("单个实例转换")]
        public void TestMethod1()
        {
            var user = new UserPoco();
            user.Name = "jack";
            user.Age = 13;
            user.Hobbies = new HobbyPoco[]
            {
                new HobbyPoco() {Name = "游泳"},
                new HobbyPoco() {Name = "爬山"},
                new HobbyPoco() {Name = "羽毛球"},
                new HobbyPoco() {Name = "唱歌"},
            };

            var people = _userMapFunc(user);
            Assert.IsTrue(people.Name == user.Name, "姓名不一致");
            Assert.IsTrue(people.Age == user.Age, "年龄不一致");
            Assert.IsTrue(people.Hobbies[0] == "游泳", "爱好不一致");
        }

        [TestMethod]
        [TestCategory("1千万个实例转换")]
        public void TestMethod2()
        {
            var user = new UserPoco();
            user.Name = "jack";
            user.Age = 13;
            user.Hobbies = new HobbyPoco[]
            {
                new HobbyPoco() {Name = "游泳"},
                new HobbyPoco() {Name = "爬山"},
                new HobbyPoco() {Name = "羽毛球"},
                new HobbyPoco() {Name = "唱歌"},
            };

            UserEntity people = null;
            const int count = 10000000;
            for (int i = 0; i <= 10000000; i++)
            {
                user.Age = i;
                people = _userMapFunc(user);
            }

            Assert.IsNotNull(people, "转换失败");
            Assert.IsTrue(people.Name == user.Name, "姓名不一致");
            Assert.IsTrue(people.Age == count, "年龄不一致");
            Assert.IsTrue(people.Hobbies[0] == "游泳", "爱好不一致");
        }
    }
}
