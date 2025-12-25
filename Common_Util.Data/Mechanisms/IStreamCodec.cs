using Common_Util.Data.Struct;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Mechanisms
{
    /// <summary>
    /// 流式编解码器
    /// </summary>
    /// <typeparam name="TUnit">传输单元</typeparam>
    public interface IStreamCodec<TUnit> : IStreamSerializer<TUnit>, IStreamDeserializer<TUnit>
    {
    }

    /// <summary>
    /// 流式序列化器
    /// </summary>
    /// <typeparam name="TUnit">传输单元</typeparam>
    public interface IStreamSerializer<TUnit>
    {
        /// <summary>
        /// 将负载数据流式地序列化为传输单元写入到通道 <paramref name="channel"/>
        /// </summary>
        /// <typeparam name="TPayload"></typeparam>
        /// <param name="payload">负载数据, 不可为空</param>
        /// <param name="channel">失败的情况下, 已写入数据不会回退, 如果有这样子的需求, 需要另行实现</param>
        /// <returns></returns>
        IOperationResult Serialize<TPayload>([DisallowNull] TPayload payload, IWritableChannel<TUnit> channel);
    }

    /// <summary>
    /// 流式反序列化器
    /// </summary>
    /// <typeparam name="TUnit">传输单元</typeparam>
    public interface IStreamDeserializer<TUnit>
    {
        /// <summary>
        /// 从通道流式地读取数据并反序列化为负载数据
        /// </summary>
        /// <typeparam name="TPayload"></typeparam>
        /// <param name="channel"></param>
        /// <returns></returns>
        IOperationResult<TPayload> Deserialize<TPayload>(IReadableChannel<TUnit> channel);
    }


    /// <summary>
    /// 异步流式编解码器
    /// </summary>
    /// <typeparam name="TUnit">传输单元</typeparam>
    public interface IAsyncStreamCodec<TUnit> : IAsyncStreamSerializer<TUnit>, IAsyncStreamDeserializer<TUnit>
    {
    }

    /// <summary>
    /// 异步流式序列化器
    /// </summary>
    /// <typeparam name="TUnit">传输单元</typeparam>
    public interface IAsyncStreamSerializer<TUnit>
    {
        /// <summary>
        /// 将负载数据流式地序列化为传输单元写入到通道 <paramref name="channel"/>
        /// </summary>
        /// <typeparam name="TPayload"></typeparam>
        /// <param name="payload">负载数据, 不可为空</param>
        /// <param name="channel"></param>
        /// <returns></returns>
        ValueTask<IOperationResult> SerializeAsync<TPayload>([DisallowNull] TPayload payload, IAsyncWritableChannel<TUnit> channel, CancellationToken cancellationToken);
        ValueTask<IOperationResult> SerializeAsync<TPayload>([DisallowNull] TPayload payload, IWritableChannel<TUnit> channel, CancellationToken cancellationToken);
    }

    /// <summary>
    /// 异步流式反序列化器
    /// </summary>
    /// <typeparam name="TUnit">传输单元</typeparam>
    public interface IAsyncStreamDeserializer<TUnit>
    {
        /// <summary>
        /// 从通道流式地读取数据并反序列化为负载数据
        /// </summary>
        /// <typeparam name="TPayload"></typeparam>
        /// <param name="channel"></param>
        /// <returns></returns>
        ValueTask<IOperationResult<TPayload>> DeserializeAsync<TPayload>(IAsyncReadableChannel<TUnit> channel, CancellationToken cancellationToken);

        ValueTask<IOperationResult<TPayload>> DeserializeAsync<TPayload>(IReadableChannel<TUnit> channel, CancellationToken cancellationToken);
    }
}
