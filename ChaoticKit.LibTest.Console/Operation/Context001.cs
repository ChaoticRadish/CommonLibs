using ChaoticKit.Data.Struct;
using ChaoticKit.Data.Struct.Operation.Impls;
using ChaoticKit.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaoticKit.LibTest.Console.Operation
{
    internal class Context001() : TestBase("测试操作上下文默认实现: 仅运行一次的异步操作上下文")
    {
        protected override void RunImpl()
        {
        }
        protected override async Task RunImplAsync()
        {
            /* AI 生成测试内容后做了一些微调 */

            // 假设这是一个由外部控制器（如 ViewModel）控制的 Token
            // 5 秒钟后取消
            var ctsOperation = new CancellationTokenSource();
            _ = Task.Run(async () =>
            {
                await Task.Delay(5000 + 500); // 错开 0.5 秒, 方便观察
                WriteLine($"ctsOperation.Cancel()");
                ctsOperation.Cancel();
            });

            // 创建 Context，每秒输出一行文本
            var context = SingleRunOperationContext.Create<OperationResult>(async cancellationToken =>
            {
                foreach (int i in 100.ForUntil())
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return "取消执行";
                    }
                    WriteLine($"context: {i}");
                    await Task.Delay(1000);
                }
                return true;
            }, ctsOperation.Token);

            // --- 线程 1：UI 界面 ---
            // 用户只愿意等 2 秒
            try
            {
                WriteLine("[模拟 UI ] context.ExecuteAsync");
                await context.ExecuteAsync(new CancellationTokenSource(2000).Token);
                WriteLine("[模拟 UI ] " + context.Result.FullInfoString());   // 预期是未执行完成
            }
            catch (OperationCanceledException)
            {
                WriteLine("[模拟 UI ] OperationCanceledException");
                WriteLine("[模拟 UI ] " + context.Result.FullInfoString());
            }

            // --- 线程 2：后台服务 ---
            // 愿意一直等，直到任务真正结束
            try
            {
                WriteLine("[模拟后台] context.ExecuteAsync");
                await context.ExecuteAsync();
                WriteLine("[模拟后台] " + context.Result.FullInfoString()); // 预期是成功
            }
            catch (OperationCanceledException)
            {
                // 如果捕获到这里，说明 waitToken 触发了，但在这个场景下我们传的是 None，
                // 所以只会因为任务本身被 ctsOperation 取消而结束。
                // 这里不会抛异常，因为我们吞掉了 operation 类型的取消。
                WriteLine("[模拟后台] OperationCanceledException");
                WriteLine("[模拟后台] " + context.Result.FullInfoString());
            }
        }
    }
}
