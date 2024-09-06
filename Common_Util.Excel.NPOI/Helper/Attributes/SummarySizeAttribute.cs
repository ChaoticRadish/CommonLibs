using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Excel.NPOI.Helper.Attributes
{
    /// <summary>
    /// 汇总行尺寸
    /// </summary>
    public class SummarySizeAttribute : Attribute
    {
        public SummarySizeAttribute(float height, float fontHeight)
        {
            Height = height > 0 ? height : 1;
            FontHeight = fontHeight > 0 ? fontHeight : 1;
        }

        /// <summary>
        /// 高度
        /// </summary>
        public float Height { get; private set; }
        /// <summary>
        /// 字体高度
        /// </summary>
        public float FontHeight { get; private set; }
    }
}
