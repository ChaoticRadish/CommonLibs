using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Struct
{
    public struct OperationResultEx : IOperationResultEx
    {
        public bool IsSuccess { get; set; }
        public bool IsFailure { readonly get => !IsSuccess; set => IsSuccess = !value; }
        public string? SuccessInfo { get; set; }
        public string? FailureReason { get; set; }

        /// <summary>
        /// 操作结果是否包含异常, 不支持直接设置值! 
        /// </summary>
        public bool HasException { readonly get => Exception != null; set => throw new NotSupportedException(); }
        public Exception? Exception { get; set; }

        public override readonly string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('<');
            if (IsSuccess)
            {
                builder.Append("成功");
            }
            else
            {
                if (HasException)
                {
                    builder.Append("异常");
                }
                else
                {
                    builder.Append("失败");
                }
            }
            builder.Append('>');
            if (IsSuccess)
            {
                if (!SuccessInfo.IsEmpty())
                {
                    builder.Append(' ').Append(SuccessInfo);
                }
            }
            else
            {
                // 如果有异常, 优先显示异常信息
                if (HasException)
                {
                    builder.Append(' ').Append(Exception!.Message);
                }
                else if (!FailureReason.IsEmpty())
                {
                    builder.Append(' ').Append(FailureReason);
                }

            }
            return builder.ToString();
        }

        #region 静态方法
        /// <summary>
        /// 成功的
        /// </summary>
        public static OperationResultEx Success => OperationResultHelper.Success<OperationResultEx>();
        /// <summary>
        /// 包含成功信息的成功结果
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static OperationResultEx SuccessWithInfo(string info)
        {
            return OperationResultHelper.Success<OperationResultEx>(info);
        }

        /// <summary>
        /// 失败的
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public static OperationResultEx Failure(string? reason)
        {
            return OperationResultHelper.Failure<OperationResultEx>(reason);
        }
        /// <summary>
        /// 失败的
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static OperationResultEx Failure(Exception ex)
        {
            return new OperationResultEx()
            {
                Exception = ex,
                IsSuccess = false,
                FailureReason = "发生异常: " + ex.Message,
            };
        }
        /// <summary>
        /// 由子操作产生的失败
        /// </summary>
        /// <param name="child"></param>
        /// <param name="childDesc">对子操作的描述, 比如大概是做什么的子操作, 这个方法会加上冒号 ": "</param>
        /// <returns></returns>
        public static OperationResultEx Failure(IOperationResult child, string? childDesc = null)
            => OperationResultHelper.FailureEx<OperationResultEx>(child, childDesc);


        #endregion

        #region 隐式转换
        public static implicit operator bool(OperationResultEx result)
        {
            return result.IsSuccess;
        }
        public static implicit operator OperationResultEx(bool b)
        {
            return b ? Success : Failure("失败!");
        }
        public static implicit operator OperationResultEx(string failureReason)
        {
            return Failure(failureReason);
        }
        public static implicit operator OperationResultEx(Exception ex)
        {
            return Failure(ex);
        }
        public static implicit operator OperationResultEx((bool isSuccess, string? successInfoOrFailureReason) obj)
        {
            return obj.isSuccess ?
                obj.successInfoOrFailureReason == null ? Success : SuccessWithInfo(obj.successInfoOrFailureReason)
                :
                Failure(obj.successInfoOrFailureReason);
        }

        public static implicit operator OperationResultEx((IOperationResult result, Exception? ex) obj)
        {
            return new OperationResultEx()
            {
                IsSuccess = obj.result.IsSuccess,
                FailureReason = obj.result.FailureReason,
                SuccessInfo = obj.result.SuccessInfo,
                Exception = obj.ex,
            };
        }
        #endregion
    }
    /// <summary>
    /// 附带数据的扩展操作结果
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct OperationResultEx<T> : IOperationResultEx<T>
    {
        public bool IsSuccess { get; set; }
        public bool IsFailure { get => !IsSuccess; set => IsSuccess = !value; }
        public string? SuccessInfo { get; set; }
        public string? FailureReason { get; set; }
        public T? Data { get; set; }

        /// <summary>
        /// 操作结果是否包含异常, 不支持直接设置值! 
        /// </summary>
        public bool HasException { readonly get => Exception != null; set => throw new NotSupportedException(); }
        public Exception? Exception { get; set; }

        public override readonly string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('<');
            if (IsSuccess)
            {
                builder.Append("成功");
            }
            else
            {
                if (HasException)
                {
                    builder.Append("异常");
                }
                else
                {
                    builder.Append("失败");
                }
            }
            builder.Append('>');
            if (IsSuccess)
            {
                if (!SuccessInfo.IsEmpty())
                {
                    builder.Append(' ').Append(SuccessInfo);
                }
            }
            else
            {
                // 如果有异常, 优先显示异常信息
                if (HasException)
                {
                    builder.Append(' ').Append(Exception!.Message);
                }
                else if (!FailureReason.IsEmpty())
                {
                    builder.Append(' ').Append(FailureReason);
                }

            }
            return builder.ToString();
        }
        /// <summary>
        /// 成功的
        /// </summary>
        public static OperationResultEx<T> Success(T? obj)
            => OperationResultHelper.Success<OperationResultEx<T>, T>(obj);
        /// <summary>
        /// 成功的
        /// </summary>
        public static OperationResultEx<T> Success(T? obj, string? info)
            => OperationResultHelper.Success<OperationResultEx<T>, T>(obj, info);
        /// <summary>
        /// 失败的
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public static OperationResultEx<T> Failure(string? reason)
            => OperationResultHelper.Failure<OperationResultEx<T>, T>(reason);
        /// <summary>
        /// 失败的
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static OperationResultEx<T> Failure(Exception ex)
        {
            return new OperationResultEx<T>()
            {
                Exception = ex,
                IsSuccess = false,
                FailureReason = "发生异常: " + ex.Message,
            };
        }
        /// <summary>
        /// 失败的 (附带数据)
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public static OperationResultEx<T> Failure((string? reason, T data) args)
        {
            var result = OperationResultHelper.Failure<OperationResultEx<T>, T>(args.reason);
            result.Data = args.data;
            return result;
        }
        /// <summary>
        /// 失败的
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public static OperationResultEx<T> Failure(object reason)
            => OperationResultHelper.Failure<OperationResultEx<T>, T>(reason);
        /// <summary>
        /// 由子操作产生的失败
        /// </summary>
        /// <param name="child"></param>
        /// <param name="childDesc">对子操作的描述, 比如大概是做什么的子操作, 这个方法会加上冒号 ": "</param>
        /// <returns></returns>
        public static OperationResultEx<T> Failure(IOperationResult<T> child, string? childDesc = null)
            => OperationResultHelper.FailureEx<OperationResultEx<T>, T>(child, childDesc);


        #region 隐式转换
        public static implicit operator OperationResult(OperationResultEx<T> result)
        {
            return new OperationResult()
            {
                IsSuccess = result.IsSuccess,
                FailureReason = result.FailureReason,
                SuccessInfo = result.SuccessInfo,
            };
        }
        public static implicit operator bool(OperationResultEx<T> result)
        {
            return result.IsSuccess;
        }
        public static implicit operator OperationResultEx<T>(T obj)
        {
            return Success(obj);
        }
        public static implicit operator OperationResultEx<T>(bool b)
        {
            return b ? Success(default) : Failure("失败!");
        }
        public static implicit operator OperationResultEx<T>(Exception ex)
        {
            return Failure(ex);
        }
        public static implicit operator OperationResultEx<T>(string failureReason)
        {
            return Failure(failureReason);
        }
        public static implicit operator OperationResultEx<T>((string failureReason, T data) args)
        {
            return Failure(args);
        }

        /// <summary>
        /// 隐式转换
        /// </summary>
        /// <param name="args">参数 info: 成功时作为成功信息, 失败时作为失败原因信息</param>
        public static implicit operator OperationResultEx<T>((bool b, T? data, string? info) args)
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
        public static implicit operator OperationResultEx<T>((IOperationResult<T> result, Exception? ex) obj)
        {
            return new OperationResultEx<T>()
            {
                Data = obj.result.Data,
                IsSuccess = obj.result.IsSuccess,
                FailureReason = obj.result.FailureReason,
                SuccessInfo = obj.result.SuccessInfo,
                Exception = obj.ex,
            };
        }
        #endregion
    }

}
