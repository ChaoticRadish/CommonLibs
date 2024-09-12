using Common_Util.Extensions;
using Common_Util.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.IO
{
    internal class Directory001() : TestBase("测试文件夹帮助类遍历方法")
    {
        protected override void RunImpl()
        {
            string strPath = Path.GetFullPath("..\\..\\..");
            WriteLine("遍历路径: " + strPath);

            WriteLine("遍历所有子文件夹");
            foreach (var file in DirectoryHelper.TraversalFiles(strPath, true))
            {
                WriteLine(file.FullName);
            }
            WriteLine("仅遍历根文件夹");
            foreach (var file in DirectoryHelper.TraversalFiles(strPath, false))
            {
                WriteLine(file.FullName);
            }


            WriteLine("遍历所有子文件夹中的 cs 文件");
            foreach (var file in DirectoryHelper.TraversalFiles(strPath, true).MatchSuffix("cs"))
            {
                WriteLine(file.FullName);
            }
            WriteLine("仅遍历根文件夹中的 cs 文件");
            foreach (var file in DirectoryHelper.TraversalFiles(strPath, false).MatchSuffix("cs"))
            {
                WriteLine(file.FullName);
            }



            WriteLine("遍历所有子文件夹中的 cs 文件或 txt 文件");
            foreach (var file in DirectoryHelper.TraversalFiles(strPath, true).MatchSuffix("cs", "TXt"))
            {
                WriteLine(file.FullName);
            }
            WriteLine("仅遍历根文件夹中的 cs 文件或 txt 文件");
            foreach (var file in DirectoryHelper.TraversalFiles(strPath, false).MatchSuffix("cs", "TXt"))
            {
                WriteLine(file.FullName);
            }

        }
    }
}
