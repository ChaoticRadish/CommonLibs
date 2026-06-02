using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ChaoticKit.LibTest.Console.CSharp
{
    internal class Bit002() : TestBase("测试计算 int 值的非 0 位数量")
    {
        protected override void RunImpl()
        {
            test(int.MaxValue);
            test(0b111_0111_1_0101);
            test(0b111_0101_1_0101);

        }
        private void test(int value, [CallerArgumentExpression(nameof(value))] string name = "")
        {
            WritePair(name, BitOperations.PopCount((uint)value));
        }
    }
}
