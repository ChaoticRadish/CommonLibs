using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Common_Util.Xml
{
    /// <summary>
    /// 分为头部和主体部分的文件类型, 头部的数据保存为Xml格式
    /// </summary>
    /// <typeparam name="HeadType"></typeparam>
    public struct XmlHeadBodyStream<HeadType>
    {

        /// <summary>
        /// 对应的文件 (不为null时优先读取文件)
        /// </summary>
        public FileInfo File { get; set; }
        /// <summary>
        /// 对应的流
        /// </summary>
        public Stream Stream { get; set; }

        /// <summary>
        /// 头部的长度
        /// </summary>
        public long HeadLength { get; set; }
        /// <summary>
        /// 头部数据
        /// </summary>
        public HeadType Head { get; set; }
        /// <summary>
        /// 主体部分数据的起点
        /// </summary>
        public long BodyStart { get => XmlHeadBodyStreamHelper.HEAD_SIZE_FLAG_LENGTH + HeadLength; }

        /// <summary>
        /// 读取文件指定部分的数据
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public byte[] Read(long offset, long length)
        {
            using Stream stream = File == null ? Stream : File.OpenRead();
            byte[] buffer = new byte[length];
            stream.Seek(offset, SeekOrigin.Begin);
            int temp;
            for (long i = 0; i < length && (temp = stream.ReadByte()) >= 0; i++)
            {
                buffer[i] = (byte)temp;
            }
            return buffer;
        }
        /// <summary>
        /// 读取文件主体指定部分的数据
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public byte[] ReadBody(long offset, long length)
        {
            return Read(offset + BodyStart, length);
        }
    }
}
