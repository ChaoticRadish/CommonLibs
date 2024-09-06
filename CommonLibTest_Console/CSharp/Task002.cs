using Common_Util;
using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommonLibTest_Console.CSharp
{
    internal class Task002() : TestBase("测试取消等待Task (创建一个新的Task来等待Task, 可以取消新的Task, 以达到取消等待的效果)")
    {
        protected override void RunImpl()
        {
            test1();

            WriteEmptyLine(3);

            test2();

            WriteEmptyLine(3);

            test3();
        }

        private void test1()
        {
            ResetStartTime();

            CancellationTokenSource cts = new CancellationTokenSource();

            Task t1 = WaitSomeTime(1, 8000);
            Task t2 = WaitSomeTime(2, 2000);
            Task t3 = WaitSomeTime(3, 4000, () =>
            {
                WriteTimeLine("取消等待! ");
                try
                {
                    cts.Cancel();
                }
                catch (Exception ex)
                {
                    WriteTimeLine("取消等待发生异常, " + ex.Message);
                }
            });
            Task t4 = WaitSomeTimeThrowException(4, 3000);

            WriteTimeLine("等待任意一个执行结束");
            Task.WaitAny(t1, t2, t3);
            WriteTimeLine("Flag");

            WriteTimeLine("等待全部执行结束, 或者被取消等待");
            WaitAllUntilCancel([t1, t2, t3, t4], cts.Token);
            WriteTimeLine("Flag");

            WriteTimeLine("等待全部执行结束");
            Task.WaitAll(new List<Task>([t1, t2, t3, t4]).Where(i => !i.IsCompleted).ToArray());
            WriteTimeLine("Flag");

        }
        private void test2()
        {
            ResetStartTime();

            CancellationTokenSource cts = new CancellationTokenSource();

            Task t1 = WaitSomeTime(1, 8000);
            Task t2 = WaitSomeTime(2, 2000);
            Task t3 = WaitSomeTime(3, 4000, () =>
            {
                WriteTimeLine("取消等待! ");
                try
                {
                    cts.Cancel();
                }
                catch (Exception ex)
                {
                    WriteTimeLine("取消等待发生异常, " + ex.Message);
                }
            });
            Task t4 = WaitSomeTimeThrowException(4, 3000);

            WriteTimeLine("等待任意一个执行结束");
            Task.WaitAny(t1, t2, t3);
            WriteTimeLine("Flag");

            //WriteTimeLine("等待 t1 结束, 或者被取消等待");
            //t1.WaitUntilCancel(cts.Token);
            //WriteTimeLine("Flag");

            WriteTimeLine("等待全部执行结束, 或者被取消等待");
            try
            {
                TaskHelper.WaitAllUntilCancel([t1, t2, t3, t4], cts.Token).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                WriteTimeLine("等待得到异常: " + ex.Message);
            }
            WriteTimeLine("Flag");

            WriteTimeLine("等待全部执行结束");
            Task.WaitAll(new List<Task>([t1, t2, t3, t4]).Where(i => !i.IsCompleted).ToArray());
            WriteTimeLine("Flag");

        }

        private void test3()
        {
            ResetStartTime();

            CancellationTokenSource cts = new CancellationTokenSource();

            Task t1 = WaitSomeTime(1, 2000, () =>
            {
                WriteTimeLine("取消等待! ");
                try
                {
                    cts.Cancel();
                }
                catch (Exception ex)
                {
                    WriteTimeLine("取消等待发生异常, " + ex.Message);
                }
            });
            Task t2 = WaitSomeTimeThrowException(2, 4000);

            WriteTimeLine("等待 t2 执行结束, 或者取消等待");
            t2.WaitAsync(cts.Token).GetAwaiter().GetResult();   // 预期会抛出 task 被取消的异常
            WriteTimeLine("Flag");
        }

        Task WaitSomeTimeThrowException(int index, int time)
        {
            return Task.Run(async () =>
            {
                WriteTimeLine($"Test {index} Start");

                await Task.Delay(time);
                // Thread.Sleep(time);

                WriteTimeLine($"Test {index} 触发异常");

                throw new Exception("测试");
            });
        }
        Task WaitSomeTime(int index, int time, Action? doneAction = null)
        {
            return Task.Run(async () =>
            {
                try
                {
                    WriteTimeLine($"Test {index} Start");

                    await Task.Delay(time);
                    // Thread.Sleep(time);

                    WriteTimeLine($"Test {index} End");

                    doneAction?.Invoke();
                }
                catch (Exception ex)
                {
                    WriteTimeLine($"Test {index} 异常: " + ex.Message);
                }
            });
        }

        void WaitAllUntilCancel(Task[] tasks, CancellationToken token)
        {
            Task.Run(() =>
            {
                WriteTimeLine("- 开始等待");
                try
                {
                    Task.WaitAll(tasks, token);
                }
                catch (OperationCanceledException)
                {
                    WriteTimeLine("- 等待取消: OperationCanceledException");
                }
                WriteTimeLine("- 等待结束");
                WriteTimeLine("- 当前状态: \n" + Common_Util.String.StringHelper.Concat(tasks.Select(i => $"-- IsCompleted: {i.IsCompleted}; IsFaulted: {i.IsFaulted};").ToList()));
            }).GetAwaiter().GetResult();
        }

    }
}
