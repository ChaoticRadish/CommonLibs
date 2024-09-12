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
                    }
                    else
                    {
                        currentEnumerator = stack.Pop();
                        leave = true;
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
    }
}
