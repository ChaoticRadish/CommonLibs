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
        #region 路径类型判断
        /// <summary>
        /// 简单得判断路径是否绝对路径, 适用于 Window 和 Unix/Linux
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsAbsolutePathSimpleCheck(string path)
        {
            // 检查是否为Windows绝对路径
            if (Path.IsPathRooted(path) && Path.VolumeSeparatorChar == ':' && path.IndexOf(Path.VolumeSeparatorChar) > 0)
            {
                return true;
            }

            // 检查是否为Unix/Linux绝对路径
            if (path.StartsWith('/'))
            {
                return true;
            }

            // 如果以上都不满足，则为相对路径
            return false;
        }

        #endregion

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

        #region 绝对路径与相对路径的分支情况

        /// <summary>
        /// 判断一个路径是否绝对路径, 如果是绝对路径, 就直接返回, 如果不是, 就以当前应用程序域的根目录作为基准, 取输入路径的绝对路径
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isAbsoluteCheck">判断输入路径是否绝对路径的方法, 如果为 <see langword="null"/> 则采用 <see cref="IsAbsolutePathSimpleCheck(string)"/></param>
        /// <returns></returns>
        public static string RelativelyPathToAppBaseDirectory(string path, Func<string, bool>? isAbsoluteCheck = null)
        {
            isAbsoluteCheck ??= IsAbsolutePathSimpleCheck;
            if (isAbsoluteCheck(path))
            {
                return path;
            }
            else
            {
                return GetAbsolutePath(AppDomain.CurrentDomain.BaseDirectory, path);
            }
        }

        #endregion
    }
}
