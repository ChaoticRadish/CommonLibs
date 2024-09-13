using Common_Util.Data.Constraint;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Exceptions.General
{
    /// <summary>
    /// 值不匹配异常
    /// </summary>
    /// <remarks>
    /// 一般用于预期值/理论值与实际值不匹配的情况
    /// </remarks>
    public class ValueMismatchException : Exception, IEnumerable<ValueMismatchException.ValueMismatchItem>
    {
        private readonly List<ValueMismatchItem> MismatchItems = [];

        public ValueMismatchException(string message) : base(message) { }

        public void Add(ValueMismatchItem item) => MismatchItems.Add(item);

        public IEnumerator<ValueMismatchItem> GetEnumerator()
        {
            return ((IEnumerable<ValueMismatchItem>)MismatchItems).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)MismatchItems).GetEnumerator();
        }



        /// <param name="ExpectedInfo"> 预期值描述文本 </param>
        /// <param name="ExpectedValue"> 预期值 </param>
        /// <param name="ActualInfo"> 实际值描述文本 </param>
        /// <param name="ActualValue"> 描述值 </param>
        public record struct ValueMismatchItem(
            object? ExpectedValue, object? ActualValue,
            [CallerArgumentExpression(nameof(ExpectedValue))] string ExpectedInfo = "", 
            [CallerArgumentExpression(nameof(ActualValue))] string ActualInfo = "")
        {
            /// <summary>
            /// 是否相等? 
            /// </summary>
            public bool Equal { get; } = ExpectedValue == ActualValue;

        }
    }
}
