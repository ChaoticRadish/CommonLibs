using Common_Util;
using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CommonLibTest_Console.Xml.ReadWrite001;

namespace CommonLibTest_Console.CSharp
{
    internal class Reflection006() : TestBase("遍历泛型参数")
    {

        protected override void RunImpl()
        {
            test(typeof(string));
            test(typeof(int?));
            test(typeof(TestA<>).GetMethod(nameof(TestA<object>.Method01))!.ReturnType);
            test(typeof(TestA<>).GetMethod(nameof(TestA<object>.Method02))!.ReturnType);
            test(typeof(TestA<>).GetMethod(nameof(TestA<object>.Method02))!.ReturnType, false);
        }

        class TestA<T>
        {
            public Dictionary<string, TA> Method01<TA>()
            {
                return new();
            }
            public Dictionary<Dictionary<Dictionary<T1, string>, TA>, Dictionary<T1, TA>> Method02<T1, TA>()
                where T1 : notnull
            {
                return new();
            }
        }

        private void test(Type type, bool includeSelf = true)
        {
            WriteLine("类型: " + type.ToString() + $"  ({includeSelf.ToString("includeSelf!", "not includeSelf")})");
            int index = 0;
            foreach (Type t in ReflectionHelper.PreorderGenericParameterTree(type, includeSelf))
            {
                WriteLine($"{index}. " + t.ToString());
                index++;
            }
            WriteEmptyLine();
        }
    }
}
