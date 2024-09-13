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


        #region T : FileInfo
        /// <summary>
        /// 过滤以获取文件扩展名与输入的任意一项匹配的 <see cref="FileInfo"/>
        /// </summary>
        /// <remarks>
        /// 比较时将忽略大小写
        /// </remarks>
        /// <param name="files"></param>
        /// <param name="matchItems">匹配项的集合, 需要输入想要的扩展名 (不需要在前方加 '.', 如果有, 将会被移除), 例如: "txt"</param>
        /// <returns></returns>
        public static IEnumerable<FileInfo> MatchSuffix(this IEnumerable<FileInfo> files, params string[] matchItems)
        {
            var _mItems = matchItems.Select(i => '.' + i.TrimStart('.')).Distinct().ToArray();
            if (_mItems.Length == 0) yield break;

            foreach (FileInfo file in files)
            {
                if (_mItems.Any(i => i.Equals(file.Extension, StringComparison.CurrentCultureIgnoreCase)))
                {
                    yield return file;
                }
            }
        } 

        #endregion

    }
}
