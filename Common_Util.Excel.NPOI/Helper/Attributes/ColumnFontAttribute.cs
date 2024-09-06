using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Excel.NPOI.Helper.Attributes
{
    public class ColumnFontAttribute : Attribute
    {
        /// <summary>
        /// 字体高度, 单位: 点
        /// </summary>
        public float Height { get => height; set => height = value > 0 ? value : 1; }
        private float height = 14;
        /// <summary>
        /// 是粗体
        /// </summary>
        public bool IsBold { get; set; } = false;
    }
}
