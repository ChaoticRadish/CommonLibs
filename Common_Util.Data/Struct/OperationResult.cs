using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace Common_Util.Data.Struct
{
    public struct OperationResult : IOperationResult
    {
        public bool IsSuccess { get; set; }
        public bool IsFailure { get => !IsSuccess; set => IsSuccess = !value; }
        public string? SuccessInfo { get; set; }
        public string? FailureReason { get; set; }

        public override readonly string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('<').Append(IsSuccess ? "成功" : "失败").Append('>');
            if (IsSuccess)
            {
                if (!SuccessInfo.IsEmpty())
                {
                    builder.Append(' ').Append(SuccessInfo);
                }
            }
            else
            {
                if (!FailureReason.IsEmpty())
                {
                    builder.Append(' ').Append(FailureReason);
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// 将当前操作结果作为子操作结果, 返回一个新的操作结果
        /// </summary>
        /// <param name="operationInfo"></param>
        /// <returns></returns>
        public readonly OperationResult AsSubOperation(string operationInfo)
        {
            if (IsSuccess)
            {
                return SubSuccess(this, operationInfo);
            }
            else
            {
                return Failure(this, operationInfo);
            }
        }


        /// <summary>
        /// 成功的
        /// </summary>
        public static OperationResult Success => OperationResultHelper.Success<OperationResult>();
        /// <summary>
        /// 由子操作产生的成功
        /// </summary>
        /// <param name="child"></param>
        /// <param name="childDesc">对子操作的描述, 比如大概是做什么的子操作, 这个方法会加上冒号 ": "</param>
        /// <returns></returns>
        public static OperationResult SubSuccess(OperationResult child, string? childDesc = null)
        {
            return OperationResultHelper.Success<OperationResult>(child, childDesc);
        }

        /// <summary>
        /// 失败的
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public static OperationResult Failure(string? reason)
        {
            return OperationResultHelper.Failure<OperationResult>(reason);
        }
        /// <summary>
        /// 失败的
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public static OperationResult Failure(object reason)
        {
            return OperationResultHelper.Failure<OperationResult>(reason);
        }
        /// <summary>
        /// 由子操作产生的失败
        /// </summary>
        /// <param name="child"></param>
        /// <param name="childDesc">对子操作的描述, 比如大概是做什么的子操作, 这个方法会加上冒号 ": "</param>
        /// <returns></returns>
        public static OperationResult Failure(OperationResult child, string? childDesc = null)
        {
            return OperationResultHelper.Failure<OperationResult>(child, childDesc);
        }
        /// <summary>
        /// 由子操作产生的失败
        /// </summary>
        /// <param name="child"></param>
        /// <param name="childDesc">对子操作的描述, 比如大概是做什么的子操作, 这个方法会加上冒号 ": "</param>
        /// <returns></returns>
        public static OperationResult Failure<T>(OperationResult<T> child, string? childDesc = null)
        {
            return OperationResultHelper.Failure<OperationResult, T>(child, childDesc);
        }

        #region 隐式转换
        public static implicit operator bool(OperationResult result)
        {
            return result.IsSuccess;
        }
        public static implicit operator OperationResult(bool b)
        {
            return b ? Success : Failure("失败!");
        }
        public static implicit operator OperationResult((bool isSuccess, string? successInfoOrFailureReason) obj)
        {
            return obj.isSuccess ?
                new OperationResult() { IsSuccess = true, SuccessInfo = obj.successInfoOrFailureReason }
                :
                Failure(obj.successInfoOrFailureReason);
        }
        public static implicit operator OperationResult(string failureReason)
        {
            return Failure(failureReason);
        }
        public static implicit operator OperationResultEx(OperationResult result)
        {
            return new OperationResultEx()
            {
                Exception = null,
                IsSuccess = result.IsSuccess,
                FailureReason = result.FailureReason,
                SuccessInfo = result.SuccessInfo,
            };
        }
        public static implicit operator OperationResult(OperationResultEx result)
        {
            return new OperationResult()
            {
                IsSuccess = result.IsSuccess,
                FailureReason = result.IsFailure ? result.FailureString(null) : result.FailureReason,
                SuccessInfo = result.SuccessInfo,
            };
        }
        #endregion

    }
    /// <summary>
    /// 附带数据的操作结果
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct OperationResult<T> : IOperationResult<T>
    {
        public bool IsSuccess { get; set; }
        public bool IsFailure { get => !IsSuccess; set => IsSuccess = !value; }
        public string? SuccessInfo { get; set; }
        public string? FailureReason { get; set; }
        public T? Data { get; set; }


        /// <summary>
        /// 断言操作结果数据 <see cref="Data"/> 不可能是 <see langword="null"/>, 如果出现了 <see langword="null"/>, 则抛出 <see cref="Common_Util.Exceptions.General.ImplementationException"/>
        /// </summary>
        /// <param name="dataDesc">数据 <see cref="Data"/> 的描述信息, 会填充到异常描述里</param>
        [MemberNotNull(nameof(Data))]
        public readonly void DataImpossibleNull(string? dataDesc = null)
        {
            if (Data == null) throw new Common_Util.Exceptions.General.ImplementationException($"{(dataDesc ?? "结果数据")}此处不应该是 null 值");
        }

        public override readonly string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('<').Append(IsSuccess ? "成功" : "失败").Append('>');
            if (IsSuccess)
            {
                if (!SuccessInfo.IsEmpty())
                {
                    builder.Append(' ').Append(SuccessInfo);
                }
            }
            else
            {
                if (!FailureReason.IsEmpty())
                {
                    builder.Append(' ').Append(FailureReason);
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// 成功的
        /// </summary>
        public static OperationResult<T> Success(T? obj)
            => OperationResultHelper.Success<OperationResult<T>, T>(obj);
        /// <summary>
        /// 成功的
        /// </summary>
        public static OperationResult<T> Success(T? obj, string? info)
            => OperationResultHelper.Success<OperationResult<T>, T>(obj, info);
        /// <summary>
        /// 失败的
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public static OperationResult<T> Failure(string? reason)
            => OperationResultHelper.Failure<OperationResult<T>, T>(reason);
        /// <summary>
        /// 失败的 (附带数据)
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public static OperationResult<T> Failure((string? reason, T data) args)
        {
            var result = OperationResultHelper.Failure<OperationResult<T>, T>(args.reason);
            result.Data = args.data;
            return result;
        }
        /// <summary>
        /// 失败的
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public static OperationResult<T> Failure(object reason)
            => OperationResultHelper.Failure<OperationResult<T>, T>(reason);
        /// <summary>
        /// 由子操作产生的失败
        /// </summary>
        /// <param name="child"></param>
        /// <param name="childDesc">对子操作的描述, 比如大概是做什么的子操作, 这个方法会加上冒号 ": "</param>
        /// <returns></returns>
        public static OperationResult<T> Failure(IOperationResult<T> child, string? childDesc = null)
            => OperationResultHelper.Failure<OperationResult<T>, T>(child, childDesc);

        #region 隐式转换
        public static implicit operator OperationResult(OperationResult<T> result)
        {
            return new OperationResult()
            {
                IsSuccess = result.IsSuccess,
                FailureReason = result.FailureReason,
                SuccessInfo = result.SuccessInfo,
            };
        }
        public static implicit operator bool(OperationResult<T> result)
        {
            return result.IsSuccess;
        }
        public static implicit operator OperationResult<T>(T obj)
        {
            return Success(obj);
        }
        public static implicit operator OperationResult<T>(bool b)
        {
            return b ? Success(default) : Failure("失败!");
        }
        public static implicit operator OperationResult<T>(string failureReason)
        {
            return Failure(failureReason);
        }
        public static implicit operator OperationResult<T>((string failureReason, T data) args)
        {
            return Failure(args);
        }

        /// <summary>
        /// 隐式转换
        /// </summary>  
        /// <param name="args">参数 info: 成功时作为成功信息, 失败时作为失败原因信息</param>
        public static implicit operator OperationResult<T>((bool b, T? data, string? info) args)
        {
            if (args.b)
            {
                return Success(args.data, args.info);
            }
            else
            {
                return Failure(args.info);
            }
        }

        public static implicit operator OperationResultEx<T>(OperationResult<T> result)
        {
            return new OperationResultEx<T>()
            {
                Exception = null,
                Data = result.Data,
                IsSuccess = result.IsSuccess,
                FailureReason = result.FailureReason,
                SuccessInfo = result.SuccessInfo,
            };
        }
        public static implicit operator OperationResult<T>(OperationResultEx<T> result)
        {
            string? failureReason = result.FailureString();
            return new OperationResult<T>()
            {
                Data = result.Data,
                IsSuccess = result.IsSuccess,
                FailureReason = failureReason,
                SuccessInfo = result.SuccessInfo,
            };
        }
        #endregion
    }



}
