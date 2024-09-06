using Common_Util.Data.Exceptions;
using Common_Util.Data.Struct;
using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.DataStruct
{
    internal class OperationResult002() : TestBase("测试 OperationFailureException")
    {
        protected override void RunImpl()
        {
            test(true);
            test(false);
        }

        private void test(bool success)
        {
            WriteLine("test: " + success);
            WriteEmptyLine();

            WriteLine("测试 OperationResult (发生异常)");
            test0(() =>
            {
                var result = test1(success);
                if (!result)
                {
                    throw new OperationFailureException(result);
                }
                return result;
            });
            WriteEmptyLine();


            WriteLine("测试 OperationResult (返回失败)");
            test0(() =>
            {
                var result = test3(success);
                if (!result)
                {
                    throw new OperationFailureException(result);
                }
                return result;
            });
            WriteEmptyLine();

            WriteLine("测试 OperationResultEx");
            test0(() =>
            {
                var result = test2(success);
                if (!result)
                {
                    throw new OperationFailureException(result);
                }
                return result;
            });
            WriteEmptyLine();

            WriteEmptyLine();
            WriteEmptyLine();
            WriteEmptyLine();
        }

        private void test0(Func<object> action)
        {
            try
            {
                var result = action();
                WriteLine(result.FullInfoString());
            }
            catch (Exception ex)
            {
                WriteLine($"异常对象({ex.GetType()}): ");
                WriteLine(ex.FullInfoString());
            }
        }

        private OperationResult test1(bool success)
        {
            if (success)
            {
                return true;
            }
            else
            {
                throw new Exception("异常测试!!! ");
            }

        }
        private OperationResult test3(bool success)
        {
            if (success)
            {
                return true;
            }
            else
            {
                return "测试失败";
            }

        }
        private OperationResultEx test2(bool success)
        {
            try
            {
                if (success)
                {
                    return true;
                }
                else
                {
                    throw new Exception("异常测试!!! ");
                }
            }
            catch (Exception ex)
            {
                return ex;
            }

        }

    }
}
