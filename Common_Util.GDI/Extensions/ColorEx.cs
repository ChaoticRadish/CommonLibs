using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.GDI.Extensions
{
    public static class ColorEx
    {
        /// <summary>
        /// 将颜色的RGB值都乘以输入数字, 然后钳制到[0, 255]
        /// </summary>
        /// <param name="color"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color Add(this Color color, int r, int g, int b)
        {
            return ClampAndFromArgb(color.A, color.R + r, color.G + g, color.B + b);
        }
        /// <summary>
        /// 将颜色的RGB值都乘以输入数字, 然后钳制到[0, 255]
        /// </summary>
        /// <param name="color"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color Multi(this Color color, double d)
        {
            return Multi(color, d, d, d);
        }
        /// <summary>
        /// 将颜色的RGB值分别乘以输入数字, 然后钳制到[0, 255]
        /// </summary>
        /// <param name="color"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color Multi(this Color color, double r, double g, double b)
        {
            return ClampAndFromArgb(
                color.A,
                (int)(color.R * r),
                (int)(color.G * g),
                (int)(color.B * b));
        }
        /// <summary>
        /// 将颜色的RGB值分别乘以输入数字, 乘count次, 然后钳制到[0, 255]
        /// </summary>
        /// <param name="color"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color Multi(this Color color, double r, double g, double b, double count)
        {
            return ClampAndFromArgb(
                color.A, 
                (int)(color.R * Math.Pow(r, count)),
                (int)(color.G * Math.Pow(g, count)),
                (int)(color.B * Math.Pow(b, count)));
        }

        /// <summary>
        /// 钳制 ARGB 值到[0, 255], 然后使用这些值创建 <see cref="Color"/>
        /// </summary>
        /// <param name="A"></param>
        /// <param name="R"></param>
        /// <param name="G"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Color ClampAndFromArgb(int A, int R, int G, int B)
        {
            if (A < 0) A = 0;
            if (A > 255) A = 255;
            if (R < 0) R = 0;
            if (R > 255) R = 255;
            if (G < 0) G = 0;
            if (G > 255) G = 255;
            if (B < 0) R = 0;
            if (B > 255) B = 255;

            return Color.FromArgb(A, R, G, B);
        }
    }
}
