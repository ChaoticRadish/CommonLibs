using System;
using System.Collections;
using System.Collections.Generic;

namespace Common_Util.Data.Structure.Map
{
    /// <summary>
    /// 默认值Map
    /// <para>访问不存在的键时返回默认值，设置不存在的键时自动添加</para>
    /// </summary>
    /// <typeparam name="TKey">键类型</typeparam>
    /// <typeparam name="TValue">值类型</typeparam>
    public class DefaultValueMap<TKey, TValue> : IDefaultValueMap<TKey, TValue>, IDictionary
        where TKey : notnull
    {
        private readonly Dictionary<TKey, TValue> _data;

        #region 构造函数

        /// <summary>
        /// 初始化 <see cref="DefaultValueMap{TKey, TValue}"/> 的新实例
        /// </summary>
        /// <param name="defaultValue">访问不存在键时的默认返回值</param>
        public DefaultValueMap(TValue defaultValue)
        {
            _data = new Dictionary<TKey, TValue>();
            DefaultValue = defaultValue;
        }

        /// <summary>
        /// 使用指定的初始容量初始化 <see cref="DefaultValueMap{TKey, TValue}"/> 的新实例
        /// </summary>
        /// <param name="defaultValue">访问不存在键时的默认返回值</param>
        /// <param name="capacity">初始容量</param>
        public DefaultValueMap(TValue defaultValue, int capacity)
        {
            _data = new Dictionary<TKey, TValue>(capacity);
            DefaultValue = defaultValue;
        }

        /// <summary>
        /// 使用指定的比较器初始化 <see cref="DefaultValueMap{TKey, TValue}"/> 的新实例
        /// </summary>
        /// <param name="defaultValue">访问不存在键时的默认返回值</param>
        /// <param name="comparer">键比较器</param>
        public DefaultValueMap(TValue defaultValue, IEqualityComparer<TKey> comparer)
        {
            _data = new Dictionary<TKey, TValue>(comparer);
            DefaultValue = defaultValue;
        }

        #endregion

        #region 属性

        /// <inheritdoc/>
        public TValue DefaultValue { get; }

        /// <inheritdoc/>
        public ICollection<TKey> Keys => _data.Keys;

        /// <inheritdoc/>
        public ICollection<TValue> Values => _data.Values;

        /// <inheritdoc/>
        public int Count => _data.Count;

        /// <inheritdoc/>
        public bool IsReadOnly => false;

        #endregion

        #region 索引器

        /// <inheritdoc/>
        public TValue this[TKey key]
        {
            get
            {
                if (key == null)
                    throw new ArgumentNullException(nameof(key));

                return _data.TryGetValue(key, out var value) ? value : DefaultValue;
            }
            set
            {
                if (key == null)
                    throw new ArgumentNullException(nameof(key));

                if (_data.ContainsKey(key))
                {
                    _data[key] = value;
                }
                else
                {
                    _data.Add(key, value);
                }
            }
        }

        #endregion

        #region IDictionary<TKey, TValue> 实现

        /// <inheritdoc/>
        public void Add(TKey key, TValue value)
        {
            _data.Add(key, value);
        }

        /// <inheritdoc/>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)_data).Add(item);
        }

        /// <inheritdoc/>
        public void Clear()
        {
            _data.Clear();
        }

        /// <inheritdoc/>
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)_data).Contains(item);
        }

        /// <inheritdoc/>
        public bool ContainsKey(TKey key)
        {
            return _data.ContainsKey(key);
        }

        /// <inheritdoc/>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)_data).CopyTo(array, arrayIndex);
        }

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        /// <inheritdoc/>
        public bool Remove(TKey key)
        {
            return _data.Remove(key);
        }

        /// <inheritdoc/>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)_data).Remove(item);
        }

        /// <inheritdoc/>
        public bool TryGetValue(TKey key, out TValue value)
        {
            return _data.TryGetValue(key, out value!);
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        #region IDictionary 实现

        /// <inheritdoc/>
        public bool IsFixedSize => false;

        /// <inheritdoc/>
        public bool IsSynchronized => false;

        /// <inheritdoc/>
        public object SyncRoot => _data;

        /// <inheritdoc/>
        ICollection IDictionary.Keys => _data.Keys;

        /// <inheritdoc/>
        ICollection IDictionary.Values => _data.Values;

        /// <inheritdoc/>
        public bool Contains(object? key)
        {
            return key is TKey k && ContainsKey(k);
        }

        /// <inheritdoc/>
        public void Add(object? key, object? value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            Add((TKey)key, (TValue)value!);
        }

        /// <inheritdoc/>
        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return new DictionaryEnumerator(_data.GetEnumerator());
        }

        /// <inheritdoc/>
        public void Remove(object? key)
        {
            if (key is TKey k)
            {
                Remove(k);
            }
        }

        /// <inheritdoc/>
        object? IDictionary.this[object? key]
        {
            get
            {
                if (key == null)
                    throw new ArgumentNullException(nameof(key));

                return this[(TKey)key];
            }
            set
            {
                if (key == null)
                    throw new ArgumentNullException(nameof(key));

                this[(TKey)key] = (TValue)value!;
            }
        }

        /// <inheritdoc/>
        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            foreach (var kvp in _data)
            {
                array.SetValue(new DictionaryEntry(kvp.Key, kvp.Value), index++);
            }
        }

        private class DictionaryEnumerator : IDictionaryEnumerator
        {
            private readonly IEnumerator<KeyValuePair<TKey, TValue>> _enumerator;

            public DictionaryEnumerator(IEnumerator<KeyValuePair<TKey, TValue>> enumerator)
            {
                _enumerator = enumerator;
            }

            public DictionaryEntry Entry => new(_enumerator.Current.Key, _enumerator.Current.Value);

            public object Key => _enumerator.Current.Key!;

            public object? Value => _enumerator.Current.Value;

            public object Current => Entry;

            public bool MoveNext() => _enumerator.MoveNext();

            public void Reset() => _enumerator.Reset();
        }

        #endregion
    }
}
