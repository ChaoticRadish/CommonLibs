using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Struct
{
    public static class OperationResultHelper
    {
        #region OperationResult 与 OperationResultEx 通用
        /// <summary>
        /// 尝试执行指定的可以返回操作结果的方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="body"></param>
        /// <param name="catchFunc"></param>
        /// <param name="finallyAction"></param>
        /// <returns></returns>
        public static T TryDo<T>(Func<T> body, Func<Exception, T>? catchFunc = null, Action? finallyAction = null)
            where T : IOperationResult, new()
        {
            try
            {
                return body();
            }
            catch (Exception ex)
            {
                if (catchFunc != null)
                {
                    return catchFunc(ex);
                }
                else
                {
                    if (typeof(T).IsAssignableTo(typeof(IOperationResultEx)))
                    {
                        return (T)_failure_IOperationResultEx(typeof(T), ex);
                    }
                    else
                    {
                        return Failure<T>("发生异常: " + ex.Message);
                    }
                }
            }
            finally
            {
                finallyAction?.Invoke();
            }
        }

        /// <summary>
        /// 尝试执行数次指定的方法, 直到成功或者达到最大尝试次数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="body"></param>
        /// <param name="maxCount"></param>
        /// <param name="afterDo">输入参数0: 对应执行轮次的执行结果, 参数1: 当前执行轮次序号; 返回结果: 是否提前结束</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T DoWhile<T>(Func<T> body, int maxCount, Func<T, int, bool>? afterDo = null)
            where T : IOperationResult
        {
            T output;
            int index = 0;
            do
            {
                output = body();
                if (afterDo != null)
                {
                    bool advanceEnd = afterDo(output, index);
                    if (advanceEnd)
                    {
                        return output;
                    }
                }
                index++;
            } while (output.IsFailure && index < maxCount);
            return output;
        }

        /// <summary>
        /// 尝试执行数次指定的方法, 直到成功或者达到最大尝试次数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="body"></param>
        /// <param name="maxCount"></param>
        /// <param name="afterDo">输入参数0: 对应执行轮次的执行结果, 参数1: 当前执行轮次序号; 返回结果: 是否提前结束</param>
        /// <returns></returns>
        public static async Task<T> DoWhileAsync<T>(Func<Task<T>> body, int maxCount, Func<T, int, Task<bool>>? afterDo = null)
            where T : IOperationResult
        {
            T output;
            int index = 0;
            do
            {
                output = await body();
                if (afterDo != null)
                {
                    bool advanceEnd = await afterDo(output, index);
                    if (advanceEnd)
                    {
                        return output;
                    }
                }
                index++;
            } while (output.IsFailure && index < maxCount);
            return output;
        }

        /// <summary>
        /// 按枚举顺序执行传入方法集合, 直到遇到出现任意失败项时将失败结果返回, 否则返回成功 (<see cref="OperationResult"/> 但是不带成功信息)
        /// </summary>
        /// <param name="funcs"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IOperationResult FirstFailure(params Func<IOperationResult>[] funcs)
        {
            return funcs.FirstFailure();
        }

        /// <summary>
        /// 按枚举顺序执行传入方法集合, 直到遇到出现任意失败项时将失败结果返回, 否则返回成功 (<see cref="OperationResult"/> 但是不带成功信息)
        /// </summary>
        /// <param name="funcs"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IOperationResult FirstFailure<TResult>(params Func<TResult>[] funcs)
            where TResult : IOperationResult
        {
            return funcs.FirstFailure();
        }

        /// <summary>
        /// 按枚举顺序执行传入方法集合, 直到遇到出现任意失败项时, 将失败结果及对应的标记组成元组返回, 否则返回成功 ( (default(TFlag), <see cref="OperationResult"/> ) 但是不带成功信息 )
        /// </summary>
        /// <typeparam name="TFlag">方法标记类型</typeparam>
        /// <param name="funcArr"></param>
        /// <returns>失败时: flag: 导致失败的方法对应的标记. operationResult: 导致失败的方法的返回数据</returns>
        public static (TFlag? flag, IOperationResult operationResult) FirstFailure<TFlag>(params (TFlag flag, Func<IOperationResult> func)[] funcArr)
        {
            IOperationResult? result = null;
            foreach (var (flag, func) in funcArr)
            {
                result = func.Invoke();
                if (!result.IsSuccess)
                {
                    return (flag, result);
                }
            }
            return (default(TFlag?), OperationResult.Success);
        }
        /// <summary>
        /// 按枚举顺序执行传入方法集合, 直到遇到出现任意失败项时, 将失败结果及对应的标记组成元组返回, 否则返回成功 ( (default(TFlag), <see cref="OperationResult"/> ) 但是不带成功信息 )
        /// </summary>
        /// <typeparam name="TFlag">方法标记类型</typeparam>
        /// <param name="funcArr"></param>
        /// <returns>失败时: flag: 导致失败的方法对应的标记. operationResult: 导致失败的方法的返回数据</returns>
        public static (TFlag? flag, IOperationResult operationResult) FirstFailure<TFlag, TResult>(params (TFlag flag, Func<TResult> func)[] funcArr)
            where TResult : IOperationResult
        {
            IOperationResult? result = null;
            foreach (var (flag, func) in funcArr)
            {
                result = func.Invoke();
                if (!result.IsSuccess)
                {
                    return (flag, result);
                }
            }
            return (default(TFlag?), OperationResult.Success);
        }
        /// <summary>
        /// 按枚举顺序执行传入方法集合, 直到遇到出现任意失败项时, 将失败结果及对应的标记组成元组返回, 否则返回成功 ( (default(TFlag), <see cref="OperationResult"/> ) 但是不带成功信息 ), 异步版本
        /// </summary>
        /// <typeparam name="TFlag">方法标记类型</typeparam>
        /// <param name="funcArr"></param>
        /// <returns>失败时: flag: 导致失败的方法对应的标记. operationResult: 导致失败的方法的返回数据</returns>
        public static async Task<(TFlag? flag, IOperationResult operationResult)> FirstFailureAsync<TFlag>(params (TFlag flag, Func<Task<IOperationResult>> func)[] funcArr)
        {
            IOperationResult? result = null;
            foreach (var (flag, func) in funcArr)
            {
                result = await func.Invoke();
                if (!result.IsSuccess)
                {
                    return (flag, result);
                }
            }
            return (default(TFlag?), OperationResult.Success);
        }
        /// <summary>
        /// 按枚举顺序执行传入方法集合, 直到遇到出现任意失败项时, 将失败结果及对应的标记组成元组返回, 否则返回成功 ( (default(TFlag), <see cref="OperationResult"/> ) 但是不带成功信息 ), 异步版本
        /// </summary>
        /// <typeparam name="TFlag">方法标记类型</typeparam>
        /// <typeparam name="TResult">统一的结果类型</typeparam>
        /// <param name="funcArr"></param>
        /// <returns>失败时: flag: 导致失败的方法对应的标记. operationResult: 导致失败的方法的返回数据</returns>
        public static async Task<(TFlag? flag, IOperationResult operationResult)> FirstFailureAsync<TFlag, TResult>(params (TFlag flag, Func<Task<TResult>> func)[] funcArr)
            where TResult : IOperationResult
        {
            IOperationResult? result = null;
            foreach (var (flag, func) in funcArr)
            {
                result = await func.Invoke();
                if (!result.IsSuccess)
                {
                    return (flag, result);
                }
            }
            return (default(TFlag?), OperationResult.Success);
        }

        /// <summary>
        /// 根据一个操作结果, 将一个数据包装为 <see cref="OperationResult{T}"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <param name="data"></param>
        /// <param name="defaultFailureInfo">如果操作结果没有失败信息, 则填入此默认值</param>
        /// <returns></returns>
        public static OperationResult<T> Wrapper<T>(this IOperationResult result, T data, string defaultFailureInfo = "")
        {
            if (result.IsSuccess)
            {
                return data;
            }
            else
            {
                return result.FailureReason ?? defaultFailureInfo;
            }
        }
        #endregion

        #region OperationResultEx
        /// <summary>
        /// 调用 <see cref="Failure{T}(Exception)"/>, 不作输入类型检查, 需确保满足原方法的要求
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        private static object _failure_IOperationResultEx(Type type, Exception ex)
        {
            MethodInfo method = typeof(OperationResultHelper).GetMethod(nameof(Failure), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public, [typeof(Exception)])!;
            method = method.MakeGenericMethod(type);
            return method.Invoke(null, [ex])!;
        }
        /// <summary>
        /// 失败的
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static T Failure<T>(Exception ex) where T : IOperationResultEx, new()
        {
            return new T()
            {
                IsSuccess = false,
                FailureReason = "发生异常",
                Exception = ex,
            };
        }
        /// <summary>
        /// 失败的
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="failureInfo"></param>
        /// <returns></returns>
        public static T Failure<T>(Exception ex, string? failureInfo) where T : IOperationResultEx, new()
        {
            return new T()
            {
                IsSuccess = false,
                FailureReason = failureInfo,
                Exception = ex,
            };
        }
        #endregion




        #region 创建操作结果
        /// <summary>
        /// 成功的
        /// </summary>
        public static T Success<T>() where T : IOperationResult, new()
        {
            return new T()
            {
                IsSuccess = true,
                FailureReason = null,
            };
        }
        /// <summary>
        /// 成功的
        /// </summary>
        public static T Success<T>(string? successInfo) where T : IOperationResult, new()
        {
            return new T()
            {
                IsSuccess = true,
                SuccessInfo = successInfo,
            };
        }
        /// <summary>
        /// 失败的
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public static T Failure<T>(string? reason) where T : IOperationResult, new()
        {
            return new T()
            {
                IsSuccess = false,
                FailureReason = reason
            };
        }
        /// <summary>
        /// 失败的
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public static T Failure<T>(object? reason) where T : IOperationResult, new()
        {
            return Failure<T>(reason?.ToString());
        }

        #region 由子操作产生的结果
        /// <summary>
        /// 由子操作产生的成功
        /// </summary>
        /// <param name="child"></param>
        /// <param name="childDesc">对子操作的描述, 比如大概是做什么的子操作, 这个方法会加上冒号 ": "</param>
        /// <returns></returns>
        public static T Success<T>(IOperationResult child, string? childDesc = null) where T : IOperationResult, new()
        {
            string? successStr = child.SuccessInfo;
            childDesc = childDesc?.Trim();
            if (!string.IsNullOrEmpty(childDesc))
            {
                successStr = childDesc + ": " + successStr;
            }
            return Success<T>(successStr);
        }
        /// <summary>
        /// 由子操作产生的成功
        /// </summary>
        /// <param name="child"></param>
        /// <param name="childDesc">对子操作的描述, 比如大概是做什么的子操作, 这个方法会加上冒号 ": "</param>
        /// <returns></returns>
        public static T Success<T, TData>(IOperationResult<TData> child, string? childDesc = null) where T : IOperationResult, new()
        {
            string? successStr = child.SuccessInfo;
            childDesc = childDesc?.Trim();
            if (!string.IsNullOrEmpty(childDesc))
            {
                successStr = childDesc + ": " + successStr;
            }
            return Success<T>(successStr);
        }



        /// <summary>
        /// 由子操作产生的失败
        /// </summary>
        /// <param name="child"></param>
        /// <param name="childDesc">对子操作的描述, 比如大概是做什么的子操作, 这个方法会加上冒号 ": "</param>
        /// <returns></returns>
        public static T Failure<T>(IOperationResult child, string? childDesc = null) where T : IOperationResult, new()
        {
            string? failedStr = child.FailureReason;
            childDesc = childDesc?.Trim();
            if (!string.IsNullOrEmpty(childDesc))
            {
                failedStr = childDesc + ": " + failedStr;
            }
            return Failure<T>(failedStr);
        }
        /// <summary>
        /// 由子操作产生的失败
        /// </summary>
        /// <param name="child"></param>
        /// <param name="childDesc">对子操作的描述, 比如大概是做什么的子操作, 这个方法会加上冒号 ": "</param>
        /// <returns></returns>
        public static T Failure<T, TData>(IOperationResult<TData> child, string? childDesc = null) where T : IOperationResult, new()
        {
            string? failedStr = child.FailureReason;
            childDesc = childDesc?.Trim();
            if (!string.IsNullOrEmpty(childDesc))
            {
                failedStr = childDesc + ": " + failedStr;
            }
            return Failure<T>(failedStr);
        }


        /// <summary>
        /// 由子操作产生的失败
        /// </summary>
        /// <param name="child"></param>
        /// <param name="childDesc">对子操作的描述, 比如大概是做什么的子操作, 这个方法会加上冒号 ": "</param>
        /// <returns></returns>
        public static T FailureEx<T>(IOperationResult child, string? childDesc = null) where T : IOperationResultEx, new()
        {
            string? failedStr = child.FailureReason;
            childDesc = childDesc?.Trim();
            if (!string.IsNullOrEmpty(childDesc))
            {
                failedStr = childDesc + ": " + failedStr;
            }
            if (child is IOperationResultEx childEx && childEx.HasException)
            {
                return Failure<T>(childEx.Exception!, failedStr);
            }
            return Failure<T>(failedStr);
        }
        /// <summary>
        /// 由子操作产生的失败
        /// </summary>
        /// <param name="child"></param>
        /// <param name="childDesc">对子操作的描述, 比如大概是做什么的子操作, 这个方法会加上冒号 ": "</param>
        /// <returns></returns>
        public static T FailureEx<T, TData>(IOperationResult<TData> child, string? childDesc = null) where T : IOperationResultEx, new()
        {
            string? failedStr = child.FailureReason;
            childDesc = childDesc?.Trim();
            if (!string.IsNullOrEmpty(childDesc))
            {
                failedStr = childDesc + ": " + failedStr;
            }
            if (child is IOperationResultEx childEx && childEx.HasException)
            {
                return Failure<T>(childEx.Exception!, failedStr);
            }
            return Failure<T>(failedStr);
        }
        #endregion

        #endregion



        /// <summary>
        /// 成功的
        /// </summary>
        public static T Success<T, TData>(TData? obj) where T : IOperationResult<TData>, new()
        {
            return new T()
            {
                Data = obj,
                IsSuccess = true,
                FailureReason = null,
            };
        }
        /// <summary>
        /// 成功的
        /// </summary>
        public static T Success<T, TData>(TData? obj, string? successInfo) where T : IOperationResult<TData>, new()
        {
            return new T()
            {
                Data = obj,
                IsSuccess = true,
                SuccessInfo = successInfo,
                FailureReason = null,
            };
        }
        /// <summary>
        /// 失败的
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public static T Failure<T, TData>(string? reason) where T : IOperationResult<TData>, new()
        {
            return new T()
            {
                IsSuccess = false,
                FailureReason = reason
            };
        }
        /// <summary>
        /// 失败的
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public static T Failure<T, TData>(object? reason) where T : IOperationResult<TData>, new()
        {
            return Failure<T, TData>(reason?.ToString());
        }

    }

}
