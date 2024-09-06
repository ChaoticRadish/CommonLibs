using Common_Util.Attributes.General;
using Common_Util.Extensions;
using Common_Util.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.Text
{
    public class FullStringInfo001() : TestBase("测试获取对象信息的默认实现 （参数有数据流）")
    {
        protected override void RunImpl()
        {
            byte[] bs = [11, 22, 33, 44, 55, 66];
            using MemoryStream ms = new MemoryStream(bs);
            TestClass testClass = new TestClass()
            {
                GValueA = "asidjasdjpasd",
                Stream = new(ms, 2, 4),
                GValueB = "123123awsedqwe213",
                GValueC = new()
                {
                    A = "wqae1d5qaw14e",
                    B = "12"
                }
            };
            WriteLine(testClass.FullInfoString());
        }

        public class TestClass
        {
            public string GValueA { get; set; } = string.Empty;

            public OffsetWrapperStream? Stream { get; set; }

            public string GValueB { get; set; } = string.Empty;

            [InfoToString]
            public TestClassB? GValueC { get; set; }
        }

        public class TestClassB
        {
            public string? A { get; set; }
            public string? B { get; set; }

            public override string ToString()
            {
                return $"A:{A}; B:{B}";
            }
        }
    }
}
