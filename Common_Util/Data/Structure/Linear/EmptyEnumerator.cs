using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Structure.Linear
{
    /// <summary>
    /// 空的遍历器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly struct EmptyEnumerator<T> : IEnumerator<T>
    {
        public static EmptyEnumerator<T> Shared { get; } = new EmptyEnumerator<T>();

        /// <summary>
        /// 用于实现 <see cref="IEnumerator{T}"/> 的属性, 返回 <typeparamref name="T"/> 的默认值, 
        /// 可能出现 <see langword="null"/>
        /// </summary>
        public readonly T Current => default!;

        readonly object IEnumerator.Current => Current!;

        public readonly void Dispose() { }

        public readonly bool MoveNext() { return false; }

        public readonly void Reset() { }
    }
}
