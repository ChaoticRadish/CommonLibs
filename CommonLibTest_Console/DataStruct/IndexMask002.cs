using Common_Util.Data.Struct;
using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.DataStruct
{
    internal class IndexMask002() : TestBase("使用 IndexMask 的一些场景的测试")
    {
        protected override void RunImpl()
        {
            IndexMask mask1 = IndexMask.FromTrueChar("..111.1.1.1", '1');
            WritePair(mask1);
            int counter = 0;
            int targetCount = 5;
            IndexMask mask2 = new(mask1.All().Select(i =>
            {
                if (!i) return false;
                else return counter++ < targetCount;
            }) );
            WritePair(mask2);

            int[] ints = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
            WriteLine("ints.Filtering(mask1).WithIndex()");
            foreach (var (index, value) in ints.Filtering(mask1).WithIndex())
            {
                WriteLine($"{index}, {value}");
            }
            WriteLine("ints.WithIndex().Filtering(mask1)");
            foreach (var (index, value) in ints.WithIndex().Filtering(mask1))
            {
                WriteLine($"{index}, {value}");
            }
        }
    }
}
