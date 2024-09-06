using Common_Util.Data.Structure.Value;
using Common_Util.Extensions;
using Common_Util.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.IO
{
    internal class TempFile001() : TestBase("简单测试 TempFileHelper ")
    {
        protected override void RunImpl()
        {
            TempFileHelper.Logger = GetLevelLogger("临时文件帮助类");
            RunTest(test1, "测试 1");
            RunTest(test2, "测试 2");
        }

        private void test1()
        {
            var logger = GetLevelLogger("测试 1 ");

            using var tempFile = TempFileHelper.NewTempFile();

            using var fs1 = tempFile.OpenStream();

            logger.Info("此时的文件流1: \n " + fs1.FullInfoString());

            logger.Info("取得临时文件: \n" + tempFile.FullInfoString());

            StreamWriter sw = new(fs1);

            logger.Info("随便写入点东西");

            sw.WriteLine("asdasdasdasdasdqweqawsdazfase");

            logger.Info("此时的文件流1: \n " + fs1.FullInfoString());

            logger.Info("流写入器.Flush()");
            sw.Flush();

            logger.Info("此时的文件流1: \n " + fs1.FullInfoString());


            logger.Info("再随便写入点东西");

            sw.WriteLine("123123213");

            logger.Info("此时的文件流1: \n " + fs1.FullInfoString());

            logger.Info("关闭流写入器");

            sw.Close();

            logger.Info("此时的文件流1: \n " + fs1.FullInfoString());

            using var fs2 = tempFile.OpenStream();


            StreamReader sr = new StreamReader(fs2);

            string str = sr.ReadToEnd();

            logger.Info("临时文件内的全文: \n" + str);

            logger.Info("此时的文件流2: \n " + fs2.FullInfoString());

            sr.Close();
        }
        private void test2()
        {
            var logger = GetLevelLogger("测试 2 ");

            TempFileHelper.CustomTempFileDir = Path.Combine(GetTestDir());
            using var tempFile = TempFileHelper.NewTempFile();

            using var fs1 = tempFile.OpenStream();

            logger.Info("此时的文件流1: \n " + fs1.FullInfoString());

            logger.Info("取得临时文件: \n" + tempFile.FullInfoString());

            Thread.Sleep(1000);

            StreamWriter sw = new(fs1);

            logger.Info("随便写入点东西");

            sw.WriteLine("asdasdasdasdasdqweqawsdazfase");

            logger.Info("此时的文件流1: \n " + fs1.FullInfoString());
            Thread.Sleep(1000);

            logger.Info("流写入器.Flush()");
            sw.Flush();

            logger.Info("此时的文件流1: \n " + fs1.FullInfoString());
            Thread.Sleep(1000);


            logger.Info("再随便写入点东西");

            sw.WriteLine("123123213");

            logger.Info("此时的文件流1: \n " + fs1.FullInfoString());
            Thread.Sleep(1000);

            logger.Info("关闭流写入器");

            sw.Close();

            logger.Info("此时的文件流1: \n " + fs1.FullInfoString());
            Thread.Sleep(1000);

            using var fs2 = tempFile.OpenStream();


            StreamReader sr = new StreamReader(fs2);

            string str = sr.ReadToEnd();

            logger.Info("临时文件内的全文: \n" + str);

            logger.Info("此时的文件流2: \n " + fs2.FullInfoString());
            Thread.Sleep(1000);

            FileSegment fsegment = new FileSegment(tempFile.Path);
            WriteLine(fsegment);

            sr.Close();
        }
    }
}
