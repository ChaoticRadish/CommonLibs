using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Mechanisms.Extensions.Channel
{
    public static partial class IWritableChannelExtensions
    {
        /* 专门用于传输单元为 char 的扩展方法 */

        /// <summary>
        /// 向可写通道写入字符
        /// </summary>
        /// <typeparam name="TChannel"></typeparam>
        /// <param name="channel"></param>
        /// <param name="c"></param>
        /// <param name="autoFlush">写入完成后是否调用 <see cref="IWritableChannel{T}.Flush()"/></param>
        /// <returns></returns>
        public static TChannel Append<TChannel>(this TChannel channel, char c, bool autoFlush = true) where TChannel : IWritableChannel<char>
        {
            channel.Write([c]);
            if (autoFlush)
            {
                channel.Flush();
            }
            return channel;
        }


        /// <summary>
        /// 向可写通道写入字符
        /// </summary>
        /// <typeparam name="TChannel"></typeparam>
        /// <param name="channel"></param>
        /// <param name="str"></param>
        /// <param name="autoFlush">写入完成后是否调用 <see cref="IWritableChannel{T}.Flush()"/></param>
        /// <returns></returns>
        public static TChannel Append<TChannel>(this TChannel channel, string str, bool autoFlush = true) where TChannel : IWritableChannel<char>
        {
            channel.Write(str);
            if (autoFlush)
            {
                channel.Flush();
            }
            return channel;
        }



    }
}
