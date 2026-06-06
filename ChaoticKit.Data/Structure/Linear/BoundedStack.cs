using ChaoticKit.Maths;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaoticKit.Data.Structure.Linear
{
    /// <summary>
    /// 有限容量栈
    /// </summary>
    /// <remarks>
    /// 当置入超过容量限制的元素时, 会抛弃栈底部的元素 <br/>
    /// 可遍历, 遍历时从栈底开始遍历到栈顶, 遍历过程不会导致栈的内容变更
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public class BoundedStack<T> : IEnumerable<T>
    {
        #region 实际存储
        private readonly T[] _items;
        private int _stackElementCount;
        private int _stackBottomIndex;

        /// <summary>
        /// 转换索引值为存储结构的索引, 不执行入参校验
        /// </summary>
        /// <param name="indexStartFromBottom">相对于栈底的索引 (从栈底开始到栈顶的索引)</param>
        /// <returns></returns>
        private int _indexConvert(int indexStartFromBottom)
        {
            return MathHelper.AddModuloUnchecked(_stackBottomIndex, indexStartFromBottom, Capacity);
        }
        private T _getItem(int index)
        {
            return _items[_indexConvert(index)];
        }
        private void _setItem(int index, T item)
        {
            _items[_indexConvert(index)] = item;
        }
        #endregion


        /// <summary>
        /// 获取当前栈中元素的数量
        /// </summary>
        public int Count => _stackElementCount;
        /// <summary>
        /// 获取栈的最大容量
        /// </summary>
        public int Capacity => _items.Length;
        /// <summary>
        /// 当前栈是否已满
        /// </summary>
        public bool IsFull => Count == Capacity;

        /// <summary>
        /// 初始化 BoundedStack 的新实例
        /// </summary>
        /// <param name="capacity">栈的最大容量</param>
        public BoundedStack(int capacity)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(capacity, 0);

            _items = new T[capacity];
        }

        /// <summary>
        /// 将对象插入到栈顶。
        /// </summary>
        /// <remarks>
        /// 如果栈已满，将自动移除栈底的元素。
        /// </remarks>
        public void Push(T item)
        {
            if (IsFull)
            {
                _stackBottomIndex = MathHelper.AddModuloUnchecked(_stackBottomIndex, 1, Capacity);
                _setItem(Count - 1, item);
            }
            else
            {
                _setItem(Count, item);
                _stackElementCount++;
            }
        }

        #region 操作
        /// <summary>
        /// 尝试弹出栈顶的元素
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool TryPop(out T item)
        {
            if (Count == 0)
            {
                item = default!;
                return false;
            }
            else
            {
                item = _pop();
                return true;
            }

        }
        /// <summary>
        /// 弹出并返回栈顶的元素
        /// </summary>
        public T Pop()
        {
            if (Count == 0)
                throw new InvalidOperationException("栈为空");
            return _pop();
        }
        private T _pop()
        {
            var index = _indexConvert(Count - 1);
            var item = _items[index];
            _items[index] = default!;
            _stackElementCount--;
            return item;
        }
        /// <summary>
        /// 尝试返回栈顶的元素
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool TryPeek(out T item)
        {
            if (Count == 0)
            {
                item = default!;
                return false;
            }
            else
            {
                item = _getItem(Count - 1);
                return true;
            }
        }
        /// <summary>
        /// 返回栈顶的元素但不移除
        /// </summary>
        public T Peek()
        {
            if (Count == 0)
                throw new InvalidOperationException("栈为空");
            return _getItem(Count - 1);
        }

        /// <summary>
        /// 清空栈
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < _items.Length; i++) 
                _items[i] = default!;
            _stackElementCount = 0;
            _stackBottomIndex = 0;
        }

        /// <summary>
        /// 确定栈中是否包含特定值
        /// </summary>
        public bool Contains(T item, IEqualityComparer<T>? equalityComparer = null)
        {
            equalityComparer ??= EqualityComparer<T>.Default;
            for (int i = 0; i < Count; i++)
            {
                var index = MathHelper.AddModuloUnchecked(_stackBottomIndex, i, Capacity);
                if (equalityComparer.Equals(item, _items[index])) return true;
            }
            return false;
        }
        #endregion

        #region 遍历
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                var index = MathHelper.AddModuloUnchecked(_stackBottomIndex, i, Capacity);
                yield return _items[index];
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion


    }
}
