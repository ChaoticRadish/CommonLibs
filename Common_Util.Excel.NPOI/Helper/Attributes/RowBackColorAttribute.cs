using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Excel.NPOI.Helper.Attributes
{
    /// <summary>
    /// 行背景颜色
    /// </summary>
    public class RowBackColorAttribute : Attribute
    {
        public RowBackColorAttribute()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="headColor">列头行</param>
        /// <param name="summaryColor">汇总行</param>
        /// <param name="oddColor">奇数行</param>
        /// <param name="evenColor">偶数行</param>
        public RowBackColorAttribute(
            System.Drawing.Color headColor,
            System.Drawing.Color summaryColor,
            System.Drawing.Color oddColor, 
            System.Drawing.Color evenColor)
        {
            HeadRowBackColor = headColor;
            SummaryRowBackColor = summaryColor;
            OddRowBackColor = oddColor;
            EvenRowBackColor = evenColor;
        }

        /// <summary>
        /// 奇数行背景颜色
        /// </summary>
        public System.Drawing.Color OddRowBackColor { get; set; }
        /// <summary>
        /// 偶数行背景颜色
        /// </summary>
        public System.Drawing.Color EvenRowBackColor { get; set; }
        /// <summary>
        /// 列头行背景颜色
        /// </summary>
        public System.Drawing.Color HeadRowBackColor { get; set; }
        /// <summary>
        /// 汇总行背景颜色
        /// </summary>
        public System.Drawing.Color SummaryRowBackColor { get; set; }
    }
}
