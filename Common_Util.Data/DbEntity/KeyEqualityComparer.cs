using Common_Util.Data.Comparers;
using Common_Util.Exceptions.General;
using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.DbEntity
{
    public static class KeyEqualityComparer
    {
        /// <summary>
        /// 使用 <see cref="KeyEqualityComparer{T}.Shared"/> 比较传入的两个值是否相等
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool Equals<T>(T v1, T v2)
        {
            return KeyEqualityComparer<T>.Shared.Equals(v1, v2);
        }
    }

    /// <summary>
    /// 键值相等比较器
    /// </summary>
    /// <remarks>
    /// 两个均为 <see langword="null"/> 的对象视为相等, 一方为 <see langword="null"/> 另一方不为 <see langword="null"/> 的视为不相等 <br/>
    /// 均不为  <see langword="null"/> 的情况下, 比较实体类型中, 被 <see cref="System.ComponentModel.DataAnnotations.KeyAttribute"/> 标记的属性是否都相等 <br/>
    /// 注: 使用了反射
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public sealed class KeyEqualityComparer<T> : IEqualityComparer<T>
    {
        public static KeyEqualityComparer<T> Shared => shared.Value;
        private readonly static Lazy<KeyEqualityComparer<T>> shared = new(() =>
        {
            Type type = typeof(T);
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            if (properties.Length == 0) throw new InvalidOperationException($"无法构建不含属性的实体类型的比较表达式树");
            var keyProperties = properties.Where(i => i.ExistCustomAttribute<System.ComponentModel.DataAnnotations.KeyAttribute>()).ToArray();
            if (keyProperties.Length == 0) throw new InvalidOperationException($"无法构建不含带有主键标识的属性的实体类型的比较表达式树");
            return new KeyEqualityComparer<T>()
            {
                comparer = PropertyEqualityComparer<T>.PropertyNames(keyProperties.Select(i => i.Name).ToArray())
            };
        });

        private KeyEqualityComparer() { }

        private PropertyEqualityComparer<T> comparer;

        public bool Equals(T? x, T? y)
        {
            return comparer.Equals(x, y);
        }

        public int GetHashCode([DisallowNull] T obj)
        {
            return comparer.GetHashCode(obj);
        }
    }
}
