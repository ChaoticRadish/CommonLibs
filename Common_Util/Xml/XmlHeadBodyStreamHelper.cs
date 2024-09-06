using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Common_Util.Xml
{
    public static class XmlHeadBodyStreamHelper
    {
        /// <summary>
        /// 标识头部数据尺寸的数据长度, long类型占用8个字节的位置
        /// </summary>
        public const int HEAD_SIZE_FLAG_LENGTH = 8;

        /// <summary>
        /// 读取指定的文件
        /// </summary>
        /// <typeparam name="HeadType"></typeparam>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static XmlHeadBodyStream<HeadType> ReadAs<HeadType>(string fileName)
        {
            XmlHeadBodyStream<HeadType> output = new()
            {
                File = new(fileName)
            };
            if (!output.File.Exists)
            {
                throw new FileNotFoundException("等待读取的文件不存在", fileName);
            }
            return ReadAs(output);
        }
        /// <summary>
        /// 读取指定的流
        /// </summary>
        /// <typeparam name="HeadType"></typeparam>
        /// <param name="datas"></param>
        /// <returns></returns>
        public static XmlHeadBodyStream<HeadType> ReadAs<HeadType>(byte[] datas)
        {
            return ReadAs(new XmlHeadBodyStream<HeadType>()
            {
                Stream = new MemoryStream(datas),
            });
        }
        /// <summary>
        /// 读取目标对象关联的信息
        /// </summary>
        /// <typeparam name="HeadType"></typeparam>
        /// <param name="target"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private static XmlHeadBodyStream<HeadType> ReadAs<HeadType>(XmlHeadBodyStream<HeadType> target)
        {
            // 取得头部长度
            byte[] headLengthBytes = target.Read(0, HEAD_SIZE_FLAG_LENGTH);
            target.HeadLength = BitConverter.ToInt64(headLengthBytes, 0);

            if (target.HeadLength < 0)
            {
                throw new ArgumentException($"文件/流内部存储了无效的头部长度信息: {target.HeadLength}");
            }

            // 取得头部数据
            using Stream headData = new MemoryStream(target.Read(HEAD_SIZE_FLAG_LENGTH, target.HeadLength));
            XmlSerializer serializer = new(typeof(HeadType));
            target.Head = (HeadType)serializer.Deserialize(headData);

            return target;
        }


        /// <summary>
        /// 保存文件为
        /// </summary>
        /// <typeparam name="HeadType"></typeparam>
        /// <param name="fileName"></param>
        /// <param name="head"></param>
        /// <param name="bodyStream"></param>
        public static void Save<HeadType>(string fileName, HeadType head, Stream bodyStream)
        {
            FileInfo File = new(fileName);

            // 打开文件
            using FileStream fileStream = File.Open(FileMode.Create);

            // 写入流
            WriteToStream(fileStream, head, bodyStream);
        }

        /// <summary>
        /// 将数据写入流中
        /// </summary>
        /// <typeparam name="HeadType"></typeparam>
        /// <param name="stream"></param>
        /// <param name="head"></param>
        /// <param name="bodyStream"></param>
        public static void WriteToStream<HeadType>(Stream stream, HeadType head, Stream bodyStream)
        {
            // 头部数据转为byte流, 并取得长度
            using MemoryStream memoryStream = new();
            using XmlWriter writer = XmlWriter.Create(memoryStream, new XmlWriterSettings()
            {
                Indent = true,
                NewLineOnAttributes = true,
            });
            XmlSerializer serializer = new(typeof(HeadType));
            serializer.Serialize(writer, head);
            long headLength = memoryStream.Length;

            long positionNow = stream.Position;
            // 写入长度信息
            stream.Write(BitConverter.GetBytes(headLength));
            // 写入头部数据
            memoryStream.Seek(0, SeekOrigin.Begin);
            int temp;
            for (int i = 0; i < headLength && (temp = memoryStream.ReadByte()) >= 0; i++)
            {
                stream.WriteByte((byte)temp);
            }
            // 写入主体数据
            if (bodyStream != null)
            {
                stream.Seek(positionNow + headLength + HEAD_SIZE_FLAG_LENGTH, SeekOrigin.Begin);
                bodyStream.Seek(0, SeekOrigin.Begin);
                bodyStream.CopyTo(stream);
            }
        }
    }
}
