using ChaoticKit;
using ChaoticKit.Enums;
using ChaoticKit.Extensions.EnumType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaoticKit.LibTest.Console.Text
{
    internal class TextTypeEnum001() : TestBase("对文本类型枚举的扩展方法的简单测试")
    {
        protected override void RunImpl()
        {
            EnumHelper.ForEach<TextTypeEnum>(t =>
            {
                try
                {
                    WritePair($"{t} => {t.DefaultContentTypeValue()}");
                }
                catch (Exception ex)
                {
                    WriteLine(t.ToString() + " => " + ex.ToString());
                }
            });
        }
    }
}
