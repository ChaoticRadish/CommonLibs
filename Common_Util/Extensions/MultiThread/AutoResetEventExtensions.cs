using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common_Util.Extensions.MultiThread
{
    public static class AutoResetEventExtensions
    {
        /// <summary>
        /// 尝试等待信号, 如果被取消或超时, 返回 <see langword="false"/>
        /// </summary>
        /// <param name="are"></param>
        /// <param name="timeout">超时时间, 如果是复数, 则不限制超时时间. 单位: ms</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async ValueTask<bool> TryWaitOneAsync(this AutoResetEvent are, int timeout = -1, CancellationToken? cancellationToken = null)
        {
            ArgumentNullException.ThrowIfNull(are);

            var usingCancellationToken = cancellationToken ?? CancellationToken.None;

            var tcs = new TaskCompletionSource<bool>(); // tcs.Task: 实际执行等待的 Task
            // 注册取消回调
            usingCancellationToken.Register(() => tcs.TrySetCanceled(usingCancellationToken));

            RegisteredWaitHandle registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject(
                are,
                (state, timedOut) => {
                    var localTcs = (TaskCompletionSource<bool>)state!;
                    if (timedOut)
                        localTcs.SetResult(false);
                    else
                        localTcs.SetResult(true);
                },
                tcs,
                timeout,
                executeOnlyOnce: true);

            // 当任务完成时，注销注册的等待句柄
            Task task = tcs.Task.ContinueWith(
                (_, state) => ((RegisteredWaitHandle)state!).Unregister(null),
                registeredWaitHandle,
                TaskScheduler.Default);

            // 等待任务完成，或者取消
            await Task.WhenAny(tcs.Task, task);

            return !usingCancellationToken.IsCancellationRequested && tcs.Task.Result;

        }

    }
}
