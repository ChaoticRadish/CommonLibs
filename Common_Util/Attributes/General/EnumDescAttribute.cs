using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Attributes.General
{
    [AttributeUsage(AttributeTargets.Field)]
    public class EnumDescAttribute : System.Attribute
    {
        public const string NULL_VALUE_DEFAULT_TEXT = "无描述枚举";

        public EnumDescAttribute(string desc)
        {
            Desc = string.IsNullOrEmpty(desc) ? NULL_VALUE_DEFAULT_TEXT : desc.Trim();
        }

        public string Desc { get; }
    }
}
