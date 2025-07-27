using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common_Util.Module.Concurrency
{
    /// <summary>
    /// 工作计数守卫
    /// </summary>
    /// <remarks>
    /// 使用方式: <br/>
    /// 1. 调用 <see cref="TryBeginWork(TimeSpan?)"/>, 在令牌作用域内 (即获取到释放前), 阻止请求锁定 (<see cref="TryAcquireLock(TimeSpan?)"/>) <br/>
    /// 2. 调用 <see cref="TryAcquireLock(TimeSpan?)"/>, 如果存在任意工作, 则等待所有工作完成后取得锁定令牌. 在等待期间, 新发起的工作令牌请求会被挂起, 直到锁定令牌被释放 <br/>
    /// </remarks>
    public sealed class WorkCountGuard
    {
        private readonly object _syncRoot = new();
        private int _activeWorkers;
        private bool _isLocked;
        private bool _lockRequested;
        private static readonly Stopwatch _sharedStopwatch = Stopwatch.StartNew();

        /// <summary>
        /// 当前工作数量
        /// </summary>
        public int ActiveWorkers
        {
            get
            {
                lock (_syncRoot) return _activeWorkers;
            }
        }

        /// <summary>
        /// 尝试获取工作令牌
        /// </summary>
        /// <param name="timeout">超时时间, 如果为 <see langword="null"/> 则无限等待</param>
        /// <returns></returns>
        public WorkerToken TryBeginWork(TimeSpan? timeout = null)
        {
            lock (_syncRoot)
            {
                long startTicks = _sharedStopwatch.ElapsedTicks;

                // 等待直到没有锁定和锁定请求
                while (_isLocked || _lockRequested)
                {
                    if (timeout == null)
                    {
                        Monitor.Wait(_syncRoot);
                    }
                    else
                    {
                        // 计算剩余时间
                        TimeSpan elapsed = TicksToTimeSpan(_sharedStopwatch.ElapsedTicks - startTicks);
                        if (elapsed >= timeout)
                        {
                            return new WorkerToken("等待锁定状态释放超时");
                        }

                        TimeSpan remaining = timeout.Value - elapsed;
                        if (!Monitor.Wait(_syncRoot, remaining))
                        {
                            return new WorkerToken("等待锁定状态释放超时");
                        }
                    }
                }

                _activeWorkers++;
                return new WorkerToken(this);
            }
        }

        /// <summary>
        /// 工作令牌（确保工作正确结束）
        /// </summary>
        public sealed class WorkerToken : TokenBase
        {
            internal WorkerToken(string failureInfo) : base(failureInfo) { }
            internal WorkerToken(WorkCountGuard guard) : base(guard) { }

            /// <summary>
            /// 标记工作已完成
            /// </summary>
            public void CompleteWork() => Dispose();

            protected override void DisposeBody(WorkCountGuard guard)
            {
                guard._activeWorkers--;
                if (guard._activeWorkers == 0)
                {
                    Monitor.PulseAll(guard._syncRoot); // 通知等待线程
                }
            }
        }

        /// <summary>
        /// 尝试获取锁定令牌
        /// </summary>
        /// <param name="timeout">超时时间, 如果为 <see langword="null"/> 则无限等待</param>
        /// <returns></returns>
        public LockToken TryAcquireLock(TimeSpan? timeout = null)
        {
            lock (_syncRoot)
            {
                // 设置锁定请求
                _lockRequested = true;
                Monitor.PulseAll(_syncRoot); // 唤醒可能等待的工作线程

                try
                {
                    long startTicks = _sharedStopwatch.ElapsedTicks;
                    while (true)
                    {
                        // 检查条件：无活跃工作线程
                        if (_activeWorkers == 0)
                        {
                            _isLocked = true;
                            return new LockToken(this);
                        }

                        // 处理超时
                        if (timeout == null)
                        {
                            Monitor.Wait(_syncRoot);
                        }
                        else
                        {
                            // 计算剩余时间
                            TimeSpan elapsed = TicksToTimeSpan(_sharedStopwatch.ElapsedTicks - startTicks);
                            if (elapsed >= timeout)
                            {
                                return new LockToken("等待锁定状态释放超时");
                            }

                            TimeSpan remaining = timeout.Value - elapsed;
                            if (!Monitor.Wait(_syncRoot, remaining))
                            {
                                return new LockToken("等待锁定状态释放超时");
                            }
                        }
                    }
                }
                finally
                {
                    _lockRequested = false;
                    Monitor.PulseAll(_syncRoot);
                }
            }
        }

        /// <summary>
        /// 锁定令牌
        /// </summary>
        public sealed class LockToken : TokenBase
        {
            internal LockToken(string failureInfo) : base(failureInfo) { }
            internal LockToken(WorkCountGuard guard) : base(guard) { }

            /// <summary>
            /// 释放锁定状态
            /// </summary>
            public void Release() => Dispose();

            protected override void DisposeBody(WorkCountGuard guard)
            {
                guard._isLocked = false;
                Monitor.PulseAll(guard._syncRoot);
            }
        }


        public abstract class TokenBase : IDisposable
        {
            /// <summary>
            /// 获取令牌是否成功
            /// </summary>
            public bool GetSuccess { get; private set; }
            /// <summary>
            /// 获取令牌的失败信息
            /// </summary>
            public string FailureInfo { get; private set; } = string.Empty;

            private WorkCountGuard? _guard;
            private bool _isDispose;

            internal TokenBase(string failureInfo)
            {
                FailureInfo = failureInfo;
                GetSuccess = false;
            }
            internal TokenBase(WorkCountGuard? guard)
            {
                _guard = guard;
                GetSuccess = true;
            }

            public void Dispose()
            {
                if (!GetSuccess || _isDispose || _guard == null) return;
                _isDispose = true;

                lock (_guard._syncRoot)
                {
                    DisposeBody(_guard);
                    _guard = null;
                }
            }
            protected abstract void DisposeBody(WorkCountGuard guard);
        }


        private static TimeSpan TicksToTimeSpan(long ticks)
        {
            if (Stopwatch.IsHighResolution)
            {
                double seconds = (double)ticks / Stopwatch.Frequency;
                return TimeSpan.FromSeconds(seconds);
            }
            return TimeSpan.FromTicks(ticks);
        }
    }
}
