using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Extensions
{
    public static class TaskExtensions
    {
        /// <summary>
        /// 取得可触发属性变更通知的对象 (通知完成状态)
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public static NotifyCompletedTaskWrapper GetNotify(this Task task)
        {
            return new NotifyCompletedTaskWrapper(task);
        }



        #region 取得等待输入的Task执行完成的Task
        /* 与自带的 Task.WaitAsync(CancellationToken) 的区别: 这里会捕获任务被取消的异常, 可以在取消的状况下返回输入的特定值
         */

        /// <summary>
        /// 取得等待输入的Task执行完成的Task, 直到取消等待 (不是取消传入的 task, 而是取消等待其结束的 task)
        /// </summary>
        /// <param name="task"></param>
        /// <param name="cts">可以用来取消等待</param>
        /// <returns>一个新的Task, 其内容就是等待输入的task执行完成</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task WaitUntilCancel(this Task task, out CancellationTokenSource cts)
        {
            cts = new();
            return WaitUntilCancel(task, cts.Token);
        }

        /// <summary>
        /// 取得等待输入的Task执行完成的Task, 直到取消等待 (不是取消传入的 task, 而是取消等待其结束的 task)
        /// </summary>
        /// <param name="task"></param>
        /// <param name="token"></param>
        /// <returns>一个新的Task, 其内容就是等待输入的task执行完成</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task WaitUntilCancel(this Task task, CancellationToken token)
        {
            return Task.Run(async () =>
            {
                try
                {
                    await task;
                }
                catch (OperationCanceledException)
                {
                    // 取消了执行
                    return;
                }
            }, token);
        }


        /// <summary>
        /// 取得等待输入的Task执行完成的Task, 直到取消等待 (不是取消传入的 task, 而是取消等待其结束的 task)
        /// </summary>
        /// <param name="task"></param>
        /// <param name="cts">可以用来取消等待</param>
        /// <param name="cancelValue">如果取消, 需要返回的值</param>
        /// <returns>一个新的Task, 其内容就是等待输入的task执行完成</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<T> WaitUntilCancel<T>(this Task<T> task, T cancelValue, out CancellationTokenSource cts)
        {
            cts = new();
            return WaitUntilCancel(task, cancelValue, cts.Token);
        }

        /// <summary>
        /// 取得等待输入的Task执行完成的Task, 直到取消等待 (不是取消传入的 task, 而是取消等待其结束的 task)
        /// </summary>
        /// <param name="task"></param>
        /// <param name="token"></param>
        /// <param name="cancelValue">如果取消, 需要返回的值</param>
        /// <returns>一个新的Task, 其内容就是等待输入的task执行完成</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<T> WaitUntilCancel<T>(this Task<T> task, T cancelValue, CancellationToken token)
        {
            return Task.Run(async () =>
            {
                try
                {
                    return await task.WaitAsync(token);
                }
                catch (OperationCanceledException)
                {
                    // 取消了执行
                    return cancelValue;
                }
            }, token);
        }
        #endregion
    }

    /// <summary>
    /// 通知结束状态的 Task 包装
    /// </summary>
    public class NotifyCompletedTaskWrapper : INotifyPropertyChanged
    {
        /// <summary>
        /// 需要被通知状态信息的 Task
        /// </summary>
        public Task Task { get; }
        /// <summary>
        /// 等待 Task 执行结束的 Task
        /// </summary>
        public Task WaitCompletedTask { get; }

        public NotifyCompletedTaskWrapper(Task task)
        {
            Task = task;
            WaitCompletedTask = watchTask();
        }

        #region Task属性
        public TaskStatus Status => Task.Status;

        public bool IsCompleted => Task.IsCompleted;

        public bool IsNotCompleted => !Task.IsCompleted;

        public bool IsSuccessfullyCompleted => Task.Status == TaskStatus.RanToCompletion;

        public bool IsCanceled => Task.IsCanceled;

        public bool IsFaulted => Task.IsFaulted;

        public AggregateException? Exception => Task.Exception;

        public Exception? InnerException => Exception?.InnerException;

        #endregion

        #region 变更事件
        public event EventHandler? Completed;
        private void OnCompleted()
        {
            Completed?.Invoke(this, EventArgs.Empty);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        private async Task watchTask()
        {
            try
            {
                await Task;
            }
            finally
            {
                OnTaskPropertyChanged();
                OnCompleted();
            }
        }

        private void OnTaskPropertyChanged()
        {
            if (PropertyChanged == null) return;

            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(IsCompleted));
            OnPropertyChanged(nameof(IsNotCompleted));
            if (Task.IsCanceled)
            {
                OnPropertyChanged(nameof(IsCanceled));
            }
            else if (Task.IsFaulted)
            {
                OnPropertyChanged(nameof(IsFaulted));
                OnPropertyChanged(nameof(Exception));
                OnPropertyChanged(nameof(InnerException));
            }
            else
            {
                OnPropertyChanged(nameof(IsSuccessfullyCompleted));
            }
        }

    }

}
