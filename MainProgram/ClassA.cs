using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainProgram
{
    public class ClassA
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public IEnumerable<ClassC> ClassCs { get; set; }
    }
}
