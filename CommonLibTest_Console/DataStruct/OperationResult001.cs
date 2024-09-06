using Common_Util.Data.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.DataStruct
{
    internal class OperationResult001() : TestBase("测试 Func<IOperationResult>.DoWhile(maxCount) ")
    {
        protected override void RunImpl()
        {
            test(5);
            test(5);
            test(5);
            test(10);
            test(10);
            test(10);
            test(15);
            test(15);
            test(15);
            test(20);
            test(20);
            test(20);



            test2(5);
            test2(5);
            test2(5);
            test2(10);
            test2(10);
            test2(10);
            test2(15);
            test2(15);
            test2(15);
            test2(20);
            test2(20);
            test2(20);
        }

        private void test(int maxCount)
        {
            WriteLine("测试, 最大次数: " + maxCount);
            Func<OperationResult> func = () =>
            {
                if (Common_Util.Random.RandomValueTypeHelper.RandomTrue(0.1))
                {
                    return true;
                }
                else
                {
                    return "随机失败";
                }
            };

            OperationResult result = func.DoWhile(maxCount, (r1, index) =>
            {
                WriteLine($"当前执行 {index} => {r1}");
                return false;
            });
            WriteLine("最终结果: " + result);
            WriteLine();
            WriteLine();
        }

        private void test2(int maxCount)
        {
            testAsync(maxCount).GetAwaiter().GetResult();
        }
        private async Task testAsync(int maxCount)
        {
            WriteLine("测试, 最大次数: " + maxCount);
            Func<Task<OperationResult>> func = async () =>
            {
                await Task.Delay(100);
                if (Common_Util.Random.RandomValueTypeHelper.RandomTrue(0.1))
                {
                    return true;
                }
                else
                {
                    return "随机失败";
                }
            };

            OperationResult result = await func.DoWhileAsync(maxCount, async (r1, index) =>
            {
                await Task.Delay(50);
                WriteLine($"当前异步执行 {index} => {r1}");
                return false;
            });
            WriteLine("最终结果: " + result);
            WriteLine();
            WriteLine();
        }
    }
}
