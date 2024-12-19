using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Structure.Pair
{
    /// <summary>
    /// 无序对: 无序的数据对结构
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    public interface IDisorderPair<T1, T2>
    {
        /// <summary>
        /// 第一个数据
        /// </summary>
        T1 Item1 { get; set; }
        /// <summary>
        /// 第二个数据
        /// </summary>
        T2 Item2 { get; set; }
    }

    /// <summary>
    /// 无序对: 无序的数据对结构
    /// </summary>
    /// <remarks>
    /// 在两个无序对互相比较时, 只要拥有的两个值相等即可
    /// </remarks>
    public struct DisorderPair<T> : IDisorderPair<T, T>
    {
        public T Item1 { get; set; }
        public T Item2 { get; set; }

        #region 比较方法
        public readonly override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is DisorderPair<T> pair1)
            {
                return this == pair1;
            }
            return base.Equals(obj);
        }

        /// <summary>
        /// 比较两个无序对是否相等
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="comparer1"></param>
        /// <param name="comparer2"></param>
        /// <returns></returns>
        public static bool Equals(DisorderPair<T> p1, DisorderPair<T> p2,
            IEqualityComparer<T>? comparer = null)
        {
            comparer ??= EqualityComparer<T>.Default;
            return 
                (
                    comparer.Equals(p1.Item1, p2.Item1) && comparer.Equals(p1.Item2, p2.Item2)
                ) 
                ||
                (
                    comparer.Equals(p1.Item1, p2.Item2) && comparer.Equals(p1.Item2, p2.Item1)
                );
        }

        public readonly override int GetHashCode()
        {
            return GetHashCode(null);
        }
        /// <summary>
        /// 使用指定的比较器取得哈希码
        /// </summary>
        /// <param name="comparer1"></param>
        /// <param name="comparer2"></param>
        /// <returns></returns>
        public readonly int GetHashCode(IEqualityComparer<T>? comparer)
        {
            comparer ??= EqualityComparer<T>.Default;
            int i1 = Item1 == null ? 0 : comparer.GetHashCode(Item1);
            int i2 = Item2 == null ? 0 : comparer.GetHashCode(Item2);
            (i1, i2) = i1 > i2 ? (i1, i2) : (i2, i1);
            return HashCode.Combine(i1, i2);
        }
        #endregion

        #region 运算符重载
        public static bool operator ==(DisorderPair<T> p1, DisorderPair<T> p2)
        {
            return Equals(p1, p2, null);
        }
        public static bool operator !=(DisorderPair<T> left, DisorderPair<T> right)
        {
            return !(left == right);
        }
        #endregion

        #region 隐式转换
        public static implicit operator DisorderPair<T>(DisorderPair<T, T> other)
        {
            return new DisorderPair<T>()
            {
                Item1 = other.Item1,
                Item2 = other.Item2,
            };
        }
        public static implicit operator DisorderPair<T>(KeyValuePair<T, T> pair)
        {
            return new DisorderPair<T>()
            {
                Item1 = pair.Key,
                Item2 = pair.Value,
            };
        }
        public static implicit operator DisorderPair<T>((T, T) pair)
        {
            return new DisorderPair<T>()
            {
                Item1 = pair.Item1,
                Item2 = pair.Item2,
            };
        }
        #endregion
    }

    /// <summary>
    /// 无序对: 无序的数据对结构
    /// </summary>
    /// <remarks>
    /// 在两个无序对互相比较时, 只要拥有的两个值相等即可
    /// </remarks>
    public struct DisorderPair<T1, T2> : IDisorderPair<T1, T2>
    {
        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }

        #region 比较方法
        public readonly override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is DisorderPair<T1, T2> pair1)
            {
                return this == pair1;
            }
            else if (obj is DisorderPair<T2, T1> pair2)
            {
                return this == pair2;
            }
            return base.Equals(obj);
        }
        /// <summary>
        /// 比较两个无序对是否相等
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="comparer1"></param>
        /// <param name="comparer2"></param>
        /// <returns></returns>
        public static bool Equals(DisorderPair<T1, T2> p1, DisorderPair<T1, T2> p2,
            IEqualityComparer<T1>? comparer1 = null, IEqualityComparer<T2>? comparer2 = null)
        {
            comparer1 ??= EqualityComparer<T1>.Default;
            comparer2 ??= EqualityComparer<T2>.Default;
            return comparer1.Equals(p1.Item1, p2.Item1) && comparer2.Equals(p1.Item2, p2.Item2);
        }
        /// <summary>
        /// 比较两个无序对是否相等
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="comparer1"></param>
        /// <param name="comparer2"></param>
        /// <returns></returns>
        public static bool Equals(DisorderPair<T1, T2> p1, DisorderPair<T2, T1> p2,
            IEqualityComparer<T1>? comparer1 = null, IEqualityComparer<T2>? comparer2 = null)
        {
            comparer1 ??= EqualityComparer<T1>.Default;
            comparer2 ??= EqualityComparer<T2>.Default;
            return comparer1.Equals(p1.Item1, p2.Item2) && comparer2.Equals(p1.Item2, p2.Item1);
        }

        public readonly override int GetHashCode()
        {
            return GetHashCode(null, null);
        }
        /// <summary>
        /// 使用指定的比较器取得哈希码
        /// </summary>
        /// <param name="comparer1"></param>
        /// <param name="comparer2"></param>
        /// <returns></returns>
        public readonly int GetHashCode(IEqualityComparer<T1>? comparer1, IEqualityComparer<T2>? comparer2)
        {
            comparer1 ??= EqualityComparer<T1>.Default;
            comparer2 ??= EqualityComparer<T2>.Default;
            int i1 = Item1 == null ? 0 : comparer1.GetHashCode(Item1);
            int i2 = Item2 == null ? 0 : comparer2.GetHashCode(Item2);
            (i1, i2) = i1 > i2 ? (i1, i2) : (i2, i1);
            return HashCode.Combine(i1, i2);
        }

        #endregion

        #region 运算符重载
        public static bool operator ==(DisorderPair<T1, T2> p1, DisorderPair<T1, T2> p2)
        {
            return Equals(p1, p2, null, null);
        }
        public static bool operator ==(DisorderPair<T1, T2> p1, DisorderPair<T2, T1> p2)
        {
            return Equals(p1, p2, null, null);
        }

        public static bool operator !=(DisorderPair<T1, T2> left, DisorderPair<T1, T2> right)
        {
            return !(left == right);
        }
        public static bool operator !=(DisorderPair<T1, T2> left, DisorderPair<T2, T1> right)
        {
            return !(left == right);
        }
        #endregion

        #region 隐式转换
        public static implicit operator DisorderPair<T1, T2>(DisorderPair<T2, T1> other)
        {
            return new DisorderPair<T1, T2>()
            {
                Item1 = other.Item2,
                Item2 = other.Item1,
            };
        }
        public static implicit operator DisorderPair<T1, T2>(KeyValuePair<T1, T2> pair)
        {
            return new DisorderPair<T1, T2>()
            {
                Item1 = pair.Key,
                Item2 = pair.Value,
            };
        }
        public static implicit operator DisorderPair<T1, T2>(KeyValuePair<T2, T1> pair)
        {
            return new DisorderPair<T1, T2>()
            {
                Item1 = pair.Value,
                Item2 = pair.Key,
            };
        }
        public static implicit operator DisorderPair<T1, T2>((T1, T2) pair)
        {
            return new DisorderPair<T1, T2>()
            {
                Item1 = pair.Item1,
                Item2 = pair.Item2,
            };
        }
        public static implicit operator DisorderPair<T1, T2>((T2, T1) pair)
        {
            return new DisorderPair<T1, T2>()
            {
                Item1 = pair.Item2,
                Item2 = pair.Item1,
            };
        }
        #endregion

    }
}
