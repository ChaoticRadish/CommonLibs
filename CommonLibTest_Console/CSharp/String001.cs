using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.CSharp
{
    internal class String001() : TestBase("字符串与 0x00 字符")
    {
        protected override void RunImpl()
        {
            string str = ((char)0).ToString();
            WritePair(str.Length);
            WritePair(str.FullInfoString());
            WritePair(str.Select(i => (byte)i).ToHexString());
        }
    }
}
