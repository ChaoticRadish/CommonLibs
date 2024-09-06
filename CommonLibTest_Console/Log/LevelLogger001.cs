using Common_Util.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.Log
{
    internal class LevelLogger001() : TestBase("测试 LevelLoggerCreator")
    {
        protected override void RunImpl()
        {
            LevelLoggerCreator creator = new() { CacheCapacity = 10 };

            {
                var logger = creator.Get("测试", "AAA");
                randomLogSth(logger, 8);
            }
            {
                var logger = creator.Get("TEST", "B");
                randomLogSth(logger, 8);
            }

            creator.TargetLogger = this;

            {
                var logger = creator.Get("测试", "CCC");
                randomLogSth(logger, 8);
            }

            creator.TargetLogger = null;

            {
                var logger = creator.Get("测试", "DDD");
                randomLogSth(logger, 8);
            }
            {
                var logger = creator.Get("测试", "EEE");
                randomLogSth(logger, 8);
            }

            creator.TargetLogger = this;

            creator.TargetLogger = null;

            {
                var logger = creator.Get("测试", "FFF");
                randomLogSth(logger, 8);
            }

            creator.TargetLogger = this;
        }

        private int index = 0;
        private void randomLogSth(ILevelLogger logger, int count)
        {
            Random random = new Random();

            for (int i = 0; i < count; i++)
            {
                logger.Info($"{index++} {Common_Util.Random.RandomStringHelper.GetRandomEnglishString(8, random)}");
            }
        }
    }
}
