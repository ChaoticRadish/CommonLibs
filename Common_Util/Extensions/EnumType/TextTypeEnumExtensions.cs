using Common_Util.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Extensions.EnumType
{
    public static class TextTypeEnumExtensions
    {
        /// <summary>
        /// 返回 <paramref name="textType"/> 默认情况下的 MIME 类型对象
        /// </summary>
        /// <param name="textType"></param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException"></exception>
        public static System.Net.Mime.ContentType DefaultContentType(this TextTypeEnum textType)
        {
            return new System.Net.Mime.ContentType(DefaultContentTypeValue(textType));
        }
        /// <summary>
        /// 返回 <paramref name="textType"/> 默认情况下的 MIME 类型字符串值
        /// </summary>
        /// <param name="textType"></param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException"></exception>
        public static string DefaultContentTypeValue(this TextTypeEnum textType)
        {
            return textType switch
            {
                TextTypeEnum.Plain => "text/plain",
                TextTypeEnum.Html => "text/html",
                TextTypeEnum.Json => "application/json",
                TextTypeEnum.Xml => "application/xml",
                TextTypeEnum.Csv => "text/csv",
                TextTypeEnum.Markdown => "text/markdown",
                _ => throw new InvalidDataException($"{textType} 未映射默认 MIME 类型"),
            };
        }
    }
}
