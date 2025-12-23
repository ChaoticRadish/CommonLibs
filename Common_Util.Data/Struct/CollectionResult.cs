using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Struct
{
    /// <summary>
    /// 集合结果
    /// </summary>
    /// <typeparam name="T"></typeparam>
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

        #region 输出字符串
        /// <summary>
        /// 将结果转换为详细的字符串
        /// </summary>
        /// <param name="itemToString">数据项转换为字符串的方法</param>
        /// <param name="rowNumber">是否输出行号</param>
        /// <param name="rowSplit">行分隔符</param>
        /// <returns></returns>
        public readonly string ToString(Func<T, string> itemToString, bool rowNumber = false, string rowSplit = "\n")
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('<').Append(IsSuccess ? "成功" : "失败").Append('>');
            if (IsSuccess)
            {
                if (!SuccessInfo.IsEmpty())
                {
                    builder.Append(' ').Append(SuccessInfo.TrimEnd());
                }
                if (Datas != null && Datas.Length > 0)
                {
                    int index = 0;
                    foreach (var data in Datas)
                    {
                        index++;
                        builder.Append(rowSplit);
                        if (rowNumber)
                        {
                            builder.Append(index).Append(". ");
                        }
                        builder.Append(itemToString(data));
                    }
                }
                else
                {
                    builder.Append(rowSplit);
                    builder.Append("< 空结果集 >");
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
        #endregion

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
        /// 转换为 <see langword="bool"/> 值: 是否查询成功
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

    /// <summary>
    /// 附带额外数据的集合结果
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <typeparam name="TExtra">附带数据的类型</typeparam>
    public struct CollectionResult<TItem, TExtra> : IOperationResult, ICollectionResult<TItem>
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
        public TItem[] Datas { get; set; }
        /// <summary>
        /// 额外附带的数据
        /// </summary>
        public TExtra? Extra { get; set; }

        public bool IsSuccess { get; set; }
        public bool IsFailure { get => !IsSuccess; set => IsSuccess = !value; }
        public string? SuccessInfo { get; set; }
        public string? FailureReason { get; set; }

        #region 断言

        /// <summary>
        /// 断言操作结果数据 <see cref="Extra"/> 不可能是 <see langword="null"/>, 如果出现了 <see langword="null"/>, 则抛出 <see cref="Common_Util.Exceptions.General.ImplementationException"/>
        /// </summary>
        /// <param name="extreDesc">数据 <see cref="Extra"/> 的描述信息, 会填充到异常描述里</param>
        [MemberNotNull(nameof(Extra))]
        public readonly void ExtraImpossibleNull(string? extreDesc = null)
        {
            if (Extra == null) throw new Common_Util.Exceptions.General.ImplementationException($"{(extreDesc ?? "附带数据")}此处不应该是 null 值");
        }

        #endregion

        #region 静态生成结果

        /// <summary>
        /// 成功查询, 但是得到空结果
        /// </summary>
        public static CollectionResult<TItem, TExtra> EmptyResult(string? successInfo = null) => new()
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
        public static CollectionResult<TItem, TExtra> Failure(string reason)
            => new()
            {
                Datas = [],
                Extra = default,
                FailureReason = reason,
                IsFailure = true,
                PossibleTotal = 0,
            };
        public static CollectionResult<TItem, TExtra> Success(int possibleTotal, IEnumerable<TItem> datas, TExtra? extra = default)
            => new()
            {
                Datas = datas.ToArray(),
                IsSuccess = true,
                PossibleTotal = possibleTotal,
                Extra = extra,
            };
        public static CollectionResult<TItem, TExtra> Success(IEnumerable<TItem> datas, TExtra? extra = default)
            => new()
            {
                Datas = datas.ToArray(),
                IsSuccess = true,
                PossibleTotal = datas.Count(),
                Extra = extra,
            };
        public static CollectionResult<TItem, TExtra> Success(int possibleTotal, TItem[] datas, TExtra? extra = default)
            => new()
            {
                Datas = datas,
                IsSuccess = true,
                PossibleTotal = possibleTotal,
                Extra = extra,
            };
        public static CollectionResult<TItem, TExtra> Success(TItem[] datas, TExtra? extra = default)
            => new()
            {
                Datas = datas,
                IsSuccess = true,
                PossibleTotal = datas.Length,
                Extra = extra,
            };


        #endregion

        #region 隐式转换
        public static implicit operator CollectionResult<TItem, TExtra>(string failureReason)
        {
            return Failure(failureReason);
        }
        public static implicit operator CollectionResult<TItem, TExtra>((int possibleTotal, IEnumerable<TItem>? datas) obj)
        {
            return Success(obj.possibleTotal, obj.datas ?? []);
        }
        public static implicit operator CollectionResult<TItem, TExtra>((int possibleTotal, IEnumerable<TItem>? datas, TExtra? extra) obj)
        {
            return Success(obj.possibleTotal, obj.datas ?? [], obj.extra);
        }
        public static implicit operator CollectionResult<TItem, TExtra>(TItem[] datas)
        {
            return Success(datas);
        }
        public static implicit operator CollectionResult<TItem, TExtra>((TItem[] datas, TExtra? extra) obj)
        {
            return Success(obj.datas, obj.extra);
        }
        public static implicit operator CollectionResult<TItem, TExtra>(List<TItem> datas)
        {
            return Success(datas);
        }
        public static implicit operator CollectionResult<TItem, TExtra>((List<TItem> datas, TExtra? extra) obj)
        {
            return Success(obj.datas, obj.extra);
        }


        /// <summary>
        /// 转换为 <see langword="bool"/> 值: 是否查询成功
        /// </summary>
        /// <param name="result"></param>
        public static implicit operator bool(CollectionResult<TItem, TExtra> result)
        {
            return result.IsSuccess;
        }
        public static implicit operator TItem[](CollectionResult<TItem, TExtra> result)
        {
            return result.Datas;
        }
        #endregion
    }
}
