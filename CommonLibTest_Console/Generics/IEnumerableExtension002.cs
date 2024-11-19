using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.Generics
{
    internal class IEnumerableExtension002() : TestBase("可枚举对象相关扩展方法的测试: UntilAnyAway<T1, T2>")
    {
        protected override void RunImpl()
        {
            test(["111", null, "222"], ["333", null]);
            test([123, 32, 1], [1, 2, 3, 4, 5]);
            test<int?>([123, 32, 1], [1, 2, 3, 4, 5]);
        }

        void test<T>(T[] tArr1, T[] tArr2)
        {
            WriteEmptyLine();
            int index = 0;
            foreach ((T? t1, T? t2) in (tArr1, tArr2).UntilAnyAway())
            {
                WriteLine($"{index}  =>  t1: {t1?.ToString() ?? "<null>"} ::: t2: {t2?.ToString() ?? "<null>"}");
                index++;
            }
            WriteEmptyLine();
        }
        
    }
}
