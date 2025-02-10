using Common_Util.Exceptions.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Extensions
{
    /// <summary>
    /// 表达式树相关的扩展方法
    /// </summary>
    public static class ExpressionExtensions
    {
        /// <summary>
        /// 合并传入的表达式为一个表达式, 传入对象符合其中一个表达式时, 就会返回 <see langword="true"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expressions">需要合并的表达式, 返回的类型需要是 <see langword="bool"/>, 不能为空</param>
        /// <returns>返回类型是 <see langword="bool"/> 的表达式</returns>
        public static Expression MergeToMatchAny<TExpression>(this IEnumerable<TExpression> expressions)
            where TExpression : Expression
        {
            Expression? output = null;
            foreach (var expression in expressions)
            {
                if (expression == null)
                {
                    continue;
                }
                if (expression.Type != typeof(bool))
                {
                    throw new ArgumentException($"表达式 ({expression}) 预期的返回类型不是 {typeof(bool)}");
                }
                if (output == null)
                {
                    output = expression;
                }
                else
                {
                    output = Expression.Or(output, expression);
                }
            }
            if (output == null) throw new InvalidOperationException("传入集合为空或者其集合项均为 null");
            return output;
        }
        /// <summary>
        /// 合并传入的所有对某一对象作判断的表达式为一个表达式, 传入对象符合所有表达式时, 才会返回 <see langword="true"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expressions">需要合并的表达式, 返回的类型需要是 <see langword="bool"/>, 不能为空</param>
        /// <returns>返回类型是 <see langword="bool"/> 的表达式</returns>
        public static Expression MergeToMatchAll<TExpression>(this IEnumerable<TExpression> expressions)
            where TExpression : Expression
        {
            Expression? output = null;
            foreach (var expression in expressions)
            {
                if (expression == null)
                {
                    continue;
                }
                if (output == null)
                {
                    output = expression;
                }
                else
                {
                    output = Expression.And(output, expression);
                }
            }
            if (output == null) throw new InvalidOperationException("传入集合为空或者其集合项均为 null");
            return output;
        }
    }
}
