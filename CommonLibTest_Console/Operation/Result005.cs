using Common_Util.Data.Enums;
using Common_Util.Data.Struct.Modal;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.Operation
{
    internal class Result005() : TestBase("测试模态结果接口")
    {
        protected override void RunImpl()
        {
            ITagModalResult<string> temp = new MyTest() { Tag = "123", Result = ModalResult.Yes };
            WriteLine(temp.Tag);

            ITagModalResult temp2 = temp;
            WriteLine(temp2.Tag);

        }
        public class MyTest : ITagModalResult<string>
        {
            public required string Tag { get; set; }

            public ModalResult Result { get; set; }
        }
    }
}
