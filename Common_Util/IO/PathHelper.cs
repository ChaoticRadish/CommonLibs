using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.IO
{
    /// <summary>
    /// 路径帮助类
    /// </summary>
    public static class PathHelper
    {
        #region 相对路径转换
        /// <summary>
        /// 取得相对路径 (传入参数 relatively) 相对于绝对路径 (传入路径 absolute) 的绝对路径
        /// <para>简单理解为: absolute + relatively = 绝对路径返回值</para>
        /// </summary>
        /// <param name="absolute"></param>
        /// <param name="relatively"></param>
        /// <returns></returns>
        public static string GetAbsolutePath(string absolute, string relatively) 
        {
            Uri uriA = new Uri(absolute, UriKind.Absolute);
            Uri uriR = new Uri(relatively, UriKind.Relative);
            return new Uri(uriA, uriR).LocalPath;
        }

        /// <summary>
        /// 取得两个路径间的相对路径 (path2 相对于 path1)
        /// </summary>
        /// <param name="path1"></param>
        /// <param name="path2"></param>
        /// <returns></returns>
        public static string GetRelativelyPath(string path1, string path2)
        {
            Uri uri1 = new Uri(Path.GetFullPath(path1));
            Uri uri2 = new Uri(Path.GetFullPath(path2));
            return uri1.MakeRelativeUri(uri2).ToString();
        }
        #endregion
    }
}
