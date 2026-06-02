using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaoticKit.LibTest.Console.CSharp
{
    internal class Equal001() : TestBase("验证类型比较")
    {
        protected override void RunImpl()
        {
            WritePair(typeof((string str, int v)) == typeof((string, int)));
            WritePair(typeof((string str, int v)) == typeof((string rts, int myInt)));
        }
    }
}
