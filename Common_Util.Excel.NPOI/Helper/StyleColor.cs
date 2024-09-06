using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Excel.NPOI.Helper
{
    public class StyleColor
    {
        /// <summary>
        /// 默认颜色为白色
        /// </summary>
        public StyleColor() { }

        public StyleColor(System.Drawing.Color color) 
        {
            R = color.R; G = color.G; B = color.B;
        }

        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }

        public byte[] ToRgbBytes()
        {
            return [R, G, B];
        }
    }
}
