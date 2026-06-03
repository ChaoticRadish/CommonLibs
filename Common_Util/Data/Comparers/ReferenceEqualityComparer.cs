using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Comparers
{
    /// <summary>
    /// 基于 <see cref="ReferenceEqualityComparer"/> 实现, 用于偷懒
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T>
    {
        /// <summary>
        /// 取得实例
        /// </summary>
        public static ReferenceEqualityComparer<T> Instance => Lazy.Value;
        private readonly static Lazy<ReferenceEqualityComparer<T>> Lazy = new(() => new());

        public bool Equals(T? x, T? y)
            => ReferenceEqualityComparer.Instance.Equals(x, y);

        public int GetHashCode([DisallowNull] T obj)
            => ReferenceEqualityComparer.Instance.GetHashCode(obj);
    }
}
