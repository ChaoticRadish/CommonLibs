using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Common_Util.Module.Command
{
    public interface IAsyncCommand : ICommand
    {
        /// <summary>
        /// 异步调用指令
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        Task ExecuteAsync(object? parameter);
    }

    public static class AsyncCommandHelper
    {
        /// <summary>
        /// 尝试以异步方式执行指令
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameter"></param>
        /// <returns>
        /// 可能的返回值: 
        /// 1. <see cref="Task"/> 或 <see cref="Task{T}"/> <br/>
        /// 2. <see cref="ValueTask"/> 或 <see cref="ValueTask{T}"/> <br/>
        /// 3. <see langword="null"/>: 当不是前两者时返回 <see langword="null"/> <br/>
        /// </returns>
        public static object? TryGetExecuteAsyncTask(ICommand command, object? parameter)
        {
            if (command == null) return Task.CompletedTask;

            // 本库定义的异步指令
            if (command is IAsyncCommand asyncCommand)
            {
                return asyncCommand.ExecuteAsync(parameter);
            }

            // 用反射找 ExecuteAsync(object) 方法
            var method = command.GetType().GetMethod(
                "ExecuteAsync",
                BindingFlags.Public | BindingFlags.Instance,
                null,
                new[] { typeof(object) },
                null);
            if (method != null && (
                method.ReturnType.IsAssignableTo(typeof(Task))
                || method.ReturnType == typeof(ValueTask)
                || TypeHelper.GenericTypeIsDefinitionFrom(method.ReturnType, typeof(ValueTask<>))
                ))
            {
                return method.Invoke(command, [parameter]);
            }
            // 用反射找 ExecuteAsync() 方法
            method = command.GetType().GetMethod(
                "ExecuteAsync",
                BindingFlags.Public | BindingFlags.Instance,
                null,
                Type.EmptyTypes, 
                null);
            if (method != null && (
                method.ReturnType.IsAssignableTo(typeof(Task))
                || method.ReturnType == typeof(ValueTask)
                || TypeHelper.GenericTypeIsDefinitionFrom(method.ReturnType, typeof(ValueTask<>))
                ))
            {
                return method.Invoke(command, null);
            }

            return null;
        }
    }
}
