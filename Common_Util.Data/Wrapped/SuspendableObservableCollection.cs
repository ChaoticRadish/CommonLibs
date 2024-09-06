using Common_Util.Data.Constraint;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Wrapped
{
    /// <summary>
    /// 可被暂挂的 <see cref="ObservableCollection{T}"/>. 实现了批量添加的方法
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SuspendableObservableCollection<T> : ObservableCollection<T>, ISuspendUpdate
    {
        /// <summary>
        /// 用于测试的ID值
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        /// 当前允许更新 (未被挂起)
        /// </summary>
        private bool updatesEnabled = true;
        /// <summary>
        /// 挂起过程中, 集合发生了变化
        /// </summary>
        private bool collectionChanged = false;
        /// <summary>
        /// 挂起过程中, 集合的变化情况
        /// </summary>
        private List<NotifyCollectionChangedAction> actionsInSuspended = new();

        public void ResumeUpdate()
        {
            updatesEnabled = true;
            if (collectionChanged)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
            ResumeUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void SuspendUpdate()
        {
            updatesEnabled = false;
            SuspendUpdated?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (updatesEnabled)
            {
                base.OnCollectionChanged(e);
            }
            else
            {
                collectionChanged = true;
                actionsInSuspended.Add(e.Action);
            }
        }

        #region 事件
        public event EventHandler? SuspendUpdated;
        public event EventHandler? ResumeUpdated;
        #endregion

        #region 通知
        /// <summary>
        /// 触发集合变更的通知 (Action:<see cref="NotifyCollectionChangedAction.Reset"/>)
        /// </summary>
        public void TriggerCollectionChanged()
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        #endregion

        #region 批量添加

        /// <summary>
        /// 批量添加
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="suspend">是否在遍历前后挂起与恢复</param>
        public void AddRange(IEnumerable<T> collection, bool suspend = true)
        {
            if (!collection.Any()) return;
            if (suspend)
            {
                SuspendUpdate();
                foreach (T item in collection)
                {
                    Add(item);
                }
                ResumeUpdate();
            }
            else
            {
                foreach (T item in collection)
                {
                    Add(item);
                }
            }
        }
        /// <summary>
        /// 批量添加, 讲输入对象逐一添加, 不挂起
        /// </summary>
        /// <param name="collection"></param>
        public void AddRange(params T[] collection)
        {
            AddRange(collection, false);
        }
        #endregion

    }
}
