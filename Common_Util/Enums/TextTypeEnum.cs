using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Enums
{
    /// <summary>
    /// 文本类型的枚举
    /// </summary>
    public enum TextTypeEnum
    {
        /// <summary>
        /// 普通文本
        /// </summary>
        Plain,
        /// <summary>
        /// Json 格式文本
        /// </summary>
        Json,
        /// <summary>
        /// Xml 格式文本
        /// </summary>
        Xml,
        /// <summary>
        /// Html 格式文本
        /// </summary>
        Html,
        /// <summary>
        /// 逗号分隔值文件
        /// </summary>
        Csv,
        /// <summary>
        /// Markdown 格式文本
        /// </summary>
        Markdown,
    }
}
