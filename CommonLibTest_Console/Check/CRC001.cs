using Common_Util.Check;
using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.Check
{
    internal class CRC001() : TestBase("CRC8 拷贝来的代码的测试")
    {
        protected override void RunImpl()
        {
            RunTest<byte[]>(test_byte, [0x99, 0x85, 0x12, 0x10], "byte[] 测试01");
            RunTest<byte[]>(test_byte, [0x11, 0x44, 0x12, 0x10, 0x88], "byte[] 测试02");
            RunTest<byte[]>(test_byte, [0x11, 0x44, 0x12, 0x10, 0x88, 0x99], "byte[] 测试03");
            RunTest<byte[]>(test_byte, [0x11, 0x44, 0x12, 0x10, 0x88, 0x99, 0x00], "byte[] 测试04");


            RunTest<string>(test_str, "[0x99, 0x85, 0x12, 0x10]", "string 测试01");
            RunTest<string>(test_str, "[0x11, 0x44, 0x12, 0x10, 0x88]", "string 测试02");
            RunTest<string>(test_str, "[0x11, 0x44, 0x12, 0x10, 0x88, 0x99]", "string 测试03");
        }

        private void test_byte(byte[] data)
        {
            WriteLine("测试数据: ");
            WriteLine(data.ToHexString());

            WriteLine("生成 CRC8 校验码: ");
            WriteLine("十进制: " + CRCHelper.CRC8(data));
            WriteLine("十六进制: " + CRCHelper.CRC8(data).ToString("X2").ToUpper().PadLeft(2, '0'));

        }
        private void test_str(string str)
        {
            byte[] data = Encoding.UTF8.GetBytes(str);
            WriteLine("测试数据: ");
            WriteLine(data.ToHexString());

            WriteLine("生成 CRC8 校验码: ");
            WriteLine("十进制: " + CRCHelper.CRC8(data));
            WriteLine("十六进制: " + CRCHelper.CRC8(data).ToString("X2").ToUpper().PadLeft(2, '0'));

        }
    }
}
