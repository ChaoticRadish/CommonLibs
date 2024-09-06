using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.CSharp
{
    internal class Enum001() : TestBase("测试位标志枚举")
    {
        protected override void RunImpl()
        {
            WritePair(Enum.GetUnderlyingType(typeof(E1)).Name ?? string.Empty);
            WritePair(Enum.GetUnderlyingType(typeof(E2)).Name ?? string.Empty);
            WritePair(Enum.GetUnderlyingType(typeof(E3)).Name ?? string.Empty);

            WritePair(E1.A.AddFlagWhen(true, E1.B));
            WritePair(E1.A.AddFlagWhen((true, E1.B), (true, E1.C)));

            int testCount = 1000_0000;
            RunTest(test1, "使用扩展方法", testCount, true);
            RunTest(test2, "直接计算", testCount, true);
            RunTest(test3, "转换为 int 再计算", testCount, true);
            RunTest(test4, "使用扩展方法连续检查", testCount, true);
            RunTest(test5, "直接连续检查与计算", testCount, true);
            RunTest(test6, "转换为 int 再计算, 最后转换为 object 再转换为枚举", testCount, true);

        }


        private void test1()
        {
            E1 output = E1.A.AddFlagWhen(true, E1.B);
        }
        private void test2()
        {
            E1 output = E1.A;
            if (true)
            {
                output |= E1.B;
            }
        }
        private void test3()
        {
            E1 output = E1.A;
            if (true)
            {
                output = (E1)(((int)output) | ((int)E1.B));
            }
        }
        private void test6()
        {
            E1 output = E1.A;
            if (true)
            {
                output = (E1)(object)(((int)output) | ((int)E1.B));
            }
        }

        private void test4()
        {
            E1 output = E1.A.AddFlagWhen(
                (true, E1.B), 
                (true, E1.C));
        }
        private void test5()
        {
            E1 output = E1.A;
            if (true)
            {
                output |= E1.B;
            }
            if (true)
            {
                output |= E1.C;
            }
        }

        [Flags]
        private enum E1 : int
        {
            A = 0b001,
            B = 0b010,
            C = 0b100,
        }
        private enum E2 : byte
        {
            A = 1,
            B = 2,
        }
        private enum E3 : long
        {
            A = 1,
            B = 2,
        }
    }
}
