using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util
{
    /// <summary>
    /// Task 相关的一些帮助方法
    /// </summary>
    public static class TaskHelper
    {
        /// <summary>
        /// 取得可以等待传入的所有任务执行结束的Task
        /// </summary>
        /// <param name="tasks"></param>
        /// <param name="cancellationToken">可用来取消等待</param>
        public static Task WaitAllUntilCancel(Task[] tasks, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                try
                {
                    Task.WaitAll(tasks);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }, cancellationToken);
        }
        /// <summary>
        /// 取得可以等待传入的所有任务执行结束的Task
        /// </summary>
        /// <param name="tasks"></param>
        /// <param name="cancellationToken">可用来取消等待</param>
        public static Task WaitAllUntilCancel(Task[] tasks, out CancellationTokenSource cts)
        {
            cts = new();
            return WaitAllUntilCancel(tasks, cts.Token);
        }
    }
}
