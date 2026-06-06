using System;
using System.Collections.Generic;

namespace ChaoticKit.Data.Structure.Map
{
    /// <summary>
    /// 默认值Map接口
    /// <para>访问不存在的键时返回默认值，设置不存在的键时自动添加</para>
    /// </summary>
    /// <typeparam name="TKey">键类型</typeparam>
    /// <typeparam name="TValue">值类型</typeparam>
    public interface IDefaultValueMap<TKey, TValue> : IDictionary<TKey, TValue>
        where TKey : notnull
    {
        /// <summary>
        /// 获取默认值
        /// <para>当访问不存在的键时返回此值</para>
        /// </summary>
        TValue DefaultValue { get; }
    }
}
