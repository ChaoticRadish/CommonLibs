using Common_Util.Data.Structure.Tree;
using Common_Util.Data.Structure.Tree.Extensions;
using Common_Util.Data.Structure.Value;
using Common_Util.Data.Structure.Value.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.DataStruct
{
    internal class Tree005() : TestBase("测试线性化多叉树")
    {
        protected override void RunImpl()
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
            var tree1 = temp.AsMultiTree(null);
            var tree2 = tree1.ToGeneralTree(node => node.NodeValue);
            WriteLine("tree2 GetSimpleTreeString");
            WriteLine(tree2.GetSimpleTreeString(
                getScopeStringFunc: code => codeToString(code),
                getValueStringFunc: code => codeToString(code)));

            WriteLine("tree2 Linearize");
            var tree3 = tree2.Linearize();

            foreach (var node in tree3.Nodes)
            {
                WriteLine($"{node.NodeIndex}. {codeToString(node.NodeValue)} (parent: {node.ParentIndex}, child: [{string.Join(',', node.ChildrenIndices.ToArray().Select(i => i.ToString()))}])");
            }

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
