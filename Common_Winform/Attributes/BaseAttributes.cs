using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Winform.Attributes
{
    public class ShowInfoAttribute : Attribute
    {
        public ShowInfoAttribute(string name, string? desc = null)
        {
            Name = name;
            Desc = desc;
        }

        public string Name { get; }
        public string? Desc { get; }
    }

    public class MethodAttribute : Attribute
    {
        public MethodAttribute(string methodName)
        {
            MethodName = methodName;
        }

        public string MethodName { get; }
    }
}
