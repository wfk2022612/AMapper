using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AMapper.UnitTest
{
 public   class UserEntity
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public int Gender { get;  set; }

        public string[] Hobbies { get; set; }
    }
}
