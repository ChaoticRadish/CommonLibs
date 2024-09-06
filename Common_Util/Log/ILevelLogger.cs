using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Log
{
    /// <summary>
    /// 可按不同级别输出日志的输出器接口
    /// </summary>
    public interface ILevelLogger
    {
        void Info(string message);  
        void Debug(string message);
        void Warning(string message, Exception? ex = null, bool logTrack = false);
        void Error(string message, Exception? ex = null);
        void Fatal(string message, Exception? ex = null);
    }

    public interface ILevelConfig
    {
        public string DebugLevel { get; set; }

        public string ErrorLevel { get; set; }

        public string FatalLevel { get; set; }

        public string InfoLevel { get; set; }

        public string WarningLevel { get; set; }
    }

    /// <summary>
    /// 可按不同级别输出日志, 且可以被释放的输出器接口
    /// </summary>
    public interface IDisposableLevelLogger : IDisposable
    {

    }

    public static class LevelLoggerHelper
    {
        /// <summary>
        /// 创建使用指定枚举值的日志相关扩展方法输出日志的 <see cref="ILevelLogger"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enum"></param>
        /// <returns></returns>
        public static ILevelLogger EnumLog<T>(T @enum) where T : Enum
        {
            return new EnumLevelLogger<T>(@enum);
        }
        private class EnumLevelLogger<T>(T @enum) : ILevelLogger
             where T : Enum
        {
            public T EnumValue { get; private set; } = @enum;

            public void Debug(string message)
            {
                EnumLogExtensions.Debug(EnumValue, message);
            }

            public void Error(string message, Exception? ex)
            {
                EnumLogExtensions.Error(EnumValue, message, ex, 1);
            }

            public void Fatal(string message, Exception? ex)
            {
                EnumLogExtensions.Fatal(EnumValue, message, ex, 1);
            }

            public void Info(string message)
            {
                EnumLogExtensions.Info(EnumValue, message);
            }

            public void Warning(string message, Exception? ex, bool logTrack)
            {
                EnumLogExtensions.Warning(EnumValue, message, ex, logTrack, 1);
            }
        }

        /// <summary>
        /// 创建输出日志到指定 <see cref="ILogger"/> 的 <see cref="ILevelLogger"/>
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="config">输出到 <see cref="ILogger"/> 时使用的配置, 分类与子分类均使用空字符串 </param>
        /// <returns></returns>
        public static ILevelLogger LogTo(ILogger logger, LogToLoggerConfig? config = null)
        {
            config ??= LogToLoggerConfig.Default;
            return new LogToLogger([logger], config.Value);
        }
        public static ILevelLogger LogTo(ILogger[] loggers, LogToLoggerConfig? config = null)
        {
            config ??= LogToLoggerConfig.Default;
            return new LogToLogger(loggers, config.Value);
        }
        /// <summary>
        /// 创建输出日志到指定 <see cref="ILogger"/> 的 <see cref="ILevelLogger"/>
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="category">输出到 <see cref="ILogger"/> 时使用的分类 </param>
        /// <param name="subcategory">输出到 <see cref="ILogger"/> 时使用的子分类 </param>
        /// <returns></returns>
        public static ILevelLogger LogTo(ILogger logger, string category, string subcategory)
        {
            return new LogToLogger([logger], LogToLoggerConfig.GetDefault(category, subcategory));
        }
        public static ILevelLogger LogTo(ILogger[] loggers, string category, string subcategory)
        {
            return new LogToLogger(loggers, LogToLoggerConfig.GetDefault(category, subcategory));
        }
        public struct LogToLoggerConfig : ILevelConfig
        {
            public string Category;

            public string SubCategory;

            public string DebugLevel { get; set; }

            public string ErrorLevel { get; set; }

            public string FatalLevel { get; set; }

            public string InfoLevel { get; set; }

            public string WarningLevel { get; set; }

            public static LogToLoggerConfig Default => GetDefault(string.Empty, string.Empty);
            public static LogToLoggerConfig GetDefault(string category, string subCategory = "")
            {
                return new LogToLoggerConfig
                {
                    DebugLevel = "Debug",
                    ErrorLevel = "Error",
                    FatalLevel = "Fatal",
                    InfoLevel = "Info",
                    WarningLevel = "Warning",
                    Category = category,
                    SubCategory = subCategory,
                };
            }
        }
        private class LogToLogger(ILogger[] targets, LogToLoggerConfig config) : ILevelLogger
        {
            private readonly ILogger[] targets = targets;
            private readonly LogToLoggerConfig config = config;

            public void Debug(string message)
            {
                var logData = createLogData(message, config.DebugLevel, null, false);
                foreach (var logger in targets)
                {
                    logger.Log(logData);
                }
            }

            public void Error(string message, Exception? ex)
            {
                var logData = createLogData(message, config.ErrorLevel, ex, true);
                foreach (var logger in targets)
                {
                    logger.Log(logData);
                }
            }

            public void Fatal(string message, Exception? ex)
            {
                var logData = createLogData(message, config.FatalLevel, ex, true);
                foreach (var logger in targets)
                {
                    logger.Log(logData);
                }
            }

            public void Info(string message)
            {
                var logData = createLogData(message, config.InfoLevel, null, false);
                foreach (var logger in targets)
                {
                    logger.Log(logData);
                }
            }

            public void Warning(string message, Exception? ex, bool logTrack = false)
            {
                var logData = createLogData(message, config.WarningLevel, ex, logTrack);
                foreach (var logger in targets)
                {
                    logger.Log(logData);
                }
            }

            private LogData createLogData(string message, string level, Exception? ex, bool logTrack)
            {
                StackFrame[]? frames = null;
                if (logTrack)
                {
                    StackTrace trace = new(2, true);
                    frames = trace.GetFrames();
                }

                return new LogData()
                {
                    Category = config.Category, 
                    SubCategory = config.SubCategory,
                    Level = level,
                    Message = message,
                    StackFrames = frames,
                    Time = DateTime.Now,
                    Exception = ex,
                };
            }
        }
    }
}
