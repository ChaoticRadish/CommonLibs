using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.Hash
{
    internal class SHA256_001() : TestBase("测试使用 SHA_256 算法生成哈希值")
    {
        protected override void RunImpl()
        {
            RunTest(nameof(test01));
            RunTest(nameof(test01));
            RunTest(nameof(test01));
        }

        const string TestSource1 = "测试使用 SHA_256 算法生成哈希值";

        [TestMethod("1. 字符串以 UTF-8 格式转换为 byte[] 再计算哈希值")]
        private void test01()
        {
            string testSource = TestSource1;
            byte[] bs = Encoding.UTF8.GetBytes(testSource);
            WriteLine("UTF-8 数据: ");
            WriteLine(bs.ToHexString());

            SHA256 sha256 = SHA256.Create();
            byte[] result = sha256.ComputeHash(bs);
            WriteLine("哈希值: ");
            WriteLine(result.ToHexString());

        }
    }
}
