using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Wrapper
{
    /// <summary>
    /// 对象代理
    /// </summary>
    /// <remarks>
    /// 通过包装一个对象或一系列最终取得对象的操作等方式, 以代理的形式去访问类型为 <typeparamref name="T"/> 的对象
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public interface IObjectProxy<out T> 
    {
        /// <summary>
        /// 根据具体实现, 取得对象或对象的代理对象
        /// </summary>
        T Object { get; }
    }

    #region 默认实现
    /// <summary>
    /// 通过包装实例的方式实现的对象代理
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    public readonly struct InstanceObjectProxy<T>(T obj) : IObjectProxy<T>
    {
        public T Object { get; } = obj;
    }
    /// <summary>
    /// 通过包装提供方法的方式实现的对象代理
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="providerFunc"></param>
    public readonly struct ProviderObjectProxy<T>(Func<T> providerFunc) : IObjectProxy<T>
    {
        private readonly Func<T> providerFunc = providerFunc;
        public T Object => providerFunc();
    }
    /// <summary>
    /// 固定抛出异常的对象代理
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly struct ExceptionObjectProxy<T> : IObjectProxy<T>
    {
        private readonly Exception? exception;
        private readonly Func<Exception>? getExceptionFunc;

        /// <summary>
        /// 获取 <see cref="Object"/> 时固定抛出 <see cref="NullReferenceException"/>
        /// </summary>
        public ExceptionObjectProxy()
        {
            exception = null;
            getExceptionFunc = null;
        }
        /// <summary>
        /// 获取 <see cref="Object"/> 时固定抛出 <paramref name="ex"/>
        /// </summary>
        /// <param name="ex"></param>
        public ExceptionObjectProxy(Exception ex)
        {
            exception = ex;
            getExceptionFunc = null;
        }
        /// <summary>
        /// 获取 <see cref="Object"/> 时, 通过 <paramref name="getEx"/> 获取异常对象并抛出
        /// </summary>
        /// <param name="getEx"></param>
        public ExceptionObjectProxy(Func<Exception> getEx)
        {
            exception = null;
            getExceptionFunc = getEx;
        }

        public T Object
        {
            get
            {
                if (exception != null) throw exception;
                if (getExceptionFunc != null) throw getExceptionFunc();
                throw new NullReferenceException();
            }
        }
    }

    #endregion


    public static class ObjectProxy
    {
        /// <summary>
        /// 创建一个包装实例的对象代理
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IObjectProxy<T> Create<T>(T obj)
        {
            return new InstanceObjectProxy<T>(obj);
        }
        /// <summary>
        /// 创建一个包装提供方法的对象代理
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="providerFunc"></param>
        /// <returns></returns>
        public static IObjectProxy<T> Create<T>(Func<T> providerFunc)
        {
            return new ProviderObjectProxy<T>(providerFunc);
        }

        /// <summary>
        /// 创建一个具有降级方案的对象代理
        /// </summary>
        /// <remarks>
        /// 会先从 <paramref name="proxy"/> 代理取值的对象代理, 当发生异常时, 将降级取代理 <paramref name="degradeProxy"/> 的值
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="proxy">取值代理</param>
        /// <param name="degradeProxy">降级取值代理</param>
        /// <param name="whenException">如果出现了通过 <paramref name="proxy"/> 取值过程发生异常, 则会在通过 <paramref name="degradeProxy"/> 取值前调用这个委托. </param>
        /// <returns></returns>
        public static IObjectProxy<T> CreateDegrade<T>(IObjectProxy<T> proxy, IObjectProxy<T> degradeProxy, Action<Exception>? whenException = null)
        {
            return Create(() =>
            {
                try
                {
                    return proxy.Object;
                }
                catch (Exception ex)
                {
                    whenException?.Invoke(ex);
                    return degradeProxy.Object;
                }
            });
        }
    }

}
