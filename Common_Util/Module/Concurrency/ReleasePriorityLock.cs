using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Module.Concurrency
{
    /// <summary>
    /// 资源创建/释放的锁, 释放操作优先执行
    /// </summary>
    /// <remarks>
    /// 创建与释放是互斥的, 不同的创建操作或释放操作也是互斥的
    /// </remarks>
    public sealed class ReleasePriorityLock : IDisposable
    {
        private readonly object _workLock = new();

        private readonly object _createEnterLock = new();
        private readonly object _createExitLock = new();
        private readonly object _releaseEnterLock = new();
        private readonly object _releaseExitLock = new();

        private int _activeCreators = 0;
        private int _pendingReleases = 0;

        private bool _disposed = false;

        private bool _createOperating = false;
        private bool _releaseOperating = false;

        /// <summary>
        /// 进入创建操作模式
        /// </summary>
        public void EnterCreate()
        {
            lock (_workLock)
            {
                // 等待直到没有任意操作在执行中, 且没有等待处理的释放操作
                while ((_pendingReleases > 0 || _createOperating || _releaseOperating) && !_disposed)
                {
                    Monitor.Wait(_workLock);
                }
                ObjectDisposedException.ThrowIf(_disposed, typeof(ReleasePriorityLock));
                _createOperating = true;
            }
        }

        /// <summary>
        /// 退出创建操作模式，并检查是否需要执行释放
        /// </summary>
        public void ExitCreate()
        {
            lock (_workLock)
            {
                _createOperating = false;
                Monitor.PulseAll(_workLock);
            }
        }

        /// <summary>
        /// 进入释放操作模式（具有优先权）
        /// </summary>
        public void EnterRelease()
        {
            lock (_workLock)
            {
                Interlocked.Increment(ref _pendingReleases);
                // 等待直到没有任意操作在执行中
                while ((_createOperating || _releaseOperating) && !_disposed)
                {
                    Monitor.Wait(_workLock);
                }
                ObjectDisposedException.ThrowIf(_disposed, typeof(ReleasePriorityLock));
                _releaseOperating = true;
            }
        }

        /// <summary>
        /// 退出释放操作模式
        /// </summary>
        public void ExitRelease()
        {
            lock (_workLock)
            {
                Interlocked.Decrement(ref _pendingReleases);
                _releaseOperating = false;
                Monitor.PulseAll(_workLock);
            }
        }



        /// <summary>
        /// 获取一个创建操作作用域, 在作用域结束时自动退出创建模式
        /// </summary>
        /// <returns></returns>
        public CreateLockScope Create()
        {
            return new CreateLockScope(this);
        }
        /// <summary>
        /// 获取一个释放操作作用域, 在作用域结束时自动退出释放模式
        /// </summary>
        /// <returns></returns>
        public ReleaseLockScope Release()
        {
            return new ReleaseLockScope(this);
        }

        /// <summary>
        /// 释放锁资源
        /// </summary>
        public void Dispose()
        {
            _disposed = true;
            Monitor.PulseAll(_workLock);
        }

        /// <summary>
        /// 自动管理创建操作作用域
        /// </summary>
        public readonly struct CreateLockScope : IDisposable
        {
            private readonly ReleasePriorityLock _lock;

            public CreateLockScope(ReleasePriorityLock @lock)
            {
                _lock = @lock;
                _lock.EnterCreate();
            }

            public void Dispose()
            {
                _lock.ExitCreate();
            }
        }

        /// <summary>
        /// 自动管理释放操作作用域（具有优先权）
        /// </summary>
        public readonly struct ReleaseLockScope : IDisposable
        {
            private readonly ReleasePriorityLock _lock;

            public ReleaseLockScope(ReleasePriorityLock createReleaseLock)
            {
                _lock = createReleaseLock;
                _lock.EnterRelease();
            }

            public void Dispose()
            {
                _lock.ExitRelease();
            }
        }
    }
}
