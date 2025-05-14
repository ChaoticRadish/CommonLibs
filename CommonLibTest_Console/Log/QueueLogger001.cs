using Common_Util.Extensions;
using Common_Util.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.Log
{
    class QueueLogger001() : TestBase("测试队列日志输出器")
    {
        class QueueLoggerTest(QueueLogger001 parent) : QueueLogger
        {
            QueueLogger001 parent = parent;
            protected override void Output(LogData log)
            {
                parent.WriteLine(DateTime.Now.ToString("HH:mm:ss:ffff"));
                parent.Log(log);
                Thread.Sleep(120);
            }
        }

        protected override void RunImpl()
        {
        }
        protected override async Task RunImplAsync()
        {
            var queueLogger = new QueueLoggerTest(this);
            var logger = LevelLoggerHelper.LogTo(queueLogger);
            var task1 = Task.Run(async () =>
            {
                foreach (int i in 10.ForUntil())
                {
                    logger.Info($"测试 loop1: {i}");
                    await Task.Delay(100);
                }
            });
            var task2 = Task.Run(async () =>
            {
                foreach (int i in 10.ForUntil())
                {
                    logger.Info($"测试 loop2: {i}");
                    await Task.Delay(100);
                }
            });
            foreach (int i in 100.ForUntil())
            {
                logger.Info($"测试 loop3: {i}");
            }

            Task.WaitAll(task1, task2);

            WriteLine("Pause()");
            queueLogger.Pause();

            await Task.Delay(2000);

            WriteLine("Continue()");
            queueLogger.Continue();

            await Task.Delay(2000);

            WriteLine("Stop()");
            queueLogger.Stop();

            await Task.Delay(1000);

            WriteLine("Continue()");
            queueLogger.Continue();

            WriteLine("await WaitUntilEmptyAsync()");
            await queueLogger.WaitUntilEmptyAsync();
            WriteLine("日志输出结束");
        }
    }
}
