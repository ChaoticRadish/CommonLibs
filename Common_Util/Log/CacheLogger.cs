using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Log
{
    public class CacheLogger : ILogger
    {

        /// <summary>
        /// 日志缓存容量, 0时将不会缓存日志, 负数时表示缓存数量不作限制
        /// </summary>
        public required int CacheCapacity 
        {
            get => cacheCapacity;
            init
            {
                cacheCapacity = value;

                if (cacheCapacity > 0)
                {
                    _cache = new(cacheCapacity);
                }
                else
                {
                    _cache = [];
                }
            }
        }
        private int cacheCapacity;

        private readonly object locker = new(); 

        public void Log(LogData log)
        {
            if (CacheCapacity == 0) return;

            lock (locker)
            {
                if (CacheCapacity > 0)
                {
                    while (Caches.Count >= CacheCapacity)
                    {
                        Caches.Dequeue();
                    }
                }
                Caches.Enqueue(log);
            }

            OnCache?.Invoke(log);
        }

        private Queue<LogData>? _cache;
        private Queue<LogData> Caches
        {
            get
            {
                _cache ??= new Queue<LogData>();
                return _cache;
            }
        }

        /// <summary>
        /// 将缓存的日志输出到指定的日志输出器
        /// </summary>
        /// <param name="other"></param>
        public void OutputLog(ILogger other)
        {
            lock (locker)
            {
                while (Caches.TryDequeue(out LogData? data) && data != null)
                {
                    other.Log(data);
                }
            }
        }

        #region 事件
        /// <summary>
        /// 得到新的数据并缓存起来的事件
        /// </summary>
        public event Action<LogData>? OnCache;
        #endregion
    }
}
