using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Log
{
    /// <summary>
    /// 默认的命令行日志输出器
    /// </summary>
    internal class DefaultConsoleLogger : QueueLogger
    {

        #region 输出
        protected override void Output(LogData log)
        {
            StringBuilder sb = new StringBuilder();

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
            Console.WriteLine(sb.ToString());
        }
        #endregion

    }
}
