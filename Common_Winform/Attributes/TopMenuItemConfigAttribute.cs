using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Winform.Attributes
{
    public class TopMenuItemConfigAttribute : Attribute
    {
        public TopMenuItemConfigAttribute(ulong index)
        {
            Index = index;
        }
        public ulong Index { get; set; }
    }
}
