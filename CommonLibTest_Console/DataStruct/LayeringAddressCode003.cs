using Common_Util.Data.Structure.Tree.Extensions;
using Common_Util.Data.Structure.Value;
using Common_Util.Data.Structure.Value.Extensions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.DataStruct
{
    internal class LayeringAddressCode003() : TestBase("测试随机生成编码值")
    {
        protected override void RunImpl()
        {
            RunTest(test1, "随机生成编码集合", 5);
            RunTest(test2, "测试 2", 5);
        }
        Random random = new();
        private void test1()
        {
            WriteLine("执行测试: ");
            var codes = LayeringAddressCodeHelper.Random(
                ["aaa", "bb", "c"], ["1", "2", "3", "4", "5", "6", "7", "8"],
                2, 4, 15, 20, 0.95, 0.5, random: random).ToList();
            WriteList(codes.Select(code => (LayeringAddressCode)(code.IsRange, code.LayerValues)).ToList(), "list", true);
            var str = codes
                .AsMultiTree()
                .ToGeneralTree(node => (LayeringAddressCode)(node.NodeValue.IsRange, node.NodeValue.LayerValues))
                .GetSimpleTreeString(
                    (value) => value.IsAll() ? "ALL!" : value.Endpoint(),
                    (scope) => scope.IsAll() ? "ALL!" : scope.Endpoint());
            WriteLine(str);
            WriteEmptyLine();
        }

        private void test2()
        {
            WriteLine("执行测试: ");
            var codes1 = LayeringAddressCodeHelper.Random(
                ["aaa", "bb", "c"], ["1", "2", "3", "4", "5", "6", "7", "8"],
                2, 4, 15, 20, 0.95, 0.5, random: random);

            print(codes1, "codes 1");

            var codes2 = codes1.ConcatRange((LayeringAddressCode)"qqq.xx");

            print(codes2, "codes 2");
        }

        private void print(IEnumerable<ILayeringAddressCode<string>> codes, string name = "list")
        {
            WriteList(codes.Select(code => (LayeringAddressCode)(code.IsRange, code.LayerValues)).ToList(), name, true);
        }
    }
}
