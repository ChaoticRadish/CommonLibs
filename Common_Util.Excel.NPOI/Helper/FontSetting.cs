using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Excel.NPOI.Helper
{
    class FontSetting
    {
        /// <summary>
        /// 字体高度, 单位: 点
        /// </summary>
        public float Height { get; set; } = 14;
        /// <summary>
        /// 是粗体
        /// </summary>
        public bool IsBold { get; set; } = false;
    }
}
