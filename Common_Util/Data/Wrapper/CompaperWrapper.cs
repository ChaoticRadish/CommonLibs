using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Wrapper
{
    /// <summary>
    /// 比较器的包装器, 实现基于<b>比较由对象得到的某一键值</b>的比较效果
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="keySelector"></param>
    /// <param name="desc"></param>
    /// <param name="comparer"></param>
    public readonly struct CompaperWrapper<TSource, TKey>(Func<TSource, TKey> keySelector, bool desc = false, IComparer<TKey>? comparer = null) : IComparer<TSource>
    {
        /// <summary>
        /// 是否对 <see cref="Comparer"/> 取反, 以达到降序效果
        /// </summary>
        public bool Desc { get; } = desc;
        /// <summary>
        /// 从源类型中取得比较键值的方法
        /// </summary>
        public Func<TSource, TKey> KeySelector { get; } = keySelector;
        /// <summary>
        /// 用以比较键值的比较器
        /// </summary>
        public IComparer<TKey> KeyComparer { get; } = comparer ?? Comparer<TKey>.Default;

        public int Compare(TSource? x, TSource? y)
        {
            TKey? kx = x == null ? default : KeySelector(x);
            TKey? ky = y == null ? default : KeySelector(y);
            return KeyComparer.Compare(kx, ky) * (Desc ? -1 : 1);
        }
    }
}
