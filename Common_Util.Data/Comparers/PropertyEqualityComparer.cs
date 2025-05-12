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

namespace Common_Util.Data.Comparers
{
    /// <summary>
    /// 比较属性值是否相等的比较器
    /// </summary>
    /// <remarks>
    /// 两个均为 <see langword="null"/> 的对象视为相等, 一方为 <see langword="null"/> 另一方不为 <see langword="null"/> 的视为不相等 <br/>
    /// 注: 使用了反射
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public readonly struct PropertyEqualityComparer<T> : IEqualityComparer<T>
    {
        #region 静态内容

        /// <summary>
        /// 默认比较器, 比较所有公共属性
        /// </summary>
        public static PropertyEqualityComparer<T> Default => @default.Value;
        private readonly static Lazy<PropertyEqualityComparer<T>> @default = new(() =>
        {
            Type type = typeof(T);
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            if (properties.Length == 0) throw new InvalidOperationException($"无法构建不含公共属性的比较表达式树");
            return new PropertyEqualityComparer<T>(BuildEqualsFunc(type, properties), BuildGetHashCodeFunc(type, properties));
        });

        /// <summary>
        /// 创建比较特定名字的公共属性的比较器
        /// </summary>
        /// <param name="propertyNames"></param>
        /// <returns></returns>
        public static PropertyEqualityComparer<T> PropertyNames(params string[] propertyNames)
        {
            Type type = typeof(T);
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            IEnumerable<(string name, PropertyInfo? property)> useProperties = propertyNames
                .Select(name => (name, properties.FirstOrDefault(i => i.Name == name)));
            var found = useProperties.FirstOrDefault(i => i.property == null);
            if (found != default)
            {
                throw new InvalidOperationException($"未找到名字为 {found.name} 的公共属性");
            }
            var usePropertiesArr = useProperties.Select(i => i.property!).ToArray();
            return new PropertyEqualityComparer<T>(
                BuildEqualsFunc(type, usePropertiesArr), 
                BuildGetHashCodeFunc(type, usePropertiesArr));
        }

        #region 生成比较方法

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

        #endregion

        #endregion
        private PropertyEqualityComparer(Func<T, T, bool> equalsFunc, Func<T, int> getHashCodeFunc)
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

        public readonly bool Equals(T? x, T? y)
        {
            if (x == null && y == null) return true;
            if (x == null && y != null) return false;
            if (x != null && y == null) return false;
            if (x != null && y != null) return EqualsFunc(x, y);
            throw new ImpossibleForkException();
        }

        public readonly int GetHashCode([DisallowNull] T obj) => GetHashCodeFunc(obj);
    }
}
