using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AMapper.UnitTest.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AMapper.UnitTest
{
    [TestClass]
    public class AMapperTest
    {
        private Func<UserPoco, UserEntity> _userMapFunc;
        private Func<HobbyPoco, HobbyEntity> _hobbyMapFunc;

        [TestInitialize]
        public void Init()
        {
            _hobbyMapFunc = Map.Create<HobbyPoco, HobbyEntity>()
                .ForMember(t => t.CharString, s => s.Chars != null && s.Chars.Any() ? new string(s.Chars) : null)
                .Compile();
            _userMapFunc = Map.Create<UserPoco, UserEntity>()
                .ForMember(t => t.Hobbies, s => (s.Hobbies ?? new HobbyPoco[0]).Select(x => x.Name).ToArray())
                .ForMember(t => t.HobbieEntities, s => s.Hobbies)
                .Compile();
        }
        [TestMethod]
        [TestCategory("单个实例转换")]
        public void TestMethod1()
        {
            var user = new UserPoco();
            user.Name = "jack";
            user.Age = 13;
            user.EmptyList=new List<string>() ;
            user.NullSet = null;
            user.Hobbies = new HobbyPoco[]
            {
                new HobbyPoco() {Name = "游泳",Chars = new [] {'Y','Y'}},
                new HobbyPoco() {Name = "爬山",Chars = new [] {'P','S'}},
                new HobbyPoco() {Name = "羽毛球",Chars = new [] {'Y','M','Q'}},
                new HobbyPoco() {Name = "唱歌",Chars = new [] {'C','G'}},
            };

            var people = _userMapFunc(user);
            Assert.IsTrue(people.Name == user.Name, "姓名不一致");
            Assert.IsTrue(people.Age == user.Age, "年龄不一致");
            Assert.IsTrue(people.Hobbies[0] == "游泳", "爱好不一致");
            Assert.IsTrue(people.HobbieEntities.First().Name == "游泳", "爱好不一致");
            Assert.IsTrue(people.HobbieEntities.First().CharString == "YY", "爱好首字母不一致");
            Assert.IsTrue(!people.EmptyList.Any(),"空列表不为空");
            Assert.IsTrue(people.NullSet==null, "NullSet不为NULL");
        }

    }
}
