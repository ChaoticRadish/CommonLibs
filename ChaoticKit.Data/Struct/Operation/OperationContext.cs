using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaoticKit.Data.Struct.Operation
{
    /// <summary>
    /// 操作内容上下文
    /// </summary>
    public interface IOperationContext
    {
        /// <summary>
        /// 执行操作内容
        /// </summary>
        void Execute();

        /// <summary>
        /// 执行结果
        /// </summary>
        IOperationResultEx Result { get; }
    }
    /// <summary>
    /// 异步操作内容上下文
    /// </summary>
    public interface IAsyncOperationContext
    {
        /// <summary>
        /// 异步执行操作内容
        /// </summary>
        /// <param name="waitToken">等待执行完成的取消令牌, 仅取消等待, 不取消具体操作内容的执行</param>
        /// <returns></returns>
        ValueTask ExecuteAsync(CancellationToken waitToken = default);

        /// <summary>
        /// 执行结果
        /// </summary>
        IOperationResultEx Result { get; }
    }



}
