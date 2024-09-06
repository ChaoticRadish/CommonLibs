using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Attributes.Xml
{
    /// <summary>
    /// 标记一个属性可以直接输出为字符串值, 而不用带有类型信息
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class XmlTextValueAttribute : Attribute
    {
        /// <summary>
        /// 如果属性是时间, 使用此格式字符串
        /// </summary>
        public string? DateTimeFormat { get; set; } = "yyyy-MM-dd HH:mm:ss:fff";
    }
}
