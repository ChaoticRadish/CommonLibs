using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Log
{
    /// <summary>
    /// <see cref="ILevelLogger"/> 创建器
    /// </summary>
    public class LevelLoggerCreator
    {
        public LevelLoggerCreator() 
        {
            innerLogger = new(this);
        }

        /// <summary>
        /// 日志缓存容量, 0时将不会缓存日志, 负数时表示缓存数量不作限制
        /// </summary>
        public required int CacheCapacity
        {
            get => cacheCapacity;
            init
            {
                cacheCapacity = value;

                if (cacheCapacity != 0)
                {
                    cacheLogger = new()
                    {
                        CacheCapacity = cacheCapacity,
                    };
                }
            }
        }
        private int cacheCapacity;

        private ILogger? targetLogger { get; set; }
        /// <summary>
        /// 产生的日志需要输出到这个输出器
        /// <para>如果为 null, 可按照配置缓存待输出日志</para>
        /// </summary>
        public ILogger? TargetLogger
        {
            get => targetLogger;
            set
            {
                lock (locker)
                {
                    targetLogger = value;
                    if (targetLogger != null && cacheLogger != null)
                    {
                        cacheLogger.OutputLog(targetLogger);
                    }
                }

            }
        }

        private CacheLogger? cacheLogger;

        private InnerLogger innerLogger;

        private readonly object locker = new();


        /// <summary>
        /// 取得日志输出器
        /// </summary>
        /// <param name="category"></param>
        /// <param name="subCategory"></param>
        /// <returns></returns>
        public ILevelLogger Get(string category, string subCategory = "")
        {
            return LevelLoggerHelper.LogTo(innerLogger, new LevelLoggerHelper.LogToLoggerConfig()
            {
                DebugLevel = LevelConfig.DebugLevel,
                ErrorLevel = LevelConfig.ErrorLevel,
                FatalLevel = LevelConfig.FatalLevel,
                InfoLevel = LevelConfig.InfoLevel,
                WarningLevel = LevelConfig.WarningLevel,    
                Category = category,
                SubCategory = subCategory,
            });
        }

        #region 配置

        public struct LevelConfigStruct : ILevelConfig
        {
            public string DebugLevel { get; set; }

            public string ErrorLevel { get; set; }

            public string FatalLevel { get; set; }

            public string InfoLevel { get; set; }

            public string WarningLevel { get; set; }

            public static LevelConfigStruct Default { get; private set; } = new LevelConfigStruct()
            {
                DebugLevel = "Debug",
                ErrorLevel = "Error",
                FatalLevel = "Fatal",
                InfoLevel = "Info",
                WarningLevel = "Warning",
            };
        }

        /// <summary>
        /// 日志级别相关配置
        /// </summary>
        public LevelConfigStruct LevelConfig { get; set; } = LevelConfigStruct.Default;
        #endregion

        private struct InnerLogger(LevelLoggerCreator father) : ILogger
        {
            private readonly LevelLoggerCreator father = father;

            public void Log(LogData log)
            {
                lock (father.locker)
                {
                    (father.TargetLogger ?? father.cacheLogger)?.Log(log);
                }
            }
        }
    }
}
