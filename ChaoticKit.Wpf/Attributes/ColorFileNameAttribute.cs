using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaoticKit.Wpf.Attributes
{
    internal class ColorFileNameAttribute : System.Attribute
    {
        public ColorFileNameAttribute(string fileName)
        {
            FileName = fileName;
        }

        public string FileName { get; }
    }
}
