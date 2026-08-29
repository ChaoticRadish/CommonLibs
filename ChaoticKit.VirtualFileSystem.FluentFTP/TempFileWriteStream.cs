using ChaoticKit.Interfaces.IO;

namespace ChaoticKit.VirtualFileSystem.FluentFTP
{
    /// <summary>
    /// 临时文件写入流: 以临时文件承载数据; 释放 (提交) 时先关闭写入流, 再执行上传回调 (读取临时文件上传), 随后释放临时文件 (删除)
    /// </summary>
    internal sealed class TempFileWriteStream : Stream, IAsyncDisposable
    {
        private readonly ITempFile _tempFile;
        private readonly FileStream _inner;
        private readonly Func<Task> _upload;
        private bool _committed;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tempFile">承载数据的临时文件, 提交后一并释放</param>
        /// <param name="upload">提交上传回调 (临时文件的写入流已关闭, 回调内可打开读取流)</param>
        public TempFileWriteStream(ITempFile tempFile, Func<Task> upload)
        {
            _tempFile = tempFile;
            _upload = upload;
            _inner = (FileStream)tempFile.OpenWrite();
        }

        /// <inheritdoc/>
        public override bool CanRead => false;

        /// <inheritdoc/>
        public override bool CanSeek => _inner.CanSeek;

        /// <inheritdoc/>
        public override bool CanWrite => true;

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
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override int ReadByte()
        {
            throw new NotSupportedException();
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
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return _inner.WriteAsync(buffer, cancellationToken);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    if (!_committed)
                    {
                        _committed = true;
                        _inner.Flush();
                        _inner.Dispose(); // 先关闭写入流, 释放文件占用, 再读取上传
                        _upload().GetAwaiter().GetResult();
                        _tempFile.Dispose();
                    }
                }
                finally
                {
                    _inner.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        public override async ValueTask DisposeAsync()
        {
            try
            {
                if (!_committed)
                {
                    _committed = true;
                    await _inner.FlushAsync();
                    await _inner.DisposeAsync(); // 先关闭写入流, 释放文件占用, 再读取上传
                    await _upload();
                    _tempFile.Dispose();
                }
            }
            finally
            {
                await _inner.DisposeAsync();
            }
        }
    }
}
