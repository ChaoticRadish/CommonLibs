using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.IO
{
    internal class Path002() : TestBase("检查文件名是否超长")
    {
        protected override void RunImpl()
        {
            string str = GetSuggestFilePath("");
            foreach (var i in 300.ForUntil())
            {
                test(str);
                str += '1';
            }
        }

        public void test(string path)
        {
            WriteLine($"路径: {path}  长度: {path.Length}");
            try
            {
                FileInfo fi = new FileInfo(path);
                var fs = fi.Create();
                fs.Close();
                fs.Dispose();
                fi.Delete();
                WriteLine($"正常, 完全限定的文件名长度: {fi.FullName.Length} 文件名长度: {fi.Name.Length}");
                if (fi.Name.Length == 255)
                {

                }
            }
            catch (Exception ex)
            {
                WriteLine("异常: " + ex.Message);
            }

            WriteEmptyLine();
        }
    }
}
