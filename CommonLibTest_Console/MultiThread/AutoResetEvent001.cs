using Common_Util.Extensions.MultiThread;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.MultiThread
{
    internal class AutoResetEvent001() : TestBase("测试 AutoResetEvent 的扩展方法 TryWaitOneAsync ")
    {
        protected override void RunImpl()
        {
        }
        protected override async Task RunImplAsync()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            AutoResetEvent are = new AutoResetEvent(false);


            Task task0 = Task.Run(async () =>
            {
                var log = GetLevelLogger("Task0");
                try
                {
                    log.Info("启动");

                    await Task.Delay(6 * 1000);
                    log.Info("取消");
                    cts.Cancel();

                    log.Info("结束");
                }
                catch (Exception ex)
                {
                    log.Error("异常", ex);
                }
            });
            Task task1 = Task.Run(async () =>
            {
                var log = GetLevelLogger("Task1");
                try
                {
                    log.Info("启动");

                    bool b = await are.TryWaitOneAsync(10 * 1000, cts.Token);
                    log.Info("结果: " + b);

                    log.Info("结束");
                }
                catch (Exception ex)
                {
                    log.Error("异常", ex);
                }

            });
            Task task2 = Task.Run(async () =>
            {
                var log = GetLevelLogger("Task2");
                try
                {
                    log.Info("启动");

                    bool b = await are.TryWaitOneAsync(3 * 1000, cts.Token);
                    log.Info("结果: " + b);

                    log.Info("结束");
                }
                catch (Exception ex)
                {
                    log.Error("异常", ex);
                }

            });
            Task task3 = Task.Run(async () =>
            {
                var log = GetLevelLogger("Task3");
                try
                {
                    log.Info("启动");

                    bool b = await are.TryWaitOneAsync(15 * 1000);
                    log.Info("结果: " + b);

                    log.Info("结束");
                }
                catch (Exception ex)
                {
                    log.Error("异常", ex);
                }

            });


            Task task5 = Task.Run(async () =>
            {
                var log = GetLevelLogger("Task5");
                try
                {
                    log.Info("启动");

                    await Task.Delay(12 * 1000);
                    log.Info("信号");
                    are.Set();

                    log.Info("结束");
                }
                catch (Exception ex)
                {
                    log.Error("异常", ex);
                }
            });

            await Task.WhenAll(task0, task1, task2, task3, task5);
        }
    }
}
