using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AMapper.UnitTest.Model
{
 public   class UserEntity
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public int Gender { get;  set; }

        public string[] Hobbies { get; set; }

        public ISet<HobbyEntity> HobbieEntities { get; set; }

        public IList<string> EmptyList { get; set; }

        public HashSet<string> NullSet { get; set; }
    }
}
