using ChaoticKit.Interfaces.IO;

namespace ChaoticKit.VirtualFileSystem.FluentFTP
{
    /// <summary>
    /// 临时文件读取流: 包装临时文件的读取流, 释放时一并释放临时文件 (删除)
    /// </summary>
    internal sealed class TempFileReadStream : Stream, IAsyncDisposable
    {
        private readonly ITempFile _tempFile;
        private readonly FileStream _inner;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tempFile">承载数据的临时文件, 释放本流时一并释放</param>
        public TempFileReadStream(ITempFile tempFile)
        {
            _tempFile = tempFile;
            _inner = (FileStream)tempFile.OpenRead();
        }

        /// <inheritdoc/>
        public override bool CanRead => true;

        /// <inheritdoc/>
        public override bool CanSeek => _inner.CanSeek;

        /// <inheritdoc/>
        public override bool CanWrite => false;

        /// <inheritdoc/>
        public override long Length => _inner.Length;

        /// <inheritdoc/>
        public override long Position { get => _inner.Position; set => _inner.Position = value; }

        /// <inheritdoc/>
        public override void Flush()
        {
            _inner.Flush();
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return _inner.Read(buffer, offset, count);
        }

        /// <inheritdoc/>
        public override int ReadByte()
        {
            return _inner.ReadByte();
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            return _inner.Seek(offset, origin);
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return _inner.ReadAsync(buffer, cancellationToken);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _inner.Dispose();
                _tempFile.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        public override async ValueTask DisposeAsync()
        {
            await _inner.DisposeAsync();
            _tempFile.Dispose();
        }
    }
}
