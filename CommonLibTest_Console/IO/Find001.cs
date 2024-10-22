using Common_Util.Extensions;
using Common_Util.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.IO
{
    class Find001() : TestBase("简单测试遍历寻找特定字符串")
    {
        protected override void RunImpl()
        {
            run(@"D:\工作\广州华奕\项目\GreatPDR\code\Trunk\Sql", "fnGetDictData", "sql");
        }

        private void run(string dirName, string findStr, params string[] suffixs)
        {
            WriteLine($"遍历文件夹: {dirName}  寻找: {findStr}  后缀: {Common_Util.String.StringHelper.Concat(suffixs, ", ")}");
            foreach (var file in DirectoryHelper.TraversalFiles(dirName, true).MatchSuffix(suffixs))
            {
                WriteLine($"文件: {file.FullName}");
                int index = 0;
                using FileStream fs = File.OpenRead(file.FullName);
                using StreamReader sr = new StreamReader(fs);
                string? str;
                while ((str = sr.ReadLine()) != null)
                {
                    index++;
                    if (str?.Contains(findStr) == true)
                    {
                        WriteLine($"找到行：{index} \n行内容: {str}");
                    }
                }
                
            }
            
        }
    }
}
