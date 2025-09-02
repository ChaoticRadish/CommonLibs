using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Structure.Map
{
    /// <summary>
    /// 双向字典接口
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public interface IBidirectionalDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        /// <summary>
        /// 判断当前字典内是否包含输入值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        bool ContainsValue(TValue value);

        /// <summary>
        /// 通过键获取值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        TValue GetValue(TKey key);
        /// <summary>
        /// 通过值获取键
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        TKey GetKey(TValue value);

        /// <summary>
        /// 通过键移除记录
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool RemoveByKey(TKey key);
        /// <summary>
        /// 通过值移除记录
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        bool RemoveByValue(TValue value);
        /// <summary>
        /// 尝试通过键获取记录
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool TryGetByKey(TKey key, [MaybeNullWhen(false)] out TValue value);
        /// <summary>
        /// 尝试通过值获取记录
        /// </summary>
        /// <param name="value"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        bool TryGetByValue(TValue value, [MaybeNullWhen(false)] out TKey key);
    }
}
