using Common_Util.Data.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Exceptions
{
    /// <summary>
    /// 操作失败实例化而来的异常
    /// </summary>
    public class OperationFailureException : Exception
    {
        /// <summary>
        /// 使用失败原因字符串创建失败结果实例, 再用其创建异常实例
        /// </summary>
        /// <param name="failureMsg"></param>
        public OperationFailureException(string failureMsg) : this((OperationResult)failureMsg) { }

        public OperationFailureException(IOperationResult result) 
            : base(
                  result.FailureReason,
                  result is IOperationResultEx resultEx ? resultEx.Exception : null)
        {
            Result = result;
        }

        public IOperationResult Result { get; }

        /// <summary>
        /// 如果传入的操作结果是失败结果, 则抛出异常
        /// </summary>
        /// <param name="result"></param>
        public static void ThrowIfFailure(IOperationResult result)
        {
            if (result.IsFailure)
            {
                throw new OperationFailureException(result);
            }
        }
    }
}
