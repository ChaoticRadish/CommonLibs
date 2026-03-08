using System;
using System.Collections.Generic;

namespace Common_Util.Data.Structure.Linear
{
    /// <summary>
    /// 自动扩展列表接口
    /// <para>支持任意非负索引访问，get 时返回默认值，set 时自动扩展</para>
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    public interface IAutoExpandList<T> : IList<T>
    {
        /// <summary>
        /// 获取默认值
        /// <para>当访问不存在的索引时返回此值</para>
        /// </summary>
        T DefaultValue { get; }

        /// <summary>
        /// 获取已设置值的索引集合
        /// </summary>
        IEnumerable<int> SetIndexes { get; }

        /// <summary>
        /// 判断指定索引是否已设置值
        /// </summary>
        /// <param name="index">索引值</param>
        /// <returns>如果已设置值返回 <see langword="true"/>，否则返回 <see langword="false"/></returns>
        bool ContainsIndex(int index);
    }
}
