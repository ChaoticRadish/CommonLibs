using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Extensions.JsonParsing
{
    public static class JsonStringExtensions
    {
        private static Lazy<System.Text.Json.JsonSerializerOptions> indented
            = new(() =>
            {
                return new System.Text.Json.JsonSerializerOptions()
                {
                    WriteIndented = true
                };
            });
        private static Lazy<System.Text.Json.JsonSerializerOptions> notIndented
            = new(() =>
            {
                return new System.Text.Json.JsonSerializerOptions()
                {
                    WriteIndented = false
                };
            });
        /// <summary>
        /// 将对象转换为 Json 字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="isIndented">是否显示格式化</param>
        /// <returns></returns>
        public static string ToJson(this object? obj, bool isIndented = false)
        {
            return System.Text.Json.JsonSerializer.Serialize(obj, 
                options: isIndented ? indented.Value : notIndented.Value);
        }
        /// <summary>
        /// 将 Json 字符串转换为指定类型的对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonStr"></param>
        /// <returns></returns>
        public static T? ToObject<T>(this string? jsonStr)
        {
            return jsonStr == null ? default : System.Text.Json.JsonSerializer.Deserialize<T>(jsonStr);
        }
        /// <summary>
        /// 尝试将 Json 字符串转换为指定类型的对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonStr"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool TryToObject<T>(this string? jsonStr, [NotNullWhen(true)] out T? obj)
        {
            obj = default;
            if (jsonStr == null) return false;
            try
            {
                obj = System.Text.Json.JsonSerializer.Deserialize<T>(jsonStr);
                return obj != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
