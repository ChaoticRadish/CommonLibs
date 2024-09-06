using Common_Util.Attributes.General;
using Common_Util.Data.Wrapped;
using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.DataWrapper
{
    internal class ShowText001() : TestBase("枚举的显示文本的包装器, 基础功能测试")
    {
        protected override void RunImpl()
        {
            EnumTest test1 = TestEnum.AAA;
            EnumTest test2 = TestEnum.BBB;
            EnumTest test3 = (TestEnum?)null;
            var list = EnumShowTextWrapperHelper.CreateList<TestEnum, EnumTest>();

            WritePair(test1.ShowText);
            WritePair(test2.ShowText);
            WritePair(test3.ShowText);
            WritePair(list.Select(i => i.ShowText).ToArray().FullInfoString());
            WritePair(list.Append(EnumTest.Empty).Select(i => i.ShowText).ToArray().FullInfoString());
            WritePair(list.InsertHead(EnumTest.Empty).Select(i => i.ShowText).ToArray().FullInfoString());
        }

        public enum TestEnum
        {
            AAA,
            [EnumDesc("测试B")]
            BBB,
            CCC,
        }

        public class EnumTest : EnumShowTextWrapper<TestEnum?>
        {
            public EnumTest() : base() { }
            public EnumTest(TestEnum? value, string? showText = null) : base(value, showText) { }
        
            public static EnumTest Empty { get; } = new(null, "空选项");
            public static EnumShowTextWrapper<TestEnum?> Empty2 { get; set; } = new(null, "空选项2");

            #region 隐式转换
            public static implicit operator TestEnum?(EnumTest wrapper)
            {
                return wrapper == null ? default : wrapper.Value;
            }
            public static implicit operator EnumTest(TestEnum? property)
            {
                return property == null ? Empty : new(property);
            }
            public static implicit operator EnumTest((TestEnum? property, string showText) obj)
            {
                return new(obj.property, obj.showText);
            }
            public static implicit operator EnumTest((string showText, TestEnum? property) obj)
            {
                return new(obj.property, obj.showText);
            }
            #endregion
        }

    }
}
