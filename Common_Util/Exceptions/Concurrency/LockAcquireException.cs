using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common_Util.Exceptions.Concurrency
{
    /// <summary>
    /// 获取锁相关的异常
    /// </summary>
    public class LockAcquireException : Exception
    {
        public LockAcquireException(object? key) : base() 
        {
            Key = key;
        }
        public LockAcquireException(object? key, string msg) : base(msg)
        {
            Key = key;
        }

        /// <summary>
        /// 锁的键值, 也可以是对锁的目标资源的描述信息
        /// </summary>
        public object? Key { get; }
    }
    /// <summary>
    /// 获取锁超时
    /// </summary>
    public class LockAcquireTimeoutException : LockAcquireException
    {
        public LockAcquireTimeoutException(object? key) : base($"获取锁 '{key}' 的操作超时。")
        {
        }
        public LockAcquireTimeoutException(object? key, TimeSpan timeSpan) : base($"获取锁 '{key}' 的操作在 {timeSpan.TotalMilliseconds} 毫秒后超时。")
        {
            Timeout = timeSpan;
        }

        public LockAcquireTimeoutException(object? key, string msg) : base(key, msg)
        {
        }
        public LockAcquireTimeoutException(object? key, TimeSpan timeSpan, string msg) : base(key, msg)
        {
            Timeout = timeSpan;
        }

        /// <summary>
        /// 等待超时的时长
        /// </summary>
        public TimeSpan? Timeout { get; }
    }
    /// <summary>
    /// 获取锁的操作被取消
    /// </summary>
    public class LockAcquireCanceledException : LockAcquireException
    {
        public LockAcquireCanceledException(object? key) : base(key)
        {
        }

        public LockAcquireCanceledException(object? key, string msg) : base(key, msg)
        {
        }
    }
}
