namespace ChaoticKit.Streams
{
    /// <summary>
    /// 流长度计算辅助: 支持定位的流直接取长度, 否则分块读取累加
    /// </summary>
    public static class StreamLengthHelper
    {
        private const int BufferSize = 81920;

        /// <summary>
        /// 取得流的字节长度
        /// </summary>
        /// <remarks>不会关闭传入的流; 不支持定位的流在计算后会被读取到末尾</remarks>
        /// <param name="stream"></param>
        /// <param name="cancellationToken"></param>
        public static async Task<long> CountLengthAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            if (stream.CanSeek)
            {
                return stream.Length;
            }

            long length = 0;
            byte[] buffer = new byte[BufferSize];
            int read;
            while ((read = await stream.ReadAsync(buffer, cancellationToken)) > 0)
            {
                length += read;
            }
            return length;
        }
    }
}
