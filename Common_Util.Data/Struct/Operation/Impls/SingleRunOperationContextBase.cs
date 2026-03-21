using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Struct.Operation.Impls
{
    /// <summary>
    /// 提供一个线程安全的操作上下文，确保操作内容只执行一次。
    /// </summary>
    /// <remarks>
    /// 支持同步和异步调用，且多次调用会等待首次执行完成。
    /// </remarks>
    public abstract class SingleRunOperationContextBase : IAsyncOperationContext, IOperationContext
    {
        private readonly object _syncRoot = new object();
        private readonly CancellationToken _operationToken;
        private Task<IOperationResultEx>? _executionTask;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="operationToken">执行内容的取消令牌</param>
        protected SingleRunOperationContextBase(CancellationToken operationToken = default)
        {
            this._operationToken = operationToken;
        }

        /// <summary>
        /// 取得未执行完成的结果实例
        /// </summary>
        protected abstract IOperationResultEx GetNotExecutedResult();
        /// <summary>
        /// 取得发生异常时包装异常内容的结果实例
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        protected abstract IOperationResultEx GetExceptionResult(Exception ex);
        /// <summary>
        /// 取得操作取消时的结果实例
        /// </summary>
        /// <param name="innerTask"></param>
        /// <returns></returns>
        protected virtual IOperationResultEx GetOperationCancelResult(Task innerTask)
        {
            return GetExceptionResult(new TaskCanceledException(innerTask));
        }

        /// <summary>
        /// 派生类需实现具体执行内容
        /// </summary>
        /// <returns></returns>
        protected abstract Task<IOperationResultEx> OnExecuteAsync(CancellationToken cancellationToken);

        public async ValueTask ExecuteAsync(CancellationToken waitToken = default)
        {
            if (_executionTask == null)
            {
                lock (_syncRoot)
                {
                    if (_executionTask == null)
                    {
                        _executionTask = Task.Run(() => OnExecuteAsync(_operationToken), _operationToken);
                    }
                }
            }
            try
            {
                await _executionTask.WaitAsync(waitToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (waitToken.IsCancellationRequested)
            {
                // 取消等待执行, 抛出异常给调用者
                throw;
            }
            catch (OperationCanceledException) when (_operationToken.IsCancellationRequested)
            {
                // 取消等待操作内容, 属于正常结束
            }
            catch { }
        }

        public void Execute()
        {
            try
            {
                ExecuteAsync(CancellationToken.None).AsTask().GetAwaiter().GetResult();
            }
            catch { }
        }

        public IOperationResultEx Result
        {
            get
            {
                if (_executionTask == null || !_executionTask.IsCompleted)
                {
                    return GetNotExecutedResult();
                }

                if (_executionTask.IsFaulted)
                {
                    var actualException = _executionTask.Exception?.InnerException
                                      ?? _executionTask.Exception
                                      ?? new Exception("未知错误");
                    return GetExceptionResult(actualException);
                }
                if (_executionTask.IsCanceled)
                {
                    return GetExceptionResult(new TaskCanceledException(_executionTask));
                }
                return _executionTask.GetAwaiter().GetResult();
            }
        }
    }

    public static class SingleRunOperationContext
    {
        public static SingleRunOperationContextBase Create<TOperationResult>(Func<CancellationToken, Task<TOperationResult>> body,
            CancellationToken operationCancelToken = default)
            where TOperationResult : IOperationResult
        {
            return new SingleRunOperationContextInnerImpl(async token => await body(token), operationCancelToken);
        }

        private sealed class SingleRunOperationContextInnerImpl(
            Func<CancellationToken, Task<IOperationResult>> body, 
            CancellationToken operationCancelToken) : SingleRunOperationContextBase(operationCancelToken)
        {
            private readonly Func<CancellationToken, Task<IOperationResult>> body = body;

            protected override IOperationResultEx GetExceptionResult(Exception ex)
            {
                return (OperationResultEx)ex;
            }

            protected override IOperationResultEx GetNotExecutedResult()
            {
                return (OperationResultEx)"操作未执行完成";
            }

            protected override async Task<IOperationResultEx> OnExecuteAsync(CancellationToken cancellationToken)
            {
                try
                {
                    return (OperationResultEx)(await body(cancellationToken), null);
                }
                catch (Exception ex)
                {
                    return (OperationResultEx)ex;
                }
            }
        }
    }
}
