using Common_Util.Extensions;
using Common_Util.Log;
using Common_Util.Module.Concurrency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommonLibTest_Console.MultiThread
{
    internal class WorkCountGuard001() : TestBase("测试 WorkCountGuard")
    {
        WorkCountGuard guard = new WorkCountGuard();

        protected override void RunImpl()
        {
            ILoggerOutEmptyLine = false;
        }
        protected override async Task RunImplAsync()
        {

            List<Task> tasks = new();
            tasks.Add(RunWork("工作线程 1", 3));
            await Task.Delay(500);
            tasks.Add(RunWork("工作线程 2", 3));
            tasks.Add(RunWork("工作线程 3", 3));
            await Task.Delay(100);
            tasks.Add(RunLock("锁定线程 1", 3));
            await Task.Delay(100);
            tasks.Add(RunWork("工作线程 4", 3));
            await Task.Delay(700);
            tasks.Add(RunLock("锁定线程 2", 3));
            await Task.Delay(700);
            tasks.Add(RunWork("工作线程 5", 5));
            await Task.Delay(100);
            tasks.Add(RunLock("锁定线程 3", 3, TimeSpan.FromSeconds(3)));
            tasks.Add(RunLock("锁定线程 4", 3, TimeSpan.FromSeconds(8)));
            Task.WaitAll(tasks.Where(t => !t.IsCompleted).ToArray());
            tasks.Add(RunWork("工作线程 6", 5));
            await Task.Delay(100);
            tasks.Add(RunLock("锁定线程 5", 3, TimeSpan.FromSeconds(3)));
            tasks.Add(RunLock("锁定线程 6", 3, TimeSpan.FromSeconds(8)));


            Task.WaitAll(tasks.Where(t => !t.IsCompleted).ToArray());
        }
        private Task RunWork(string title, int times, TimeSpan? timeout = null)
        {
            return Run(logger =>
            {
                logger.Info($"请求工作, 当前工作数: {guard.ActiveWorkers}");
                using var token = guard.TryBeginWork(timeout ?? TimeSpan.FromSeconds(10));
                if (!token.GetSuccess)
                {
                    logger.Warning("获取令牌失败");
                }
                else
                {
                    logger.Info("开始工作");
                    foreach (int i in times.ForUntil())
                    {
                        Thread.Sleep(1000);
                        //logger.Info("work work");
                    }
                    logger.Info("工作完成");
                }
            }, title);
        }
        private Task RunLock(string title, int times, TimeSpan? timeout = null)
        {
            return Run(logger =>
            {
                logger.Info($"请求锁定, 当前工作数: {guard.ActiveWorkers}");
                using var token = guard.TryAcquireLock(timeout ?? TimeSpan.FromSeconds(10));
                if (!token.GetSuccess)
                {
                    logger.Warning("获取令牌失败");
                }
                else
                {
                    logger.Info("锁定工作");
                    foreach (int i in times.ForUntil())
                    {
                        Thread.Sleep(1000);
                        //logger.Info("lock lock");
                    }
                    logger.Info("锁定结束");
                }
            }, title);
        }
        private Task Run(Action<ILevelLogger> action, string title)
        {
            var config = LevelLoggerHelper.LogToLoggerConfig.GetDefault(title);
            // config.MessageHandler = str => $"{title} - {str}";
            var logger = Common_Util.Log.LevelLoggerHelper.LogTo(this, config: config);
            return Task.Run(() =>
            {
                action(logger);
            });
        }
    }
}
