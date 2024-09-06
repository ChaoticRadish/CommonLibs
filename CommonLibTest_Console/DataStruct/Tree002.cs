using Common_Util.Data.Structure.Tree;
using Common_Util.Data.Structure.Tree.Extensions;
using Common_Util.Data.Structure.Value;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.DataStruct
{
    internal class Tree002() : TestBase("测试 ObservableMultiTree 的相关操作")
    {
        protected override void RunImpl()
        {
            RunTest(test1, "测试 1");
            RunTest(test2, "测试 2");
            RunTest(test3, "测试 3");
            RunTest(test4, "测试 4");
            RunTest(test5, "测试 5");
        }

        private void test1()
        {
            ObservableMultiTreeNode<LayeringAddressCode> testNode1 = new(null, "aa");
            ObservableMultiTreeNode<LayeringAddressCode> testNode2 = new(testNode1, "aa.bb:1");
            ObservableMultiTreeNode<LayeringAddressCode> testNode3 = new(testNode1, "aa.bb:1");

            testNode1.Childrens.Add(testNode2);
            testNode1.Childrens.Add(testNode3);

            WritePair(testNode1.CheckCodeTreeFork<string, LayeringAddressCode>());
        }
        private void test2()
        {
            ObservableMultiTreeNode<LayeringAddressCode> testNode1 = new(null, "aa");
            ObservableMultiTreeNode<LayeringAddressCode> testNode2 = new(testNode1, "aa.bb.cc:1");
            ObservableMultiTreeNode<LayeringAddressCode> testNode3 = new(testNode1, "aa.bb.cc:2");

            testNode1.Childrens.Add(testNode2);
            testNode1.Childrens.Add(testNode3);

            WritePair(testNode1.CheckCodeTreeFork<string, LayeringAddressCode>());
        }
        private void test3()
        {
            ObservableMultiTreeNode<LayeringAddressCode> testNode1 = new(null, "aa");
            ObservableMultiTreeNode<LayeringAddressCode> testNode2 = new(testNode1, "cc:1");
            ObservableMultiTreeNode<LayeringAddressCode> testNode3 = new(testNode1, "cc.2:a");

            testNode1.Childrens.Add(testNode2);
            testNode1.Childrens.Add(testNode3);

            WritePair(testNode1.CheckCodeTreeFork<string, LayeringAddressCode>());
        }
        private void test4()
        {
            ObservableMultiTreeNode<LayeringAddressCode> testNode1 = new(null, "aa");
            ObservableMultiTreeNode<LayeringAddressCode> testNode2 = new(testNode1, "aa:1");
            ObservableMultiTreeNode<LayeringAddressCode> testNode3 = new(testNode1, "aa.bb:2");

            testNode1.Childrens.Add(testNode2);
            testNode1.Childrens.Add(testNode3);

            WritePair(testNode1.CheckCodeTreeFork<string, LayeringAddressCode>());
        }
        private void test5()
        {
            ObservableMultiTreeNode<LayeringAddressCode> testNode1 = new(null, "aa");
            ObservableMultiTreeNode<LayeringAddressCode> testNode2 = new(testNode1, "aa:1");
            ObservableMultiTreeNode<LayeringAddressCode> testNode3 = new(testNode1, "aa.bb");
            ObservableMultiTreeNode<LayeringAddressCode> testNode4 = new(testNode3, "aa.bb:2");

            testNode1.Childrens.Add(testNode2);
            testNode1.Childrens.Add(testNode3);
            testNode3.Childrens.Add(testNode4);

            var checkResult = testNode1.CheckCodeTreeFork<string, LayeringAddressCode>();
            WritePair(checkResult);
            WritePair(checkResult == IMultiTreeExtensions.CodeTreeForkCheckResultEnum.Full);
            WritePair(checkResult > 0);
        }
    }
}
