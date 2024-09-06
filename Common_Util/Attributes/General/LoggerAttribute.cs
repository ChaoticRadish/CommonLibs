using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Attributes.General
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class LoggerAttribute : Attribute
    {
        public LoggerAttribute(string category, string? subCategory = null) 
        {
            Category = category;
            SubCategory = subCategory ?? string.Empty;
        }

        public bool Enable { get; set; } = true;
        public string Category { get; set; } = string.Empty;
        public string SubCategory { get; set; } = string.Empty;


    }


    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Class)]
    public class LoggerInitiatorAttribute : Attribute
    {
        public LoggerInitiatorAttribute(Type type)
        {
            InitiatorType = type;
        }

        public Type InitiatorType { get; }
    }

}
