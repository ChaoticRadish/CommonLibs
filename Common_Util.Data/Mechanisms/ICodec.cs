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
    /// 适用于特定介质类型 (<typeparamref name="TMedium"/>) 的 Coder-Decoder（编码器-解码器 / 编解码器）接口
    /// </summary>
    /// <typeparam name="TMedium"></typeparam>
    public interface ICodec<TMedium> : ISerializer<TMedium>, IDeserializer<TMedium>
    {

    }

    /// <summary>
    /// 适用于特定介质类型 (<typeparamref name="TMedium"/>) 的序列化器接口
    /// </summary>
    /// <typeparam name="TMedium">传输介质, 负载数据序列化之后用于传输或存储等用途的数据类型</typeparam>
    public interface ISerializer<TMedium>
    {
        /// <summary>
        /// 序列化负载数据为传输介质
        /// </summary>
        /// <typeparam name="TPayload"></typeparam>
        /// <param name="payload"></param>
        /// <returns></returns>
        IOperationResult<TMedium> Serialize<TPayload>([DisallowNull] TPayload payload);
    }
    /// <summary>
    /// 适用于特定介质类型 (<typeparamref name="TMedium"/>) 的反序列化器接口
    /// </summary>
    /// <typeparam name="TMedium">传输介质, 负载数据序列化之后用于传输或存储等用途的数据类型</typeparam>
    public interface IDeserializer<TMedium>
    {
        /// <summary>
        /// 反序列化传输介质为负责数据
        /// </summary>
        /// <typeparam name="TPayload"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        IOperationResult<TPayload> Deserialize<TPayload>(TMedium obj);
    }
    
}
