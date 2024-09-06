using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Attributes.Xml
{
    /// <summary>
    /// 标记属性不需要类型标签
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class XmlNoTypeTagAttribute : Attribute
    {
    }
}
