using System;
using System.Linq;
using System.Linq.Expressions;

namespace MainProgram
{
    class Program
    {
        static void Main(string[] args)
        {
            //Func<ClassA, object> exp = ca => ca.ClassCs.Select(x => x.Name).ToArray();

            //  exp(new ClassA());

            //  DynamicMethod di=new DynamicMethod("convert",typeof(string[]),new Type[] {typeof(ClassA)},typeof(Program).Module);
            //  di.CreateDelegate(exp.GetType(), exp.Target);

            var a=new ClassA();
            a.Name = "1";
            a.ClassCs = new[] {new ClassC() {Name = "c1"},};

            var map = FastMapper.FastMap.CreateMap<ClassA, ClassB>()
                .ForMember(t => t.CNames, s => s.ClassCs.Select(x => x.Name).ToArray())
                .Compile();

            

            var b = map(a);

            Console.WriteLine(b.Id);
            Console.WriteLine(b.Name);
            Console.WriteLine(string.Join(",", b.CNames));

            Console.ReadLine();
        }
    }
}
