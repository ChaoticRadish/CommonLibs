using Common_Util.Data.Structure.Value;
using Common_Util.Data.Structure.Value.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.DataStruct
{
    internal class LayeringAddressCode002() : TestBase("测试合并编码路径的功能")
    {
        protected override void RunImpl()
        {
            List<LayeringAddressCode> codes = new()
            {
                "a",
                "",
                ":b",
                "aaa.bb.cc",
                "aaa.bb:1",
                "aaa.bb:2",
                "aaa.bb.cc:1",
                "aaa.bb.cc:2",
                "aaa.dd:123",
                "aaa.dd.ee:123",
                "aaa.bb.cc:3",
                "aaa.dd:1234",
                "aaa.dd.ee:1234",
            };
            WriteFullInfoPair(codes.Select(c => (string)c).ToList(), split: " => \n");

            LayeringAddressCode range = "wu";

            var newCodes = codes.ConcatRange(range);

            WriteFullInfoPair(newCodes.Select(c => c.ToDefaultFormatString()).ToList(), split: " => \n");
        }
    }
}
