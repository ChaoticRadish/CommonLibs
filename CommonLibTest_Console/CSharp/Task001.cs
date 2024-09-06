using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.CSharp
{
    internal class Task001() : TestBase("对 Task 的一些测试验证, 主要在抛出异常方面")
    {
        protected override void RunImpl()
        {
        }
        protected override async Task RunImplAsync()
        {
            WriteTimeLine("第一部分");
            await part1();

            WriteEmptyLine(3);
            WriteTimeLine("第二部分");
            await part2();

        }

        private async Task part1()
        {
            try
            {
                Task test1 = GetTestTask(1, false)();

                await Task.Delay(100);

                Task test2 = GetTestTask(2, true, 1000)();

                WriteTimeLine("await test1");
                await test1;
                WriteTimeLine("await test2");
                await test2;
                WriteTimeLine("结束");
            }
            catch (Exception ex)
            {
                WriteTimeLine("发生异常: " + ex.Message);
            }
            finally
            {
                WriteTimeLine("finally");
            }
        }
        private async Task part2()
        {
            try
            {
                Task test1 = GetTestTask(1, false)();

                await Task.Delay(100);

                Task test2 = TryTask(GetTestTask(2, true, 1000)());

                WriteTimeLine("await test1");
                await test1;
                WriteTimeLine("await test2");
                await test2;
                WriteTimeLine("结束");
            }
            catch (Exception ex)
            {
                WriteTimeLine("发生异常: " + ex.Message);
            }
            finally
            {
                WriteTimeLine("finally");
            }
        }

        Func<Task> GetTestTask(int index, bool exception, int throwTime = 0)
        {
            return async () =>
            {
                WriteTimeLine("测试 Task " + index + " 开始");
                if (exception)
                {
                    await Task.Delay(throwTime > 0 ? throwTime : 1);
                    WriteTimeLine("触发异常: " + index);
                    throw new Exception("测试异常! " + index);
                }
                else
                {
                    await Task.Delay(5000);
                }
                WriteTimeLine("测试 Task " + index + " 结束");
            };
        }
        private async Task TryTask(Task task)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                WriteTimeLine("捕获异常: " + ex.Message);
            }
        }
    }
}
