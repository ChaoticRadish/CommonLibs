using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Common_Util.Module.Command
{
    /// <summary>
    /// 最简单的命令类型
    /// <para>不触发 <see cref="CanExecuteChanged"/> </para>
    /// </summary>
    public class SampleCommand : ICommand
    {
        private readonly bool ignoreParamWhenExec;
        private readonly Action? execAction_noParam;
        private readonly Action<object?>? execAction;
        private readonly Func<object?, bool>? changeFunc;

        /// <summary>
        /// 实例化命令, 执行命令时不需要使用传入参数.
        /// </summary>
        /// <param name="execAction"></param>
        /// <param name="changeFunc">如果该参数为 null, 则无论传入什么参数, <see cref="CanExecute"/> 都将返回 true </param>
        public SampleCommand(Action execAction, Func<object?, bool>? changeFunc = null)
        {
            ignoreParamWhenExec = true;
            execAction_noParam = execAction;
            this.changeFunc = changeFunc;
        }

        /// <summary>
        /// 实例化命令
        /// </summary>
        /// <param name="execAction"></param>
        /// <param name="changeFunc">如果该参数为 null, 则无论传入什么参数, <see cref="CanExecute"/> 都将返回 true </param>
        public SampleCommand(Action<object?> execAction, Func<object?, bool>? changeFunc = null)
        {
            ignoreParamWhenExec = false;
            this.execAction = execAction;
            this.changeFunc = changeFunc;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            if (this.changeFunc == null) return true;
            return this.changeFunc.Invoke(parameter);
        }

        public void Execute(object? parameter)
        {
            if (ignoreParamWhenExec)
            {
                this.execAction_noParam!.Invoke();
            }
            else
            {
                this.execAction!.Invoke(parameter);
            }
        }
    }
}
