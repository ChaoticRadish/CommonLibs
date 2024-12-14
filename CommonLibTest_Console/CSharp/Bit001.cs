using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.CSharp
{
    internal class Bit001() : TestBase("移位运算学习测试")
    {
        protected override void RunImpl()
        {
            byte b = 0b0100_1100;
            WriteLine(Convert.ToString(b, 2).PadLeft(8, '0'));
            b = (byte)(b << 2);
            WriteLine(Convert.ToString(b, 2).PadLeft(8, '0'));
            b = (byte)(b << -1);
            WriteLine(Convert.ToString(b, 2).PadLeft(8, '0'));
            b = (byte)(b << -3);
            WriteLine(Convert.ToString(b, 2).PadLeft(8, '0'));
            b = 0b0100_1100;
            WriteLine(Convert.ToString(b, 2).PadLeft(8, '0'));
            b = (byte)(b << 1);
            WriteLine(Convert.ToString(b, 2).PadLeft(8, '0'));
            b = (byte)(b << 4);
            WriteLine(Convert.ToString(b, 2).PadLeft(8, '0'));
            b = (byte)(b << 9);
            WriteLine(Convert.ToString(b, 2).PadLeft(8, '0'));
            b = 0b0100_1100;
            WriteLine(Convert.ToString(b, 2).PadLeft(8, '0'));
            b = (byte)(b << 33);
            WriteLine(Convert.ToString(b, 2).PadLeft(8, '0'));

            WriteEmptyLine();

            b = 0b0100_1100;
            WriteLine(Convert.ToString(b, 2).PadLeft(8, '0'));
            b = (byte)(b >> 2);
            WriteLine(Convert.ToString(b, 2).PadLeft(8, '0'));
            b = (byte)(b >> -1);
            WriteLine(Convert.ToString(b, 2).PadLeft(8, '0'));
            b = (byte)(b >> -3);
            WriteLine(Convert.ToString(b, 2).PadLeft(8, '0'));
            b = 0b0100_1100;
            WriteLine(Convert.ToString(b, 2).PadLeft(8, '0'));
            b = (byte)(b >> 1);
            WriteLine(Convert.ToString(b, 2).PadLeft(8, '0'));
            b = (byte)(b >> 4);
            WriteLine(Convert.ToString(b, 2).PadLeft(8, '0'));
            b = (byte)(b >> 9);
            WriteLine(Convert.ToString(b, 2).PadLeft(8, '0'));
            b = 0b0100_1100;
            WriteLine(Convert.ToString(b, 2).PadLeft(8, '0'));
            b = (byte)(b >> 33);
            WriteLine(Convert.ToString(b, 2).PadLeft(8, '0'));
        }
    }
}
