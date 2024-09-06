using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Interfaces.Behavior
{
    /// <summary>
    /// 会发出关闭信号的东西
    /// </summary>
    public interface ICloseSignal
    {
        /// <summary>
        /// 关闭信号产生事件
        /// </summary>
        public event EventHandler<object?>? OnCloseSignal;
    }
    /// <summary>
    /// 会发出关闭信号的东西, 同时将附带一个指定类型的东西
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICloseSignal<T> : ICloseSignal
    {
        /// <summary>
        /// 关闭信号产生事件
        /// </summary>
        public new event EventHandler<T?>? OnCloseSignal;
    }

    /// <summary>
    /// 包含 <see cref="ICloseSignal{T}"/> 默认实现的基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class CloseSignalBase<T> : ICloseSignal<T>
    {
        public event EventHandler<T?>? OnCloseSignal;

        private event EventHandler<object?>? baseInterfaceOnCloseSignal;
        event EventHandler<object?>? ICloseSignal.OnCloseSignal
        {
            add
            {
                baseInterfaceOnCloseSignal += value;
            }

            remove
            {
                baseInterfaceOnCloseSignal -= value;
            }
        }

        /// <summary>
        /// 触发关闭信号事件
        /// </summary>
        /// <param name="arg"></param>
        protected void TriggerCloseSignal(T? arg)
        {
            OnCloseSignal?.Invoke(this, arg);
            baseInterfaceOnCloseSignal?.Invoke(this, arg);
        }
    }
}
