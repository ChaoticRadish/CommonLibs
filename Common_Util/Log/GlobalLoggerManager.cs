using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Log
{
    /// <summary>
    /// 全局日志帮助类
    /// </summary>
    public static class GlobalLoggerManager
    {
        static GlobalLoggerManager()
        {
            _defaultLogger = new DefaultConsoleLogger();
        }
        private static ILogger _defaultLogger;

        /// <summary>
        /// 缓存输入的日志, 直到设置新的输出器
        /// </summary>
        public static void CacheLogUntilSetNewLogger()
        {
            CurrentLogger = new CacheToNextLogger() { CacheCapacity = -1 };
        }

        /// <summary>
        /// 当前的日志输出器
        /// </summary>
        public static ILogger CurrentLogger 
        {
            get => _logger ?? _defaultLogger;
            set
            {
                lock (_log_lock)
                {
                    AfterSetLogger?.Invoke(value);
                    _logger = value;
                    OnSetLogger?.Invoke(_logger);
                }
            }
        }
        private static ILogger? _logger;

        /// <summary>
        /// 清理掉当前的日志输出器 (将会使用默认的输出器, 而不是不输出)
        /// </summary>
        public static void ClearCurrentLogger()
        {
            lock (_log_lock)
            {
                _logger = null;
                OnClearLogger?.Invoke(_defaultLogger);
            }
        }
        /// <summary>
        /// 将当前日志输出器替换为空输出器
        /// </summary>
        public static void SetEmptyLogger()
        {
            CurrentLogger = EmptyLogger.Instance;
        }

        /// <summary>
        /// 当前是否在使用默认的日志输出器
        /// </summary>
        public static bool UsingDefaultLogger { get => _logger == null; }

        #region 事件 

        #region 日志输出器设置相关事件, 这些事件互斥, 不会同时执行

        /// <summary>
        /// 日志输出器即将被设置上之前的事件 (同步)
        /// <para>注: 事件方法中不能够同步得调用 <see cref="Log"/>, 以避免死锁</para>
        /// </summary>
        public static event Action<ILogger>? AfterSetLogger;
        /// <summary>
        /// 日志输出器已经被设置上的事件 (同步)
        /// <para>注: 事件方法中不能够同步得调用 <see cref="Log"/>, 以避免死锁</para>
        /// </summary>
        public static event Action<ILogger>? OnSetLogger;
        /// <summary>
        /// 日志输出器已被清除的事件 (同步), 传入的参数为当前的默认输出器
        /// <para>注: 事件方法中不能够同步得调用 <see cref="Log"/>, 以避免死锁</para>
        /// </summary>
        public static event Action<ILogger>? OnClearLogger;

        #endregion

        #endregion

        #region 写日志
        /// <summary>
        /// 锁定输出日志与设定当前输出器的代码块
        /// <para>用以避免出现</para>
        /// </summary>
        private static object _log_lock = new object();

        /// <summary>
        /// 输出一条日志
        /// </summary>
        /// <param name="log"></param>
        public static void Log(LogData log)
        {
            lock (_log_lock)
            {
                CurrentLogger.Log(log);
            }
        }
        /// <summary>
        /// 输出一条日志
        /// </summary>
        /// <param name="level"></param>
        /// <param name="category"></param>
        /// <param name="message"></param>
        /// <param name="logTrack"></param>
        /// <param name="ex"></param>
        /// <param name="depth"></param>
        public static void Log(string level, string category, string subCategory, string message, Exception? ex = null, bool logTrack = false, int depth = 0) 
        {
            if (logTrack)
            {
                Log(LogData.Create(level, category, subCategory, message, ex, depth + 1));
            }
            else
            {
                Log(LogData.CreateNonTrace(level, category, subCategory, message, ex));
            }
        }

        #endregion
    }
}
