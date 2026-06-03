using Common_Util.Data.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Extensions.Results
{
    /// <summary>
    /// 包含字符串转换为各类结果的扩展方法
    /// </summary>
    public static class StringToResultExtensions
    {
        /// <summary>
        /// 将 <paramref name="str"/> 作为一个操作结果的描述文本, 转换为 <see cref="IOperationResult"/>
        /// </summary>
        /// <param name="str"></param>
        /// <param name="isSuccess">操作是否成功</param>
        /// <returns></returns>
        public static IOperationResult AsOperationResult(this string str, bool isSuccess)
        {
            return (OperationResult)(isSuccess, str);
        }
        /// <summary>
        /// 将 <paramref name="str"/> 作为一个操作结果的描述文本, 转换为 <see cref="IOperationResult"/>, 
        /// 再调用 <see cref="Task.FromResult{TResult}(TResult)"/> 得到 <see cref="Task"/>
        /// </summary>
        /// <param name="str"></param>
        /// <param name="isSuccess"></param>
        /// <returns></returns>
        public static Task<IOperationResult> AsTaskOperationResult(this string str, bool isSuccess)
        {
            return Task.FromResult((IOperationResult)(OperationResult)(isSuccess, str));
        }
    }
}
