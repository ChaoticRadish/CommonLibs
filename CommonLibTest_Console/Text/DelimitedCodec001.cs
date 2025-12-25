using Common_Util.Attributes.General;
using Common_Util.Data.Constraint;
using Common_Util.Data.Mechanisms.Impl;
using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.Text
{
    internal class DelimitedCodec001() : TestBase("测试分隔符编解码器的一个默认实现 DelimitedCodec")
    {
        protected override void RunImpl()
        {
            DelimitedCodec codec = new();

            TestModel1 test = new()
            {
                Name = "ABCDE",
                Age = 123,
                Model = new TestModel2() { TestInt = -1, TestString = "Model1" },
                Enumerable = [
                    new TestModel2() { TestInt = 1, TestString = "Enumerable1" },
                    new TestModel2() { TestInt = 2, TestString = "Enumerable2" },
                    ],
                Collection = [
                    new TestModel2() { TestInt = 3, TestString = "Collection1" },
                    new TestModel2() { TestInt = 4, TestString = "Collection2" },
                    ],
                List = [
                    new TestModel2() { TestInt = 5, TestString = "List1" },
                    new TestModel2() { TestInt = 6, TestString = "List2" },
                    ],
                Array = [
                    new TestModel2() { TestInt = 7, TestString = "Array1" },
                    new TestModel2() { TestInt = 8, TestString = "Array2" },
                    ],
            };

            var result1 = codec.Serialize(test);
            WritePair(result1.FullInfoString(), split: ": \n");

            WriteLine();

            var result2 = codec.Deserialize<TestModel1>(result1.Data ?? string.Empty);
            WritePair(result2.FullInfoString(), split: ": \n");
        }

        private struct TestModel1
        {
            [SequenceFlag(0)]
            public string Name { get; set; }

            [SequenceFlag(3)]
            public int Age { get; set; }

            [SequenceFlag(5)]
            public TestModel2 Model { get; set; }

            [SequenceFlag(4)]
            public IEnumerable<TestModel2> Enumerable { get; set; }

            [SequenceFlag]
            public ICollection<TestModel2> Collection { get; set; }

            [SequenceFlag]
            public List<TestModel2> List { get; set; }

            [SequenceFlag]
            public TestModel2[] Array { get; set; }
        }

        public struct TestModel2 : IStringConveying<TestModel2>
        {
            [SequenceFlag(5)]
            public string TestString { get; set; }
            [SequenceFlag(2)]
            public int TestInt { get; set; }


            static explicit IStringConveying<TestModel2>.operator TestModel2(string s)
            {
                if (s.StartsWith('"'))
                    s = s[1..];
                if (s.EndsWith('"'))
                    s = s[..^1];
                return DelimitedCodec.Shared.Deserialize<TestModel2>(s).Data;
            }

            static explicit IStringConveying<TestModel2>.operator string(TestModel2 t)
            {
                var output = $"\"{DelimitedCodec.Shared.Serialize(t).Data ?? string.Empty}\"";
                return output;
            }
        }
    }
}
