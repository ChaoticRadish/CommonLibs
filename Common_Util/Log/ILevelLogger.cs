﻿using System;
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
        #region 空输出器

        /// <summary>
        /// 取得一个啥都不干的日志输出器
        /// </summary>
        /// <returns></returns>
        public static ILevelLogger Empty()
        {
            return EmptyLevelLogger.Shared;
        }

        private readonly struct EmptyLevelLogger : ILevelLogger
        {
            public static EmptyLevelLogger Shared = new();

            public void Debug(string message)
            {
            }

            public void Error(string message, Exception? ex = null)
            {
            }

            public void Fatal(string message, Exception? ex = null)
            {
            }

            public void Info(string message)
            {
            }

            public void Warning(string message, Exception? ex = null, bool logTrack = false)
            {
            }
        }
        #endregion

        #region 带锁输出器

        /// <summary>
        /// 取得一个输出到 <paramref name="stringBuilder"/> 的日志输出器
        /// </summary>
        /// <remarks>
        /// 调用输出方法时, 使用 <paramref name="toStringFunc"/> 将输入内容转换为字符串后, 调用 <see cref="StringBuilder.AppendLine(string?)"/> 以记录日志内容 <br/>
        /// 这个实现下, 输出日志是同步的, 速度会慢点
        /// </remarks>
        /// <param name="stringBuilder"></param>
        /// <param name="toStringFunc"></param>
        /// <returns></returns>
        public static ILevelLogger WriteTo(StringBuilder stringBuilder, Func<LevelLoggerItem, string> toStringFunc)
        {
            return new LockLevelLogger(new((item) =>
            {
                stringBuilder.AppendLine(toStringFunc(item));
            }));
        }

        private readonly struct LockLevelLogger(Action<LevelLoggerItem> outputAction) : ILevelLogger
        {
            private readonly Action<LevelLoggerItem> outputAction = outputAction;
            private readonly object lockObject = new object();
            private void Handle(LevelLoggerItem item)
            {
                lock (lockObject)
                {
                    outputAction(item);
                }
            }
            private LevelLoggerItem CreateItem(string level, string msg, Exception? ex, bool track)
            {
                StackFrame[]? frames = null;
                if (track)
                {
                    StackTrace trace = new(2, true);
                    frames = trace.GetFrames();
                }
                return new(DateTime.Now, level, msg, ex, frames);
            }

            public void Debug(string message) => Handle(CreateItem(nameof(Debug), message, null, false));

            public void Error(string message, Exception? ex = null) => Handle(CreateItem(nameof(Error), message, ex, true));

            public void Fatal(string message, Exception? ex = null) => Handle(CreateItem(nameof(Fatal), message, ex, true));

            public void Info(string message) => Handle(CreateItem(nameof(Info), message, null, false));

            public void Warning(string message, Exception? ex = null, bool logTrack = false) => Handle(CreateItem(nameof(Warning), message, ex, logTrack));
        }
        #endregion

        /// <summary>
        /// <see cref="ILevelLogger"/> 的日志数据项
        /// </summary>
        /// <param name="Level"></param>
        /// <param name="Message"></param>
        /// <param name="Exception"></param>
        public record LevelLoggerItem(DateTime Time, string Level, string Message, Exception? Exception, StackFrame[]? Frames); 

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
        /// <param name="stackFrameDepth">如果需要输出堆栈信息, 要输出多少深度的堆栈信息, 如果是负数, 则不作限制</param>
        /// <param name="config">输出到 <see cref="ILogger"/> 时使用的配置, 分类与子分类均使用空字符串 </param>
        /// <returns></returns>
        public static ILevelLogger LogTo(ILogger logger, int stackFrameDepth, LogToLoggerConfig? config = null)
        {
            config ??= LogToLoggerConfig.Default;
            return new LogToLogger([logger], config.Value) 
            {
                FrameDepthLimit = stackFrameDepth,
            };
        }
        public static ILevelLogger LogTo(ILogger[] loggers, int stackFrameDepth, LogToLoggerConfig? config = null)
        {
            config ??= LogToLoggerConfig.Default;
            return new LogToLogger(loggers, config.Value)
            {
                FrameDepthLimit = stackFrameDepth,
            };
        }
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

            /// <summary>
            /// 堆栈信息深度的设置
            /// </summary>
            public int FrameDepthLimit { get; init; } = -1;

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
                    if (FrameDepthLimit >= 0)
                    {
                        frames = frames[..Math.Min(frames.Length, FrameDepthLimit)];
                    }
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
