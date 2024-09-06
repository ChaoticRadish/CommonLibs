using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.TestsTest
{
    internal class Async001() : TestBase("测试TestBase.RunImplAsync")
    {
        protected override void RunImpl()
        {
            WriteLine("!!!");
        }
        protected override async Task RunImplAsync()
        {
            for (int i = 0; i < 10; i++)
            {
                WriteLine("测试 " + i);
                await Task.Delay(1000);
            }
            WriteLine("结束");
        }
    }
}
