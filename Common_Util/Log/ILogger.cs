using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Log
{
    /// <summary>
    /// 日志输出器接口
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// 记录下日志数据
        /// </summary>
        /// <param name="log"></param>
        void Log(LogData log);
    }
    public class LogData
    {
        /// <summary>
        /// 日志记录的时间点
        /// </summary>
        public DateTime Time { get; set; }
        /// <summary>
        /// 日志的级别, 如Info, Debug, Error或其他自定义级别
        /// </summary>
        public string Level { get; set; } = string.Empty;
        /// <summary>
        /// 日志的分类, 一般用于指明日志的所属模块
        /// </summary>
        public string Category { get; set; } = string.Empty;
        /// <summary>
        /// 日志的子分类, 一般用于指明日志所在模块的具体哪个功能组/子模块/事务等
        /// </summary>
        public string SubCategory { get; set; } = string.Empty;
        /// <summary>
        /// 日志的正文
        /// </summary>
        public string Message { get; set; } = string.Empty;
        /// <summary>
        /// 如果日志记录了一个异常, 将会被放在这里
        /// </summary>
        public Exception? Exception { get; set; }
        /// <summary>
        /// 日志发生位置的堆栈追踪帧数组, 其顺序为从当前位置往上层/外层排列
        /// </summary>
        public StackFrame[]? StackFrames { get; set; }


        /// <summary>
        /// 创建一个不包含堆栈追踪信息的日志数据, 时间为调用时间. 
        /// </summary>
        /// <param name="level"></param>
        /// <param name="category"></param>
        /// <param name="subCategory"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static LogData CreateNonTrace(string level, string category, string subCategory, string message, Exception? exception = null)
        {
            return new LogData()
            {
                Time = DateTime.Now,
                Level = level,
                Category = category,
                SubCategory = subCategory,
                Message = message,
                Exception = exception,
                StackFrames = null,
            };
        }

        /// <summary>
        /// 创建一个包含堆栈追踪信息的日志数据, 时间为调用时间. 
        /// </summary>
        /// <param name="level"></param>
        /// <param name="category"></param>
        /// <param name="subCategory"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        /// <param name="depth">与此方法的在堆栈追踪上的距离</param>
        /// <returns></returns>
        public static LogData Create(string level, string category, string subCategory, string message, Exception? exception = null, int depth = 0)
        {
            int _d = 1 + depth;
            if (_d < 0)
            {
                _d = 0;
            }
            StackFrame[] stackFrames = new StackTrace(true).GetFrames();
            return new LogData()
            {
                Time = DateTime.Now,
                Level = level,
                Category = category,
                SubCategory = subCategory,
                Message = message,
                Exception = exception,
                StackFrames = stackFrames.Length > _d ? stackFrames[_d..] : null,
            };
        }



        /// <summary>
        /// 创建一个包含堆栈追踪信息的日志数据, 时间为调用时间. 
        /// </summary>
        /// <param name="level"></param>
        /// <param name="category"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        /// <param name="depth">与此方法的在堆栈追踪上的距离</param>
        /// <returns></returns>
        public static LogData Create(string level, string category, string message, Exception? exception = null, int depth = 0)
        {
            return Create(level, category, string.Empty, message, exception, depth + 1);
        }


    }

}
