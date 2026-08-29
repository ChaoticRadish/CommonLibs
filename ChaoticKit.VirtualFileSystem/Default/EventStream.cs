namespace ChaoticKit.VirtualFileSystem.Default
{
    /// <summary>
    /// 事件包装流: 包装底层流, 释放 (<see cref="Dispose"/>) 时触发 onClose 回调 (用于触发 CloseStream 操作事件)
    /// </summary>
    internal sealed class EventStream : Stream
    {
        private readonly Stream _inner;
        private readonly Action _onClose;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inner">底层流</param>
        /// <param name="onClose">关闭回调</param>
        public EventStream(Stream inner, Action onClose)
        {
            _inner = inner;
            _onClose = onClose;
        }

        /// <inheritdoc/>
        public override bool CanRead => _inner.CanRead;

        /// <inheritdoc/>
        public override bool CanSeek => _inner.CanSeek;

        /// <inheritdoc/>
        public override bool CanWrite => _inner.CanWrite;

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
            _inner.SetLength(value);
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            _inner.Write(buffer, offset, count);
        }

        /// <inheritdoc/>
        public override void WriteByte(byte value)
        {
            _inner.WriteByte(value);
        }

        /// <inheritdoc/>
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return _inner.ReadAsync(buffer, cancellationToken);
        }

        /// <inheritdoc/>
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return _inner.WriteAsync(buffer, cancellationToken);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _onClose();
                _inner.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
