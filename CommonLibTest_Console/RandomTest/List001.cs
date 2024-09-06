using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.RandomTest
{
    internal class List001() : TestBase("随机从列表中取一项")
    {
        List<string> testList1 = ["111", "222", "333", "aaa", "bbb", "ccc"];
        List<string?> testList2 = ["111", "222", "333", "aaa", null, "bbb", "ccc"];

        protected override void RunImpl()
        {
            RunTest(test1, "测试1", 100);
            RunTest(test2, "测试2 测试排除功能", 100);
        }

        private void test1()
        {

            if (testList1.TryGetRandom(out var item1))
            {
                WritePair(item1);
            }
            if (testList2.TryGetRandom(out var item2))
            {
                item2 ??= "<null>";
                WritePair(item2);
            }
            if (testList2.TryGetRandom(out var item3, ["111", "222", "333"]))
            {
                item3 ??= "<null>";
                WritePair(item3);
            }
            if (testList2.TryGetRandom(out var item4, ["111", "222", "333", "aaa", "bbb"]))
            {
                item4 ??= "<null>";
                WritePair(item4);
            }
            if (testList2.TryGetRandom(out var item5, ["111", "222", "333", "aaa", "bbb", "ccc", null]))
            {
                item5 ??= "<null>";
                WritePair(item5);
            }
            else
            {
                WriteLine("item5 get false");
            }
            WriteEmptyLine();
        }
        int index = 0;
        private void test2()
        {
            WriteLine($"执行测试 2 ::: {++index}");

            var test1 = new List<string?>() { "111", "222", "333" };
            if (testList2.TryGetRandom(out var item3, test1))
            {
                if (test1.Contains(item3))
                {
                    // 没有正确排除时将被打印
                    item3 ??= "<null>"; 
                    WritePair(item3);
                }
            }
            var test2 = new List<string?>() { "111", "222", "333", "aaa", "bbb" };
            if (testList2.TryGetRandom(out var item4, test2))
            {
                if (test2.Contains(item4))
                {
                    // 没有正确排除时将被打印
                    item4 ??= "<null>";
                    WritePair(item4);
                }
            }
            WriteEmptyLine();
        }
    }
}
