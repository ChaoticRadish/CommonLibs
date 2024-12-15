using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.Generics
{
    internal class IEnumerableExtension003() : TestBase("测试无序比较方法")
    {
        protected override void RunImpl()
        {
            test(new int[] { 1, 2, 3, 4, 5, 6, 7 },
                new int[] { 1, 2, 3, 4, 5, 6, 7 });
            test(new int[] { 1, 2, 3, 4, 5, 6, 7 },
                new int[] { 1, 2, 4, 4, 4, 4, 7 });
            test(new int?[] { 1, 2, 3, null, 4, 5, 6, 7 },
                new int?[] { 1, 5, 3, 4, 2, 6, 7, null });
            test(new int?[] { 1, 2, 3, 4, 5, 6, null, null, null, 7 },
                new int?[] { 1, 2, 4, 4, 4, 4, null, null, 7 });
            test(new int?[] { 1, null, 3, 4, 5, 6, null, 2, null, 7 },
                new int?[] { 1, 2, null, 4, 4, 4, null, 6, 7 });


        }

        private void test<T>(IEnumerable<T> t1, IEnumerable<T> t2)
        {
            WritePair(t1.ToArray().FullInfoString());
            WritePair(t2.ToArray().FullInfoString());

            WritePair(t1.DisorderEquals(t2));
            var edResult = t1.ExcludeDisorderEquals(t2);
            foreach (var ((index1, i1), (index2, i2)) in edResult.UntilAllAwayWithIndex())
            {
                WriteLine($"{(index1 < 0 ? "<end>" : (i1?.ToString() ?? "<null>"))} --- {(index2 < 0 ? "<end>" : (i2?.ToString() ?? "<null>"))}");
            }
        }
    }
}
