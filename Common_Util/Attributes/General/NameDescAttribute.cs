using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Attributes.General
{
    /// <summary>
    /// 配置信息
    /// </summary>
    public class NameDescAttribute : System.Attribute
    {
        public NameDescAttribute(string name, string? desc = null)
        {
            Name = name;
            if (string.IsNullOrEmpty(desc))
            {
                Desc = name;
            }
            else
            {
                Desc = desc;
            }
        }

        public string Name { get; }
        public string Desc { get; }
    }
}
