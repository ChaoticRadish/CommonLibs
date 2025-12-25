using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Mechanisms
{
    /// <summary>
    /// 双向通道接口。同时支持读取和写入。
    /// </summary>
    /// <typeparam name="T">数据单元类型</typeparam>
    public interface IChannel<T> : IReadableChannel<T>, IWritableChannel<T>
    {
    }
    /// <summary>
    /// 可读通道接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReadableChannel<T> : IDisposable
    {
        /// <summary>
        /// 从通道中读取数据。
        /// </summary>
        /// <param name="buffer">目标缓冲区</param>
        /// <returns>实际读取的数据单元数量。如果到达流末尾，返回 0。</returns>
        int Read(Span<T> buffer);
    }
    /// <summary>
    /// 可写通道接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IWritableChannel<T> : IDisposable
    {
        /// <summary>
        /// 向通道中写入数据。
        /// </summary>
        /// <param name="buffer">包含要写入数据的只读缓冲区</param>
        void Write(ReadOnlySpan<T> buffer);

        /// <summary>
        /// 刷新通道。
        /// </summary>
        /// <remarks>
        /// 清除缓冲区，将所有缓冲数据立即写入底层存储。
        /// </remarks>
        void Flush();
    }



    /// <summary>
    /// 异步双向通道接口。同时支持读取和写入。
    /// </summary>
    /// <typeparam name="T">数据单元类型</typeparam>
    public interface IAsyncChannel<T> : IAsyncReadableChannel<T>, IAsyncWritableChannel<T>
    {
    }

    /// <summary>
    /// 异步可读通道接口。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IAsyncReadableChannel<T> : IAsyncDisposable
    {
        /// <summary>
        /// 从通道中异步读取数据。
        /// </summary>
        /// <param name="buffer">目标缓冲区</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>实际读取的数据单元数量。如果到达流末尾，返回 0。</returns>
        ValueTask<int> ReadAsync(Memory<T> buffer, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// 异步可写通道接口。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IAsyncWritableChannel<T> : IAsyncDisposable
    {
        /// <summary>
        /// 向通道中异步写入数据。
        /// </summary>
        /// <param name="buffer">包含要写入数据的只读缓冲区</param>
        /// <param name="cancellationToken">取消令牌</param>
        ValueTask WriteAsync(ReadOnlyMemory<T> buffer, CancellationToken cancellationToken = default);

        /// <summary>
        /// 异步刷新通道。
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <remarks>
        /// 清除缓冲区，将所有缓冲数据立即写入底层存储。
        /// </remarks>
        ValueTask FlushAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// 同时提供同步异步操作的可读通道
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDualReadableChannel<T> : IReadableChannel<T>, IAsyncReadableChannel<T> { }

    /// <summary>
    /// 同时提供同步异步操作的可写通道
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDualWritableChannel<T> : IWritableChannel<T>, IAsyncWritableChannel<T> { }


}
