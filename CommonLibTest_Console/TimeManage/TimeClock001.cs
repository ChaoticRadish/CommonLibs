using Common_Util.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.TimeManage
{
    class TimeClock001() : TestBase("对 Step 方法的测试")
    {
        protected override void RunImpl()
        {
            TimeClock clock = new TimeClock(TimeClock.TimerMode.Normal);
            clock.Start();
            int index = 0;
            while (true)
            {
                if (clock.Step(0.25, true))
                {
                    WriteTimeLine($"测试 {++index}");
                }
                Thread.Sleep(10);
                if (clock.ElapseTime > 10) return;
            }
        }
    }
}
