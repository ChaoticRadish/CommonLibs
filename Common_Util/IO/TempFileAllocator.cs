using Common_Util.Interfaces.IO;
using Common_Util.Log;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Common_Util.IO.TempFileHelper;

namespace Common_Util.IO
{
    /// <summary>
    /// 基于需求大小, 决定是否创建新临时文件的临时文件分配器, 可申请临时文件片段 <see cref="TempFileSegment"/>
    /// </summary>
    /// <remarks>
    /// 释放时将释放由此分配器从临时文件管理器申请的所有临时文件 <br/>
    /// 释放由此分配器创建的临时文件片段时, 会判断对应的临时文件包含的片段是否全部释放, 如果已经全部释放, 则释放该临时文件
    /// </remarks>
    public sealed class TempFileAllocatorBaseSize : IDisposable
    {
        public TempFileAllocatorBaseSize(ITempFileManager tempFileManager) 
        {
            TempFileManager = tempFileManager;
            NewTempFile();
        }

        #region 设置

        /// <summary>
        /// 硬性的文件最大尺寸限制, 如果尝试申请超过此值的大小, 将抛出异常! 
        /// </summary>
        /// <remarks>
        /// 默认值为 <see cref="long.MaxValue"/> <br/>
        /// 如果设置了非正数, 将不作限制
        /// </remarks>
        public long MustMaxFileSizeLimit { get; init; } = long.MaxValue;

        /// <summary>
        /// 当申请的尺寸加上最后一个临时文件已分配出去的大小超过此值时, 将创建新临时文件再分配文件片段 
        /// </summary>
        /// <remarks>
        /// 默认值为 <see cref="long.MaxValue"/> <br/>
        /// 如果设置了非正数, 每次申请都会创建一个新的临时文件, 这将导致分配器失去意义! 
        /// </remarks>
        public long NewTempFileSizeLimit { get; init; } = long.MaxValue;
        /// <summary>
        /// 日志输出接口, 主要输出临时文件片段的申请和释放
        /// </summary>
        public ILevelLogger? Logger { get; init; }

        #endregion

        #region 状态
        /// <summary>
        /// 当前使用的临时文件管理器
        /// </summary>
        public ITempFileManager TempFileManager { get; }

        private Item CurrentTempFile;

        private long CurrentOffset;

        private readonly object NewTempFileLocker = new object();
        private int NextId;

        private ConcurrentDictionary<int, Item> Items = [];

        [MemberNotNull(nameof(CurrentTempFile))]
        private void NewTempFile()
        {
            lock (NewTempFileLocker)
            {
                ObjectDisposedException.ThrowIf(disposed, this);

                var tempFile = TempFileManager.NewOne();
                CurrentTempFile = new()
                {
                    Id = NextId,
                    TempFile = tempFile,
                };
                CurrentOffset = 0;

                if (!Items.TryAdd(NextId, CurrentTempFile))
                {
                    throw new Exception($"意料之外的失败! 使用管理器创建新临时文件后, 未能将其记录到分配器! ");
                }
                NextId++;
            }
        }

        #endregion



        #region 操作


        private readonly object AllocateLocker = new object();

        /// <summary>
        /// 申请分配指定长度的临时文件片段
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public TempFileSegment Allocate(long size)
        {
            lock (AllocateLocker)
            {
                ObjectDisposedException.ThrowIf(disposed, this);

                if (MustMaxFileSizeLimit > 0)
                {
                    if (size > MustMaxFileSizeLimit)
                    {
                        throw new ArgumentException($"需求的片段尺寸 {size} 已超过允许的最大尺寸 {MustMaxFileSizeLimit}", nameof(size));
                    }
                }

                if (CurrentTempFile.ReferenceCount == 0)
                {
                    // 如果当前的临时文件已经没有引用了, 则重置偏移量避免再次创建临时文件
                    CurrentOffset = 0;
                }

                if (NewTempFileSizeLimit <= 0 // 总是创建新临时文件
                    || size > NewTempFileSizeLimit // 需求尺寸就已经超过限制值了
                    || long.MaxValue - CurrentOffset < size // 相加会超过 long 的最大值
                    || CurrentOffset + size > NewTempFileSizeLimit  // 相加后超过了限制值
                    )
                {
                    NewTempFile();
                }

                int itemId = CurrentTempFile.Id;
                var tempFile = CurrentTempFile.TempFile;
                TempFileSegment output = new TempFileSegment()
                {
                    TempFile = tempFile,
                    Offset = CurrentOffset,
                    Length = size,
                    OnDisposing = () =>
                    {
                        lock (AllocateLocker)
                        {
                            if (Items.TryGetValue(itemId, out var item))
                            {
                                item.ReferenceCount--;
                                Logger?.Info($"释放片段, 管理项编号: {itemId}, 当前引用数量: {item.ReferenceCount}");
                                if (itemId == CurrentTempFile.Id)
                                {
                                    // Logger?.Info("最后一个临时文件管理项, 暂时无需释放文件");
                                }
                                else if (item.ReferenceCount == 0 && Items.Remove(itemId, out _))
                                {
                                    item.Dispose();
                                }
                            }
                            else
                            {
                                Logger?.Warning($"释放片段, 管理项编号: {itemId}, 未能取得管理项实例");
                            }
                        }
                    },
                };
                CurrentTempFile.ReferenceCount++;
                CurrentOffset += size;

                Logger?.Info($"申请取得片段, 管理项编号: {itemId}, 当前引用数量: {CurrentTempFile.ReferenceCount} - {output.SegmentBrief}");
                return output;
            }
        }

        private bool disposed = false;
        public void Dispose()
        {
            if (disposed) return;
            lock (NewTempFileLocker)
            {
                if (disposed) return;

                Logger?.Info($"释放从临时文件管理器申请的所有临时文件 ({Items.Count})");
                foreach (var item in Items.Values)
                {
                    item.Dispose();
                }
                Items.Clear();

                disposed = true;
            }
        }

        #endregion

        private class Item : IDisposable
        {

            /// <summary>
            /// 编号, 用于释放时查找对应项
            /// </summary>
            public required int Id { get; set; }

            public required ITempFile TempFile { get; set; }

            /// <summary>
            /// 引用计数, 目前有多少个文件片段引用了这一个临时文件
            /// </summary>
            public int ReferenceCount { get; set; }

            public void Dispose()
            {
                TempFile.Dispose();
            }
        }

    }

    /// <summary>
    /// 临时文件数据片段
    /// </summary>
    public struct TempFileSegment : IDisposable
    {
        /// <summary>
        /// 所属临时文件
        /// </summary>
        public ITempFile TempFile { get; set; }

        /// <summary>
        /// 临时文件的绝对路径
        /// </summary>
        public readonly string Path => TempFile.Path;

        /// <summary>
        /// 片段在文件中的偏移量 (起点)
        /// </summary>
        public long Offset { get; set; }

        /// <summary>
        /// 片段长度
        /// </summary>
        public long Length { get; set; }

        /// <summary>
        /// 释放资源操作
        /// </summary>
        public Action? OnDisposing { get; set; }

        public void Dispose()
        {
            OnDisposing?.Invoke();
        }

        /// <summary>
        /// 获取片段简易描述
        /// </summary>
        /// <returns></returns>
        public readonly string SegmentBrief
        {
            get => $"{System.IO.Path.GetFileName(Path)} [{Offset}+{Length}]";
        }
    }
}
