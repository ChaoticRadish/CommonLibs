using ChaoticKit.Interfaces.IO;
using ChaoticKit.Log;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaoticKit.IO
{
    /// <summary>
    /// 基于本地文件实现, 使用 <see cref="CustomTempFileDir"/> 用作临时文件文件夹的帮助类
    /// </summary>
    public static class TempFileHelper
    {
        #region Manager
        /// <summary>
        /// 获取一个基于 <see cref="TempFileHelper.NewTempFile"/> 的临时文件管理器
        /// </summary>
        /// <remarks>
        /// 调用该管理器创建的临时文件将在释放管理器时被释放
        /// </remarks>
        public static ITempFileManager GetDefaultManager()
        {
            return new tempFileManager_HelperImpl();
        }
        private class tempFileManager_HelperImpl : ITempFileManager<TempFile>
        {
            private List<int> ids = [];

            public TempFile NewOne()
            {
                var output = NewTempFile();
                int id = output.Id;
                ids.Add(id);
                return output;
            }
            public void Dispose()
            {
                _ = ReleaseTempFileAsync(ids);
            }

            ITempFile ITempFileManager.NewOne()
            {
                return NewOne();
            }
        }

        #endregion


        #region 设定

        /// <summary>
        /// 自定义的临时文件文件夹, 如果为 null, 将使用 <see cref="Path.GetTempFileName"/> 创建临时文件
        /// </summary>
        public static string? CustomTempFileDir { get; set; }

        #endregion

        #region 日志

        public static ILevelLogger? Logger { get; set; }

        #endregion

        public struct TempFile : ITempFile, IDisposable
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
            /// 文件完整路径
            /// </summary>
            public string FileDescription => System.IO.Path.GetFullPath(Path);

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
            public readonly Stream OpenRead()
            {
                return new FileStream(Path, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);
            }
            public readonly Stream OpenWrite()
            {
                return new FileStream(Path, FileMode.OpenOrCreate, FileAccess.Write);
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
        public static void ReleaseTempFile(int id)
        {
            if (tempFiles.TryRemove(id, out var exist))
            {
                try
                {
                    File.Delete(exist.Path);
                }
                catch (Exception ex)
                {
                    Logger?.Error($"释放临时文件发生异常: [{exist.Id}] {exist.Path}", ex);
                    throw;
                }
                Logger?.Info($"释放临时文件: [{exist.Id}] {exist.Path}");
            }
            
        }
        /// <summary>
        /// 释放临时文件, 将关闭文件流, 并将其删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Task ReleaseTempFileAsync(int id)
        {
            return Task.Run(() => ReleaseTempFile(id));
        }
        /// <summary>
        /// 批量释放临时文件, 将关闭文件流, 并将其删除
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static Task ReleaseTempFileAsync(IEnumerable<int> ids)
        {
            return Task.Run(() =>
            {
                foreach (var id in ids)
                {
                    ReleaseTempFile(id);
                }
            });
        }
        #endregion

    }

    /// <summary>
    /// 包装 <see cref="TempFileHelper"/> 从而实现为 <see cref="ITempFileManager{T}"/> 接口
    /// </summary>
    /// <remarks>
    /// 释放时, 主动通过 <see cref="TempFileHelper"/> 释放 <see cref="CreatedByThis"/> 中的 Id
    /// </remarks>
    public class TempFileManagerOfHelper : ITempFileManager<TempFileHelper.TempFile>
    {

        /// <summary>
        /// 通过当前实例获取的临时文件 ID 集合
        /// </summary>
        /// <remarks>
        /// 获取的是获取的时刻的所有 ID 拷贝而得到的集合, 如果有变更, 则不会反映到该集合内
        /// </remarks>
        public IReadOnlySet<int> CreatedByThis => _createdByThis.ToHashSet();
        private ConcurrentBag<int> _createdByThis { get; } = new();

        public TempFileHelper.TempFile NewOne()
        {
            var output = TempFileHelper.NewTempFile();
            _createdByThis.Add(output.Id);
            return output;
        }

        ITempFile ITempFileManager.NewOne()
        {
            return NewOne();
        }

        #region 释放   
        private bool disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                    var ids = CreatedByThis;
                    foreach (var id in ids)
                    {
                        TempFileHelper.ReleaseTempFile(id);
                    }
                }

                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

        // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        ~TempFileManagerOfHelper()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
}

        #endregion

    }
}
