using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Log
{
    public class LogStringBuilder
    {
        public LogStringBuilderConfig Config { get; set; } = new();

        public string Build(LogData log)
        {

            StringBuilder sb = new StringBuilder();

            if (Config.OutLevel && !string.IsNullOrEmpty(log.Level))
            {
                sb.Append($"<{log.Level}>");
            }
            if (Config.OutCategory && !string.IsNullOrEmpty(log.Category))
            {
                sb.Append($"[{log.Category}]");
            }
            if (Config.OutSubCategory && !string.IsNullOrEmpty(log.SubCategory))
            {
                sb.Append($"({log.SubCategory})");
            }
            if (Config.OutTime)
            {
                sb.Append(' ').Append(log.Time.ToString(Config.TimeFormat)).Append(' ');
            }
            if (Config.OutAnyHead && Config.HeadSameLine)
            {
                sb.Append(':');
            }
            if (Config.HeadSameLine)
            {
                if (Config.OutAnyHead)
                {
                    sb.Append(' ');
                }
            }
            else
            {
                sb.AppendLine();
            }
            sb.Append(log.Message);
            if (Config.OutException && log.Exception != null)
            {
                sb.AppendLine($"异常: " + log.Exception.ToString());
            }
            if (Config.OutStackTrack && log.StackFrames != null && log.StackFrames.Length > 0)
            {
                sb.AppendLine("堆栈追踪: ");
                for (int index = 0; index < log.StackFrames.Length; index++)
                {
                    var frame = log.StackFrames[index];
                    sb.AppendLine($"{(index + ".").PadRight(5, ' ')}{frame.GetMethod()?.DeclaringType} :: {(frame.GetMethod())} - {frame.GetFileName()}:{frame.GetFileLineNumber()},{frame.GetFileColumnNumber()}");
                }
            }
            bool endWithNewLine;    // 移除末尾的换行符
            do
            {
                endWithNewLine = false;
                if (sb.Length >= 2 && sb[^2] == '\r' && sb[^1] == '\n')
                {
                    endWithNewLine = true;
                    sb.Remove(sb.Length - 2, 2);
                }
                else if (sb.Length >= 1 && sb[^1] == '\n')
                {
                    endWithNewLine = true;
                    sb.Remove(sb.Length - 1, 1);
                }

            } while (endWithNewLine);
            sb.Replace("\n", $"\n{Config.RowHeadString}");

            return sb.ToString();
        }
    }

    public class LogStringBuilderConfig
    {
        public string TimeFormat { get; set; } = "yyyy-MM-dd HH:mm:ss:fff";

        public bool OutLevel { get; set; } = true;

        public bool OutCategory { get; set; } = true;   

        public bool OutSubCategory { get; set; } = true;

        public bool OutTime {  get; set; } = true;


        public bool OutAnyHead => OutLevel || OutCategory || OutSubCategory || OutTime;


        public bool HeadSameLine { get; set; } = false;

        public bool OutException { get; set; } = true;

        public bool OutStackTrack { get; set; } = true;

        /// <summary>
        /// 除首行外其他行的行首填充的字符串
        /// </summary>
        public string RowHeadString { get; set; } = "◇ ";

    }
}
