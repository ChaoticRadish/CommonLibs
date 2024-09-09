using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.IO
{
    public static class DirectoryHelper
    {

        /// <summary>
        /// 删除目标文件夹下的所有文件与目录
        /// </summary>
        /// <param name="path"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clear(string path)
        {
            Clear(new DirectoryInfo(path));
        }
        /// <summary>
        /// 删除目标文件夹下的所有文件与目录
        /// </summary>
        /// <param name="dir"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clear(DirectoryInfo dir)
        {
            if (!dir.Exists)
            {
                return;
            }
            foreach (FileInfo file in dir.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo subDir in dir.GetDirectories())
            {
                subDir.Delete(recursive: true);
            }

        }
    }
}
