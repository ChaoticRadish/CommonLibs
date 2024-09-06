using Common_Util.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Common_Util.Module.LayerComponentBaseLong;

namespace CommonLibTest_Console.IO
{
    internal class Path001() : TestBase("测试 PathHelper.GetAbsolutePath 求一个相对路径相对于一个绝对路径的绝对路径")
    {

        private string absPath = "C:\\A\\B\\C.mf";


        protected override void RunImpl()
        {
            test("..\\..\\asd.psd");
            test("..\\..\\..\\..\\asd.psd");
            test(".\\asd.psd");
            test("asd.psd");
            test(".\\Q\\asd.psd");
            test("Q\\asd.psd");

            test2("D:\\ASD.DDD");
        }
        private void test(string relatively)
        {
            WriteEmptyLine();
            string result = PathHelper.GetAbsolutePath(absPath, relatively);
            WriteLine("绝对路径: " + absPath);
            WriteLine("相对路径: " + relatively);
            WriteLine("结果: " + result);
            WriteLine("结果相对于绝对路径的相对路径: " + PathHelper.GetRelativelyPath(absPath, result));
        }

        private void test2(string absolute)
        {
            WriteEmptyLine();

            WriteLine("绝对路径1: " + absPath);
            WriteLine("绝对路径2: " + absolute);

            WriteLine("2相对于1的相对路径: " + PathHelper.GetRelativelyPath(absPath, absolute));
        }
    }
}
