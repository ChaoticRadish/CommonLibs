using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.CSharp
{
    internal class Reflection002() : TestBase("反射测试, 判断是否实现特定接口")
    {
        protected override void RunImpl()
        {
            test(check01, "type.IsAssignableTo(typeof(ITest))");
            test(check02, "type.GetInterfaces().Contains(typeof(ITest))");
        }

        private void test(Func<Type, bool> checkFunc, string info)
        {
            WriteEmptyLine();
            WriteEmptyLine();
            WriteLine("测试: " + info);
            WriteEmptyLine();

            WritePair(nameof(ITest), checkFunc(typeof(ITest)));
            WritePair(nameof(ClassA), checkFunc(typeof(ClassA)));
            WritePair(nameof(ClassB), checkFunc(typeof(ClassB)));
            WritePair(nameof(ClassC), checkFunc(typeof(ClassC)));
            WriteEmptyLine();
            WriteEmptyLine();
        }
        private bool check01(Type type)
        {
            return type.IsAssignableTo(typeof(ITest));
        }
        private bool check02(Type type)
        {
            return type.GetInterfaces().Contains(typeof(ITest));
        }

        public interface ITest { }

        public class ClassA : ITest { }

        public class ClassB : ClassA { }

        public class ClassC : ClassA, ITest { }
    }
}
