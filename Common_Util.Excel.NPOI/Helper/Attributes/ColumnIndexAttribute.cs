using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Excel.NPOI.Helper.Attributes
{
    /// <summary>
    /// 列索引
    /// </summary>
    public class ColumnIndexAttribute : Attribute
    {
        public ColumnIndexAttribute(int index)
        {
            Index = index;
        }
        public int Index { get; set; }
    }
}
