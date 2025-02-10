using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using SysExpressions = System.Linq.Expressions;

namespace Common_Util.Module.Expression
{
    /// <summary>
    /// 用于替换表达式中 <see cref="ParameterExpression"/> 的表达式访问其
    /// </summary>
    public sealed class ParameterReplacer : ExpressionVisitor
    {
        /// <summary>
        /// 替换表达式的方法委托
        /// </summary>
        /// <param name="oldParamExpr"></param>
        /// <returns>返回值为 <see langword="null"/> 时, 不做替换</returns>
        public delegate ParameterExpression? ReplaceHandler(ParameterExpression oldParamExpr);

        /// <summary>
        /// 如何替换 <see cref="ParameterExpression"/> 的方法
        /// </summary>
        public required ReplaceHandler HowReplace { get; init; }

        protected override SysExpressions.Expression VisitParameter(ParameterExpression node)
        {
            var newOne = HowReplace(node);
            if (newOne != null)
            {
                return newOne;
            }
            return base.VisitParameter(node);
        }

        #region 静态创建替换器的方法
        /// <summary>
        /// 将所有的 <see cref="ParameterExpression"/> 都替换为 <paramref name="paramExpr"/>
        /// </summary>
        /// <param name="paramExpr"></param>
        /// <returns></returns>
        public static ParameterReplacer AllReplaceTo(ParameterExpression paramExpr)
        {
            return new ParameterReplacer()
            {
                HowReplace = (old) => paramExpr,
            };
        }
        #endregion
    }

}
