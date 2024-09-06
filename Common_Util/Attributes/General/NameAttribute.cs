using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Attributes.General
{
    /// <summary>
    /// 通用的名字特性, 标注名字
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class NameAttribute : System.Attribute
    {
        public NameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

}
