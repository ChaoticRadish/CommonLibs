using Common_Util.Data.Exceptions;
using Common_Util.Exceptions.General;
using Common_Util.Extensions;
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
                throw new ImpossibleForkException(_exMessage_noSuccess_noFailure);
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
        /// <param name="successAction">操作成功, 且附带了非空数据</param>
        /// <param name="successButNullAction">操作成功, 但附带了 <see langword="null"/> 数据</param>
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
                throw new ImpossibleForkException(_exMessage_noSuccess_noFailure);
            }
            return output;
        }

        /// <summary>
        /// 根据操作结果的成功与否, 执行对应的方法
        /// </summary>
        /// <param name="result"></param>
        /// <param name="successAction">操作成功, 且附带了非空数据</param>
        /// <param name="successButNullAction">操作成功, 但附带了 <see langword="null"/> 数据</param>
        /// <param name="failureAction"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TResult Match<T, TResult>(this IOperationResult<T> result, Func<T, TResult> successAction, Func<TResult> successButNullAction, Func<IOperationResult<T>, TResult> failureAction)
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
                output = failureAction(result);
            }
            else
            {
                throw new ImpossibleForkException(_exMessage_noSuccess_noFailure);
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




        /// <summary>
        /// 取得操作结果的数据, 否则抛出异常
        /// </summary>
        /// <remarks>
        /// 当操作成功, <see cref="IOperationResult{T}.Data"/> 却为 <see langword="null"/> 时, 也将抛出异常
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <returns></returns>
        /// <exception cref="OperationFailureException"></exception>
        /// <exception cref="ImpossibleForkException"></exception>
        public static T GetDataElseThrow<T>(this IOperationResult<T> result)
        {
            if (result.IsFailure) 
            {
                throw new OperationFailureException(result);
            }
            else if (result.IsSuccess)
            {
                if (result.Data == null)
                {
                    throw new ImpossibleForkException("操作结果是成功的, 当时其数据却为 null! ");
                }
                else
                {
                    return result.Data;
                }
            }
            else 
            {
                throw new ImpossibleForkException("操作结果不成功, 也不失败! ");
            }
        }

        #endregion

        #region 带数据结果重新包装
        /// <summary>
        /// 将成功操作结果包含的数据替换为另一个数据, 得到新的操作结果. 如果传入操作结果为失败, 其数据将固定为 <see langword="null"/>
        /// </summary>
        /// <remarks>
        /// 当前实现下, 会返回 <see cref="OperationResult{T2}"/>
        /// </remarks>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="result"></param>
        /// <param name="selectFunc">选取方法, 或者说转换方法</param>
        /// <returns></returns>
        public static IOperationResult<T2> Select<T1, T2>(this IOperationResult<T1> result, Func<T1?, T2?> selectFunc)
        {
            if (result.IsSuccess)
            {
                return new OperationResult<T2>()
                {
                    Data = selectFunc(result.Data),
                    FailureReason = result.FailureReason,
                    IsSuccess = result.IsSuccess,
                    SuccessInfo = result.SuccessInfo,
                };
            }
            else
            {
                return new OperationResult<T2>()
                {
                    Data = default,
                    FailureReason = result.FailureReason,
                    IsSuccess = result.IsSuccess,
                    SuccessInfo = result.SuccessInfo,
                };
            }
        }
        /// <summary>
        /// 将成功操作结果包含的数据替换为另一个数据, 得到新的操作结果. 获取新数据的过程可能失败. 如果传入操作结果为失败, 或者获取数据的操作为失败, 其数据将固定为 <see langword="null"/>
        /// </summary>
        /// <remarks>
        /// 当前实现下, 会返回 <see cref="OperationResult{T2}"/>
        /// </remarks>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="result"></param>
        /// <param name="selectFunc">选取方法, 或者说转换方法</param>
        /// <returns></returns>
        public static IOperationResult<T2> SelectMaybeFailure<T1, T2>(this IOperationResult<T1> result, Func<T1?, OperationResult<T2>> selectFunc)
        {
            if (result.IsSuccess)
            {
                var selectResult = selectFunc(result.Data);
                if (selectResult)
                {
                    return new OperationResult<T2>()
                    {
                        Data = selectResult.Data,
                        FailureReason = null,
                        IsSuccess = true,
                        SuccessInfo = result.SuccessInfo ?? selectResult.SuccessInfo,
                    };
                }
                else
                {
                    return new OperationResult<T2>()
                    {
                        Data = default,
                        FailureReason = selectResult.FailureReason,
                        IsSuccess = false,
                        SuccessInfo = null,
                    };
                }
            }
            else
            {
                return new OperationResult<T2>()
                {
                    Data = default,
                    FailureReason = result.FailureReason,
                    IsSuccess = result.IsSuccess,
                    SuccessInfo = result.SuccessInfo,
                };
            }
        }

        /// <summary>
        /// 将可携带异常的成功操作结果包含的数据替换为另一个数据, 得到新的操作结果,. 如果传入操作结果为失败, 其数据将固定为 <see langword="null"/>
        /// </summary>
        /// <remarks>
        /// 当前实现下, 会返回 <see cref="OperationResultEx{T2}"/>
        /// </remarks>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="result"></param>
        /// <param name="selectFunc">选取方法, 或者说转换方法</param>
        /// <returns></returns>
        public static IOperationResultEx<T2> SelectEx<T1, T2>(this IOperationResultEx<T1> result, Func<T1?, T2?> selectFunc)
        {
            if (result.IsSuccess)
            {
                return new OperationResultEx<T2>()
                {
                    Data = selectFunc(result.Data),
                    Exception = result.Exception,
                    FailureReason = result.FailureReason,
                    IsSuccess = result.IsSuccess,
                    SuccessInfo = result.SuccessInfo,
                };
            }
            else
            {
                return new OperationResultEx<T2>()
                {
                    Data = default,
                    Exception = result.Exception,
                    FailureReason = result.FailureReason,
                    IsSuccess = result.IsSuccess,
                    SuccessInfo = result.SuccessInfo,
                };
            }
        }

        /// <summary>
        /// 将可携带异常的成功操作结果包含的数据替换为另一个数据, 得到新的操作结果. 获取新数据的过程可能失败. 如果传入操作结果为失败, 或者获取数据的操作为失败, 其数据将固定为 <see langword="null"/>
        /// </summary>
        /// <remarks>
        /// 当前实现下, 会返回 <see cref="OperationResultEx{T2}"/>
        /// </remarks>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="result"></param>
        /// <param name="selectFunc">选取方法, 或者说转换方法</param>
        /// <returns></returns>
        public static IOperationResultEx<T2> SelectMaybeFailureEx<T1, T2>(this IOperationResultEx<T1> result, Func<T1?, OperationResultEx<T2>> selectFunc)
        {
            if (result.IsSuccess)
            {
                var selectResult = selectFunc(result.Data);
                if (selectResult)
                {
                    return new OperationResultEx<T2>()
                    {
                        Data = selectResult.Data,
                        Exception = null,
                        FailureReason = null,
                        IsSuccess = true,
                        SuccessInfo = result.SuccessInfo ?? selectResult.SuccessInfo,
                    };
                }
                else
                {
                    return new OperationResultEx<T2>()
                    {
                        Data = default,
                        Exception = selectResult.Exception,
                        FailureReason = selectResult.FailureReason,
                        IsSuccess = false,
                        SuccessInfo = null,
                    };
                }
            }
            else
            {
                return new OperationResultEx<T2>()
                {
                    Data = default,
                    Exception = result.Exception,
                    FailureReason = result.FailureReason,
                    IsSuccess = result.IsSuccess,
                    SuccessInfo = result.SuccessInfo,
                };
            }
        }
        #endregion

        #region 结果文本
        /// <summary>
        /// 转换操作结果为简述文本的通用方法
        /// </summary>
        /// <param name="result"></param>
        /// <param name="operationDesc">该操作结果对应操作的描述</param>
        /// <param name="splitParts">分割不同部分的字符串, 比如当结果类型是 <see cref="IOperationResultEx"/> 时, 需要使用此值, 将异常与成功或失败信息分割开</param>
        /// <returns></returns>
        public static string GetBrief(this IOperationResult result, string? operationDesc = null, string splitParts = "\n")
        {
            StringBuilder sb = new StringBuilder();


            if (operationDesc.IsNotEmpty())
            {
                sb.Append(operationDesc).Append(splitParts);
            }
            if (result.IsSuccess)
            {
                sb.Append("<成功>");
                if (result.SuccessInfo.IsNotEmpty())
                {
                    sb.Append(' ').Append(result.SuccessInfo);
                }
            }
            else
            {
                sb.Append("<失败>");
                if (result.FailureReason.IsNotEmpty())
                {
                    sb.Append(' ').Append(result.FailureReason);
                }

            }
            Type resultType = result.GetType();
            if (TypeHelper.ExistInterfaceIsDefinitionFrom(resultType, typeof(IOperationResult<>), out Type? matchInterface))
            {
                var dataType = matchInterface.GetGenericArguments()[0] ?? throw new ImpossibleForkException();
                var property = resultType.GetProperty(nameof(IOperationResult<object>.Data)) ?? throw new ImpossibleForkException();
                var data = property.GetValue(result, null);
                if (data == null)
                {
                    sb.Append(splitParts).Append('[').Append(dataType.Name).Append(' ').Append("无数据!").Append(']');
                }
                else
                {
                    sb.Append(splitParts).Append('[').Append(dataType.Name).Append(' ').Append(data.ToString()?.Brief(30)).Append(']');
                }
            }
            if (result is IOperationResultEx resultEx)
            {
                if (resultEx.HasException)
                {
                    var ex = resultEx.Exception;
                    sb.Append(splitParts).Append("异常: ").Append(ex.GetType().Name).Append(' ').Append(ex.Message);
                }
            }
            return sb.ToString();
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
