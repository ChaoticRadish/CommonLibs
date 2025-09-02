using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Structure.Map
{
    /// <summary>
    /// 双向查询接口的默认实现
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public sealed class BidirectionalDictionary<TKey, TValue> : IBidirectionalDictionary<TKey, TValue>
        where TKey : notnull
        where TValue : notnull
    {
        private readonly Dictionary<TKey, TValue> _key_to_value;
        private readonly Dictionary<TValue, TKey> _value_to_key;

        private ReaderWriterLockSlim _lock = new();

        public BidirectionalDictionary()
        {
            _key_to_value = [];
            _value_to_key = [];
        }
        public BidirectionalDictionary(int capacity)
        {
            _key_to_value = new Dictionary<TKey, TValue>(capacity);
            _value_to_key = new Dictionary<TValue, TKey>(capacity);
        }

        #region 配置
        /// <summary>
        /// 如果索引 <see langword="set"/> 访问器方括号内的参数不存在, 是否添加
        /// </summary>
        public bool AddIfNoExists { get; set; } = false;
        #endregion

        public TValue this[TKey key] 
        {
            get => GetValue(key);
            set
            {
                _lock.EnterWriteLock();
                try
                {
                    if (!AddIfNoExists) // 不存在时会抛出异常
                    {
                        var exist = _key_to_value[key];
                        if (IsEquals(exist, value)) return;
                        else if (_value_to_key.ContainsKey(value))
                            throw new ArgumentException($"Value '{value}' already exists.");
                        else
                        {
                            _key_to_value[key] = value;
                            _value_to_key.Add(value, key);
                            _value_to_key.Remove(exist);
                        }
                    }
                    else
                    {
                        _addBody(key, value);
                    }
                }
                finally { _lock.ExitWriteLock(); }
            }
        }
        public TKey this[TValue i_value]
        {
            get => GetKey(i_value);
            set
            {
                _lock.EnterWriteLock();
                try
                {
                    if (!AddIfNoExists) // 不存在时会抛出异常
                    {
                        var exist = _value_to_key[i_value];
                        if (IsEquals(exist, value)) return;
                        else if (_key_to_value.ContainsKey(value))
                            throw new ArgumentException($"Key '{value}' already exists.");
                        else
                        {
                            _value_to_key[i_value] = value;
                            _key_to_value.Add(value, i_value);
                            _key_to_value.Remove(exist);
                        }
                    }
                    else
                    {
                        _addBody(value, i_value);
                    }
                }
                finally { _lock.ExitWriteLock(); }
            }
        }

        public TValue GetValue(TKey key)
        {
            _lock.EnterReadLock();
            try
            {
                return _key_to_value[key];
            }
            finally { _lock.ExitReadLock(); }
        }

        public TKey GetKey(TValue value)
        {
            _lock.EnterReadLock();
            try
            {
                return _value_to_key[value];
            }
            finally { _lock.ExitReadLock(); }
        }

        public ICollection<TKey> Keys 
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _key_to_value.Keys;
                }
                finally { _lock.ExitReadLock(); }
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _value_to_key.Keys;
                }
                finally { _lock.ExitReadLock(); }
            }
        }

        public int Count
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _key_to_value.Count;
                }
                finally { _lock.ExitReadLock(); }
            }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => ((ICollection<KeyValuePair<TKey, TValue>>)_key_to_value).IsReadOnly;

        public void Add(TKey key, TValue value)
        {
            _lock.EnterWriteLock();
            try
            {
                _addBody(key, value);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        private void _addBody(TKey key, TValue value)
        {
            if (_key_to_value.ContainsKey(key))
                throw new ArgumentException($"Key '{key}' already exists.");
            if (_value_to_key.ContainsKey(value))
                throw new ArgumentException($"Value '{value}' already exists.");

            _key_to_value.Add(key, value);
            _value_to_key.Add(value, key);
        }

        public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

        public void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                _key_to_value.Clear();
                _value_to_key.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            _lock.EnterReadLock();
            try
            {
                return _key_to_value.TryGetValue(item.Key, out var value) && IsEquals(value, item.Value);
            }
            finally { _lock.ExitReadLock(); }
        }

        public bool ContainsKey(TKey key)
        {
            _lock.EnterReadLock();
            try
            {
                return _key_to_value.ContainsKey(key);
            }
            finally { _lock.ExitReadLock(); }
        }
        public bool ContainsValue(TValue value)
        {
            _lock.EnterReadLock();
            try
            {
                return _value_to_key.ContainsKey(value);
            }
            finally { _lock.ExitReadLock(); }
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            _lock.EnterReadLock();
            try
            {
                ((ICollection<KeyValuePair<TKey, TValue>>)_key_to_value).CopyTo(array, arrayIndex);
            }
            finally { _lock.ExitReadLock(); }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            _lock.EnterReadLock();
            try
            {
                return _key_to_value.GetEnumerator();
            }
            finally { _lock.ExitReadLock(); }
        }

        bool IDictionary<TKey, TValue>.Remove(TKey key) => RemoveByKey(key);
        public bool RemoveByKey(TKey key)
        {
            _lock.EnterWriteLock();
            try
            {
                return _key_to_value.Remove(key, out var value) && _value_to_key.Remove(value);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        public bool RemoveByValue(TValue value)
        {
            _lock.EnterWriteLock();
            try
            {
                return _value_to_key.Remove(value, out var key) && _key_to_value.Remove(key);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            _lock.EnterWriteLock();
            try
            {
                if (_key_to_value.TryGetValue(item.Key, out var value) && IsEquals(value, item.Value))
                {
                    return _value_to_key.Remove(value, out var key) && _key_to_value.Remove(key);
                }
                else return true;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool TryGetByKey(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            _lock.EnterReadLock();
            try
            {
                return _key_to_value.TryGetValue(key, out value);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
        public bool TryGetByValue(TValue value, [MaybeNullWhen(false)] out TKey key)
        {
            _lock.EnterReadLock();
            try
            {
                return _value_to_key.TryGetValue(value, out key);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
        bool IDictionary<TKey, TValue>.TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
            => TryGetByKey(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private static bool IsEquals(TValue? x, TValue? y)
        {
            return (x == null && y == null) 
                || (x != null && y != null && x.Equals(y));
        }
        private static bool IsEquals(TKey? x, TKey? y)
        {
            return (x == null && y == null)
                || (x != null && y != null && x.Equals(y));
        }

    }
}
