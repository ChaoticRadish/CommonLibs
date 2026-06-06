using System;
using System.Collections;
using System.Collections.Generic;

namespace ChaoticKit.Data.Structure.Linear
{
    /// <summary>
    /// 自动扩展列表的默认实现
    /// <para>使用字典存储，支持任意非负索引访问</para>
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    public class AutoExpandList<T> : IAutoExpandList<T>, IList
    {
        private readonly Dictionary<int, T> _data;
        private int _maxIndex = -1;

        #region 构造函数

        /// <summary>
        /// 初始化 <see cref="AutoExpandList{T}"/> 的新实例
        /// </summary>
        public AutoExpandList()
        {
            _data = new Dictionary<int, T>();
            DefaultValue = default!;
        }

        /// <summary>
        /// 使用指定的默认值初始化 <see cref="AutoExpandList{T}"/> 的新实例
        /// </summary>
        /// <param name="defaultValue">访问不存在索引时的默认返回值</param>
        public AutoExpandList(T defaultValue)
        {
            _data = new Dictionary<int, T>();
            DefaultValue = defaultValue;
        }

        /// <summary>
        /// 使用指定的初始容量初始化 <see cref="AutoExpandList{T}"/> 的新实例
        /// </summary>
        /// <param name="capacity">初始容量, 仅在非负数时有效</param>
        public AutoExpandList(int capacity)
        {
            _data = capacity < 0 ? new() : new Dictionary<int, T>(capacity: capacity);
            DefaultValue = default!;
        }

        /// <summary>
        /// 使用指定的默认值和初始容量初始化 <see cref="AutoExpandList{T}"/> 的新实例
        /// </summary>
        /// <param name="defaultValue">访问不存在索引时的默认返回值</param>
        /// <param name="capacity">初始容量</param>
        public AutoExpandList(T defaultValue, int capacity)
        {
            _data = capacity < 0 ? new() : new Dictionary<int, T>(capacity: capacity);
            DefaultValue = defaultValue;
        }

        #endregion

        #region 属性

        /// <inheritdoc/>
        public T DefaultValue { get; }

        /// <inheritdoc/>
        public int Count => _maxIndex + 1;

        /// <inheritdoc/>
        public bool IsReadOnly => false;

        /// <inheritdoc/>
        public IEnumerable<int> SetIndexes => _data.Keys;

        #endregion

        #region 索引器

        /// <inheritdoc/>
        public T this[int index]
        {
            get
            {
                if (index < 0)
                    throw new ArgumentOutOfRangeException(nameof(index), "索引必须为非负数");

                return _data.TryGetValue(index, out var value) ? value : DefaultValue;
            }
            set
            {
                if (index < 0)
                    throw new ArgumentOutOfRangeException(nameof(index), "索引必须为非负数");

                _data[index] = value;
                if (index > _maxIndex)
                {
                    _maxIndex = index;
                }
            }
        }

        #endregion

        #region IAutoExpandList<T> 实现

        /// <inheritdoc/>
        public bool ContainsIndex(int index)
        {
            return index >= 0 && _data.ContainsKey(index);
        }

        #endregion

        #region IList<T> 实现

        /// <inheritdoc/>
        public void Add(T item)
        {
            _data[++_maxIndex] = item;
        }

        /// <inheritdoc/>
        public void Clear()
        {
            _data.Clear();
            _maxIndex = -1;
        }

        /// <inheritdoc/>
        public bool Contains(T item)
        {
            return _data.ContainsValue(item) || Equals(item, DefaultValue);
        }

        /// <inheritdoc/>
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex), "数组索引必须为非负数");
            if (array.Length - arrayIndex < Count)
                throw new ArgumentException("目标数组空间不足");

            for (int i = 0; i <= _maxIndex; i++)
            {
                array[arrayIndex + i] = this[i];
            }
        }

        /// <inheritdoc/>
        public int IndexOf(T item)
        {
            for (int i = 0; i <= _maxIndex; i++)
            {
                if (Equals(this[i], item))
                    return i;
            }
            return -1;
        }

        /// <inheritdoc/>
        public void Insert(int index, T item)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), "索引必须为非负数");

            for (int i = _maxIndex; i >= index; i--)
            {
                _data[i + 1] = _data.TryGetValue(i, out var v) ? v : DefaultValue;
            }
            _data[index] = item;
            _maxIndex++;
        }

        /// <inheritdoc/>
        public bool Remove(T item)
        {
            var index = IndexOf(item);
            if (index < 0)
                return false;

            RemoveAt(index);
            return true;
        }

        /// <inheritdoc/>
        public void RemoveAt(int index)
        {
            if (index < 0 || index > _maxIndex)
                return;

            _data.Remove(index);
            for (int i = index; i < _maxIndex; i++)
            {
                if (_data.TryGetValue(i + 1, out var v))
                {
                    _data[i] = v;
                    _data.Remove(i + 1);
                }
            }
            _maxIndex--;
        }

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i <= _maxIndex; i++)
            {
                yield return this[i];
            }
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        #region IList 实现

        /// <inheritdoc/>
        public bool IsFixedSize => false;

        /// <inheritdoc/>
        public bool IsSynchronized => false;

        /// <inheritdoc/>
        public object SyncRoot => _data;

        /// <inheritdoc/>
        object? IList.this[int index]
        {
            get => this[index];
            set => this[index] = (T)value!;
        }

        /// <inheritdoc/>
        int IList.Add(object? value)
        {
            Add((T)value!);
            return _maxIndex;
        }

        /// <inheritdoc/>
        bool IList.Contains(object? value)
        {
            return value is T t && Contains(t);
        }

        /// <inheritdoc/>
        int IList.IndexOf(object? value)
        {
            return value is T t ? IndexOf(t) : -1;
        }

        /// <inheritdoc/>
        void IList.Insert(int index, object? value)
        {
            Insert(index, (T)value!);
        }

        /// <inheritdoc/>
        void IList.Remove(object? value)
        {
            if (value is T t)
            {
                Remove(t);
            }
        }

        /// <inheritdoc/>
        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), "数组索引必须为非负数");

            for (int i = 0; i <= _maxIndex; i++)
            {
                array.SetValue(this[i], index + i);
            }
        }

        #endregion
    }
}
