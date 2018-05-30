using System.Collections.Generic;

namespace MainProgram
{
    public class ClassA
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public IEnumerable<ClassC> ClassCs { get; set; }
    }
}
