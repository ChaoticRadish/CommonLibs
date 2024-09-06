using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Common_Util.Module.Command
{
    /// <summary>
    /// 无返回值的异步指令
    /// <para>不触发 <see cref="CanExecuteChanged"/> </para>
    /// </summary>
    public class AsyncCommand : ICommand
    {
        private readonly Func<CancellationToken, object?, Task> execute;
        private readonly Func<object?, bool> canExecute;

        #region 取消相关

        /// <summary>
        /// 取得取消异步指令的指令 
        /// </summary>
        public CancelAsyncCommand CancelCommand
        {
            get
            {
                cancelCommand ??= new(this);
                return cancelCommand;
            }
        }
        private CancelAsyncCommand? cancelCommand;
        #endregion

        public Task? CommandTask { get; private set; }
        public CancellationTokenSource? CancellationTokenSource { get; private set; }

        public AsyncCommand(Func<CancellationToken, object?, Task> execute, Func<object?, bool> canExecute)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }


        public bool IsExecuting { get => CommandTask != null && !CommandTask.IsCompleted; }

        #region 事件
        public event EventHandler<Task>? ExecuteStarted; 
        private void OnExecuteStarted(Task task)
        {
            ExecuteStarted?.Invoke(this, task);
        }

        public event EventHandler? CanExecuteChanged;
        private void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        public bool CanExecute(object? parameter)
        {
            return (CommandTask == null || CommandTask.IsCompleted) && this.canExecute.Invoke(parameter);
        }

        public async void Execute(object? parameter)
        {
            CancellationTokenSource = new CancellationTokenSource();
            CommandTask = execute.Invoke(CancellationTokenSource.Token, parameter);

            OnCanExecuteChanged();
            OnExecuteStarted(CommandTask);

            var notify = CommandTask.GetNotify();
            notify.Completed += (o, e) => OnCanExecuteChanged();

            await CommandTask;

        }

    }
    /// <summary>
    /// 取消无返回值异步指令的指令
    /// <para>不触发 <see cref="CanExecuteChanged"/> </para>
    /// </summary>
    public class CancelAsyncCommand : ICommand
    {
        public AsyncCommand RelateCommand { get; }

        public CancelAsyncCommand(AsyncCommand relateCommand)
        {
            RelateCommand = relateCommand;
            RelateCommand.ExecuteStarted += (o, task) =>
            {
                // 关联的指令开始于结束时均触发可执行状态的变更

                OnCanExecuteChanged();  

                var notify = task.GetNotify();
                notify.Completed +=
                    (_, _) =>
                    {
                        OnCanExecuteChanged();
                    };
            };
        }


        public event EventHandler? CanExecuteChanged;
        private void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool CanExecute(object? parameter)
        {
            return RelateCommand.IsExecuting && RelateCommand.CancellationTokenSource != null;
        }

        public void Execute(object? parameter)
        {
            RelateCommand.CancellationTokenSource?.Cancel();
        }
    }
}
