using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Excel.NPOI.Helper
{
    public class SheetOption
    {
        /// <summary>
        /// 工作表名字
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 绘制标题
        /// </summary>
        public bool UseTitle { get; set; }
        /// <summary>
        /// 标题高度, 单位: 点
        /// </summary>
        public float TitleHeight { get; set; }
        /// <summary>
        /// 标题字体高度, 单位: 点
        /// </summary>
        public float TitleFontHeight { get; set; }

        /// <summary>
        /// 列头高度, 单位: 点
        /// </summary>
        public float HeadHeight { get; set; }
        /// <summary>
        /// 列头字体高度, 单位: 点
        /// </summary>
        public float HeadFontHeight { get; set; }

        /// <summary>
        /// 绘制绘制
        /// </summary>
        public bool UseSummery { get; set; }
        /// <summary>
        /// 汇总行高度, 单位: 点
        /// </summary>
        public float SummaryHeight { get; set; }
        /// <summary>
        /// 汇总行字体高度, 单位: 点
        /// </summary>
        public float SummaryFontHeight { get; set; }

        /// <summary>
        /// 列头行背景颜色
        /// </summary>
        public System.Drawing.Color HeadRowBackColor { get; set; }
        /// <summary>
        /// 汇总行背景颜色
        /// </summary>
        public System.Drawing.Color SummaryRowBackColor { get; set; }
        /// <summary>
        /// 奇数行背景颜色
        /// </summary>
        public System.Drawing.Color OddRowBackColor { get; set; }
        /// <summary>
        /// 偶数行背景颜色
        /// </summary>
        public System.Drawing.Color EvenRowBackColor { get; set; }

        /// <summary>
        /// 奇数行高度
        /// </summary>
        public float DataOddRowHeight { get; set; }
        /// <summary>
        /// 偶数行高度
        /// </summary>
        public float DataEvenRowHeight { get; set; }

    }
}
