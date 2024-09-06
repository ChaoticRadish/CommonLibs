using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Structure.Pool
{
    /// <summary>
    /// 整型数字池, 可以指定使用范围, 从范围的最小值开始提取值; 如果有被弃用的数值, 从被弃用的数值里取值, 否则取下一个位置的值. (取值范围包含头尾)
    /// </summary>
    public class IntPool
    {
        #region 构造函数
        public IntPool() : this(int.MinValue, int.MaxValue) { }
        public IntPool(int min) : this(min, int.MaxValue) { }
        public IntPool(int min, int max)
        {
            if (min > max)
            {
                Max = min;
                Min = max;
            }
            else if (min == max)
            {
                if (min == int.MinValue)
                {
                    Min = min;
                    Max = min + 1;
                }
                else if (max == int.MaxValue)
                {
                    Max = max;
                    Min = max - 1;
                }
                else
                {
                    Min = min;
                    Max = max + 1;
                }
            }
            else
            {
                Max = max;
                Min = min;
            }
            GeneralPush = Min;
        }
        #endregion

        #region 属性
        /// <summary>
        /// 最小值
        /// </summary>
        public int Min { get; private set; }
        /// <summary>
        /// 最大值
        /// </summary>
        public int Max { get; private set; }
        /// <summary>
        /// 没有弃用值的情况下将会取出的数字
        /// </summary>
        public int GeneralPush { get; private set; }
        /// <summary>
        /// 是否用完了所有的数字
        /// </summary>
        public bool IsExhaust
        {
            get
            {
                return GeneralPush == Max && DiscardQueue.Count == 0;
            }
        }
        #endregion

        #region 子对象
        /// <summary>
        /// 弃用的数字们
        /// </summary>
        protected Queue<int> DiscardQueue = new Queue<int>();
        /// <summary>
        /// 被弃用的数字的数量
        /// </summary>
        public int DiscardCount { get => DiscardQueue.Count; }
        #endregion

        #region 控制
        /// <summary>
        /// 取值
        /// </summary>
        /// <returns></returns>
        public int Get()
        {
            if (DiscardQueue.Count == 0)
            {// 没有弃用的值
                int output = GeneralPush;
                if (GeneralPush < Max)
                {
                    GeneralPush++;
                }
                return output;
            }
            else
            {// 有弃用的值
                return DiscardQueue.Dequeue();
            }
        }
        /// <summary>
        /// 弃用一个数值, 需要确保它还没被弃用, 并且比 没有弃用值的情况下将会取出的数字值 小
        /// </summary>
        /// <param name="value"></param>
        public void Discard(int value)
        {
            if (value >= Min && value < GeneralPush && !DiscardQueue.Contains(value))
            {
                DiscardQueue.Enqueue(value);
            }
        }
        /// <summary>
        /// 重置
        /// </summary>
        public void Reset()
        {
            GeneralPush = Min;
            DiscardQueue.Clear();
        }
        #endregion

        #region 循环
        /// <summary>
        /// 被弃用的值执行什么的委托
        /// </summary>
        /// <param name="value"></param>
        public delegate void DiscardDoDelegate(int value);
        /// <summary>
        /// 所有被弃用的值执行操作
        /// </summary>
        public void AllDiscardDo(DiscardDoDelegate doWhat)
        {
            if (doWhat == null)
            {
                return;
            }
            int[] discards = DiscardQueue.ToArray();
            foreach (int i in discards)
            {
                doWhat.Invoke(i);
            }
        }

        #endregion
    }
}
