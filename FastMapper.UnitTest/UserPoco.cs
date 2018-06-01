using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FastMapper.UnitTest
{
  public  class UserPoco
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public int Gender { get; set; }

        public IEnumerable<HobbyPoco> Hobbies { get; set; }
    }

}
