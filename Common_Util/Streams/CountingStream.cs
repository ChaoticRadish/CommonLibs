using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Streams
{
    /// <summary>
    /// 用于计数写入的字节数量的只写流, 长度信息应该在写入完成后获取 (它是 <see cref="Position"/> 曾出现过的最大值)
    /// </summary>
    /// <remarks>
    /// 实际写入的数据会被忽略
    /// </remarks>
    public sealed class CountingStream : Stream
    {
        private long _length;
        private long _position;

        public override bool CanRead => false;
        public override bool CanSeek => true;
        public override bool CanWrite => true;
        public override long Length => _length;
        public override long Position { get => _position; set => Seek(value, SeekOrigin.Begin); }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _position += count;
            if (_position > _length) _length = _position;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            _position = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => _position + offset,
                SeekOrigin.End => _length + offset,
                _ => throw new ArgumentOutOfRangeException(nameof(origin))
            };
            return _position;
        }

        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override void SetLength(long value) => _length = value;
    }
}
