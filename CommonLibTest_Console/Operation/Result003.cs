using Common_Util.Data.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.Operation
{
    internal class Result003() : TestBase($"测试 OperationResultHelper.TryDo<OperationResultEx>(...)")
    {
        protected override void RunImpl()
        {
            RunTest(test1, "测试1", false);
            RunTest(test1, "测试2", false);
            RunTest(test1, "测试3", false);
            RunTest(test1, "测试4", false);
        }

        private OperationResultEx test1()
        {
            return OperationResultHelper.TryDo(test2);
        }

        int index = 0;
        private OperationResultEx test2()
        {
            index++;
            switch (index)
            {
                case 0:
                    return true;
                case 1:
                    return "失败";
                case 2:
                    throw new Exception("测试异常");
                case 3:
                    var test3Result = test3();
                    if (test3Result.IsSuccess)
                    {
                        return (true, "子测试 3 成功! ");
                    }
                    else
                    {
                        return OperationResultHelper.FailureEx<OperationResultEx>(test3Result, "子测试 3 ");
                    }
                default:
                    return true;

            }
        }
        private TestResult test3()
        {
            try
            {
                throw new Exception("测试异常 3");
            }
            catch (Exception ex)
            {
                return OperationResultHelper.Failure<TestResult>(ex);
            }
        }

        private class TestResult : IOperationResultEx
        {
            public bool IsSuccess { get; set; }
            public bool IsFailure { get => !IsSuccess; set => IsSuccess = !value; }
            public string? SuccessInfo { get; set; }
            public string? FailureReason { get; set; }
            public bool HasException { get => Exception != null; set => throw new NotSupportedException(); }
            public Exception? Exception { get; set; }
        }
    }
}
