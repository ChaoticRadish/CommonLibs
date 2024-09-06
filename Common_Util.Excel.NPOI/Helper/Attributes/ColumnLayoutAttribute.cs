using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Excel.NPOI.Helper.Attributes
{
    /// <summary>
    /// 列布局
    /// </summary>
    public class ColumnLayoutAttribute : Attribute
    {
        public ColumnLayoutAttribute()
        {

        }
        /// <summary>
        /// 水平对齐方式
        /// </summary>
        public HorizontalAlignment HorizontalAlignment { get; set; }
            = HorizontalAlignment.Center;
        /// <summary>
        /// 垂直对齐方式
        /// </summary>
        public VerticalAlignment VerticalAlignment { get; set; }
            = VerticalAlignment.Top;
        /// <summary>
        /// 列宽
        /// </summary>
        public int ColumnWidth { get; set; } = 15;
        /// <summary>
        /// 最大列宽
        /// </summary>
        public int MaxColumnWidth { get; set; } = 55;
        /// <summary>
        /// 自动列宽
        /// </summary>
        public bool AutoColumnWidth { get; set; } = false;
    }
}
