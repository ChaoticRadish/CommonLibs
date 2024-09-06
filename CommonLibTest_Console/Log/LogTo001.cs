using Common_Util.Extensions;
using Common_Util.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.Log
{
    internal class LogTo001() : TestBase("测试 LevelLoggerHelper.LogTo ")
    {
        protected override void RunImpl()
        {
            test(LevelLoggerHelper.LogTo(new TestLogger(this)));
            test(LevelLoggerHelper.LogTo(new TestLogger(this), "测试2", "子分类1"));
        }
        private void test(ILevelLogger levelLogger)
        {
            levelLogger.Info("测试");
            try
            {
                levelLogger.Debug("测试 Debug");
                throw new Exception("123123123");
            }
            catch (Exception ex)
            {
                levelLogger.Error("发生异常", ex);
            }
        }

        private class TestLogger(TestBase testBase) : ILogger
        {
            private readonly TestBase testBase = testBase;

            public void Log(LogData log)
            {
                StringBuilder sb = new StringBuilder("TestLogger:::");

                if (!string.IsNullOrEmpty(log.Level))
                {
                    sb.Append($"<{log.Level}>");
                }
                if (!string.IsNullOrEmpty(log.Category))
                {
                    sb.Append($"[{log.Category}]");
                }
                if (!string.IsNullOrEmpty(log.SubCategory))
                {
                    sb.Append($"({log.SubCategory})");
                }
                sb.AppendLine($" {log.Time:yyyy-MM-dd HH:mm:ss:fff}:");
                sb.AppendLine(log.Message);
                if (log.Exception != null)
                {
                    sb.AppendLine($"异常: " + log.Exception.ToString());
                }
                if (log.StackFrames != null && log.StackFrames.Length > 0)
                {
                    sb.AppendLine("堆栈追踪: ");
                    for (int index = 0; index < log.StackFrames.Length; index++)
                    {
                        var frame = log.StackFrames[index];
                        sb.AppendLine($"{(index + ".").PadRight(5, ' ')}{frame.GetMethod()?.DeclaringType} :: {(frame.GetMethod())} - {frame.GetFileName()}:{frame.GetFileLineNumber()},{frame.GetFileColumnNumber()}");
                    }
                }
                if (sb[sb.Length - 1] == '\n')
                {
                    sb.Remove(sb.Length - 1, 1);
                }
                sb.Replace("\n", "\n◇ ");
                sb.AppendLine();


                testBase.WriteLine(sb.ToString());
            }
        }
    }
}
