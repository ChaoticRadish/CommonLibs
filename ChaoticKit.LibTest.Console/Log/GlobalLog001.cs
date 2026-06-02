using ChaoticKit.Attributes.General;
using ChaoticKit.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaoticKit.LibTest.Console.Log
{
    internal class GlobalLog001() : TestBase("全局日志测试001")
    {
        protected override void RunImpl()
        {
        }
        protected override async Task RunImplAsync()
        {
            GlobalLoggerManager.CacheLogUntilSetNewLogger();

            WriteLine(" >>> mark 1 <<< ");

            await randomLogSth("设置输出器前");

            WriteLine(" >>> mark 2 <<< ");

            await Task.Delay(6000);

            WriteLine(" >>> mark 3 <<< ");

            GlobalLoggerManager.CurrentLogger = this;

            WriteLine(" >>> mark 4 <<< ");

            await randomLogSth("设置输出器后");

        }

        private int index = 0;
        private async Task randomLogSth(string head)
        {
            System.Random random = new System.Random();

            for (int i = 0; i < 25; i++)
            {
                LogEnum.AAA.Info($"{index++} {head} {ChaoticKit.Random.RandomStringHelper.GetRandomEnglishString(8, random)}");
                LogEnum.BBB.Info($"{index++} {head} {ChaoticKit.Random.RandomStringHelper.GetRandomEnglishString(8, random)}");
                LogEnum.CCC.Info($"{index++} {head} {ChaoticKit.Random.RandomStringHelper.GetRandomEnglishString(8, random)}");
                await Task.Delay(500);
            }
        }

        public enum LogEnum
        {
            [Logger("AAA")]
            AAA,
            [Logger("BBB")]
            BBB,
            [Logger("CCC")]
            CCC,
        }
    }
}
