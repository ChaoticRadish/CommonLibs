using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Comparers
{
    /// <summary>
    /// 包装两个方法为一个 <see cref="IEqualityComparer{T}"/>, 用于偷懒
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="equalsFunc"></param>
    /// <param name="getHashCodeFunc"></param>
    public class EqualityComparerWrapper<T>(Func<T?, T?, bool> equalsFunc, Func<T, int> getHashCodeFunc) : IEqualityComparer<T>
    {
        private readonly Func<T?, T?, bool> equalsFunc = equalsFunc;
        private readonly Func<T, int> getHashCodeFunc = getHashCodeFunc;

        public bool Equals(T? x, T? y) => equalsFunc(x, y);

        public int GetHashCode([DisallowNull] T obj) => getHashCodeFunc(obj);
    }
}
