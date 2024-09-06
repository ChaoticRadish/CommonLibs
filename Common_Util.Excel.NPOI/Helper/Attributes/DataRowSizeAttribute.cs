using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Excel.NPOI.Helper.Attributes
{
    /// <summary>
    /// 数据行尺寸信息
    /// </summary>
    public class DataRowSizeAttribute : Attribute
    {
        public DataRowSizeAttribute()
        {

        }
        public DataRowSizeAttribute(
            float OddRowHeight, float EvenRowHeight)
        {
            this.OddRowHeight = OddRowHeight > 0 ? OddRowHeight : 1;
            this.EvenRowHeight = EvenRowHeight > 0 ? EvenRowHeight : 1;
        }

        /// <summary>
        /// 奇数行高度
        /// </summary>
        public float OddRowHeight { get; private set; }
        /// <summary>
        /// 奇数行文字高度
        /// </summary>
        public float OddRowFontHeight { get; private set; }
        /// <summary>
        /// 偶数行高度
        /// </summary>
        public float EvenRowHeight { get; private set; }
        /// <summary>
        /// 偶数行文字高度
        /// </summary>
        public float EvenRowFontHeight { get; private set; }
    }
}
