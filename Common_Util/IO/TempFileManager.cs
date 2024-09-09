using Common_Util.Interfaces.IO;
using Common_Util.Log;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.IO
{
    /// <summary>
    /// 默认实现的临时文件管理器, 会在指定的目录里维护临时文件
    /// </summary>
    public sealed class TempFileManager : ITempFileManager<ITempFile>
    {
        /// <summary>
        /// 创建使用指定目录管理临时文件的管理器
        /// </summary>
        /// <remarks>
        /// 实例化时, 如果文件夹不存在, 将创建, 如果已存在, <b>将清空!</b> <br/>
        /// 不会检查路径是否可用, 所以可能会抛出异常
        /// </remarks>
        /// <param name="dirPath"></param>
        public TempFileManager(string dirPath)
        {
            bool createFlag = false;
            if (!Directory.Exists(dirPath)) 
            {
                Directory.CreateDirectory(dirPath);
                createFlag = true;
            }
            DirectoryInfo directoryInfo = new DirectoryInfo(dirPath);
            DirectoryFullPath = directoryInfo.FullName;
            if (!createFlag)
            {
                DirectoryHelper.Clear(directoryInfo);
            }
        }

        #region 设置
        /// <summary>
        /// 临时文件目录的完整路径 (绝对路径)
        /// </summary>
        public string DirectoryFullPath { get; }
        /// <summary>
        /// 日志输出接口, 主要输出临时文件的新增和释放
        /// </summary>
        public ILevelLogger? Logger { get; set; }
        /// <summary>
        /// 是否异步释放文件, 此设置仅对释放单个临时文件时生效. 在释放管理器时必定是同步的
        /// </summary>
        public bool AsyncRelease { get; set; } = false;
        #endregion

        #region ID管理

        private static int CurrentId;

        private static int NextId()
        {
            return Interlocked.Increment(ref CurrentId);
        }

        #endregion

        #region 文件名生成

        private readonly Random.RandomCharSplicer TempFileRandomCodeCreator = new(Random.RandomStringHelper.EnglishUppercases);
        /// <summary>
        /// 生成位于自定义临时文件文件夹内的一个含随机码的路径
        /// </summary>
        /// <returns></returns>
        private string NextRandomCustomTempFileName()
        {
            string output;
            string timeStr = DateTime.Now.ToString("yyyyMMddHHmmssffff");
            do
            {
                output = Path.Combine(DirectoryFullPath, $"{timeStr}_{TempFileRandomCodeCreator.Get(8)}.tmp");
            } while (Path.Exists(output));

            File.Create(output).Dispose();

            return output;
        }

        #endregion


        #region 操作


        private ConcurrentDictionary<int, TempFile> tempFiles = [];
        private readonly object newLocker = new();
        private readonly object removeLocker = new();

        public ITempFile NewOne()
        {
            lock (newLocker)
            {
                int id = NextId();
                string tempFile = NextRandomCustomTempFileName();

                TempFile output = new()
                {
                    Id = id,
                    Path = tempFile,
                    Manager = this,
                };

                if (tempFiles.TryAdd(id, output))
                {
                    Logger?.Info($"创建临时文件: [{id}] {tempFile}");
                }
                else
                {
                    throw new Exception($"意料之外的失败! 创建临时文件信息后, 未能将其添加到管理器中! ");
                }

                return output;
            }

        }

        /// <summary>
        /// 释放临时文件, 将关闭文件流, 并将其删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private void ReleaseTempFile(int id)
        {
            if (AsyncRelease)
            {
                _ = Task.Run(() => _releaseTempFile(id));
            }
            else
            {
                _releaseTempFile(id);
            }
        }
        private void _releaseTempFile(int id)
        {
            lock (removeLocker)
            {
                if (tempFiles.TryRemove(id, out var exist))
                {
                    File.Delete(exist.Path);
                    Logger?.Info($"释放临时文件: [{exist.Id}] {exist.Path}");
                }
            }
        }

        public void Dispose()
        {
            lock (removeLocker)
            {
                Logger?.Info($"释放所有临时文件 ({tempFiles.Count})");
                foreach (TempFile tempFile in tempFiles.Values)
                {
                    File.Delete(tempFile.Path);
                    Logger?.Info($"释放临时文件: [{tempFile.Id}] {tempFile.Path}");
                }
                tempFiles.Clear();
            }
        }


        #endregion

        private struct TempFile : ITempFile
        {
            public int Id { get; set; }
            public string Path { get; set; }

            public TempFileManager Manager { get; set; }

            public void Dispose()
            {
                Manager.ReleaseTempFile(Id);
            }
        }

    }
}
