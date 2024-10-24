using Common_Util.Time;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.TimeManage
{
    internal class WaitSomeTime001() : TestBase("测试等待一些时间")
    {
        protected override void RunImpl()
        {
            RunTestMark();
        }

        [TestMethod]
        void UseThreadSleep0()
        {
            runTest(() => Thread.Sleep(0));

        }
        [TestMethod]
        void UseThreadSleep1()
        {
            runTest(() => Thread.Sleep(1));
        }

        [TestMethod]
        void UseSpinWait01()
        {
            SpinWait spinWait = new SpinWait();
            runTest(spinWait.SpinOnce);
        }
        [TestMethod]
        void UseSpinWait02()
        {
            runTest(() => SpinWait.SpinUntil(() => false, 1));
        }
        [TestMethod]
        void UseStopwatch()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            runTest(() => 
            {
                var now = stopwatch.ElapsedTicks;
                while (stopwatch.ElapsedTicks - now < 10000) { }
            });
        }

        void runTest(Action wait)
        {
            TimeClock clock = new();
            clock.Start();
            for (int i = 0; i < 25; i++)
            {
                clock.UpdateMilliSecond();
                wait();
                double sleep = clock.UpdateMilliSecond();
                WriteLine(sleep.ToString());
            }

        }
    }
}
