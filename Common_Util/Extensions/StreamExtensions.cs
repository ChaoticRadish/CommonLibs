using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Extensions
{
    public static class StreamExtensions
    {
        /// <summary>
        /// 将 <paramref name="stream"/> 按 <paramref name="encoding"/> 字符集读取为字符集合
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="encoding">字符集, 如果是 <see langword="null"/>, 则使用 <see cref="Encoding.UTF8"/> </param>
        /// <returns></returns>
        public static IEnumerable<char> AsCharEnumerable(this Stream stream, Encoding? encoding = null)
        {
            return String.StringHelper.AsEnumerable(stream, encoding);
        }
    }
}
