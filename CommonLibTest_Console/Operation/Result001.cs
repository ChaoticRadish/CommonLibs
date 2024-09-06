using Common_Util.Data.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.Operation
{
    internal class Result001() : TestBase("测试可带有异常的操作结果结构体 OperationResultEx")
    {
        protected override void RunImpl()
        {
            RunTest(test01, "测试01 不附带数据, 返回接口, 预期返回异常", true);
            RunTest(test01, "测试01 不附带数据, 返回接口, 预期返回异常", false);
            RunTest(test02, "测试02 不附带数据, 返回接口, 预期返回成功");

            RunTest(test03, "测试03 不附带数据, 返回结构体, 预期返回异常");
            RunTest(test04, "测试04 不附带数据, 返回结构体, 预期返回成功");

            RunTest(test05, "测试05 附带数据, 返回接口, 预期返回异常");
            RunTest(test06, "测试06 附带数据, 返回接口, 预期返回成功");

            RunTest(test07, "测试07 附带数据, 返回结构体, 预期返回异常");
            RunTest(test08, "测试08 附带数据, 返回结构体, 预期返回成功", true);
            RunTest(test08, "测试08 附带数据, 返回结构体, 预期返回成功", false);
        }

        public IOperationResultEx test01()
        {
            try
            {
                throw new Exception("测试测测试");
            }
            catch (Exception ex)
            {
                return OperationResultEx.Failure(ex);
            }
        }
        public IOperationResultEx test02()
        {
            try
            {
                return OperationResultEx.SuccessWithInfo("成功了家人们");
            }
            catch (Exception ex)
            {
                return OperationResultEx.Failure(ex);
            }
        }
        public OperationResultEx test03()
        {
            try
            {
                throw new Exception("测试测测试");
            }
            catch (Exception ex)
            {
                return ex;
            }
        }
        public OperationResultEx test04()
        {
            try
            {
                return (true, "成功了家人们");
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        public IOperationResultEx<TestClass> test05()
        {
            try
            {
                throw new Exception("测试测测试");
            }
            catch (Exception ex)
            {
                return OperationResultEx<TestClass>.Failure(ex);
            }
        }
        public IOperationResultEx<TestClass> test06()
        {
            try
            {
                return OperationResultEx<TestClass>.Success(new TestClass()
                {
                    AValue = " c而是A值",
                    BValue = "测试B值"
                }, "成功了家人们");
            }
            catch (Exception ex)
            {
                return OperationResultEx<TestClass>.Failure(ex);
            }
        }
        public OperationResultEx<TestClass> test07()
        {
            try
            {
                throw new Exception("测试测测试");
            }
            catch (Exception ex)
            {
                return ex;
            }
        }
        public OperationResultEx<TestClass> test08()
        {
            try
            {
                return new TestClass()
                {
                    AValue = " c而是A值",
                    BValue = "测试B值"
                };
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        public class TestClass
        {
            public string AValue { get; set; } = string.Empty;
            public string BValue { get; set; } = string.Empty;
        }
    }
}
