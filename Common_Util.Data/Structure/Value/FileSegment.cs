using Common_Util.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Structure.Value
{
    /// <summary>
    /// 可以表示文件中的某一片段的类型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IFileSegment<T> where T : struct
    {
        /// <summary>
        /// 文件路径
        /// </summary>
        string FullName { get; }
        /// <summary>
        /// 片段数据在文件中对应的起点
        /// </summary>
        T Start { get; }
        /// <summary>
        /// 片段长度
        /// </summary>
        T Length { get; }
        
    }
    public static class IFileSegmentExtensions
    {
        /// <summary>
        /// 以默认参数打开流
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="segment"></param>
        /// <param name="valueConvert">将数据转换为 long 值的方法</param>
        /// <returns></returns>
        public static Stream OpenStream<T>(this IFileSegment<T> segment, Func<T, long> valueConvert)
            where T : struct
        {
            return openStream(segment.FullName, valueConvert(segment.Start), valueConvert(segment.Start));
        }
        /// <summary>
        /// 以默认参数打开流
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public static Stream OpenStream(this IFileSegment<long> segment)
        {
            return openStream(segment.FullName, segment.Start, segment.Length);
        }
        public static Stream OpenStream(this IFileSegment<uint> segment)
        {
            return openStream(segment.FullName, segment.Start, segment.Length);
        }
        public static Stream OpenStream(this IFileSegment<int> segment)
        {
            return openStream(segment.FullName, segment.Start, segment.Length);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Stream openStream(string fileName, long start, long length)
        {
            var fs = File.Open(fileName, FileMode.Open);
            var ows = new OffsetWrapperStream(fs, start, length)
            {
                DisposeSource = true,
            };
            return ows;
        }
    }
    /// <summary>
    /// 表示文件中的某一片段
    /// </summary>
    public struct FileSegment : IFileSegment<uint>
    {
        public FileSegment()
        {
            FullName = string.Empty;
            Start = 0;
            Length = 0;
        }

        /// <summary>
        /// 实例化一个文件片段信息的结构体, 其范围包含整个文件
        /// </summary>
        /// <param name="fileName"></param>
        public FileSegment(string fileName)
        {
            FileInfo fileInfo = new FileInfo(fileName);
            FullName = fileInfo.FullName;
            Start = 0;
            Length = (uint)fileInfo.Length;
        }
        /// <summary>
        /// 实例化一个文件片段信息的结构体, 其范围包含整个文件
        /// </summary>
        /// <param name="fileInfo"></param>
        public FileSegment(FileInfo fileInfo)
        {
            FullName = fileInfo.FullName;
            Start = 0;
            Length = (uint)fileInfo.Length;
        }


        public string FullName { get; set; }

        public uint Start { get; set; }

        public uint Length { get; set; }

        public readonly override string ToString()
        {
            return $"[{FullName}](S:{Start};E:{Start + Length};L:{Length})";
        }

    }
    public struct FileSegment<T> : IFileSegment<T>
        where T : struct
    {

        public string FullName { get; set; }

        public T Start { get; set; }

        public T Length { get; set; }
    }
}
