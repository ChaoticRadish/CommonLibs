using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Module.Concurrency
{
    /// <summary>
    /// 接口: 读写锁
    /// </summary>
    /// <remarks>
    /// 包含读写锁最基本的操作
    /// </remarks>
    public interface IReadWriteLock
    {
        /// <summary>
        /// 读取锁定
        /// </summary>
        /// <returns></returns>
        IReadLockReleaser LockRead();
        /// <summary>
        /// 写入锁定
        /// </summary>
        /// <returns></returns>
        IWriteLockReleaser LockWrite();
    }
    /// <summary>
    /// 接口: 读锁释放器
    /// </summary>
    public interface IReadLockReleaser : IDisposable
    {
    }
    /// <summary>
    /// 接口: 写锁释放器
    /// </summary>
    public interface IWriteLockReleaser : IDisposable
    {
    }


    /// <summary>
    /// 简易实现的读写锁, 所有读写操作都互斥
    /// </summary>
    public sealed class SimpleReadWriteLock : IReadWriteLock
    {
        private SemaphoreSlim locker = new SemaphoreSlim(1, 1);
        public IReadLockReleaser LockRead()
        {
            locker.Wait();
            return new ReadLockReleaser(this);
        }

        public IWriteLockReleaser LockWrite()
        {
            locker.Wait();
            return new WriteLockReleaser(this);
        }

        private sealed class ReadLockReleaser(SimpleReadWriteLock parent) : IReadLockReleaser
        {
            private readonly SimpleReadWriteLock parent = parent;
            private bool used = false;
            private readonly object innerLocker = new();
            public void Dispose()
            {
                if (used) return;
                lock (innerLocker)
                {
                    if (used) return;
                    parent.locker.Release();
                    used = true;
                }
            }
        }
        private sealed class WriteLockReleaser(SimpleReadWriteLock parent) : IWriteLockReleaser
        {
            private readonly SimpleReadWriteLock parent = parent;
            private bool used = false;
            private readonly object innerLocker = new();

            public void Dispose()
            {
                if (used) return;
                lock (innerLocker)
                {
                    if (used) return;
                    parent.locker.Release();
                    used = true;
                }
            }
        }
    }
}
