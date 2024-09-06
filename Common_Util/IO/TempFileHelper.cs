using Common_Util.Log;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Common_Util.IO.TempFileHelper;

namespace Common_Util.IO
{
    public class TempFileHelper
    {

        #region 设定

        /// <summary>
        /// 自定义的临时文件文件夹, 如果为 null, 将使用 <see cref="Path.GetTempFileName"/> 创建临时文件
        /// </summary>
        public static string? CustomTempFileDir { get; set; }

        #endregion

        #region 日志

        public static ILevelLogger? Logger { get; set; }

        #endregion

        public struct TempFile : IDisposable
        {
            public int Id { get; set; }



            /// <summary>
            /// 所属临时文件文件夹
            /// </summary>
            public string TempFileDir { get; set; }
            /// <summary>
            /// 文件路径
            /// </summary>
            public string Path { get; set; }

            /// <summary>
            /// 调用 <see cref="ReleaseTempFileAsync(int)"/> 将临时文件从管理中移除, 同时删除对应的临时文件
            /// </summary>
            public readonly void Dispose()
            {
                ReleaseTempFileAsync(Id);
            }

            /// <summary>
            /// 以默认的参数打开文件流
            /// </summary>
            /// <returns></returns>
            public readonly FileStream OpenStream()
            {
                return new(Path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            }
        }

        #region ID管理

        private static int CurrentId;

        private static int NextId()
        {
            return Interlocked.Increment(ref CurrentId);
        }

        #endregion

        #region 临时文件名生成

        private static readonly Random.RandomCharSplicer TempFileRandomCodeCreator = new(Random.RandomStringHelper.EnglishUppercases);
        /// <summary>
        /// 生成位于自定义临时文件文件夹内的一个含随机码的路径
        /// </summary>
        /// <returns></returns>
        private static string NextRandomCustomTempFileName()
        {
            string output;
            do
            {
                output = Path.Combine(CustomTempFileDir!, $"{DateTime.Now:yyyyMMddHHmmssffff}_{TempFileRandomCodeCreator.Get(8)}.tmp");
            } while (Path.Exists(output));

            File.Create(output).Dispose();

            return output;
        }

        /// <summary>
        /// 根据设定获取一个随机文件名
        /// </summary>
        /// <returns></returns>
        private static string GetTempFileName()
        {
            if (CustomTempFileDir != null)
            {
                return NextRandomCustomTempFileName();
            }
            else
            {
                return Path.GetTempFileName();
            }

        }
        #endregion

        #region 操作

        private static ConcurrentDictionary<int, TempFile> tempFiles = [];
        private static readonly object newLocker = new();

        /// <summary>
        /// 创建一个新的临时文件, 并打开文件流
        /// </summary>
        /// <returns></returns>
        public static TempFile NewTempFile()
        {
            lock (newLocker)
            {
                int id = NextId();
                string tempDir = CustomTempFileDir ?? Path.GetTempPath();
                string tempFile = GetTempFileName();

                TempFile output = new()
                {
                    Id = id,
                    TempFileDir = tempDir,
                    Path = tempFile,
                };

                if (tempFiles.TryAdd(id, output))
                {
                    Logger?.Info($"创建临时文件: [{id}] {tempFile}");
                }
                else
                {
                    throw new Exception($"意料之外的失败! 创建临时文件信息后, 未能将其添加到帮助类内的管理中! ");
                }

                return output;
            }

        }
        /// <summary>
        /// 创建一个新的临时文件, 并打开文件流
        /// </summary>
        /// <returns></returns>
        public static Task<TempFile> NewTempFileAsync()
        {
            return Task.Run(NewTempFile);
        }

        /// <summary>
        /// 释放临时文件, 将关闭文件流, 并将其删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected static void ReleaseTempFile(int id)
        {
            if (tempFiles.TryRemove(id, out var exist))
            {
                File.Delete(exist.Path);
                Logger?.Info($"释放临时文件: [{exist.Id}] {exist.Path}");
            }
            
        }
        /// <summary>
        /// 释放临时文件, 将关闭文件流, 并将其删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected static Task ReleaseTempFileAsync(int id)
        {
            return Task.Run(() => ReleaseTempFile(id));
        }
        #endregion

    }
}
