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
            return new KeyEqualityComparer<T>(BuildEqualsFunc(type, keyProperties), BuildGetHashCodeFunc(type, keyProperties));
        });

        private static Func<T, T, bool> BuildEqualsFunc(Type type, PropertyInfo[] keyProperties)
        {
            ParameterExpression paramT1 = Expression.Parameter(type, "t1");
            ParameterExpression paramT2 = Expression.Parameter(type, "t2");
            var finalExpression = keyProperties
                .Select(p =>
                {
                    var propertyT1 = Expression.Property(paramT1, p);
                    var propertyT2 = Expression.Property(paramT2, p);
                    return Expression.Equal(propertyT1, propertyT2);
                })
                .MergeToMatchAll();
            return Expression.Lambda<Func<T, T, bool>>(finalExpression, paramT1, paramT2).Compile();
        }
        private static Func<T, int> BuildGetHashCodeFunc(Type type, PropertyInfo[] keyProperties)
        {
            #region 需要用到的类型
            Type typeHashCode = typeof(HashCode);

            #endregion
            #region 需要调用到的方法
            MethodInfo methodHashCode_Add = typeHashCode.GetMethods()
                .FirstOrDefault(i => i.Name == nameof(HashCode.Add) && i.GetParameters().Length == 1)
                ?? throw new ImpossibleForkException();
            MethodInfo methodHashCode_ToHashCode = typeHashCode.GetMethod(
                nameof(HashCode.ToHashCode)) ?? throw new ImpossibleForkException();
            #endregion

            ParameterExpression paramInput = Expression.Parameter(type, "t");
            // 局部变量
            ParameterExpression paramHashCode = Expression.Parameter(typeHashCode, "code");
            // 代码块内容
            List<Expression> blockContent = [];
            blockContent.Add(Expression.Assign(paramHashCode, Expression.New(typeHashCode)));

            foreach (var property in keyProperties)
            {
                blockContent.Add(Expression.Call(
                    paramHashCode,
                    methodHashCode_Add.MakeGenericMethod(property.PropertyType),
                    [Expression.Property(paramInput, property)]
                    ));
            }

            // 计算哈希值
            blockContent.Add(Expression.Call(paramHashCode, methodHashCode_ToHashCode));

            // 创建并编译返回 Lambda 表达式
            BlockExpression block = Expression.Block(
                [paramHashCode],
                blockContent);
            return Expression.Lambda<Func<T, int>>(block, paramInput).Compile();
        }

        private KeyEqualityComparer(Func<T, T, bool> equalsFunc, Func<T, int> getHashCodeFunc)
        {
            GetHashCodeFunc = getHashCodeFunc;
            EqualsFunc = equalsFunc;
        }
        /// <summary>
        /// 获取哈希值的方法
        /// </summary>
        private Func<T, int> GetHashCodeFunc { get; init; }
        /// <summary>
        /// 比较两个对象是否相等的方法
        /// </summary>
        private Func<T, T, bool> EqualsFunc { get; init; }

        public bool Equals(T? x, T? y)
        {
            if (x == null && y == null) return true;
            if (x == null && y != null) return false;
            if (x != null && y == null) return false;
            if (x != null && y != null) return EqualsFunc(x, y);
            throw new ImpossibleForkException();
        }

        public int GetHashCode([DisallowNull] T obj) => GetHashCodeFunc(obj);
    }
}
