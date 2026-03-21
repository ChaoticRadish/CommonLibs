using Common_Util.Data.Struct;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Mechanisms.Extensions.Channel
{
    public static class IReadableChannelExtensions
    {

        /// <summary>
        /// 将可读通道转换为 <see cref="IEnumerable{T}"/> 序列。
        /// </summary>
        /// <typeparam name="TUnit">数据单元类型</typeparam>
        /// <param name="channel">可读通道</param>
        /// <param name="bufferSize">内部缓冲区大小（默认 1024）</param>
        /// <returns>数据单元的枚举器</returns>
        public static IEnumerable<TUnit> AsEnumerable<TUnit>(this IReadableChannel<TUnit> channel, int bufferSize = 1024)
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));
            if (bufferSize <= 0) throw new ArgumentOutOfRangeException(nameof(bufferSize));

            // 分配缓冲区
            TUnit[] buffer = new TUnit[bufferSize];

            while (true)
            {
                // 读取数据块
                int readCount = channel.Read(buffer);

                // 如果读取到 0 字节，说明流结束
                if (readCount == 0)
                {
                    break;
                }

                // 将读取到的块逐个枚举出来
                for (int i = 0; i < readCount; i++)
                {
                    yield return buffer[i];
                }
            }
        }

        /// <summary>
        /// 将异步可读通道转换为 <see cref="IAsyncEnumerable{T}"/> 序列。
        /// </summary>
        /// <typeparam name="TUnit">数据单元类型</typeparam>
        /// <param name="channel">异步可读通道</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <param name="bufferSize">内部缓冲区大小（默认 1024）</param>
        /// <returns>数据单元的异步枚举器</returns>
        public static async IAsyncEnumerable<TUnit> AsAsyncEnumerable<TUnit>(
            this IAsyncReadableChannel<TUnit> channel,
            int bufferSize = 1024,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(channel);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bufferSize);

            // 异步方法使用 Memory<T> 而非 T[]
            TUnit[] buffer = new TUnit[bufferSize];

            while (true)
            {
                // 异步读取
                int readCount = await channel.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);

                if (readCount == 0)
                {
                    break;
                }

                for (int i = 0; i < readCount; i++)
                {
                    yield return buffer[i];
                }
            }
        }




        /// <summary>
        /// 将 <see cref="IEnumerable{T}"/> 转换为可读通道。
        /// </summary>
        public static IReadableChannel<TUnit> AsReadableChannel<TUnit>(this IEnumerable<TUnit> source)
        {
            ArgumentNullException.ThrowIfNull(source);
            return new EnumerableReadableChannel<TUnit>(source);
        }
        /// <summary>
        /// 内部适配器：封装 <see cref="IEnumerable{T}"/> 以实现 <see cref="IReadableChannel{T}"/> 接口
        /// </summary>
        private sealed class EnumerableReadableChannel<TUnit> : IReadableChannel<TUnit>
        {
            private readonly IEnumerator<TUnit> _enumerator;
            private bool _disposed;
            public EnumerableReadableChannel(IEnumerable<TUnit> source)
            {
                _enumerator = source.GetEnumerator();
            }

            public int Read(Span<TUnit> buffer)
            {
                int count = 0;

                // 循环填充缓冲区，直到填满或枚举结束
                while (count < buffer.Length)
                {
                    if (!_enumerator.MoveNext())
                    {
                        break; // 枚举结束，流结束
                    }
                    buffer[count++] = _enumerator.Current;
                }

                return count;
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _enumerator?.Dispose();
                    _disposed = true;
                }
            }
        }



        /// <summary>
        /// 将 <see cref="IAsyncEnumerable{T}"/> 转换为异步可读通道。
        /// </summary>
        /// <typeparam name="TUnit"></typeparam>
        /// <param name="source"></param>
        /// <param name="cancellationToken">取得的异步可读通道异步读取时的传入的取消令牌不会起效, 需要用这一个令牌去取消</param>
        /// <returns></returns>
        public static IAsyncReadableChannel<TUnit> AsAsyncReadableChannel<TUnit>(this IAsyncEnumerable<TUnit> source, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            return new AsyncEnumerableReadableChannel<TUnit>(source.GetAsyncEnumerator(cancellationToken));
        }
        /// <summary>
        /// 内部适配器：封装 <see cref="IAsyncEnumerator{T}"/> 以实现 <see cref="IAsyncReadableChannel{T}"/> 接口
        /// </summary>
        private sealed class AsyncEnumerableReadableChannel<TUnit> : IAsyncReadableChannel<TUnit>
        {
            private readonly IAsyncEnumerator<TUnit> _enumerator;
            private bool _disposed;

            public AsyncEnumerableReadableChannel(IAsyncEnumerator<TUnit> enumerator)
            {
                _enumerator = enumerator;
            }

            public async ValueTask<int> ReadAsync(Memory<TUnit> buffer, CancellationToken cancellationToken = default)
            {
                int count = 0;
                while (count < buffer.Length)
                {
                    if (!await _enumerator.MoveNextAsync().ConfigureAwait(false))
                    {
                        break;
                    }

                    buffer.Span[count++] = _enumerator.Current;
                }

                return count;
            }

            public async ValueTask DisposeAsync()
            {
                if (!_disposed)
                {
                    await _enumerator.DisposeAsync().ConfigureAwait(false);
                    _disposed = true;
                }
            }
        }



        /// <summary>
        /// 将 <see cref="IEnumerable{T}"/> 桥接为异步可读通道。
        /// </summary>
        /// <typeparam name="TUnit"></typeparam>
        /// <param name="source">数据源</param>
        /// <param name="forceAsync">
        /// 是否强制异步读取数据源 <br/>
        /// 否则在诸如 <see cref="IList{T}"/> 或数组之类的数据源会用同步的方式去执行读取
        /// </param>
        /// <returns></returns>
        public static IAsyncReadableChannel<TUnit> AsAsyncReadableChannel<TUnit>(this IEnumerable<TUnit> source, bool forceAsync = false)
        {
            ArgumentNullException.ThrowIfNull(source);
            if (!forceAsync)
            {
                if (source is IList<TUnit> list)
                {
                    return new ListToAsyncReadableChannel<TUnit>(list);
                }
            }
            return new SyncToAsyncReadableChannel<TUnit>(source);
        }

        private sealed class SyncToAsyncReadableChannel<TUnit> : IAsyncReadableChannel<TUnit>
        {
            private readonly IEnumerator<TUnit> _enumerator;
            private bool _disposed;

            public SyncToAsyncReadableChannel(IEnumerable<TUnit> source)
            {
                _enumerator = source.GetEnumerator();
            }

            public ValueTask<int> ReadAsync(Memory<TUnit> buffer, CancellationToken cancellationToken = default)
            {
                return new ValueTask<int>(Task.Run(() =>
                {
                    int count = 0;
                    while (count < buffer.Length)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if (!_enumerator.MoveNext())
                        {
                            break;
                        }

                        buffer.Span[count++] = _enumerator.Current;
                    }
                    return count;
                }, cancellationToken));
            }

            public ValueTask DisposeAsync()
            {
                try
                {
                    _enumerator.Dispose();
                    return default;
                }
                catch (Exception ex)
                {
                    return ValueTask.FromException(ex);
                }
            }
        }

        private sealed class ListToAsyncReadableChannel<TUnit> : IAsyncReadableChannel<TUnit>
        {
            private readonly IList<TUnit> _source;
            private int _index; // 当前读取位置

            public ListToAsyncReadableChannel(IList<TUnit> source)
            {
                _source = source;
            }

            public ValueTask<int> ReadAsync(Memory<TUnit> buffer, CancellationToken cancellationToken = default)
            {
                cancellationToken.ThrowIfCancellationRequested();

                int remaining = _source.Count - _index;
                if (remaining <= 0)
                {
                    return new ValueTask<int>(0);
                }

                int countToRead = Math.Min(buffer.Length, remaining);

                for (int i = 0; i < countToRead; i++)
                {
                    buffer.Span[i] = _source[_index++];
                }

                return new ValueTask<int>(countToRead);
            }

            public ValueTask DisposeAsync()
            {
                return default;
            }
        }
    }
}
