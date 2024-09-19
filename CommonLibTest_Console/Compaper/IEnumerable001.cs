using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.Compaper
{
    internal class IEnumerable001() : TestBase("测试 IEnumerable<T>.DisorderEquals 无序相等比较方法")
    {
        protected override void RunImpl()
        {
            test(["string", "aaa", "bbb"], ["aaa", "string", "bbb", null]);
            test([null, "string", "aaa", "bbb"], ["aaa", "string", "bbb", null]);
            test(["string", "aaa", "bbb"], ["aaa", "string", "bbb"]);
            test(["string", "AAA", "bbb"], ["aaa", "string", "bbb"]);
            test(["string", "AAA", "bbb"], ["string", "bbb", "AAA"]);
            test([], []);
        }

        private void test(IEnumerable<string?> list1, IEnumerable<string?> list2)
        {
            WritePair(list1.FullInfoString());
            WritePair(list2.FullInfoString());
            WritePair(list1.DisorderEquals(list2));
            WriteEmptyLine();
        }


    }
}
