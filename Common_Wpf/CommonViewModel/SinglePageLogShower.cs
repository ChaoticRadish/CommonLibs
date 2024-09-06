using Common_Util.Data.Wrapped;
using Common_Util.Log;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Common_Wpf.CommonViewModel
{
    /// <summary>
    /// 单页显示的日志显示器ViewModel, 输入时不支持过滤分类级别等
    /// <para>一般用于少量, 单一模块或事务日志的显示</para>
    /// </summary>
    public class SinglePageLogShower : ILogger, INotifyPropertyChanged
    {
        public SinglePageLogShower()
        {
            CacheDealTimer = new()
            {
                Enabled = true,
                Interval = 50,
                AutoReset = true,
            };
            CacheDealTimer.Elapsed += CacheDealTimer_Elapsed;
            // CacheDealTimer.Start();
        }


        #region 常量
        /// <summary>
        /// 默认容量
        /// </summary>
        public const int DEAFULT_CAPACITY = 500;

        public const int DEFAULT_MAX_DEAL_COUNT = 20;

        public const int DEFAULT_BATCH_DEAL_THRESHOLD = 10;
        #endregion

        #region 配置
        /// <summary>
        /// 日志保存容量, 超出容量的日志将被丢弃
        /// </summary>
        public int Capacity 
        {
            get => capacity;
            set
            {
                capacity = Math.Max(1, value);
                OnPropertyChanged();
            }
        }
        private int capacity = DEAFULT_CAPACITY;

        /// <summary>
        /// 在一次处理过程中, 最多处理多少条缓存日志数据
        /// </summary>
        public int MaxDealCount
        {
            get => maxDealCount;
            set
            {
                maxDealCount = Math.Max(1, value);
                OnPropertyChanged();
            }
        }
        private int maxDealCount = DEFAULT_MAX_DEAL_COUNT;

        /// <summary>
        /// 在一次处理过程中, 触发批量添加的阈值
        /// </summary>
        public int BatchDealThreshold
        {
            get => batchDealThreshold;
            set
            {
                batchDealThreshold = Math.Max(1, value);
                OnPropertyChanged();
            }
        }
        private int batchDealThreshold = DEFAULT_BATCH_DEAL_THRESHOLD;
        #endregion

        #region 日志数据
        public SuspendableObservableCollection<LogData> ShowingDatas { get; } = [];
        private readonly Queue<LogData> CacheDatas = [];
        /// <summary>
        /// 当前显示的最后一个数据
        /// </summary>
        public LogData? ShowingLast => ShowingDatas.LastOrDefault();
        #endregion


        #region 操作


        #region 日志输入
        public void Log(LogData log)
        {
            CacheDatas.Enqueue(log);
        }
        #endregion

        /// <summary>
        /// 清理所有日志
        /// </summary>
        public void Clear()
        {
            lock (CacheDealLocker)
            {
                CacheDatas.Clear();

                ShowingDatas.Clear();
            }
        }
        #endregion

        #region 缓存队列处理计时器, 降低短时间内高频率更新日志显示数据造成卡顿
        private System.Timers.Timer CacheDealTimer;
        private readonly object CacheDealLocker = new();
        private List<LogData> DealCaches = [];

        private void CacheDealTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            lock (CacheDealLocker)
            {
                int cacheCount = CacheDatas.Count;
                if (cacheCount == 0) return;

                int readyDealCount = Math.Min(Math.Max(cacheCount / 20, 1), MaxDealCount);
                int dealIndex = 0;
                while (dealIndex < readyDealCount && CacheDatas.TryDequeue(out LogData? data))
                {
                    dealIndex++;
                    DealCaches.Add(data);
                }

                RunInUiThread(AddDealCacheToShowing);

                DealCaches.Clear();
            }

            ShowLastData();
        }
        private void AddDealCacheToShowing()
        {
            int changeCount = DealCaches.Count + (DealCaches.Count + ShowingDatas.Count - Capacity);
            bool suspend = changeCount > BatchDealThreshold;
            if (suspend)
            {
                ShowingDatas.SuspendUpdate();

                ShowingDatas.AddRange(DealCaches);
                RemoveOverCapacity();

                ShowingDatas.ResumeUpdate();
            }
            else
            {
                ShowingDatas.AddRange(DealCaches);
                RemoveOverCapacity();
            }
        }
        private void RemoveOverCapacity()
        {
            while (ShowingDatas.Count > Capacity)
            {
                ShowingDatas.RemoveAt(0);
            }
        }
        #endregion

        #region 必要外部操作
        /// <summary>
        /// 将一个操作放在UI线程中运行, 此操作在锁住处理线程的状态下执行, 会造成处理线程阻塞
        /// </summary>
        public required Action<Action> RunInUiThread { get; init; }
        /// <summary>
        /// 显示当前最后一个数据, 有数据被处理时就会调用, 不锁住任何东西, 可以考虑以降频, 异步的方式实现
        /// </summary>
        public required Action ShowLastData { get; init; }
        #endregion

        #region 属性变更

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    
}
