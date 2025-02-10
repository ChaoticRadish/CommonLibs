using Common_Util.Interfaces.Behavior;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Structure.Linear
{
    /// <summary>
    /// 循环容器, 记录当前索引位置, 可添加项并推荐索引位置, 当索引超出容器范围, 将回到首部
    /// </summary>
    /// <remarks>
    /// 可以实例化为 0 容量, 此时 <see cref="Add(T)"/> 操作将什么都不发生 <br/>
    /// 继承 <see cref="IEnumerable{T}"/>, 从距离当前索引位置最远处开始遍历到当前位置
    /// </remarks>
    public class CycleContainer<T> : IAddable<T>, IEnumerable<T>
    {
        #region 数据与状态
        private T[] Datas { get; init; }
        public int Capacity { get => Datas.Length; }
        /// <summary>
        /// 当前索引位置
        /// </summary>
        private int CurrentIndex { get; set; } = 0;
        #endregion

        #region 实例化
        private CycleContainer(T[] array) 
        {
            Datas = array;
            CurrentIndex = Datas.Length - 1;
        }

        /// <summary>
        /// 使用 <paramref name="values"/> 实例化容器, 容量为其长度, 初始值为取到的值
        /// </summary>
        /// <remarks>
        /// 索引位置会移动到 <paramref name="values"/> 最后一项处
        /// </remarks>
        /// <param name="values"></param>
        public CycleContainer(IEnumerable<T> values) 
        {
            Datas = values.ToArray();
            CurrentIndex = Datas.Length - 1;
        }
        /// <summary>
        /// 实例化大小为 <paramref name="capacity"/> 的容器, 并使用 <paramref name="defaultValue"/> 填充
        /// </summary>
        /// <param name="capacity"></param>
        /// <param name="defaultValue"></param>
        public CycleContainer(int capacity, T defaultValue = default!)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(capacity, 0);

            Datas = new T[capacity];
            for (int i = 0; i < Datas.Length; i++)
            {
                Datas[i] = defaultValue;
            }
        }

        /// <summary>
        /// 实例化一个包装数据对象 <paramref name="target"/> 的循环容器
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static CycleContainer<T> Wrapper(T[] target)
        {
            return new CycleContainer<T>(target);
        }

        #endregion

        #region 操作

        /// <summary>
        /// 推进索引位置到下一个点, 然后将 <paramref name="item"/> 赋值到对应位置
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            if (Capacity == 0) return;
            CurrentIndex = (CurrentIndex + 1) % Capacity;
            Datas[CurrentIndex] = item;
        }

        /// <summary>
        /// 从当前索引的最远端开始遍历, 直到当前位置, 将遍历整个容器
        /// </summary>
        /// <returns></returns>
        public IEnumerable<T> FarToCurrent()
        {
            return Peek(Capacity);
        }
        /// <summary>
        /// 窥视当前索引位置处的项
        /// </summary>
        /// <returns></returns>
        public T Peek()
        {
            if (Capacity == 0) throw new InvalidOperationException("循环容器的容量为 0, 无法执行此操作! ");
            return Datas[CurrentIndex];
        }
        /// <summary>
        /// 窥视循环顶部的 <paramref name="count"/> 个项, 从距离当前索引 <paramref name="count"/> 距离处开始遍历, 直到当前位置
        /// </summary>
        /// <param name="count">如果此值大于 <see cref="Capacity"/>, 则取后者的值</param>
        /// <returns></returns>
        public IEnumerable<T> Peek(int count)
        {
            if (Capacity == 0) yield break;
            count = Math.Min(count, Capacity);
            for (int i = Capacity - count; i < Capacity; i++)
            {
                yield return Datas[(CurrentIndex + 1 + i) % Capacity];
            }
        }

        /// <summary>
        /// 使用 <paramref name="value"/> 清空容器
        /// </summary>
        /// <remarks>
        /// 索引位置会被重置到内置数组首部
        /// </remarks>
        /// <param name="value"></param>
        public void Clear(T value = default!)
        {
            CurrentIndex = 0;
            for (int i = 0; i < Capacity; i++)
            {
                Datas[i] = value;
            }
        }
        #endregion

        public IEnumerator<T> GetEnumerator()
        {
            return FarToCurrent().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
