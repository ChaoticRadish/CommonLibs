using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.MD5
{
    public class MD5002() : TestBase("测试从流生成 MD5 哈希值, 使用基准测试")
    {
        protected override void RunImpl()
        {
            string fileName = GetSuggestFilePath("txt");
            FileName = fileName;
            WritePair(FileName, "测试文件");
            BenchmarkRunner.Run<MyBenchmark>(null, [FileName]);

            // 运行可知 MD5FileStream 会略微比 MD5ByteArray 慢点, 但是可以减少大量的内存申请
        }

        public static string FileName { get; set; } = string.Empty;

        [MemoryDiagnoser]
        public class MyBenchmark
        {

            [Params(1000_0, 1000_000, 1000_00000)]
            public int StringLength { get; set; }


            [ParamsSource(nameof(GetFileName))]
            public string fileName = string.Empty;
            public static IEnumerable<string> GetFileName()
            {
                return [MD5002.FileName];
            }

            [GlobalSetup]
            public void Setup()
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Create))
                {
                    using (StreamWriter sw = new(fs))
                    {
                        sw.Write(Common_Util.Random.RandomStringHelper.GetRandomEnglishString(StringLength));
                    }
                }
            }


            [Benchmark]
            public void MD5ByteArray()
            {
                Common_Util.String.MD5Helper.MD5_32(File.ReadAllBytes(fileName));
            }
            [Benchmark]
            public void MD5FileStream()
            {
                using (FileStream fs = File.OpenRead(fileName))
                {
                    Common_Util.String.MD5Helper.MD5_32(fs);
                }
            }
        }
    }
    
}
