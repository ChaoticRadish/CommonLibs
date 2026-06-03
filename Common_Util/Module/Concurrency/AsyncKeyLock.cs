using Common_Util.Exceptions.Concurrency;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Module.Concurrency
{
    public interface IAsyncKeyLock<TKey>
    {
        /// <summary>
        /// 异步获取锁
        /// </summary>
        /// <param name="key"></param>
        /// <param name="timeout">等待时间, 负值或 <see langword="null"/> 时表示无限等待</param>
        /// <param name="cancellationToken"></param>
        /// <returns>返回锁的释放器</returns>
        /// <exception cref="LockAcquireTimeoutException"></exception>
        /// <exception cref="LockAcquireCanceledException"></exception>
        ValueTask<IKeyLockReleaser<TKey>> AcquireLockAsync(TKey key, TimeSpan? timeout = null, CancellationToken? cancellationToken = null);
    }
    public interface IKeyLockReleaser<TKey> : IAsyncDisposable
    {
        TKey Key { get; }
    }


    public sealed class AsyncKeyLock<TKey> : IAsyncKeyLock<TKey>
        where TKey : notnull
    {
        private readonly ConcurrentDictionary<TKey, SemaphoreSlim> _semaphores = new();
        private readonly Func<TKey, TKey>? keyHandleFunc;
        private readonly Func<TKey, bool>? keyValidCheckFunc;

        /// <summary>
        /// 实例化
        /// </summary>
        /// <param name="keyHandleFunc">键值处理函数, 对最终使用的键值统一做处理, 比如字符串类型的键值统一转换为大写之类的操作</param>
        /// <param name="keyValidCheckFunc">键值有效校验, 对最终使用的键值作检查, 比如判断字符串类型的键值是否不是空值</param>
        public AsyncKeyLock(Func<TKey, TKey>? keyHandleFunc = null, Func<TKey, bool>? keyValidCheckFunc = null)
        {
            this.keyHandleFunc = keyHandleFunc;
            this.keyValidCheckFunc = keyValidCheckFunc;
        }

        public async ValueTask<IKeyLockReleaser<TKey>> AcquireLockAsync(TKey key, TimeSpan? timeout = null, CancellationToken? cancellationToken = null)
        {
            if (keyHandleFunc != null)
            {
                key = keyHandleFunc(key);
            }
            if (keyValidCheckFunc != null)
            {
                if (!keyValidCheckFunc(key))
                {
                    throw new ArgumentException($"键值 {key} 无效", nameof(key));
                }
            }
            var usingToken = cancellationToken ?? CancellationToken.None;
            var semaphore = _semaphores.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
            bool waitResult = await semaphore.WaitAsync(millisecondsTimeout: timeout != null ? Math.Max((int)timeout.Value.TotalMilliseconds, -1) : -1, usingToken);
            if (!waitResult)
            {
                if (usingToken.IsCancellationRequested)
                {
                    throw new LockAcquireCanceledException(key);
                }
                else
                {
                    if (timeout == null)
                    {
                        throw new LockAcquireTimeoutException(key);
                    }
                    else
                    {
                        throw new LockAcquireTimeoutException(key, timeout.Value);
                    }
                }
            }
            return new LockReleaser(key, this);
        }


        private sealed class LockReleaser(TKey key, AsyncKeyLock<TKey> parent) : IKeyLockReleaser<TKey>
        {
            public TKey Key { get; } = key;
            public AsyncKeyLock<TKey> Parent { get; } = parent;

            public ValueTask DisposeAsync()
            {
                if (Parent._semaphores.TryGetValue(Key, out var semaphore))
                {
                    semaphore.Release();

                    if (semaphore.CurrentCount == 1)
                    {
                        Parent._semaphores.TryRemove(Key, out _);
                    }
                }
                return ValueTask.CompletedTask;
            }

        }
    }



}
