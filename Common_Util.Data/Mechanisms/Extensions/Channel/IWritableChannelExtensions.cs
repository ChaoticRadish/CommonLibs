using Common_Util.Data.Mechanisms.Impl;
using Common_Util.Data.Struct;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Mechanisms.Extensions.Channel
{
    public static partial class IWritableChannelExtensions
    {
        /// <summary>
        /// 将序列中的所有数据写入通道。
        /// </summary>
        /// <typeparam name="TUnit"></typeparam>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        /// <param name="bufferSize"></param>
        /// <param name="autoFlush">如果为 <see langword="true"/>，则在每次写入缓冲区后立即刷新通道；默认为 <see langword="false"/>。</param>
        public static void WriteTo<TUnit>(this IEnumerable<TUnit> source, IWritableChannel<TUnit> dest, int bufferSize = 1024, bool autoFlush = false)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(dest);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bufferSize);

            TUnit[] buffer = new TUnit[bufferSize];
            int count = 0;

            foreach (var item in source)
            {
                buffer[count++] = item;

                // 缓冲区满了，写入通道
                if (count == buffer.Length)
                {
                    dest.Write(buffer.AsSpan(0, count));
                    if (autoFlush)
                    {
                        dest.Flush();
                    }
                    count = 0;
                }
            }

            if (count > 0)
            {
                dest.Write(buffer.AsSpan(0, count));
                if (autoFlush)
                {
                    dest.Flush();
                }
            }
        }

        /// <summary>
        /// 将异步序列中的所有数据异步写入通道。
        /// </summary>
        /// <param name="source">数据源</param>
        /// <param name="dest">目标通道</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <param name="bufferSize">缓冲区大小</param>
        /// <param name="autoFlush">如果为 true，则在每次写入缓冲区后立即刷新通道；默认为 false。</param>
        public static async ValueTask WriteToAsync<TUnit>(
            this IAsyncEnumerable<TUnit> source,
            IAsyncWritableChannel<TUnit> dest,
            int bufferSize = 1024,
            bool autoFlush = false,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(dest);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bufferSize);

            TUnit[] buffer = new TUnit[bufferSize];
            int count = 0;

            await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                buffer[count++] = item;

                // 缓冲区满了
                if (count == buffer.Length)
                {
                    await dest.WriteAsync(buffer.AsMemory(0, count), cancellationToken).ConfigureAwait(false);

                    if (autoFlush)
                    {
                        await dest.FlushAsync(cancellationToken).ConfigureAwait(false);
                    }

                    count = 0;
                }
            }

            // 处理剩余的数据
            if (count > 0)
            {
                await dest.WriteAsync(buffer.AsMemory(0, count), cancellationToken).ConfigureAwait(false);

                if (autoFlush)
                {
                    await dest.FlushAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }



        /// <summary>
        /// 将列表包装为可写通道。
        /// </summary>
        /// <remarks>
        /// 每次调用写入都会将数据追加到列表的末尾。
        /// </remarks>
        public static IWritableChannel<TUnit> AsWritableChannel<TUnit>(this IList<TUnit> list)
        {
            ArgumentNullException.ThrowIfNull(list);
            return new ListWritableChannel<TUnit>(list);
        }
        private sealed class ListWritableChannel<TUnit> : IWritableChannel<TUnit>
        {
            private readonly IList<TUnit> _list;

            public ListWritableChannel(IList<TUnit> list)
            {
                _list = list;
            }

            public void Write(ReadOnlySpan<TUnit> buffer)
            {
                foreach (var item in buffer)
                {
                    _list.Add(item);
                }
            }

            public void Flush()
            {
            }

            public void Dispose()
            {
            }
        }



        /// <summary>
        /// 将列表包装为异步可写通道。
        /// </summary>
        /// <remarks>
        /// 每次调用写入都会将数据追加到列表的末尾。
        /// </remarks>
        public static IAsyncWritableChannel<TUnit> AsAsyncWritableChannel<TUnit>(this IList<TUnit> list)
        {
            ArgumentNullException.ThrowIfNull(list);
            return new AsyncListWritableChannel<TUnit>(list);
        }
        private sealed class AsyncListWritableChannel<TUnit> : IAsyncWritableChannel<TUnit>
        {
            private readonly IList<TUnit> _list;

            public AsyncListWritableChannel(IList<TUnit> list)
            {
                _list = list;
            }

            public ValueTask WriteAsync(ReadOnlyMemory<TUnit> buffer, CancellationToken cancellationToken = default)
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    _list.Add(buffer.Span[i]);
                }

                return ValueTask.CompletedTask;
            }

            public ValueTask FlushAsync(CancellationToken cancellationToken = default)
            {
                return ValueTask.CompletedTask;
            }

            public ValueTask DisposeAsync()
            {
                return ValueTask.CompletedTask;
            }
        }

        /// <summary>
        /// 将字符串构建器包装为可写通道, 
        /// </summary>
        /// <param name="stringBuilder"></param>
        /// <returns></returns>
        public static IDualWritableChannel<char> AsWritableChannel(this StringBuilder stringBuilder)
        {
            ArgumentNullException.ThrowIfNull(stringBuilder);
            return new StringBuilderWritableChannel(stringBuilder);
        }
        private sealed class StringBuilderWritableChannel(StringBuilder stringBuilder) : IDualWritableChannel<char>
        {
            private readonly StringBuilder _builder = stringBuilder;

            public void Write(ReadOnlySpan<char> buffer)
            {
                _builder.Append(buffer);
            }

            public ValueTask WriteAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _builder.Append(buffer);
                return ValueTask.CompletedTask;
            }


            public void Flush() { }
            public ValueTask FlushAsync(CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
            public void Dispose() { }
            public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        }

    }
}
