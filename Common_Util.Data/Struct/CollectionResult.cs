using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Struct
{
    public struct CollectionResult<T> : IOperationResult, ICollectionResult<T>
    {
        public CollectionResult()
        {
            Datas = [];
            PossibleTotal = 0;

            IsSuccess = false;
            SuccessInfo = null;
            FailureReason = null;
        }
        public int Total { get => Datas?.Length ?? 0; }
        public int PossibleTotal { get; set; }
        public T[] Datas { get; set; }

        public bool IsSuccess { get; set; }
        public bool IsFailure { get => !IsSuccess; set => IsSuccess = !value; }
        public string? SuccessInfo { get; set; }
        public string? FailureReason { get; set; }


        /// <summary>
        /// 成功查询, 但是得到空结果
        /// </summary>
        public static CollectionResult<T> EmptyResult(string? successInfo = null) => new()
        {
            IsSuccess = true,
            SuccessInfo = successInfo ?? "空结果",
            PossibleTotal = 0,
            Datas = [],
        };


        /// <summary>
        /// 失败的
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public static CollectionResult<T> Failure(string reason)
            => new()
            {
                Datas = [],
                FailureReason = reason,
                IsFailure = true,
                PossibleTotal = 0,
            };
        public static CollectionResult<T> Success(int possibleTotal, IEnumerable<T> datas)
            => new()
            {
                Datas = datas.ToArray(),
                IsSuccess = true,
                PossibleTotal = possibleTotal,
            };
        public static CollectionResult<T> Success(IEnumerable<T> datas)
            => new()
            {
                Datas = datas.ToArray(),
                IsSuccess = true,
                PossibleTotal = datas.Count(),
            };
        public static CollectionResult<T> Success(int possibleTotal, T[] datas)
            => new()
            {
                Datas = datas,
                IsSuccess = true,
                PossibleTotal = possibleTotal,
            };
        public static CollectionResult<T> Success(T[] datas)
            => new()
            {
                Datas = datas,
                IsSuccess = true,
                PossibleTotal = datas.Length,
            };

        public static implicit operator CollectionResult<T>(string failureReason)
        {
            return Failure(failureReason);
        }
        public static implicit operator CollectionResult<T>((int possibleTotal, IEnumerable<T>? datas) obj)
        {
            return Success(obj.possibleTotal, obj.datas ?? []);
        }
        public static implicit operator CollectionResult<T>(T[] datas)
        {
            return Success(datas);
        }
        public static implicit operator CollectionResult<T>(List<T> datas)
        {
            return Success(datas);
        }


        /// <summary>
        /// 转换为bool值: 是否查询成功
        /// </summary>
        /// <param name="result"></param>
        public static implicit operator bool(CollectionResult<T> result)
        {
            return result.IsSuccess;
        }
        public static implicit operator T[](CollectionResult<T> result)
        {
            return result.Datas;
        }
    }
}
