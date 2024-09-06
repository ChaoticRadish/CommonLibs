using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Log
{
    /// <summary>
    /// 空的日志输出器, 什么都不做
    /// </summary>
    public class EmptyLogger : ILogger
    {
        public static EmptyLogger Instance { get; } = new EmptyLogger();

        public void Log(LogData log)
        {
            // 啥都不做
        }
    }
}
