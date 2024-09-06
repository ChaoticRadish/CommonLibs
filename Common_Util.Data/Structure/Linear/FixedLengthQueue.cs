using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Structure.Linear
{
    /// <summary>
    /// 定长队列, 填满后再次置入将移除队列首部
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FixedLengthQueue<T> : Queue<T>
    {
        #region 属性
        public int Capacity { get; private set; }
        #endregion

        public T? Last { get; private set; }

        /// <summary>
        /// 实例化
        /// </summary>
        /// <param name="capacity">容量</param>
        public FixedLengthQueue(int capacity) : base()
        {
            Capacity = System.Math.Abs(capacity);
        }

        /// <summary>
        /// 放入一组项
        /// </summary>
        /// <param name="items"></param>
        public void Enqueue(params T[] items)
        {
            if (items != null)
            {
                foreach (T item in items)
                {
                    base.Enqueue(item);
                    Last = item;
                }
                dequeueUntilCapacity();
            }
        }
        /// <summary>
        /// 放入一个项
        /// </summary>
        /// <param name="item"></param>
        public new void Enqueue(T item)
        {
            base.Enqueue(item);
            Last = item;
            dequeueUntilCapacity();
        }
        /// <summary>
        /// 移除项, 直到 数量 == 容量
        /// </summary>
        private void dequeueUntilCapacity()
        {
            while (Count - Capacity > 0)
            {
                Dequeue();
            }
        }
    }
}
