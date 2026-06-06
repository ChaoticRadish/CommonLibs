using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaoticKit.Winform.Attributes
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
