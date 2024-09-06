using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Streams
{
    /// <summary>
    /// 基于偏移量, 对另一个流的包装. 
    /// <para>实例化时不会流的当前位置, 需要考虑先使用 <see cref="Seek(long, SeekOrigin)"/> 调整位置再作其他操作</para>
    /// </summary>
    public class OffsetWrapperStream : Stream
    {
        public OffsetWrapperStream(Stream source, long start, long? limitLength)
        {
            Source = source;
            Offset = start;
            LimitLength = limitLength;

            WrapperStart = Offset;
            WrapperEnd = Offset + LimitLength;
        }


        #region 包装流属性
        /// <summary>
        /// 被包装的流
        /// </summary>
        public Stream Source { get; init; }
        /// <summary>
        /// 读取源时的偏移量
        /// </summary>
        public long Offset { get; init; }
        /// <summary>
        /// 限制包装后的流, 在读取源时的可访问长度, 可以不做限制
        /// </summary>
        public long? LimitLength { get; private set; }

        /// <summary>
        /// 源流被包装范围的起点 (源流内的位置)
        /// </summary>
        public long WrapperStart { get; }
        /// <summary>
        /// 源流被包装范围的终点 (源流内的位置)
        /// <para>如果不限制可访问长度, 则此值为 null</para>
        /// </summary>
        public long? WrapperEnd { get; private set; }

        /// <summary>
        /// 释放流时, 是否释放被包装的流
        /// </summary>
        public bool DisposeSource { get; set; } = false;
        #endregion


        public override bool CanRead => Source.CanRead;

        public override bool CanSeek => Source.CanSeek;

        public override bool CanWrite => Source.CanWrite;

        /// <summary>
        /// 目前包装的有效长度
        /// </summary>
        public override long Length 
        {
            get
            {
                long output = Source.Length - Offset;
                if (output < 0) return 0;
                else if (LimitLength != null) return Math.Min(output, LimitLength.Value);
                else return output;
            }
        }

        /// <summary>
        /// 在包装后的流内的当前位置
        /// </summary>
        public override long Position
        {
            get
            {
                long output = Source.Position - Offset;
                if (output < 0) return 0;
                else if (LimitLength != null) return Math.Min(output, LimitLength.Value);
                else return output;
            }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "位置索引不在包装范围内, 该值小于 0 ");
                else if (value >= LimitLength) throw new ArgumentOutOfRangeException(nameof(value), $"位置索引不在包装范围内, 该值超过了长度限制 {LimitLength} ");

                Source.Position = value + Offset;
            }
        }

        public override void Flush() => Source.Flush();


        /// <summary>
        /// 从当前位置开始, 读取指定数量的 byte 写入传入的数组中, 将当前位置推进数个位置, 推进量与读取成功数量相等. 
        /// <para>执行读取前, 如果源留的当前位置在包装范围之前, 将使用源流的 <see cref="Stream.Seek(long, SeekOrigin)"/> 方法, 移动到包装范围的起点开始读取</para>
        /// <para>如果在包装范围之后, 则将不作读取, 直接返回 0 </para>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public override int Read(byte[] buffer, int offset, int count)
        {
            ArgumentNullException.ThrowIfNull(buffer, "传入缓存数组为空");
            ArgumentOutOfRangeException.ThrowIfLessThan(offset, 0, "数组偏移量");
            ArgumentOutOfRangeException.ThrowIfGreaterThan(offset, buffer.Length - 1, "数组偏移量");
            ArgumentOutOfRangeException.ThrowIfLessThan(count, 0, "准备写入数量");
            ArgumentOutOfRangeException.ThrowIfGreaterThan(count, buffer.Length, "准备写入数量");

            if (LimitLength != null)
            {
                if (Position + count > LimitLength.Value)
                {
                    count = (int)(LimitLength.Value - Position);
                }
            }

            if (Source.Position < WrapperStart)
            {
                Source.Seek(Offset, SeekOrigin.Begin);
            }
            if (WrapperEnd != null && Source.Position >= WrapperEnd.Value)
            {
                return 0;
            }
            return Source.Read(buffer, offset, count);
        }
        /// <summary>
        /// 从传入数组的指定位置开始, 读取指定数量的 byte 写入源流中, 将当前位置推进数个位置, 推进量与写入成功数量相等. 
        /// <para>执行写入前, 如果源留的当前位置在包装范围之前, 将使用源流的 <see cref="Stream.Seek(long, SeekOrigin)"/> 方法, 移动到包装范围的起点开始写入</para>
        /// <para>如果在包装范围之后, 则将不作写入</para>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
            ArgumentNullException.ThrowIfNull(buffer, "传入缓存数组为空");
            ArgumentOutOfRangeException.ThrowIfLessThan(offset, 0, "数组偏移量");
            ArgumentOutOfRangeException.ThrowIfGreaterThan(offset, buffer.Length - 1, "数组偏移量");
            ArgumentOutOfRangeException.ThrowIfLessThan(count, 0, "准备写入数量");
            ArgumentOutOfRangeException.ThrowIfGreaterThan(count, buffer.Length, "准备写入数量");

            // 判断是否需要截取数据, 使其长度不超过包装范围
            if (LimitLength != null)
            {
                if (Position + count > LimitLength.Value)
                {
                    count = (int)(LimitLength.Value - Position);
                }
            }

            if (Source.Position < WrapperStart)
            {
                Source.Seek(Offset, SeekOrigin.Begin);
            }
            if (WrapperEnd != null && Source.Position >= WrapperEnd.Value)
            {
                return;
            }


            Source.Write(buffer, offset, count);

        }

        /// <summary>
        /// 设置包装后的流内的当前位置
        /// <para>计算得到新位置之后, 将使用源流的 <see cref="Stream.Seek(long, SeekOrigin)"/> 方法, </para>
        /// <para>将当前位置设置为 (<see cref="Offset"/> + 计算得到的位置, <see cref="SeekOrigin.Begin"/>)</para>
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public override long Seek(long offset, SeekOrigin origin)
        {
            if (LimitLength != null) ArgumentOutOfRangeException.ThrowIfGreaterThan(offset, LimitLength.Value, "查找偏移量");

            long temp;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    temp = offset;
                    break;
                case SeekOrigin.Current:
                    temp = Position + offset;
                    break;
                case SeekOrigin.End:
                    if (LimitLength == null) temp = Math.Max(0, Source.Length - Offset + offset);
                    else temp = LimitLength.Value + offset;
                    break;
                default:
                    throw new NotImplementedException();
            }

            if (temp < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), "查找偏移量定位到了包装范围之前");
            else if (temp >= LimitLength)
                throw new ArgumentOutOfRangeException(nameof(offset), "查找偏移量定位到了包装范围之后");

            return Source.Seek(Offset + temp, SeekOrigin.Begin);
        }

        /// <summary>
        /// 设置本流所包装的范围
        /// </summary>
        /// <param name="value"></param>
        public override void SetLength(long value)
        {
            LimitLength = value;
            WrapperEnd = Offset + LimitLength;
        }
        /// <summary>
        /// 设置本流所包装的范围
        /// </summary>
        /// <param name="value"></param>
        public void SetLimitLength(long? value)
        {
            LimitLength = value;
            WrapperEnd = Offset + LimitLength;
        }

        #region 释放
        private bool _disposed = false;
        protected override void Dispose(bool disposing)
        {
            if (_disposed) return;
            base.Dispose(disposing);
            if (disposing)
            {
                if (DisposeSource)
                {
                    Source.Dispose();
                }
            }
            disposing = true;
        }
        #endregion
    }
}
