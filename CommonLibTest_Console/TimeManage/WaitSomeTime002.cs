using Common_Util.Extensions;
using Common_Util.Module.Scheduling;
using Common_Util.Time;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.TimeManage
{
    internal class WaitSomeTime002() : TestBase("测试 WaitHelper.BusyWait()")
    {
        protected override void RunImpl()
        {
        }
        protected override Task RunImplAsync()
        {
            foreach (var i in 50.ForUntil())
            {
                test(i);
            }
            return Task.CompletedTask;
        }

        private void test(int waitTime)
        {
            int testCount = 1000;
            double[] testResult = new double[testCount];

            TimeClock timeClock = new TimeClock();
            timeClock.Start();
            WriteLine($"测试等待时长: {waitTime} ms");
            foreach (var i in testCount.ForUntil())
            {
                timeClock.UpdateMilliSecond();
                WaitHelper.BusyWait(waitTime);
                var useTime = timeClock.UpdateMilliSecond();
                testResult[i] = useTime;
            }
            WriteLine($"均值: " + testResult.Average());
            WriteLine($"最大值: " + testResult.Max());
            WriteLine($"最小值: " + testResult.Min());

            WriteEmptyLine();
        }
    }
}
