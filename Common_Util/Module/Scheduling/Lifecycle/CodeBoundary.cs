using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Module.Scheduling.Lifecycle
{
    /// <summary>
    /// 提供创建代码边界的方法，用于在代码块的进入和退出时执行指定的操作。
    /// </summary>
    public static class CodeBoundary
    {
        /// <summary>
        /// 创建一个在当进入与离开代码边界时, 执行特定 <see cref="Action"/> 的代码边界
        /// </summary>
        /// <remarks>
        /// <paramref name="onEnter"/> 和 <paramref name="onExit"/> 都不会发生上下文切换, 会
        /// </remarks>
        /// <see cref=""/>
        /// <param name="onEnter"></param>
        /// <param name="onExit"></param>
        /// <returns>释放时执行 <paramref name="onExit"/></returns>
        public static IDisposableCodeBoundary Create(Action? onEnter, Action? onExit)
        {
            onEnter?.Invoke();
            return new EnterExitHandle()
            {
                DisposableBody = onExit,
                Context = null,
            };
        }
        /// <summary>
        /// 创建一个在当进入与离开代码边界时, 在特定的同步上下文中, 执行特定 <see cref="Action"/> 的代码边界
        /// </summary>
        /// <param name="context">同步上下文, 如果是 <see langword="null"/>, 则不切换上下文直接执行, 反之将使用 <see cref="SynchronizationContext.Send(SendOrPostCallback, object?)"/> 执行</param>
        /// <param name="onEnter"></param>
        /// <param name="onExit"></param>
        /// <returns>释放时执行 <paramref name="onExit"/></returns>
        public static IDisposableCodeBoundary CreateOnContextOrDefault(SynchronizationContext? context, Action? onEnter, Action? onExit)
        {
            if (onEnter != null)
            {
                if (context == null) onEnter();
                else context.Send(_ => onEnter(), null);
            }
            return new EnterExitHandle()
            {
                DisposableBody = onExit,
                Context = context,
            };
        }
        /// <summary>
        /// 尝试获取当前同步上下文以创建执行特定内容的代码边界
        /// </summary>
        /// <remarks>
        /// 如果可以取得当前同步上下文, 则用其调用 <see cref="CreateOnContextOrDefault(SynchronizationContext?, Action?, Action?)"/> 来创建, <br/>
        /// 否则会传入 <see langword="null"/>, 也就是直接执行
        /// </remarks>
        /// <param name="onEnter"></param>
        /// <param name="onExit"></param>
        /// <returns></returns>
        public static IDisposableCodeBoundary CreateOnCurrentOrDefault(Action? onEnter, Action? onExit)
        {
            var context = SynchronizationContext.Current;
            return CreateOnContextOrDefault(context, onEnter, onExit);
        }
        /// <summary>
        /// 尝试获取当前同步上下文以创建执行特定内容的代码边界
        /// </summary>
        /// <remarks>
        /// 一般用于进入上下文需要切换状态, 离开时需要切换回来的情况
        /// </remarks>
        /// <param name="invokeAction">需要执行的内容</param>
        /// <param name="enterArg">进入时调用的参数, 离开时会取相反的值</param>
        /// <returns></returns>
        public static IDisposableCodeBoundary CreateOnCurrentOrDefault(Action<bool> invokeAction, bool enterArg)
        {
            var context = SynchronizationContext.Current;
            return CreateOnContextOrDefault(context, () => invokeAction(enterArg), () => invokeAction(!enterArg));
        }
        /// <summary>
        /// 尝试获取当前同步上下文以创建执行特定内容的代码边界
        /// </summary>
        /// <remarks>
        /// 一般用于进入上下文需要切换状态, 离开时需要再次切换的情况
        /// </remarks>
        /// <param name="invokeAction">需要执行的内容</param>
        /// <param name="enterArg">进入时调用的参数</param>
        /// <param name="exitArg">离开时调用的参数</param>
        /// <returns></returns>
        public static IDisposableCodeBoundary CreateOnCurrentOrDefault<T>(Action<T> invokeAction, T enterArg, T exitArg)
        {
            var context = SynchronizationContext.Current;
            return CreateOnContextOrDefault(context, () => invokeAction(enterArg), () => invokeAction(exitArg));
        }


        private class EnterExitHandle : IDisposableCodeBoundary
        {
            private bool disposedValue;

            public required Action? DisposableBody { get; init; }
            public required SynchronizationContext? Context { get; init; }

            public void Dispose()
            {
                if (!disposedValue)
                {
                    if (DisposableBody != null)
                    {
                        if (Context == null) DisposableBody();
                        else Context.Send(_ => DisposableBody(), null);
                    }
                    disposedValue = true;
                }
                GC.SuppressFinalize(this);
            }
        }
    }
    /// <summary>
    /// 代码边界接口
    /// </summary>
    public interface ICodeBoundary 
    {

    }
    /// <summary>
    /// 可释放的代码边界接口
    /// </summary>
    public interface IDisposableCodeBoundary : ICodeBoundary, IDisposable
    {

    }

}
