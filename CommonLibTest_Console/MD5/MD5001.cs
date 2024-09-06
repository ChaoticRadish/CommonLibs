using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.MD5
{
    internal class MD5001() : TestBase("测试从流生成 MD5 哈希值")
    {
        protected override void RunImpl()
        {
            string fileName = GetSuggestFilePath("txt");
            using (FileStream fs =  new FileStream(fileName, FileMode.Create))
            {
                using (StreamWriter sw = new(fs))
                {
                    sw.Write(Common_Util.Random.RandomStringHelper.GetRandomEnglishString(1000000));
                }
            }
            WriteLine("创建测试文件完成");

            string md5_bs, md5_stream;

            md5_bs = Common_Util.String.MD5Helper.MD5_32(File.ReadAllBytes(fileName));
            WritePair(md5_bs, "读取所有byte后生成MD5");

            using (FileStream fs = File.OpenRead(fileName))
            {
                md5_stream = Common_Util.String.MD5Helper.MD5_32(fs);
                WritePair(md5_stream, "打开流后生成MD5");
            }

            WritePair(md5_bs == md5_stream, "md5_bs == md5_stream");

        }
        
    }
}
