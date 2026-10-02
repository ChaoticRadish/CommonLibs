using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ChaoticKit.Attributes.General
{
    /// <summary>
    /// 别名
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class AliasAttribute : System.Attribute
    {
        public AliasAttribute(params string[] alias) 
        {
            Alias = alias.Distinct().ToArray();
        }

        public string[] Alias { get; }
    }
}
