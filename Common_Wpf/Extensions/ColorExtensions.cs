using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Common_Wpf.Extensions
{
    public static class ColorExtensions
    {
        /// <summary>
        /// 获得颜色的修改透明度之后的新颜色
        /// </summary>
        /// <param name="color"></param>
        /// <param name="newAlpha"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]  
        public static Color Copy(this Color color, byte newAlpha)
        {
            return Color.FromArgb(newAlpha, color.R, color.G, color.B);
        }
    }
}
