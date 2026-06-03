using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Font
{
    public class FontHelper
    {
        /// <summary>
        /// 从像素大小转换成字体大小
        /// </summary>
        /// <param name="size">像素点</param>
        /// <param name="dpi">一英寸多少个像素点</param>
        /// <returns></returns>
        public static float GetEmSize(float size, float dpi)
        {
            float emsize = size / dpi * 72;
            return emsize;
        }
    }
}
