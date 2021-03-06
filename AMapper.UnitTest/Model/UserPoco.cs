﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AMapper.UnitTest.Model
{
  public  class UserPoco
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public int Gender { get; set; }

        public IEnumerable<HobbyPoco> Hobbies { get; set; }

        public List<string> EmptyList { get; set; }

        public ISet<string> NullSet { get; set; }

        public int[] IntArray { get; set; }
    }

}
