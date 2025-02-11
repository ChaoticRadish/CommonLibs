using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Struct
{
    public static class IOperationResultExExtensions
    {
        /// <summary>
        /// 根据 <paramref name="result"/> 的成功与否, 或者是否异常, 返回合适的信息字符串
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static string? InfoString(this IOperationResultEx result)
        {
            if (result.IsSuccess)
            {
                return result.SuccessInfo;
            }
            else
            {
                return FailureString(result);
            }
        }

        /// <summary>
        /// 根据 <paramref name="result"/> 是否异常, 返回合适的信息字符串
        /// </summary>
        /// <param name="result"></param>
        /// <param name="default">如果没有异常, 且没有失败信息时, 需要返回的默认字符串</param>
        /// <returns></returns>
        [return: NotNullIfNotNull(nameof(@default))]
        public static string? FailureString(this IOperationResultEx result, string? @default = "失败")
        {
            if (result.FailureReason.IsEmpty())
            {
                if (result.Exception == null)
                {
                    return @default;
                }
                else
                {
                    return "发生异常: " + result.Exception.Message;
                }
            }
            else
            {
                return result.FailureReason;
            }
        }
    }
}
