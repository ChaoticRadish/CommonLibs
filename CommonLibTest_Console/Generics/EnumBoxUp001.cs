using Common_Util;
using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CommonLibTest_Console.Generics.EnumBoxUp001;

namespace CommonLibTest_Console.Generics
{
    internal class EnumBoxUp001() : TestBase("枚举值作为泛型测试, 测试装箱")
    {
        protected override void RunImpl()
        {
            try
            {
                var t1 = new GTest<Test001>()
                {
                    Value = new(),
                };
                var t2 = new GTest<Test001>()
                {
                    Value = null,
                };
                var t3 = new GEnumTest<Test002>()
                {
                    Value = Test002.AAA,
                };
                var t4 = new GEnumTest<Test002?>()
                {
                    Value = null,
                };
                var t5 = new GEnumTest2()
                {
                    Value = null,
                };

                var t6 = new GEnumTest2()
                {
                    Value = Test002.BBB,
                };

                var t7 = new GEnumTest<Test001>()
                {
                    Value = new Test001() { x = 11},
                };

                WritePair(t1);
                WritePair(t2);
                WritePair(t3);
                WritePair(t4);
                WritePair(t5);
                WritePair(t6);
                WritePair(t7);

            }
            catch (Exception ex)
            {
                WriteLine(ex);
            }
        }

        public class Test001
        {
            public int x;
            public override string ToString()
            {
                return $"Test001 => x: {x}";
            }
        }
        public enum Test002
        {
            AAA,
            BBB,
            CCC,
        }

        public class GTest<T>
        {
            public T? Value { get; set; }

            public override string ToString()
            {
                return Value == null ? "<null>" : Value.ToString() ?? "Empty";
            }
        }
        public class GEnumTest<T> : GTest<T?>
        {
            static GEnumTest()
            {
                Type type = typeof(T);
                if (!type.IsEnum(true))
                {
                    throw new InvalidOperationException($"泛型类型 {nameof(T)} 必须是枚举类型");
                }
            }
        }
        public class GEnumTest2 : GEnumTest<Test002?>
        {

        }
    }
}
