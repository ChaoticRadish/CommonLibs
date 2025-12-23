using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common_Util.Data.Converter.Json
{
    /// <summary>
    /// 结构体转换为 uint 的 JSON 转换器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class StructToUintJsonConverter<T> : JsonConverter<T>
        where T : struct
    {
        /// <summary>
        /// <typeparamref name="T"/> 转换为数值的方式
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public abstract uint Convert(T value);
        /// <summary>
        /// 数值转换为 <typeparamref name="T"/> 的方式
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public abstract T Convert(uint value);

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Number:
                    if (reader.TryGetUInt32(out uint value))
                    {
                        return Convert(value);
                    }
                    throw new JsonException($"未能将 JSON 数字转换为有效的 uint");
                case JsonTokenType.String:
                    string? stringValue = reader.GetString();
                    if (uint.TryParse(stringValue, out uint uintValueFromString))
                    {
                        return Convert(uintValueFromString);
                    }
                    throw new JsonException($"未能将 JSON 字符串 {stringValue} 转换为有效的 uint");
                default:
                    throw new JsonException($"转换类型 {typeToConvert} 时遇到意料之外的 JSON 标识类型 {reader.TokenType}, 需要一个 uint 值或可转换为 uint 的字符串");
            }
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(Convert(value));
        }
    }
}
