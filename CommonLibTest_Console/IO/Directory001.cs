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
            string strPath = "D:\\笔记";

            foreach (var file in DirectoryHelper.TraversalFiles(strPath, true))
            {
                WriteLine(file.FullName);
            }
            WriteEmptyLine();
            foreach (var file in DirectoryHelper.TraversalFiles(strPath, false))
            {
                WriteLine(file.FullName);
            }
        }
    }
}
