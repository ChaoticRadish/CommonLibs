using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Module.Concurrency
{
    /// <summary>
    /// 异步计数器
    /// </summary>
    /// <param name="initCount">初始计数</param>
    public class AsyncCounter
    {
        private long _count;
        private readonly long min;
        private readonly long max;

        /// <summary>
        /// 用于计数过程的锁
        /// </summary>
        private readonly object _lock = new();

        private TaskCompletionSource _tcsNonpositive;

        public AsyncCounter(long initCount = 0, long min = long.MinValue, long max = long.MaxValue)
        {
            _count = _clamp(initCount);
            this.min = min;
            this.max = max;
            _tcsNonpositive = new TaskCompletionSource();
            if (_count <= 0)
            {
                _tcsNonpositive.TrySetResult();
            }
            
        }

        #region 计数

        /// <summary>
        /// 自增计数
        /// </summary>
        /// <param name="changed">变化量</param>
        public void Increment(long changed = 1)
        {
            lock (_lock)
            {
                _set(_count + changed);
            }
        }
        /// <summary>
        /// 自减计数
        /// </summary>
        /// <param name="changed">变化量</param>
        public void Decrement(long changed = 1)
        {
            lock (_lock)
            {
                _set(_count - changed);
            }
        }
        /// <summary>
        /// 设置当前计数
        /// </summary>
        public void Set(long count)
        {
            lock (_lock)
            {
                _set(count);
            }
        }

        /// <summary>
        /// 将数值钳制到当前允许的访问内
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private long _clamp(long value)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
        private void _set(long count)
        {
            count = _clamp(count);

            long origin = _count;
            if (origin == count) return;
            _count = count;

            bool changeToNonpositive = origin > 0 && count <= 0;
            bool changeToPositive = origin <= 0 && count > 0;

            if (changeToNonpositive)
            {
                _tcsNonpositive.TrySetResult();
            }
            if (changeToPositive)
            {
                _tcsNonpositive = new TaskCompletionSource();
            }
        }
        #endregion


        /// <summary>
        /// 等待当前计数小于或等于 0 
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool WaitNonpositive(int timeout, CancellationToken token)
            => WaitNonpositive(timeout < 0 ? TimeSpan.FromMilliseconds(-1) : TimeSpan.FromMilliseconds(timeout), token);
        public bool WaitNonpositive(TimeSpan timeout, CancellationToken token)
            => WaitNonpositiveAsync(timeout, token).GetAwaiter().GetResult();
        /// <summary>
        /// 异步等待当前计数小于或等于 0 
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<bool> WaitNonpositiveAsync(int timeout, CancellationToken token)
            => WaitNonpositiveAsync(timeout < 0 ? TimeSpan.FromMilliseconds(-1) : TimeSpan.FromMilliseconds(timeout), token);
        public async Task<bool> WaitNonpositiveAsync(TimeSpan timeout, CancellationToken token)
        {
            Task completionTask;
            lock (_lock)
            {
                token.ThrowIfCancellationRequested();
                completionTask = _tcsNonpositive.Task;
            }
            try
            {
                await completionTask.WaitAsync(timeout, token).ConfigureAwait(false);
                return true;
            }
            catch (TimeoutException)
            {
                return false;
            }

        }



    }
}
