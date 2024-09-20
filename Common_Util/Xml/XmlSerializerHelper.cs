using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Common_Util.Xml
{
    /// <summary>
    /// XML 格式序列化帮助类
    /// </summary>
    public static class XmlSerializerHelper
    {
        /// <summary>
        /// 将输入对象以 XML 格式序列化为二进制数据
        /// </summary>
        /// <remarks>
        /// 此方法通过实例化 <see cref="MemoryStream"/> 后调用 <see cref="WriteToStreamAsXml{T}(T, Stream, Encoding?)"/> 实现
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="encoding">字符集, 如果为 <see langword="null"/>, 则采用 <see cref="Encoding.UTF8"/></param>
        /// <returns></returns>
        public static byte[] ToBinaryAsXml<T>(T data, Encoding? encoding = null)
        {
            using MemoryStream memoryStream = new();
            WriteToStreamAsXml(data, memoryStream, encoding ?? Encoding.UTF8);
            return memoryStream.ToArray();
        }
        /// <summary>
        /// 将输入对象以 XML 格式序列化后写入传入的流
        /// </summary>
        /// <remarks>
        /// 此方法基于 <see cref="XmlSerializer.Serialize(TextWriter, object?)"/> 方法实现
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="stream"></param>
        /// <param name="encoding">字符集, 如果为 <see langword="null"/>, 则采用 <see cref="Encoding.UTF8"/></param>
        public static void WriteToStreamAsXml<T>(T data, Stream stream, Encoding? encoding = null)
        {
            stream.Seek(0, SeekOrigin.Begin);
            XmlSerializer serializer = new(typeof(T));
            TextWriter writer = new StreamWriter(stream, encoding ?? Encoding.UTF8);
            serializer.Serialize(writer, data);
        }

        /// <summary>
        /// 将二进制数据以 XML 格式反序列化为指定类型的对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static T InitByBinaryAsXml<T>(byte[] data)
        {
            using MemoryStream memoryStream = new(data);
            return InitByStreamAsXml<T>(memoryStream);
        }
        /// <summary>
        /// 将数据流以 XML 格式反序列化为指定类型的对象
        /// </summary>
        /// <remarks>
        /// 此方法基于 <see cref="XmlSerializer.Deserialize(TextReader)"/> 方法实现
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="encoding">字符集, 如果为 <see langword="null"/>, 则采用 <see cref="Encoding.UTF8"/></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static T InitByStreamAsXml<T>(Stream stream, Encoding? encoding = null)
        {
            stream.Seek(0, SeekOrigin.Begin);
            XmlSerializer serializer = new(typeof(T));
            TextReader reader = new StreamReader(stream, encoding ?? Encoding.UTF8);
            return (T)(serializer.Deserialize(reader) ?? throw new InvalidOperationException($"未能将传入数据以 XML 格式反序列化为类型 {typeof(T)}"));
        }
    }
}
