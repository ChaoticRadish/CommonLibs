using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.CSharp
{
    internal class Reflection008() : TestBase("可空类型的判断")
    {
        protected override void RunImpl()
        {
            WritePair(typeof(string).CanBeNull());
            WritePair(typeof(int?).CanBeNull());
            WritePair(typeof(int).CanBeNull());
            WritePair(typeof(MyStruct).CanBeNull());
            WritePair(typeof(MyInterface).CanBeNull());
            WritePair(typeof(Reflection008).CanBeNull());
        }

        struct MyStruct
        {

        }
        interface MyInterface
        {

        }
    }
}
