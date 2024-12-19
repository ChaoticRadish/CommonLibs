using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CommonLibTest_Console.Text
{
    internal class Unescape001() : TestBase("使用 System.Text.RegularExpressions.Regex.Unescape 反转义")
    {
        protected override void RunImpl()
        {
            WritePair(value: Regex.Unescape("\\n"));
            WritePair(value: Regex.Unescape("\\u1234"));
            WritePair(value: Regex.Unescape("\\uA001"));
            WritePair(value: Regex.Unescape("\\u005C"));
            WritePair(value: Regex.Unescape("\\u0080"));
            WritePair(value: Regex.Unescape("\\u0061"));
            WritePair(value: Regex.Unescape("\\u0063"));
            WritePair(value: Regex.Unescape("\\u0065"));
            WritePair(value: Regex.Unescape("\\x88"));
            WritePair(value: Regex.Unescape("\\x67"));
            WritePair(value: Regex.Unescape("\\x6767"));
        }
    }
}
