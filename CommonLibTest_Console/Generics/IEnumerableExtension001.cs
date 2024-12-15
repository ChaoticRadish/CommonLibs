using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.Generics
{
    internal class IEnumerableExtension001() : TestBase("可枚举对象相关扩展方法的测试: UntilAllAway<T1, T2>")
    {
        protected override void RunImpl()
        {
            RunTestMark();
        }

        [TestMethod]
        void test1s()
        {
            test1(["111", null, "222"], ["333", null]);
            test1([123, 32, 1], [1, 2, 3, 4, 5]);
            test1<int?>([123, 32, 1], [1, 2, 3, 4, 5]);
        }
        [TestMethod]
        void test2s()
        {
            test2(["111", null, "222"], ["333", null]);
            test2([123, 32, 1], [1, 2, 3, 4, 5]);
            test2<int?>([123, 32, 1], [1, 2, 3, 4, 5]);
        }

        void test1<T>(T[] tArr1, T[] tArr2)
        {
            WriteEmptyLine();
            int index = 0;
            foreach ((T? t1, T? t2) in (tArr1, tArr2).UntilAllAway())
            {
                WriteLine($"{index}  =>  t1: {t1?.ToString() ?? "<null>"} ::: t2: {t2?.ToString() ?? "<null>"}");
                index++;
            }
            WriteEmptyLine();
        }
        void test2<T>(T[] tArr1, T[] tArr2)
        {
            WriteEmptyLine();
            int index = 0;
            // foreach (((int i1, T? t1), (int i2, T? t2)) in (tArr1, tArr2).UntilAllAwayWithIndex())
            foreach (var ((i1, t1), (i2, t2)) in (tArr1, tArr2).UntilAllAwayWithIndex())
            {
                WriteLine($"{index}  =>  t1: [{i1}]{t1?.ToString() ?? "<null>"} ::: t2: [{i2}]{t2?.ToString() ?? "<null>"}");
                index++;
            }
            WriteEmptyLine();
        }

    }
}
