using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Interfaces.Behavior
{
    /// <summary>
    /// 比较 <typeparamref name="T1"/> 与 <typeparamref name="T2"/> 两个类型的值是否等价的比较器
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    public interface IEquivalentComparer<T1, T2>
    {
        /// <summary>
        /// 比较 <paramref name="x"/> 和 <paramref name="y"/> 是否等价
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        bool IsEquivalent(T1? x, T2? y);
    }

    /// <summary>
    /// 行为: 能够与 <typeparamref name="T"/> 类型对象比较是否等价
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICompareEquivalentTo<T>
    {
        /// <summary>
        /// 比较当前对象与 <typeparamref name="T"/> 是否等价
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        bool IsEquivalent(T other);
    }
}
