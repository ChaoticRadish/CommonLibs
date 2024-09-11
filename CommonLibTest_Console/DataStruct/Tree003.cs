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
    internal class Tree003() : TestBase("测试 IMultiTree<ILayeringAddressCode<TLayer>> 的相关操作")
    {
        protected override void RunImpl()
        {
            ObservableMultiTreeNode<LayeringAddressCode> testNode1 = new(null, "aa");

            ObservableMultiTree<LayeringAddressCode> tree = new();
            tree.SetRootNode(testNode1);

            tree.Add<string, LayeringAddressCode>((LayeringAddressCode)"aa.bb:1", LayeringAddressCode.CreateItem, LayeringAddressCode.CreateRange);
            tree.Add<string, LayeringAddressCode>((LayeringAddressCode)"aa.bb:2", LayeringAddressCode.CreateItem, LayeringAddressCode.CreateRange);
            tree.Add<string, LayeringAddressCode>((LayeringAddressCode)"aa.bb:3", LayeringAddressCode.CreateItem, LayeringAddressCode.CreateRange);
            tree.Add<string, LayeringAddressCode>((LayeringAddressCode)"aa.b1.a1:1", LayeringAddressCode.CreateItem, LayeringAddressCode.CreateRange);
            tree.Add<string, LayeringAddressCode>((LayeringAddressCode)"aa.b1.a1:2", LayeringAddressCode.CreateItem, LayeringAddressCode.CreateRange);
            tree.Add<string, LayeringAddressCode>((LayeringAddressCode)"aa.b1.a1:3", LayeringAddressCode.CreateItem, LayeringAddressCode.CreateRange);
            tree.Add<string, LayeringAddressCode>((LayeringAddressCode)"aa.b1.a2:1", LayeringAddressCode.CreateItem, LayeringAddressCode.CreateRange);
            tree.Add<string, LayeringAddressCode>((LayeringAddressCode)"aa.b1.a2:2", LayeringAddressCode.CreateItem, LayeringAddressCode.CreateRange);
            tree.Add<string, LayeringAddressCode>((LayeringAddressCode)"aa.b1.a2:3", LayeringAddressCode.CreateItem, LayeringAddressCode.CreateRange);
            tree.Add<string, LayeringAddressCode>((LayeringAddressCode)"aa.b1.a3:1", LayeringAddressCode.CreateItem, LayeringAddressCode.CreateRange);
            tree.Add<string, LayeringAddressCode>((LayeringAddressCode)"aa.b1.a3:2", LayeringAddressCode.CreateItem, LayeringAddressCode.CreateRange);
            tree.Add<string, LayeringAddressCode>((LayeringAddressCode)"aa.b1.a3:3", LayeringAddressCode.CreateItem, LayeringAddressCode.CreateRange);

            var tree2 = tree.ToGeneralTree(node => node.NodeValue);
            WriteLine(tree2.GetSimpleTreeString(
                getScopeStringFunc: code => codeToString(code),
                getValueStringFunc: code => codeToString(code)));

            find(tree, "aa.b1.a3:2");
            find(tree, "aa.b1.a3");
            find(tree, "aa.b1.a3:2");
            find(tree, "aa.b1.a3:4");
            find(tree, "b1.a3:4");
        }

        private void find(ObservableMultiTree<LayeringAddressCode> tree, string code)
        {
            WriteLine("尝试查找节点: " + code);
            var findResult = tree.FindNode<string, LayeringAddressCode, ObservableMultiTreeNode<LayeringAddressCode>, ObservableMultiTree<LayeringAddressCode>>(
                (LayeringAddressCode)code);
            WriteLine(findResult);
            var data = findResult.Data;
            WriteLine(data?.ToString() ?? "<null>");
            WriteLine(codeToString(findResult.Data?.NodeValue));

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
