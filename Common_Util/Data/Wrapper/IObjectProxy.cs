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
    }

}
