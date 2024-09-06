using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Struct
{
    /// <summary>
    /// 分页查询结果的帮助类
    /// </summary>
    public static class PagingQueryResultHelper
    {
        public static PagingQueryResult<T> Success<T>(PagingArgs paging, int total, IEnumerable<T> results)
        {
            return PagingQueryResult<T>.Success(paging, total, results);
        }
    }
    /// <summary>
    /// 分页查询结果
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct PagingQueryResult<T> : IOperationResult, IPagingQueryResult<T>
    {
        public PagingQueryResult()
        {
            Datas = Array.Empty<T>();
            Paging = default;
            Total = 0;
            IsSuccess = false;
            SuccessInfo = null;
            FailureReason = null;
        }

        public PagingArgs Paging { get; set; }
        public int Total { get; set; }
        public T[] Datas { get; set; }
        public bool IsSuccess { get; set; }
        public bool IsFailure { get => !IsSuccess; set => IsSuccess = !value; }
        public string? SuccessInfo { get; set; }
        public string? FailureReason { get; set; }

        #region 转换
        /// <summary>
        /// 转换当前结果的数据为目标类型 (批量转换)
        /// </summary>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="convertFunc"></param>
        /// <returns></returns>
        public PagingQueryResult<TTarget> ConvertOneTime<TTarget>(Func<T[], IEnumerable<TTarget>> convertFunc)
        {
            return new PagingQueryResult<TTarget>()
            {
                Datas = Datas.Length == 0 ? [] : convertFunc(Datas).ToArray(),
                Total = Total,
                FailureReason = FailureReason,
                IsFailure = IsFailure,
                IsSuccess = IsSuccess,  
                Paging = Paging,
                SuccessInfo = SuccessInfo,
            };
        }
        /// <summary>
        /// 转换当前结果的数据为目标类型 (逐一转换)
        /// </summary>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="convertFunc"></param>
        /// <returns></returns>
        public PagingQueryResult<TTarget> Convert<TTarget>(Func<T, TTarget> convertFunc)
        {
            return new PagingQueryResult<TTarget>()
            {
                Datas = Datas.Length == 0 ? [] : Datas.Select(i => convertFunc(i)).ToArray(),
                Total = Total,
                FailureReason = FailureReason,
                IsFailure = IsFailure,
                IsSuccess = IsSuccess,
                Paging = Paging,
                SuccessInfo = SuccessInfo,
            };
        }
        #endregion

        /// <summary>
        /// 成功查询, 但是得到空结果
        /// </summary>
        public static PagingQueryResult<T> EmptyResult(PagingArgs paging, string? successInfo = null) => new PagingQueryResult<T>()
        {
            IsSuccess = true,
            SuccessInfo = successInfo ?? "空结果",
            Paging = paging,
            Total = 0,
            Datas = Array.Empty<T>(),
        };


        /// <summary>
        /// 失败的
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public static PagingQueryResult<T> Failure(PagingArgs paging, string reason)
            => new()
            {
                Datas = Array.Empty<T>(),
                FailureReason = reason,
                IsFailure = true,
                Paging = paging,
            };
        public static PagingQueryResult<T> Success(PagingArgs paging, int total, IEnumerable<T> datas)
            => new()
            {
                Datas = datas.ToArray(),
                IsSuccess = true,
                Paging = paging,
                Total = total,
            };
        public static PagingQueryResult<T> Success(PagingArgs paging, int total, T[] datas)
            => new()
            {
                Datas = datas,
                IsSuccess = true,
                Paging = paging,
                Total = total,
            };

        public static implicit operator PagingQueryResult<T>((PagingArgs paging, string failureReason) obj)
        {
            return Failure(obj.paging, obj.failureReason);
        }
        public static implicit operator PagingQueryResult<T>((PagingArgs paging, int total, IEnumerable<T> datas) obj)
        {
            return Success(obj.paging, obj.total, obj.datas);
        }
        public static implicit operator PagingQueryResult<T>((PagingArgs paging, IEnumerable<T> datas) obj)
        {
            return Success(obj.paging, obj.datas.Count(), obj.datas);
        }


        /// <summary>
        /// 转换为bool值: 是否查询成功
        /// </summary>
        /// <param name="result"></param>
        public static implicit operator bool(PagingQueryResult<T> result)
        {
            return result.IsSuccess;
        }
        public static implicit operator T[](PagingQueryResult<T> result)
        {
            return result.Datas;
        }
    }
}
