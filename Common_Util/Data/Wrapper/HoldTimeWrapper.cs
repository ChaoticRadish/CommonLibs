using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Wrapper
{
    /// <summary>
    /// 记录包装对象时效的包装器
    /// </summary>
    public struct HoldTimeWrapper<T>
    {
        #region 实例化
        /// <summary>
        /// 实例化一个空的包装器
        /// </summary>
        public HoldTimeWrapper()
        {
            Value = default!;
            UpdateTime = null;
            Assigned = false;
        }
        /// <summary>
        /// 实例化一个未设置值的包装器, 使用 <paramref name="availableTime"/> 作为有效期
        /// </summary>
        /// <param name="availableTime"></param>
        public HoldTimeWrapper(TimeSpan availableTime)
        {
            Value = default!;
            UpdateTime = null;
            Assigned = false;
            AvailableTime = availableTime;
        }
        /// <summary>
        /// 使用当前时间作为更新时间, 实例化一个包装了 <paramref name="value"/> 的包装器, 其有效期为 <paramref name="availableTime"/>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="availableTime"></param>
        public HoldTimeWrapper(T value, TimeSpan availableTime)
        {
            Value = value;
            UpdateTime = DateTime.Now;
            Assigned = true;
            AvailableTime = availableTime;
        }
        #endregion

        /// <summary>
        /// 当前包装对象是否有效
        /// </summary>
        [MemberNotNullWhen(true, nameof(UpdateTime))]
        public readonly bool Available { get => Assigned && (DateTime.Now - UpdateTime.Value) < AvailableTime; }
        /// <summary>
        /// 当前是否已经设置值
        /// </summary>
        [MemberNotNullWhen(true, nameof(UpdateTime))]
        public bool Assigned { get; private set; }
        /// <summary>
        /// 当前包装的对象
        /// </summary>
        public T Value { get; private set; }
        /// <summary>
        /// 时效更新时间
        /// </summary>
        public DateTime? UpdateTime { get; private set; }
        /// <summary>
        /// 有效时间
        /// </summary>
        public TimeSpan AvailableTime { get; set; }

        #region 控制
        /// <summary>
        /// 重置对象, 当前包装的对象将被失效
        /// </summary>
        public void Reset()
        {
            UpdateTime = null;
            Value = default!;
            Assigned = false;
        }
        /// <summary>
        /// 设置当前包装的值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="updateTime"></param>
        public void Set(T value, DateTime? updateTime = null)
        {
            UpdateTime = updateTime ?? DateTime.Now;
            Value = value;
            Assigned = true;
        }
        #endregion
    }
}
