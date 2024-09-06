using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Attributes.Xml
{
    /// <summary>
    /// 标记一个节点为纯文本节点, 即不含任何属性, 只有文本内容
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class XmlTextNodeAttribute : Attribute
    {
    }
}
