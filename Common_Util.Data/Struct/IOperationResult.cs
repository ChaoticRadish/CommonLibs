using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Struct
{
    #region 基础版本, 区分: 成功, 失败
    /// <summary>
    /// 简单的操作结果接口
    /// </summary>
    public interface IOperationResult
    {
        /// <summary>
        /// 成功了?
        /// </summary>
        bool IsSuccess { get; set; }
        /// <summary>
        /// 失败了?
        /// </summary>
        bool IsFailure { get; set; }

        /// <summary>
        /// 失败原因
        /// </summary>
        string? FailureReason { get; set; }

        /// <summary>
        /// 成功信息
        /// </summary>
        string? SuccessInfo { get; set; }

    }
    /// <summary>
    /// 附带数据的操作结果接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IOperationResult<T> : IOperationResult
    {
        /// <summary>
        /// 操作结果附带的数据
        /// </summary>
        T? Data { get; set; }
    }


    /// <summary>
    /// 扩展操作结果接口
    /// </summary>
    public interface IOperationResultEx : IOperationResult
    {
        [MemberNotNullWhen(true, nameof(Exception))]
        bool HasException { get; set; }

        Exception? Exception { get; set; }
    }


    /// <summary>
    /// 附带数据的扩展操作结果接口
    /// </summary>
    /// <remarks>
    /// 当一个方法返回此类型的结果时, 暗示已处理可能出现的异常, 如果再有异常抛出, 则只会是意料之外的异常! (如代码编码上的问题)
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public interface IOperationResultEx<T> : IOperationResult<T>, IOperationResultEx
    {

    }

    #endregion

    #region 带集合类型的数据

    /// <summary>
    /// 带集合类型数据的结果
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICollectionResult<T>
    {
        /// <summary>
        /// 集合数据
        /// </summary>
        T[] Datas { get; }

        /// <summary>
        /// 返回的数据总量
        /// </summary>
        int Total { get; }
        /// <summary>
        /// 有可能的数据总量, 可能会大于 <see cref="Total"/>, 比如分页查询时, 
        /// <see cref="Total"/> 为返回的总量
        /// <see cref="PossibleTotal"/> 为所有页面数量的总和
        /// </summary>
        int PossibleTotal { get; }
    }

    public interface ICollectionResultEx<T> : ICollectionResult<T>, IOperationResultEx
    { 
    }

    /// <summary>
    /// 分页查询结果
    /// </summary>
    public interface IPagingQueryResult<T>
    {
        PagingArgs Paging { get; }

        int Total { get; set; }

        T[] Datas { get; set; }

    }

    public interface IPagingQueryResultEx<T> : IPagingQueryResult<T>, IOperationResultEx
    {
    }
    #endregion
}
