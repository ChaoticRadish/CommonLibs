using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Struct
{
    public static class IOperationResultExtension
    {
        #region 异常文本
        private const string _exMessage_noSuccess_noFailure = "意料之外的错误: 操作结果既不成功也不失败! ";
        private const string _exMessage_hasExcption_ButItsNull = "意料之外的错误: 操作结果含异常对象, 但其值为 null! ";
        #endregion

        #region 操作结果分支执行
        /// <summary>
        /// 如果操作结果为成功, 则执行传入方法
        /// </summary>
        /// <param name="result"></param>
        /// <param name="action"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IfSuccess(this IOperationResult result, Action action) 
        {
            if (result.IsSuccess)
            {
                action();   
            }
        }
        /// <summary>
        /// 如果操作结果为失败, 则执行传入方法
        /// </summary>
        /// <param name="result"></param>
        /// <param name="action"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IfFailure(this IOperationResult result, Action action)
        {
            if (result.IsFailure)
            {
                action();
            }
        }





        /// <summary>
        /// 根据操作结果的成功与否, 执行对应的方法
        /// </summary>
        /// <param name="result"></param>
        /// <param name="successAction"></param>
        /// <param name="failureAction"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Match(this IOperationResult result, Action? successAction, Action? failureAction)
        {
            if (successAction != null && result.IsSuccess)
            {
                successAction();
                return;
            }
            if (failureAction != null && result.IsFailure)
            {
                failureAction();
                return;
            }
        }
        /// <summary>
        /// 根据操作结果的成功与否, 执行对应的方法
        /// </summary>
        /// <param name="result"></param>
        /// <param name="successAction"></param>
        /// <param name="failureAction"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Match<T>(this IOperationResult result, Func<T> successAction, Func<T> failureAction)
        {
            T output;
            if (result.IsSuccess)
            {
                output = successAction();
            }
            else if (result.IsFailure)
            {
                output = failureAction();
            }
            else
            {
                throw new Exception(_exMessage_noSuccess_noFailure);
            }
            return output;
        }





        /// <summary>
        /// 根据操作结果的成功与否, 执行对应的方法
        /// </summary>
        /// <param name="result"></param>
        /// <param name="successAction"></param>
        /// <param name="failureAction"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Match<T>(this IOperationResult<T> result, Action<T>? successAction, Action? successButNullAction, Action? failureAction)
        {
            if (successAction != null && result.IsSuccess && result.Data != null)
            {
                successAction(result.Data);
                return;
            }
            if (successButNullAction != null && result.IsSuccess && result.Data == null)
            {
                successButNullAction();
                return;
            }
            if (failureAction != null && result.IsFailure)
            {
                failureAction();
                return;
            }
        }
        /// <summary>
        /// 根据操作结果的成功与否, 执行对应的方法
        /// </summary>
        /// <param name="result"></param>
        /// <param name="successAction"></param>
        /// <param name="failureAction"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TResult Match<T, TResult>(this IOperationResult<T> result, Func<T, TResult> successAction, Func<TResult> successButNullAction, Func<TResult> failureAction)
        {
            TResult output;
            if (result.IsSuccess)
            {
                if (result.Data != null)
                {
                    output = successAction(result.Data);
                }
                else
                {
                    output = successButNullAction();
                }
            }
            else if (result.IsFailure)
            {
                output = failureAction();
            }
            else
            {
                throw new Exception(_exMessage_noSuccess_noFailure);
            }
            return output;
        }





        /// <summary>
        /// 根据操作结果的成功与否, 执行对应的方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <param name="successAction">在以下情况时调用: 成功, 且包含的数据不为 null</param>
        /// <param name="successButNullAction">在以下情况时调用: 成功, 但包含的数据为 null</param>
        /// <param name="failureAction">在以下情况时调用: 失败, 且无异常发生</param>
        /// <param name="exceptionFailureAction">在以下情况时调用: 失败, 且有异常发生</param>
        /// <exception cref="Exception"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Match<T>(this IOperationResultEx<T> result, Action<T>? successAction, Action? successButNullAction, Action? failureAction, Action<Exception>? exceptionFailureAction)
        {
            if (successAction != null && result.IsSuccess && result.Data != null)
            {
                successAction(result.Data);
                return;
            }
            if (successButNullAction != null && result.IsSuccess && result.Data == null)
            {
                successButNullAction();
                return;
            }
            if (failureAction != null && result.IsFailure && !result.HasException)
            {
                failureAction();
                return;
            }
            if (exceptionFailureAction != null && result.IsFailure && result.HasException)
            {
                exceptionFailureAction(result.Exception ?? throw new Exception(_exMessage_hasExcption_ButItsNull));
                return;
            }
        }
        /// <summary>
        /// 根据操作结果的成功与否, 执行对应的方法
        /// </summary>
        /// <param name="result"></param>
        /// <param name="successAction">在以下情况时调用: 成功, 且包含的数据不为 null</param>
        /// <param name="successButNullAction">在以下情况时调用: 成功, 但包含的数据为 null</param>
        /// <param name="failureAction">在以下情况时调用: 失败, 且无异常发生</param>
        /// <param name="exceptionFailureAction">在以下情况时调用: 失败, 且有异常发生</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TResult Match<T, TResult>(this IOperationResultEx<T> result, Func<T, TResult> successAction, Func<TResult> successButNullAction, Func<TResult> failureAction, Func<Exception, TResult> exceptionFailureAction)
        {
            TResult output;
            if (result.IsSuccess)
            {
                if (result.Data != null)
                {
                    output = successAction(result.Data);
                }
                else
                {
                    output = successButNullAction();
                }
            }
            else if (result.IsFailure)
            {
                if (!result.HasException)
                {
                    output = failureAction();
                }
                else
                {
                    output = exceptionFailureAction(result.Exception ?? throw new Exception(_exMessage_hasExcption_ButItsNull));
                }
            }
            else
            {
                throw new Exception(_exMessage_noSuccess_noFailure);
            }
            return output;
        }





        #endregion

        /// <summary>
        /// 按枚举顺序执行传入方法集合, 直到遇到出现任意失败项时将失败结果返回, 否则返回成功 (<see cref="OperationResult"/> 但是不带成功信息)
        /// </summary>
        /// <param name="funcs"></param>
        /// <returns></returns>
        public static IOperationResult FirstFailure(this IEnumerable<Func<IOperationResult>> funcs)
        {
            IOperationResult? result = null;
            foreach (var func in funcs)
            {
                result = func.Invoke();
                if (!result.IsSuccess)
                {
                    return result;
                }
            }
            return result ?? OperationResult.Success;
        }
        /// <summary>
        /// 按枚举顺序执行传入方法集合, 直到遇到出现任意失败项时将失败结果返回, 否则返回成功 (<see cref="OperationResult"/> 但是不带成功信息)
        /// </summary>
        /// <param name="funcs"></param>
        /// <returns></returns>
        public static IOperationResult FirstFailure<TResult>(this IEnumerable<Func<TResult>> funcs)
            where TResult : IOperationResult
        {
            IOperationResult? result = null;
            foreach (var func in funcs)
            {
                result = func.Invoke();
                if (!result.IsSuccess)
                {
                    return result;
                }
            }
            return result ?? OperationResult.Success;
        }

        /// <summary>
        /// 取得失败信息
        /// </summary>
        /// <param name="result"></param>
        /// <param name="defaultValue">如果失败信息为null, 返回这个值</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string FailureReason(this IOperationResult result, string defaultValue = "")
        {
            return result.FailureReason ?? defaultValue;
        }

        /// <summary>
        /// 尝试执行数次指定的方法, 直到成功或者达到最大尝试次数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <param name="maxCount"></param>
        /// <param name="afterDo">输入参数0: 对应执行轮次的执行结果, 参数1: 当前执行轮次序号; 返回结果: 是否提前结束</param>
        /// <returns></returns>
        public static T DoWhile<T>(this Func<T> func, int maxCount, Func<T, int, bool>? afterDo = null) where T : IOperationResult
        {
            return OperationResultHelper.DoWhile(func, maxCount, afterDo);
        }
        /// <summary>
        /// 尝试执行数次指定的方法, 直到成功或者达到最大尝试次数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <param name="maxCount"></param>
        /// <param name="afterDo">输入参数0: 对应执行轮次的执行结果, 参数1: 当前执行轮次序号; 返回结果: 是否提前结束</param>
        /// <returns></returns>
        public static Task<T> DoWhileAsync<T>(this Func<Task<T>> func, int maxCount, Func<T, int, Task<bool>>? afterDo = null) where T : IOperationResult
        {
            return OperationResultHelper.DoWhileAsync(func, maxCount, afterDo);
        }
    }
}
