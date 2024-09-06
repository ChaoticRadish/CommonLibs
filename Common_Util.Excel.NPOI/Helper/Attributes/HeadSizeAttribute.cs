using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Excel.NPOI.Helper.Attributes
{
    /// <summary>
    /// 标题高度, 单位: 点
    /// </summary>
    public class HeadSizeAttribute : Attribute
    {
        public HeadSizeAttribute(float height, float fontHeight)
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
