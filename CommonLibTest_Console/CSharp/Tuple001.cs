using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.CSharp
{
    internal class Tuple001() : TestBase("测试使用元组区分最大值与最小值")
    {
        protected override void RunImpl()
        {
            test(1, 3);
            test(3, 1);
            test(2, 2);

            RunTest(test1, "使用工具库的方法测试", 100);
            WritePair(key: "错误数", errorCount);
        }

        private void test(int v1, int v2)
        {
            WritePair(key: "传入值", (v1, v2));
            deal(ref v1, ref v2);
            WritePair(key: "较小值", v1);
            WritePair(key: "较大值", v2);

            WriteEmptyLine();
        }



        private void deal(ref int min, ref int max)
        {
            (min, max) = (Math.Min(min, max), Math.Max(min, max));
        }


        int errorCount = 0;
        Random random = new Random();
        private void test1()
        {
            int v1 = random.Next(0, 100);
            int v2 = random.Next(0, 100);

            WritePair(key: "传入值", (v1, v2));
            Common_Util.Maths.CompareHelper.JudgeBigger(ref v1, ref v2);
            WritePair(key: "较小值", v1);
            WritePair(key: "较大值", v2);
            WriteEmptyLine();

            if (v1 > v2)
            {
                WriteLine($"结果错误: v1: {v1}  ---  v2: {v2}");
                errorCount ++;
            }
        }

    }
}
