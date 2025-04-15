using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.Stream
{
    internal class Char001() : TestBase("测试流转换为字符集合")
    {
        protected override void RunImpl()
        {
            base.RunTestMark();
        }

        [TestMethod]
        private void test1()
        {
            byte[] byteArray = Encoding.UTF8.GetBytes("你好，世界！");
            using MemoryStream memoryStream = new MemoryStream(byteArray);

            WritePair(memoryStream.AsCharEnumerable().Count());
            memoryStream.Seek(0, SeekOrigin.Begin);
            WritePair(new string(memoryStream.AsCharEnumerable().ToArray()));
        }
        [TestMethod]
        private void test2()
        {
            byte[] byteArray = Encoding.UTF8.GetBytes("你好，世界！");
            using MemoryStream memoryStream = new MemoryStream(byteArray);

            WritePair(memoryStream.AsCharEnumerable().Count());
            WritePair(new string(memoryStream.AsCharEnumerable().ToArray()));
        }
    }
}
