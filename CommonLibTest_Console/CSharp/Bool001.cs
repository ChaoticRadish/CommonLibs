using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.CSharp
{
    internal class Bool001() : TestBase("bool 值使用位运算符运算的测试")
    {
        protected override void RunImpl()
        {
            bool b1 = true;
            bool b2 = true;
            bool b3 = false;

            WritePair(b1 | b2);
            WritePair(b1 | b3);
            WritePair(b2 | b3);

            WritePair(b1 & b2);
            WritePair(b1 & b2 & b3);

        }
    }
}
