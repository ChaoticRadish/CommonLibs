using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.CSharp
{
    internal class Reflection007() : TestBase("测试反射获取带有可空标记的本身就可空的类型")
    {
        protected override void RunImpl()
        {
            PropertyInfo p1 = typeof(Reflection007).GetProperty(nameof(Test01))!;
            PropertyInfo p2 = typeof(Reflection007).GetProperty(nameof(Test02))!;

            WriteFullInfoPair(p1);
            WriteFullInfoPair(p1.Attributes);
            WriteFullInfoPair(p1.GetCustomAttributes().ToArray());
            WriteFullInfoPair(p1.PropertyType);

            WriteFullInfoPair(p2);
            WriteFullInfoPair(p2.Attributes);
            WriteFullInfoPair(p2.GetCustomAttributes().ToArray());
            WriteFullInfoPair(p2.PropertyType);
        }

        public string Test01 { get; set; } = string.Empty;
        public string? Test02 { get; set; } = string.Empty;
    }
}
