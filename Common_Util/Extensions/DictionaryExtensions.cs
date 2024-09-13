using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Extensions
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// 从字典取值, 如果不存在, 则尝试调用无参构造函数实例化新值后添加到字典
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dic"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>如果从字典取到了值, 或成功添加了新值, 则返回 <see langword="true"/></returns>
        public static bool TryGetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, [MaybeNullWhen(false)] out TValue value)
            where TKey : notnull
            where TValue : new()
        {
            return TryGetOrAdd(dic, key, () => new(), out value);
        }

        /// <summary>
        /// 从字典取值, 如果不存在, 则尝试添加新值到字典
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dic"></param>
        /// <param name="key"></param>
        /// <param name="getNewValue">如果不存在传入键对应项, 则调用此方法得到待添加的新值</param>
        /// <param name="value"></param>
        /// <returns>如果从字典取到了值, 或成功添加了新值, 则返回 <see langword="true"/></returns>
        public static bool TryGetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, Func<TValue> getNewValue, [MaybeNullWhen(false)] out TValue value)
            where TKey : notnull
        {
            if (dic.TryGetValue(key, out value))
            {
                return true;
            }
            else
            {
                var temp = getNewValue();
                if (dic.TryAdd(key, getNewValue()))
                {
                    value = temp;
                    return true;
                }
                else
                {
                    value = default;
                    return false;
                }
            }
        }
    }
}
