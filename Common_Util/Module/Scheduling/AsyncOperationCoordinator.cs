using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Module.Scheduling
{
    /// <summary>
    /// 异步操作协调器
    /// </summary>
    /// <remarks>
    /// 用于管理基于参数切换的异步任务执行. 使用 AI 编写实现.
    /// </remarks>
    /// <typeparam name="TArgs">执行参数的类型</typeparam>
    /// <typeparam name="TResult">执行结果的类型</typeparam>
    public class AsyncOperationCoordinator<TArgs, TResult>
    {
        private readonly Func<TArgs, CancellationToken, Task<TResult>> _func;
        private readonly IEqualityComparer<TArgs> _comparer;
        private readonly object _lock = new object();

        // 存储当前执行状态
        private Task<TResult>? _currentTask;
        private TArgs? _currentArgs;
        private CancellationTokenSource? _currentCts;

        /// <summary>
        /// 初始化协调器
        /// </summary>
        /// <param name="func">具体的异步执行逻辑，接受参数和取消令牌</param>
        /// <param name="comparer">参数比较器，用于判断新旧参数是否相同</param>
        public AsyncOperationCoordinator(
            Func<TArgs, CancellationToken, Task<TResult>> func,
            IEqualityComparer<TArgs>? comparer = null)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));
            _comparer = comparer ?? EqualityComparer<TArgs>.Default;
        }


        /// <summary>
        /// 执行或加入异步任务
        /// </summary>
        /// <param name="args">本次请求的参数</param>
        /// <returns>异步任务结果</returns>
        public Task<TResult> ExecuteAsync(TArgs args)
        {
            lock (_lock)
            {
                // 1. 检查是否有正在执行的任务 (且未完成、未取消)
                if (_currentTask != null && !_currentTask.IsCompleted)
                {
                    // 2. 检查参数是否相同
                    if (_comparer.Equals(_currentArgs, args))
                    {
                        // 相同：直接返回当前任务，调用者将等待同一个任务完成
                        return _currentTask;
                    }
                    else
                    {
                        // 不同：取消之前的任务
                        try
                        {
                            _currentCts?.Cancel();
                        }
                        catch (ObjectDisposedException)
                        {
                            // 忽略已释放的异常
                        }
                    }
                }

                // 3. 如果没有执行中的任务，或者之前的任务已取消/完成，或者需要强制重启（参数不同）
                // 则开启新的执行流程

                _currentCts = new CancellationTokenSource();
                _currentArgs = args;

                // 启动新任务
                // 注意：如果 _func 内部是立即同步抛出异常，则返回的 Task 状态为 Faulted
                _currentTask = Task.Run(() => _func(_currentArgs, _currentCts.Token), _currentCts.Token);

                return _currentTask;
            }
        }
    }
}
