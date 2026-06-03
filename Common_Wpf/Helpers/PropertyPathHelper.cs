using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Common_Wpf.Helpers
{
    public static class PropertyPathHelper
    {
        /// <summary>
        /// 将从类型 <typeparamref name="T"/> 取某属性 (或属性值的属性) 的表达式, 转换为 WPF 可用的属性路径字符串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static string ConvertToPath<T, TProperty>(Expression<Func<T, TProperty>> expression)
        {
            var stack = new Stack<string>();

            MemberExpression? member;

            // 处理可能发生的类型转换（例如 object 到 具体）
            if (expression.Body is UnaryExpression unary && unary.Operand is MemberExpression unaryMember)
            {
                member = unaryMember;
            }
            else
            {
                member = (MemberExpression)expression.Body;
            }

            while (member != null)
            {
                stack.Push(member.Member.Name);
                member = member.Expression as MemberExpression;
            }

            if (stack.Count > 0) return string.Join(".", stack);

            throw new ArgumentException("表达式必须是属性访问表达式，如 'u => u.Name'", nameof(expression));
        }
    }
}
