using Common_Util.Data.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.Operation
{
    internal class Result002() : TestBase("关于类型转换的一点验证")
    {
        protected override void RunImpl()
        {
            RunTest(test, "测试");
        }

        private IOperationResultEx<TestModels.TestModel002> test()
        {
            OperationResultEx<TestModels.TestModel002> result;
            var test1 = subTest1();
            if (!test1.IsSuccess)
            {
                return result = (test1, null);
            } 
            var test2 = subTest2();
            if (!test2.IsSuccess)
            {
                return result = (test2, null);
            }
            return result = true;
        }
        private IOperationResult<TestModels.TestModel002> subTest1()
        {
            WriteLine("子测试1");
            OperationResult<TestModels.TestModel002> result;
            return (result = new TestModels.TestModel002() { ABC = "qweqaweqaweqaweda" });
        }
        private IOperationResult<TestModels.TestModel002> subTest2()
        {
            WriteLine("子测试2");
            OperationResult<TestModels.TestModel002> result;
            return result = "失败子测试2";
        }
    }
}
