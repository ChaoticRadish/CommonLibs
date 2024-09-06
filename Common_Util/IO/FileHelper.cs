using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.IO
{
    public static class FileHelper
    {
        /// <summary>
        /// 检查输入的文件是否存在, 如果不存在, 创建该文件. (如果文件夹不存在, 也会创建文件夹)
        /// </summary>
        /// <param name="file"></param>
        /// <exception cref="DirectoryNotFoundException">如果未能取得文件所在的文件夹路径, 将抛出异常</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MakeFileExist(string file)
        {
            if (!File.Exists(file))
            {
                string path = Path.GetDirectoryName(file) ?? throw new DirectoryNotFoundException("未取得文件所在文件夹路径! ");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path!);
                }
                File.Create(file).Dispose();    // 创建后会打开流, 需要释放一下
            }
        }

        /// <summary>
        /// 检查输入路径是否为绝对路径, 如果是相对路径, 将转换为基于当前程序所在目录的相对路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ConvertToAbsolutePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;
            if (Path.IsPathRooted(path))
            {
                return path;
            }
            else
            {
                return new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path)).AbsolutePath;
            }
        }
    }
}
