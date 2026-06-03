using Common_Util.Data.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Extensions.Results
{
    /// <summary>
    /// 包含布尔值转换为各类结果的扩展方法
    /// </summary>
    public static class BooleanToResultExtensions
    {
        /// <summary>
        /// 将 <paramref name="b"/> 作为一个操作结果, 转换为 <see cref="IOperationResult"/>
        /// </summary>
        /// <param name="b"></param>
        /// <param name="msg">操作结果的描述文本, 在 <see langword="null"/> 时会有默认值: 操作成功 / 操作失败</param>
        /// <returns></returns>
        public static IOperationResult AsOperationResult(this bool b, string? msg = null)
        {
            return (OperationResult)(b, msg ?? (b ? "操作成功" : "操作失败"));
        }

        /// <summary>
        /// 将 <paramref name="b"/> 作为一个操作结果, 转换为 <see cref="IOperationResult"/>, 
        /// 再调用 <see cref="Task.FromResult{TResult}(TResult)"/> 得到 <see cref="Task"/>
        /// </summary>
        /// <param name="b"></param>
        /// <param name="msg">操作结果的描述文本, 在 <see langword="null"/> 时会有默认值: 操作成功 / 操作失败</param>
        /// <returns></returns>
        public static Task<IOperationResult> AsTaskOperationResult(this bool b, string? msg = null)
        {
            return Task.FromResult((IOperationResult)(OperationResult)(b, msg ?? (b ? "操作成功" : "操作失败")));
        }
    }
}
