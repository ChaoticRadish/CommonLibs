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
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] ToBinaryAsXml<T>(T data)
        {
            using MemoryStream memoryStream = new();
            WriteToStreamAsXml(data, memoryStream);
            return memoryStream.ToArray();
        }
        /// <summary>
        /// 将输入对象以 XML 格式序列化后写入传入的流
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="stream"></param>
        public static void WriteToStreamAsXml<T>(T data, Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            XmlSerializer serializer = new(typeof(T));
            serializer.Serialize(stream, data);
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
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static T InitByStreamAsXml<T>(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            XmlSerializer serializer = new(typeof(T));
            return (T)(serializer.Deserialize(stream) ?? throw new InvalidOperationException($"未能将传入数据以 XML 格式反序列化为类型 {typeof(T)}"));
        }
    }
}
