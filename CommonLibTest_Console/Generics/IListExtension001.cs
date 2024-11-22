using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.Generics
{
    internal class IListExtension001() : TestBase("测试新增的获取值的扩展方法")
    {
        protected override void RunImpl()
        {
            int[] intArr1 = [1, 2, 3, 4, 5];
            int?[] intArr2 = [1, 2, null, 4, 5];
            string[] strArr3 = ["1", "2", "3", "4", "5"];

            if (intArr1.TryGet(3, out var _val1))
            {
                int i = _val1;
            }
            if (intArr2.TryGet(3, out var _val2))
            {
                int? i = _val2;
            }
            if (strArr3.TryGet(3, out var _val3))
            {
                string i = _val3;
            }
            else
            {
                string? i = _val3;
            }


            WritePair(intArr1.TryGet(3, out var val1) + val1.ToString());
            WritePair(intArr2.TryGet(3, out var val2) + val2.ToString());
            WritePair(strArr3.TryGet(3, out var val3) + val3?.ToString() ?? "<null>");

            int i32 = intArr1.GetOrDefault(6);
            string? str1 = strArr3.GetOrDefault(6);
            string str2 = strArr3.GetOrDefault(6, "<null>");
        }
    }
}
