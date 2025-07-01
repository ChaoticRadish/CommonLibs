using Common_Util.Module.Concurrency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.MultiThread
{
    internal class PriorityLock001() : TestBase("测试释放优先的锁 ReleasePriorityLock")
    {
        private static readonly ReleasePriorityLock _lock = new ReleasePriorityLock();
        private static int _resourceCount = 0;
        private static readonly Random _random = new Random();
        private static readonly List<string> _log = new List<string>();
        private static readonly ManualResetEventSlim _startEvent = new ManualResetEventSlim(false);

        protected override void RunImpl() { }
        protected override async Task RunImplAsync()
        {
            Console.WriteLine("全面测试 ReleasePriorityLock 的释放优先机制");
            Console.WriteLine("=================================================");
            Console.WriteLine("测试场景包括:");
            Console.WriteLine("1. 基础创建/释放操作");
            Console.WriteLine("2. 多个释放请求排队");
            Console.WriteLine("3. 创建操作中的高并发压力");
            Console.WriteLine("4. 创建操作中的异常情况");
            Console.WriteLine("5. 长时间创建操作");
            Console.WriteLine("6. 混合操作（创建/释放交错）");
            Console.WriteLine("7. 资源计数正确性");
            Console.WriteLine("8. 对象释放后操作");
            Console.WriteLine("=================================================\n");

            try
            {
                // 场景1: 基础创建/释放操作
                TestScenario1();
                await Task.Delay(2000);

                // 重置状态
                ResetTestState();

                // 场景2: 多个释放请求排队
                TestScenario2();
                await Task.Delay(2000);

                // 重置状态
                ResetTestState();

                // 场景3: 高并发压力测试
                TestScenario3();
                await Task.Delay(2000);

                // 重置状态
                ResetTestState();

                // 场景4: 异常情况处理
                TestScenario4();
                await Task.Delay(2000);

                // 重置状态
                ResetTestState();

                // 场景5: 长时间操作
                TestScenario5();
                await Task.Delay(2000);

                // 重置状态
                ResetTestState();

                // 场景6: 混合操作（创建/释放交错）
                TestScenario6();
                await Task.Delay(2000);

                // 重置状态
                ResetTestState();

                // 场景7: 对象释放后操作
                TestScenario7();
            }
            catch (Exception ex)
            {
                LogMessage($"测试失败: {ex.Message}", ConsoleColor.Red);
            }

            Console.WriteLine("\n所有测试场景完成!");
        }


        static void ResetTestState()
        {
            lock (_log)
            {
                _resourceCount = 0;
                _log.Clear();
            }
        }

        static void TestScenario1()
        {
            LogMessage("\n=== 场景1: 基础创建/释放操作 ===", ConsoleColor.White);

            // 简单创建操作
            Thread create1 = new Thread(() => SimulateCreateOperation(1, 200));
            create1.Start();
            create1.Join();

            // 简单释放操作
            Thread release1 = new Thread(() => SimulateReleaseOperation(1));
            release1.Start();
            release1.Join();

            // 再次创建
            Thread create2 = new Thread(() => SimulateCreateOperation(2, 150));
            create2.Start();
            create2.Join();
        }

        static void TestScenario2()
        {
            LogMessage("\n=== 场景2: 多个释放请求排队 ===", ConsoleColor.White);

            // 启动多个释放操作
            Thread release1 = new Thread(() =>
            {
                Thread.Sleep(50);
                SimulateReleaseOperation(1);
            });

            Thread release2 = new Thread(() =>
            {
                Thread.Sleep(100);
                SimulateReleaseOperation(2);
            });

            Thread release3 = new Thread(() =>
            {
                Thread.Sleep(150);
                SimulateReleaseOperation(3);
            });

            release1.Start();
            release2.Start();
            release3.Start();

            // 同时启动创建操作
            for (int i = 1; i <= 5; i++)
            {
                int id = i;
                Thread t = new Thread(() => SimulateCreateOperation(id, 100));
                t.Start();
            }

            // 等待所有操作完成
            release1.Join();
            release2.Join();
            release3.Join();
        }

        static void TestScenario3()
        {
            LogMessage("\n=== 场景3: 高并发压力测试 ===", ConsoleColor.White);

            const int threadCount = 20;
            var threads = new List<Thread>();

            // 启动混合操作线程
            for (int i = 1; i <= threadCount; i++)
            {
                int id = i;
                Thread t = new Thread(() =>
                {
                    if (id % 4 == 0) // 每4个线程中有一个是释放操作
                    {
                        Thread.Sleep(_random.Next(50, 150));
                        SimulateReleaseOperation(id);
                    }
                    else
                    {
                        SimulateCreateOperation(id, _random.Next(50, 200));
                    }
                });

                threads.Add(t);
                t.Start();

                // 随机延迟启动
                Thread.Sleep(_random.Next(10, 30));
            }

            // 等待所有线程完成
            foreach (var t in threads)
            {
                t.Join();
            }
        }

        static void TestScenario4()
        {
            LogMessage("\n=== 场景4: 异常情况处理 ===", ConsoleColor.White);

            // 正常创建操作
            Thread create1 = new Thread(() => SimulateCreateOperation(1, 100));
            create1.Start();

            // 异常创建操作
            Thread create2 = new Thread(() =>
            {
                try
                {
                    LogMessage($"创建操作 2 (异常) 请求进入", ConsoleColor.Gray);
                    using (_lock.Create())
                    {
                        LogMessage($"创建操作 2 开始执行 (将抛出异常)", ConsoleColor.Green);
                        throw new InvalidOperationException("模拟创建操作异常");
                    }
                }
                catch (Exception ex)
                {
                    LogMessage($"创建操作 2 出错: {ex.Message}", ConsoleColor.Red);
                }
            });
            create2.Start();

            // 释放操作
            Thread release1 = new Thread(() =>
            {
                Thread.Sleep(150);
                SimulateReleaseOperation(1);
            });
            release1.Start();

            // 等待所有操作完成
            create1.Join();
            create2.Join();
            release1.Join();
        }

        static void TestScenario5()
        {
            LogMessage("\n=== 场景5: 长时间操作 ===", ConsoleColor.White);

            // 长时间创建操作
            Thread longCreate = new Thread(() => SimulateCreateOperation(1, 1000));
            longCreate.Start();

            // 释放操作（应在长时间创建完成后执行）
            Thread release = new Thread(() =>
            {
                Thread.Sleep(200);
                SimulateReleaseOperation(1);
            });
            release.Start();

            // 短时间创建操作（应在释放完成后执行）
            Thread shortCreate = new Thread(() =>
            {
                Thread.Sleep(300);
                SimulateCreateOperation(2, 100);
            });
            shortCreate.Start();

            // 等待所有操作完成
            longCreate.Join();
            release.Join();
            shortCreate.Join();
        }

        static void TestScenario6()
        {
            LogMessage("\n=== 场景6: 混合操作（创建/释放交错） ===", ConsoleColor.White);

            var threads = new List<Thread>();
            int opCount = 0;

            // 创建混合操作序列
            for (int i = 0; i < 10; i++)
            {
                bool isRelease = i % 3 == 0; // 每3个操作中有一个是释放操作
                int delay = _random.Next(50, 200);
                int duration = _random.Next(50, 300);
                int id = ++opCount;

                Thread t = new Thread(() =>
                {
                    Thread.Sleep(delay);
                    if (isRelease)
                    {
                        SimulateReleaseOperation(id);
                    }
                    else
                    {
                        SimulateCreateOperation(id, duration);
                    }
                });

                threads.Add(t);
                t.Start();
            }

            // 等待所有操作完成
            foreach (var t in threads)
            {
                t.Join();
            }
        }

        static void TestScenario7()
        {
            LogMessage("\n=== 场景7: 对象释放后操作 ===", ConsoleColor.White);

            // 创建本地锁实例
            var localLock = new ReleasePriorityLock();

            // 正常操作
            Thread create = new Thread(() => SimulateCreateOperation(1, 100, localLock));
            create.Start();
            create.Join();

            // 释放锁对象
            localLock.Dispose();
            LogMessage("锁对象已释放", ConsoleColor.Yellow);

            // 尝试在释放后操作
            Thread afterDispose = new Thread(() =>
            {
                try
                {
                    LogMessage("尝试在释放后创建操作", ConsoleColor.Gray);
                    using (localLock.Create())
                    {
                        LogMessage("此消息不应出现!", ConsoleColor.Red);
                    }
                }
                catch (ObjectDisposedException ex)
                {
                    LogMessage($"成功捕获对象释放异常: {ex.Message}", ConsoleColor.Green);
                }
                catch (Exception ex)
                {
                    LogMessage($"意外错误: {ex.Message}", ConsoleColor.Red);
                }
            });

            afterDispose.Start();
            afterDispose.Join();
        }



        static void SimulateCreateOperation(int id, int duration, ReleasePriorityLock? customLock = null)
        {
            var lockObj = customLock ?? _lock;

            try
            {
                LogMessage($"创建操作 {id} 请求进入", ConsoleColor.Gray);
                using (lockObj.Create())
                {
                    var startTime = DateTime.Now;
                    LogMessage($"创建操作 {id} 开始执行 (耗时: {duration}ms)", ConsoleColor.Green);

                    // 模拟资源创建
                    lock (_log) _resourceCount++;
                    int currentCount;
                    lock (_log) currentCount = _resourceCount;

                    // 模拟工作负载
                    Thread.Sleep(duration);

                    var endTime = DateTime.Now;
                    LogMessage($"创建操作 {id} 完成 (资源数: {currentCount})", ConsoleColor.DarkGreen);
                    LogTiming($"CREATE-{id}", startTime, endTime);
                }
            }
            catch (Exception ex)
            {
                LogMessage($"创建操作 {id} 出错: {ex.Message}", ConsoleColor.Red);
            }
        }
        static void SimulateCreateOperation(int id, int duration)
        {
            try
            {
                LogMessage($"创建操作 {id} 请求进入", ConsoleColor.Gray);
                using (_lock.Create())
                {
                    var startTime = DateTime.Now;
                    LogMessage($"创建操作 {id} 开始执行 (耗时: {duration}ms)", ConsoleColor.Green);

                    // 模拟资源创建
                    Interlocked.Increment(ref _resourceCount);
                    Thread.Sleep(duration); // 同步等待

                    var endTime = DateTime.Now;
                    LogMessage($"创建操作 {id} 完成 (资源数: {_resourceCount})", ConsoleColor.DarkGreen);
                    LogTiming($"CREATE-{id}", startTime, endTime);
                }
            }
            catch (Exception ex)
            {
                LogMessage($"创建操作 {id} 出错: {ex.Message}", ConsoleColor.Red);
            }
        }


        static void SimulateReleaseOperation(int releaseId)
        {
            try
            {
                LogMessage($"\n[释放操作 {releaseId} 请求进入] <<< 释放优先启动 >>>", ConsoleColor.Yellow);
                using (_lock.Release())
                {
                    var startTime = DateTime.Now;
                    LogMessage($"[释放操作 {releaseId} 开始执行] === 释放优先生效 ===", ConsoleColor.Magenta);

                    // 模拟资源释放
                    int currentCount = _resourceCount;
                    if (currentCount > 0)
                    {
                        _resourceCount = 0;
                    }
                    Thread.Sleep(200); // 释放操作耗时

                    var endTime = DateTime.Now;
                    LogMessage($"[释放操作 {releaseId} 完成] 释放了 {currentCount} 个资源", ConsoleColor.DarkMagenta);
                    LogTiming($"RELEASE-{releaseId}", startTime, endTime);
                }
            }
            catch (Exception ex)
            {
                LogMessage($"释放操作 {releaseId} 出错: {ex.Message}", ConsoleColor.Red);
            }
        }

        static void LogMessage(string message, ConsoleColor color)
        {
            lock (_log)
            {
                Console.ForegroundColor = color;
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
                Console.ResetColor();
                _log.Add($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
            }
        }

        static void LogTiming(string operation, DateTime start, DateTime end)
        {
            var duration = (end - start).TotalMilliseconds;
            var entry = $"{operation} 耗时: {duration:F0}ms (开始: {start:HH:mm:ss.fff}, 结束: {end:HH:mm:ss.fff})";
            lock (_log)
            {
                _log.Add(entry);
            }
        }

    }
}
