using Common_Util.Data.Structure.Tree.Extensions;
using Common_Util.Data.Structure.Value;
using Common_Util.Data.Structure.Value.Extensions;
using Common_Util.Extensions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.DataStruct
{
    internal class Tree001() : TestBase("测试树结构, 其数据为 LayeringAddressCode ")
    {
        protected override void RunImpl()
        {
            RunTest(test, true, "测试1. 完全范围");
            RunTest(test, false, "测试2. 不完全范围");

        }

        private void test(bool all)
        {
            List<LayeringAddressCode> testData = [
                "aaa",
                "aaa:1",
                "aaa.bb.1",
                "aaa.bb:1",
                "ccc.cc:1",
                "ccc.dd.ee:3",
                "ccc.dd.ff:3",
                "ccc.cc:2",
                "ccc.cc:3",
                "ccc.dd.ee:1",
                "ccc.dd.ff:1",
                "ccc.cc:4",
                "ccc.cc:5",
                "ccc.dd.ee:2",
                "ccc.dd.ff:2",
                "ccc.dd:1",
                "ccc.dd:2",
                "qqq",
                ];
            var temp = testData.Select(i => (ILayeringAddressCode<string>)i);
            var tree1 = temp.AsMultiTree(rootValue: all ? null : (LayeringAddressCode?)"ccc");

            //WriteLine("tree1");
            //WriteLine(tree1.FullInfoString());
            WriteLine("tree1 先根遍历");
            int index = 0;
            foreach (var value in tree1.Preorder())
            {
                if (value == null)
                {
                    WriteLine($"{index} => <null>");
                }
                else if (value.IsAll())
                {
                    WriteLine($"{index} => ALL!!!");
                }
                else
                {
                    WriteLine($"{index} => {LayeringAddressCodeHelper.Convert(value)}");
                }
                index++;
            }

            WriteLine("tree1 带索引 先根遍历");
            foreach (var item in tree1.IndexPreorder())
            {
                string valueString;
                if (item.NodeValue == null)
                {
                    valueString = "<null>";
                }
                else if (item.NodeValue.IsAll())
                {
                    valueString = "ALL!!!";
                    WriteLine($"{index} => ");
                }
                else
                {
                    valueString = LayeringAddressCodeHelper.Convert(item.NodeValue);
                }
                WriteLine($"{item.NodeIndex} Parent: {item.ParentIndex} => {valueString}");
            }

            WriteLine("tree1 后根遍历");
            foreach (var value in tree1.Postorder())
            {
                WriteLine($"{index} => {(value == null ? "<null>" : (LayeringAddressCodeHelper.Convert(value)))}");
            }

            var tree2 = tree1.ToGeneralTree(node => node.NodeValue);

            //WriteLine("tree2");
            //WriteLine(tree2.FullInfoString());

            WriteLine("tree2 GetSimpleTreeString");
            WriteLine(tree2.GetSimpleTreeString(
                getScopeStringFunc: code => codeToString(code),
                getValueStringFunc: code => codeToString(code)));

            var tree3 = tree2.Convert(node => "!TREE3!" + codeToString(node.NodeValue));
            var tree4 = tree3.ToGeneralTree(node => node.NodeValue);
            WriteLine("tree4 GetSimpleTreeString");
            WriteLine(tree4.GetSimpleTreeString(
                getScopeStringFunc: str => str,
                getValueStringFunc: str => str));

            var tree5 = tree4.AsSimpleMultiTree();
            WriteLine("tree5");
            WriteLine(tree5.FullInfoString());
            var tree6 = tree5.ToGeneralTree(tree5 => tree5.NodeValue);
            WriteLine("tree6 GetSimpleTreeString");
            WriteLine(tree6.GetSimpleTreeString(
                getScopeStringFunc: str => str ?? "<null>",
                getValueStringFunc: str => str ?? "<null>"));




            WriteEmptyLine();
            WriteEmptyLine();
            WriteEmptyLine();

        }

        private string codeToString(ILayeringAddressCode<string>? code)
        {
            if (code == null)
            {
                return "<null>";
            } 
            if (code.IsAll())
            {
                return "<ALL>";
            }
            StringBuilder sb = new StringBuilder();
            int index = 0;
            foreach (string str in code.LayerValues)
            {
                sb.Append(str);
                index++;
                if (index < code.LayerCount - 1)
                {
                    sb.Append('.');
                }
                else if (index == code.LayerCount - 1)
                {
                    sb.Append(code.IsRange ? '.' : ':');
                }


            }
            return sb.ToString();   
        }
    }
}
