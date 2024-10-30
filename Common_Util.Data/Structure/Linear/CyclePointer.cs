using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Structure.Linear
{
    /// <summary>
    /// 循环指针的接口
    /// </summary>
    /// <typeparam name="T">需实现自增, 自减, 以及比较大小的重载</typeparam>
    public interface ICyclePointer<T> 
        where T : IIncrementOperators<T>, IDecrementOperators<T>, IComparisonOperators<T, T, bool>

    {
        /// <summary>
        /// <see langword="true"/> =&gt; 增加操作为正向移动; <see langword="false"/> =&gt; 减少操作为正向移动
        /// </summary>
        bool IsAddPositive { get; }
        /// <summary>
        /// 当前位置
        /// </summary>
        T Current { get; }
        /// <summary>
        /// 指针范围的上限值 (包含)
        /// </summary>
        T Max { get; }
        /// <summary>
        /// 指针范围的下限值 (包含)
        /// </summary>
        T Min { get; }
        /// <summary>
        /// 从最小值移动到最大值, 再继续移动跳回最小值处, 所需要的自增次数
        /// </summary>
        int Length { get; }

        /// <summary>
        /// 根据传入参数 <paramref name="n"/> 的正负, 按特定的方向移动 <paramref name="n"/> 次
        /// </summary>
        /// <remarks>
        /// 0 =&gt; 不移动 <br/>
        /// 正数 =&gt; 正向移动 <br/>
        /// 负数 =&gt; 逆向移动 <br/>
        /// </remarks>
        /// <returns>移动后的新位置</returns>
        T Move(int n);

        /// <summary>
        /// 查看假如按传入参数 <paramref name="n"/> 的正负, 按特定的方向移动 <paramref name="n"/> 次后预期的新位置对应的值
        /// </summary>
        /// <remarks>
        /// 此方法不应该产生副作用
        /// </remarks>
        /// <param name="n"></param>
        /// <returns></returns>
        T Peek(int n);
    }

    /// <summary>
    /// 循环指针接口的默认实现
    /// </summary>
    /// <typeparam name="T">需实现自增, 自减, 以及比较大小的重载</typeparam>
    public struct CyclePointer<T> : ICyclePointer<T>
        where T : IIncrementOperators<T>, IDecrementOperators<T>, IComparisonOperators<T, T, bool>
    {
        public CyclePointer() : this(true, default!, default!, default!) { }
        public CyclePointer(bool isAddPositive, T start, T min, T max)
        {
            ArgumentNullException.ThrowIfNull(start);
            ArgumentNullException.ThrowIfNull(min);
            ArgumentNullException.ThrowIfNull(max);

            IsAddPositive = isAddPositive;
            (Max, Min) = max > min ? (max, min) : (min, max);
            if (start > Max)
            {
                Current = Max;
            }
            else if (start < Min) 
            {
                Current = Min;
            }
            else
            {
                Current = start;
            }
            if (Max == Min) Length = 1;
            else
            {
                T temp = Min;
                int index = 0;
                while (temp < Max)
                {
                    temp++;
                    index++;
                }
                if (temp > Max)
                {
                    throw new ArgumentException($"无法以自增手段, 从最小值 {Min} 移动到最大值 {Max}");
                }
                index++;
                Length = index;
            }

        }

        public bool IsAddPositive { get; init; }

        public T Current { get; private set; }

        public T Max { get; init; }

        public T Min { get; init; }

        public int Length { get; init; }

        public T Move(int n)
        {
            return Current = Peek(n);
        }

        public readonly T Peek(int n)
        {
            if (n == 0) return Current;
            bool needAdd = !(IsAddPositive ^ (n > 0));
            T temp = Current;
            n = Math.Abs(n);
            for (int i = 0; i < n; i++)
            {
                if (needAdd)
                {
                    if (temp == Max)
                    {
                        temp = Min;
                    }
                    else if (temp > Max)
                    {
                        throw new InvalidOperationException($"自增操作越过了最大值 {Max}");
                    }
                    else
                    {
                        temp++;
                    }
                }
                else
                {
                    if (temp == Min)
                    {
                        temp = Max;
                    }
                    else if (temp < Min)
                    {
                        throw new InvalidOperationException($"自减操作越过了最小值 {Min}");
                    }
                    else
                    {
                        temp--;
                    }
                }
            }
            return temp;
        }
    }
}
