using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Extensions
{
    public static class IEnumerableExtensions
    {


        #region T : any
        /// <summary>
        /// 调用集合内的所有实现 <see cref="IDisposable"/> 项的 <see cref="IDisposable.Dispose"/> 方法, 释放所有资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        public static void TryDisposeAll<T>(this IEnumerable<T> array)
        {
            foreach (T t in array)
            {
                if (t is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }


        #endregion



        #region T : IDisposable
        /// <summary>
        /// 调用集合内的所有项的<see cref="IDisposable.Dispose"/>方法, 释放所有资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        public static void DisposeAll<T>(this IEnumerable<T> array)
            where T : IDisposable
        {
            foreach (T t in array) t?.Dispose();
        }
        #endregion




    }
}
