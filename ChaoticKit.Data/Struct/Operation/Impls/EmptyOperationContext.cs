using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaoticKit.Data.Struct.Operation.Impls
{
    /// <summary>
    /// 空的操作内容上下文
    /// </summary>
    public readonly struct EmptyOperationContext : IOperationContext, IAsyncOperationContext
    {
        public static EmptyOperationContext Shared { get; private set; } = new() { Result = OperationResultEx.Success };

        public IOperationResultEx Result { get; private init; }

        public void Execute()
        {
        }

        public ValueTask ExecuteAsync(CancellationToken waitToken = default)
        {
            return ValueTask.CompletedTask;
        }
    }
}
