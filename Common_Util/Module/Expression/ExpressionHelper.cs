using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

using SysExpressions = System.Linq.Expressions;

namespace Common_Util.Module.Expression
{
    public static class ExpressionHelper
    {
        /// <summary>
        /// 创建属性或字段访问表达式
        /// </summary>
        /// <typeparam name="T">源对象类型</typeparam>
        /// <typeparam name="TAccessTarget">访问目标类型</typeparam>
        /// <param name="from">表达式起始点</param>
        /// <param name="accessExpr">属性或字段访问表达式</param>
        /// <returns>构建后的属性或字段访问表达式</returns>
        /// <exception cref="ArgumentException">当传入的表达式不是直接访问属性或字段时抛出</exception>
        /// <exception cref="InvalidOperationException">当无法获取属性或字段信息时抛出</exception>
        public static SysExpressions.Expression CreateMemberAccess<T, TAccessTarget>(
            SysExpressions.Expression from,
            SysExpressions.Expression<Func<T, TAccessTarget>> accessExpr)
        {
            var propertyExpression = accessExpr.Body;
            MemberExpression memberExpression;
            if (propertyExpression is MemberExpression _memberExpression1)
            {
                memberExpression = _memberExpression1;
            }
            else if (propertyExpression is UnaryExpression unaryExpression 
                && unaryExpression.NodeType == ExpressionType.Convert
                && unaryExpression.Operand is MemberExpression _memberExpression2)
            {
                memberExpression = _memberExpression2;
            }
            else
                throw new ArgumentException("表达式必须是直接访问属性或字段的表达式", nameof(accessExpr));

            if (memberExpression.Member is PropertyInfo propertyInfo)
                return SysExpressions.Expression.Property(from, propertyInfo);
            else if (memberExpression.Member is FieldInfo fieldInfo)
                return SysExpressions.Expression.Field(from, fieldInfo);
            else
                throw new InvalidOperationException("无法取得属性或字段信息");
        }
    }
}
