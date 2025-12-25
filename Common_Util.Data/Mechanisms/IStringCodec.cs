using Common_Util.Data.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Mechanisms
{
    /// <summary>
    /// 传输介质为字符串的编解码器
    /// </summary>
    public interface IStringCodec : ICodec<string>, IStringSerializer, IStringDeserializer
    {

    }

    /// <summary>
    /// 传输介质为字符串的序列器
    /// </summary>
    public interface IStringSerializer : ISerializer<string>
    {
    }

    /// <summary>
    /// 传输介质为字符串的反序列器
    /// </summary>
    public interface IStringDeserializer : IDeserializer<string>
    {
    }
}
