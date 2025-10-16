using System;
using System.Collections;
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

        /// <summary>
        /// 遍历输入文件夹路径下的所有文件
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="traverseAllSubfolders">指定是否遍历所有子文件夹</param>
        /// <returns></returns>
        public static IEnumerable<FileInfo> TraversalFiles(string dirPath, bool traverseAllSubfolders)
        {
            DirectoryInfo dir = new DirectoryInfo(dirPath);
            if (!dir.Exists)
            {
                yield break;
            }
            if (traverseAllSubfolders)
            {
                Stack<IEnumerator> stack = new Stack<IEnumerator>();

                bool leave = false; // 离开下层
                DirectoryInfo currentDir = dir;
                IEnumerator currentEnumerator = dir.GetDirectories().GetEnumerator();
                stack.Push(currentEnumerator);


                while (stack.Count > 0)
                {
                    if (!leave)
                    {
                        foreach (FileInfo file in currentDir.GetFiles())
                        {
                            yield return file;
                        }
                    }

                    if (currentEnumerator.MoveNext()) 
                    {
                        currentDir = (DirectoryInfo)currentEnumerator.Current;
                        currentEnumerator = currentDir.GetDirectories().GetEnumerator();
                        stack.Push(currentEnumerator);
                        leave = false;
                    }
                    else
                    {
                        stack.Pop();
                        if (stack.Count > 0)
                        {
                            currentEnumerator = stack.Peek();
                            leave = true;
                        }
                    }
                }
            }
            else
            {
                foreach (FileInfo file in dir.GetFiles())
                {
                    yield return file;
                }
            }
        }


        /// <summary>
        /// 清理最后写入时间在 <paramref name="days"/> 天前的文件 (仅清理输入目录下的文件, 不会清理子目录中的文件)
        /// </summary>
        /// <param name="path"></param>
        /// <param name="days"></param>
        public static void ClearOld(string path, double days)
        {
            ClearOld(path, DateTime.Now - TimeSpan.FromDays(days));
        }
        /// <summary>
        /// 清理最后写入时间小于输入日期的文件 (仅清理输入目录下的文件, 不会清理子目录中的文件)
        /// </summary>
        /// <param name="path"></param>
        /// <param name="date"></param>
        public static void ClearOld(string path, DateTime date)
        {
            DirectoryInfo info = new DirectoryInfo(path);
            if (info.Exists)
            {
                foreach (FileInfo file in info.GetFiles())
                {
                    if (file.LastWriteTime <= date)
                    {
                        file.Delete();
                    }
                }
            }
        }
    }
}
