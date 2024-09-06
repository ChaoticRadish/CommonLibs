using Common_Util.Attributes.General;
using Common_Util.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.Log
{
    internal class EnumLog001() : TestBase("测试EnumLevelLogger实现")
    {
        protected override void RunImpl()
        {
            test(LogEnum.AAA);
            WriteEmptyLine(3);

            test(LogEnum.BBB);
            WriteEmptyLine(3);

            test(LogEnum.CCC);
            WriteEmptyLine(3);

        }
        private void test(LogEnum e)
        {
            ILevelLogger logger = LevelLoggerHelper.EnumLog(e);

            logger.Info("测试 Info");
            logger.Debug("测试 Debug");
            logger.Warning("测试 Warning", null, true);

            try
            {
                throw new Exception("触发异常");
            }
            catch (Exception ex)
            {
                logger.Error("测试 Error", ex);
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
