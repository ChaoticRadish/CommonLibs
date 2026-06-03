using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Wrapper.Extensions
{
    public static class IObjectProxyExtensions
    {
        /// <summary>
        /// 将 <paramref name="source"/> 的值投影到一个新的对象代理
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static IObjectProxy<TResult> Select<T, TResult>(this IObjectProxy<T> source, 
            Expression<Func<T, TResult>> selector)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);
            var selectorFunc = selector.Compile();

            Func<TResult> resultProvider = () =>
            {
                return selectorFunc(source.Object);
            };

            return new ProviderObjectProxy<TResult>(resultProvider);
        }
    }
}
