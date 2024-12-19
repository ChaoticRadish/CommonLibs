using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.CSharp
{
    internal class Converter001() : TestBase("测试 System 下的一些转换方法是否能够指定进制转换")
    {
        protected override void RunImpl()
        {
            try
            {
                WritePair(Convert.ToInt32("0x111"));
            }
            catch (Exception ex)
            {
                WriteLine(ex.ToString());
            }

            try
            {

                WritePair(int.TryParse("0x111", out int v));
                WritePair(v);
            }
            catch (Exception ex)
            {
                WriteLine(ex.ToString());
            }
        }
    }
}
