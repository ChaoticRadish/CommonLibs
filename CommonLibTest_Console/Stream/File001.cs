using Common_Util.Data.Structure.Value;
using Common_Util.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.Stream
{
    internal class File001() : TestBase("文件流读取测试")
    {
        private string testFile = string.Empty;



        protected override void RunImpl()
        {
            testFile = GetSuggestFilePath("txt");

            RunTest(createTestFile, "创建测试文件");
            RunTest(readTestFile1, "读取测试文件");
            RunTest(editTestFile1, "编辑测试文件 1");
            RunTest(readTestFile2, "读取测试文件");
            RunTest(editTestFile2, "编辑测试文件 2");
            RunTest(readTestFile2, "读取测试文件");
            RunTest(editTestFile3, "编辑测试文件 3");
            RunTest(readTestFile2, "读取测试文件");

            RunTest(readTestFile3, "读取测试文件片段");
        }

        private void createTestFile()
        {
            using FileStream fs = File.Open(testFile, FileMode.Create);
            using StreamWriter sw = new StreamWriter(fs);

            WriteLine("文件流.Seek(2, SeekOrigin.Begin)");
            fs.Seek(2, SeekOrigin.Begin);
            WritePair("文件流当前长度", fs.Length);
            WritePair("文件流当前位置", fs.Position);

            string writeText = Common_Util.Random.RandomStringHelper.GetRandomUpperEnglishString(45);
            WritePair(key: "等待写入的文本", writeText);

            WriteLine("流写入器: 写入");
            sw.Write(writeText);
            sw.Flush();

            sw.Write("+++");
            sw.Flush();
            fs.Seek(0, SeekOrigin.Begin);
            sw.Write("oo");
            sw.Flush();
            fs.Seek(0, SeekOrigin.End);
            sw.Write("**");
            sw.Flush();

            WriteLine("流写入器.Flush()");
            sw.Flush();

            WritePair("文件流当前长度", fs.Length);
            WritePair("文件流当前位置", fs.Position);


            //WritePair("fs.CanWrite", fs.CanWrite);

            //WriteLine("流写入器2: 实例化");
            //using StreamWriter sw2 = new StreamWriter(fs);
            //WriteLine("流写入器2: 写入: aaaaaa");
            //sw2.WriteLine("aaaaaa");
            //WriteLine("流写入器2.Flush()");
            //sw2.Flush();

            //WritePair("fs.CanWrite", fs.CanWrite);


            WritePair("文件流当前长度", fs.Length);
            WritePair("文件流当前位置", fs.Position);

            WriteLine("文件流.Seek(0, SeekOrigin.Begin)");
            fs.Seek(0, SeekOrigin.Begin);

            WritePair("文件流当前长度", fs.Length);
            WritePair("文件流当前位置", fs.Position);

            using OffsetWrapperStream ows = new(fs, 10, 10);
            WriteLine("实例化偏移流完成: ");
            WritePair("偏移流起点", ows.WrapperStart);
            WritePair("偏移流终点", ows.WrapperEnd);
            WritePair("偏移流当前位置", ows.Position);
            WritePair("偏移流限制长度", ows.LimitLength);
            WritePair("偏移流当前长度", ows.Length);

            using StreamWriter sw3 = new StreamWriter(ows);

            WriteLine("偏移流写入字符串: 123456");
            sw3.Write("123456");
            WritePair("偏移流当前位置", ows.Position);
            WritePair("偏移流当前长度", ows.Length);

            WriteLine("偏移流写入字符串: 987654");
            sw3.Write("987654");
            WritePair("偏移流当前位置", ows.Position);
            WritePair("偏移流当前长度", ows.Length);

            WriteLine("偏移流写入字符串: 121212");
            sw3.Write("121212");
            WritePair("偏移流当前位置", ows.Position);
            WritePair("偏移流当前长度", ows.Length);
        }

        private void readTestFile1()
        {
            using FileStream fs = File.Open(testFile, FileMode.Open);

            using StreamReader sr1 = new StreamReader(fs);
            string text = sr1.ReadToEnd();
            WritePair(key: "全文内容", text);
            WritePair(key: "全文长度", text.Length);

            using OffsetWrapperStream ows = new(fs, 10, 10);
            WriteLine("实例化偏移流完成: ");
            WritePair("偏移流起点", ows.WrapperStart);
            WritePair("偏移流终点", ows.WrapperEnd);
            WritePair("偏移流当前长度", ows.Length);

            using StreamReader sr2 = new StreamReader(ows);

            WritePair("偏移流当前位置", ows.Position);
            WritePair("偏移流当前长度", ows.Length);
            string read = sr2.ReadToEnd();

            WritePair(key: "读取到内容", read);


            WriteLine("偏移流.Seek(0, SeekOrigin.Begin)");
            ows.Seek(0, SeekOrigin.Begin);


            WritePair("偏移流当前位置", ows.Position);
            WritePair("偏移流当前长度", ows.Length);
            string read2 = sr2.ReadToEnd();

            WritePair(key: "读取到内容", read2);

        }
        private void readTestFile2()
        {
            using FileStream fs = File.Open(testFile, FileMode.Open);

            using StreamReader sr1 = new StreamReader(fs);
            string text = sr1.ReadToEnd();
            WritePair(key: "全文内容", text);
            WritePair(key: "全文长度", text.Length);
        }
        private void readTestFile3()
        {
            using var stream = new FileSegment()
            {
                FullName = testFile,
                Start = 10,
                Length = 10,
            }.OpenStream();

            using StreamReader sr1 = new StreamReader(stream);
            WriteLine(sr1.ReadToEnd());
        }
        private void editTestFile1()
        {
            WriteLine("打开文件流");
            using FileStream fs = File.Open(testFile, FileMode.Open);
            WriteLine("打开偏移流, start: 4, length: 4");
            using OffsetWrapperStream ows = new(fs, 4, 4);
            using StreamWriter sw = new StreamWriter(ows);

            ows.Seek(0, SeekOrigin.Begin);
            WriteLine("写入: chaotic");
            sw.Write("chaotic");

            WriteLine("偏移流.SetLength(7)");
            ows.SetLength(7);

            ows.Seek(0, SeekOrigin.Begin);
            WriteLine("写入: chaotic");
            sw.Write("chaotic");

            WriteLine("写入器.Flush()");
            sw.Flush();


            WriteLine("打开偏移流2, start: 48, length: 5");
            using OffsetWrapperStream ows2 = new(fs, 48, 5) ;
            using StreamWriter sw2 = new StreamWriter(ows2);
            ows2.Seek(0, SeekOrigin.Begin);

            WriteLine("写入: ...............");
            sw2.Write("...............");

            WriteLine("写入器2.Flush()");
            sw2.Flush();




        }

        private void editTestFile2()
        {
            WriteLine("打开文件流");
            using FileStream fs = File.Open(testFile, FileMode.Open);

            WriteLine("打开偏移流, start: 48, length: null");
            using OffsetWrapperStream ows3 = new(fs, 48, null);
            using StreamWriter sw3 = new StreamWriter(ows3);
            ows3.Seek(0, SeekOrigin.Begin);

            WriteLine("写入: *****************");
            sw3.Write("*****************");

            WriteLine("写入器3.Flush()");
            sw3.Flush();
        }
        private void editTestFile3()
        {
            WriteLine("打开文件流");
            using FileStream fs = File.Open(testFile, FileMode.Open);

            WriteLine("打开偏移流, start: 60, length: null");
            using OffsetWrapperStream ows4 = new(fs, 60, null);
            using StreamWriter sw4 = new StreamWriter(ows4);
            ows4.Seek(0, SeekOrigin.End);

            WriteLine("写入: ---");
            sw4.Write("---");
            sw4.Flush();
            WriteLine("写入: ~~~~");
            sw4.Write("~~~~");
            sw4.Flush();
            WriteLine("Seek(0, SeekOrigin.Begin)");
            ows4.Seek(0, SeekOrigin.Begin);
            WriteLine("写入: !!");
            sw4.Write("!!");
            sw4.Flush();
            WriteLine("Seek(1, SeekOrigin.End)");
            ows4.Seek(1, SeekOrigin.End);
            WriteLine("写入: ??");
            sw4.Write("??");
            sw4.Flush();
            WriteLine("Seek(-3, SeekOrigin.Current)");
            ows4.Seek(-3, SeekOrigin.Current);
            WriteLine("写入: ^");
            sw4.Write("^");
            sw4.Flush();
        }
    }
}
