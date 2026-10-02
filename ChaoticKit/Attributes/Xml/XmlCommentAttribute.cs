using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaoticKit.Attributes.Xml
{
    /// <summary>
    /// XML 注释, 可以在一个 Xml 元素前加一个注释元素
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class XmlCommentAttribute : Attribute
    {

        public XmlCommentAttribute(string text)
        {
            Text = text;
        }

        public string Text { get; }
    }
}
