using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Streams
{
    /// <summary>
    /// 流对象装饰器: 在流对象被释放时执行指定的操作
    /// </summary>
    public class DisposalActionStream : Stream
    {
        private readonly Stream _baseStream;
        private readonly Action _disposeAction;
        private bool _isDisposed = false;

        public DisposalActionStream(Stream baseStream, Action disposeAction)
        {
            _baseStream = baseStream ?? throw new ArgumentNullException(nameof(baseStream));
            _disposeAction = disposeAction ?? throw new ArgumentNullException(nameof(disposeAction));
        }

        // 委托所有属性和方法给 _baseStream
        public override bool CanRead => _baseStream.CanRead;
        public override bool CanSeek => _baseStream.CanSeek;
        public override bool CanWrite => _baseStream.CanWrite;
        public override long Length => _baseStream.Length;
        public override long Position { get => _baseStream.Position; set => _baseStream.Position = value; }

        public override void Flush() => _baseStream.Flush();
        public override int Read(byte[] buffer, int offset, int count) => _baseStream.Read(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin) => _baseStream.Seek(offset, origin);
        public override void SetLength(long value) => _baseStream.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => _baseStream.Write(buffer, offset, count);

        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _baseStream?.Dispose();
                    _disposeAction?.Invoke();
                }
                _isDisposed = true;
            }
            base.Dispose(disposing);
        }
    }

}
