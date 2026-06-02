using ChaoticKit.Module.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaoticKit.LibTest.Console.Scheduling
{
    internal class Coordinator001() : TestBase("测试异步操作协调器")
    {
        protected override void RunImpl()
        {
        }
        protected override async Task RunImplAsync()
        {
            AsyncOperationCoordinator<string, string> coordinator = new(body);
            var t1 = coordinator.ExecuteAsync("a");
            await Task.Delay(1000);
            var t2 = coordinator.ExecuteAsync("a");

            await Task.WhenAll(t1, t2);

            WritePair(t1.Status);
            WritePair(t2.Status);
            WritePair(t2.Result);

            var t3 = coordinator.ExecuteAsync("t3");
            await Task.Delay(1000);
            var t4 = coordinator.ExecuteAsync("t4");

            await Task.WhenAll(t3, t4);
            WritePair(t3.Status);
            WritePair(t4.Status);
            WritePair(t4.Result);

        }


        private async Task<string> body(string arg, CancellationToken cancellationToken)
        {
            WriteTimeLine("开始执行: " + arg);

            await Task.Delay(3000);

            WriteTimeLine("执行完成: " + arg);

            return $"结果<{arg}>";
        }

    }
}
