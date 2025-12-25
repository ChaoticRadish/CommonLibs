using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Mechanisms
{
    /// <summary>
    /// 传输介质为 <see langword="byte"/>[] 的编解码器
    /// </summary>
    public interface IBinaryCodec : ICodec<byte[]>, IBinarySerializer, IBinaryDeserializer
    {

    }

    /// <summary>
    /// 传输介质为 <see langword="byte"/>[] 的序列器
    /// </summary>
    public interface IBinarySerializer : ISerializer<byte[]>
    {
    }

    /// <summary>
    /// 传输介质为 <see langword="byte"/>[] 的反序列器
    /// </summary>
    public interface IBinaryDeserializer : IDeserializer<byte[]>
    {
    }
}
