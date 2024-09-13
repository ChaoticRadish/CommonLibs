using Common_Util.Data.Struct;
using Common_Util.Exceptions.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.Operation
{
    internal class Result004() : TestBase("测试操作结果取得简述文本的方法 GetBrief ")
    {
        protected override void RunImpl()
        {
            test(OperationResult.Success);
            test((OperationResult)"测试测试");
            test((OperationResultEx)new Exception("awawaw"));
            test((OperationResultEx)new ImpossibleForkException("123123123"));
            test((OperationResultEx)new ValueMismatchException("QQQQQ")
            {
                new("AAA", "BBB"),
                new("CCC", "cCc"),
                new("eee", "ddd"),
            });


            test(OperationResult<TestClass>.Success(null));
            test(new TestResult() { IsSuccess = true });
            test(new TestResultEx() { IsSuccess = true });
            test(OperationResult<TestClass>.Success(null));
            test(OperationResult<TestClass>.Success(new("AAA")));
            test((OperationResult<TestClass>)"测试测试");
            test((OperationResultEx<TestClass>)new Exception("awawaw"));
            test((OperationResultEx<TestClass>)new ImpossibleForkException("123123123"));
            test((OperationResultEx<TestClass>)new ValueMismatchException("QQQQQ")
            {
                new("AAA", "BBB"),
                new("CCC", "cCc"),
                new("eee", "ddd"),
            });
        }

        int index = 0;
        void test(IOperationResult result)
        {
            WriteLine("测试 " + (++index));
            WriteLine(result.GetBrief());
            WriteEmptyLine();
        }

        private class TestClass(string v)
        {


            public string Value { get; } = v;

            public override string ToString()
            {
                return $"[TestClass_{Value}]";
            }
        }

        private class TestResult : IOperationResult<TestClass>
        {
            public TestClass? Data { get; set; }
            public bool IsSuccess { get; set; }
            public bool IsFailure { get => !IsSuccess; set => IsSuccess = !value; }
            public string? FailureReason { get; set; }
            public string? SuccessInfo { get; set; }
        }
        private class TestResultEx : TestResult, IOperationResultEx
        {
            public bool HasException { get => Exception != null; set => throw new NotSupportedException(); }
            public Exception? Exception { get; set; }
        }
    }
}
