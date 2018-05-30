﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FastMapper.UnitTest
{
    class UserEntity
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public int Gender { get; private set; }

        public string[] Hobbies { get; set; }
    }
}
