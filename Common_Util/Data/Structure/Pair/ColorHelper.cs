using Common_Util.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Structure.Pair
{
    public static class ColorHelper
    {
        /// <summary>
        /// 将颜色转换为RGB数组, [0] => R; [1] => G; [2] => B
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static byte[] RgbToByteArray(System.Drawing.Color color)
        {
            return
            [
                color.R, color.G, color.B,
            ];
        }

        /// <summary>
        /// HSV模型转换为RGB模型
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static System.Drawing.Color HsvToRgb(HsvaColorF color)
        {
            return HsvToRgb(color.H, color.S, color.V, color.A);
        }
        /// <summary>
        /// HSV模型转换为RGB模型
        /// </summary>
        /// <param name="h">色相</param>
        /// <param name="s">饱和度</param>
        /// <param name="v">明度</param>
        /// <param name="a">透明度</param>
        /// <returns></returns>
        public static System.Drawing.Color HsvToRgb(float h, float s, float v, float a = 1f)
        {
            float R = 0, G = 0, B = 0;
            // 将色相调整到[0, 360)
            h %= 360;
            if (h < 0)
            {
                h += 360;
            }
            if (s == 0)
            {
                R = v;
                G = v;
                B = v;
            }
            else
            {
                int hi = (int)(h / 60);
                float f = h / 60 - hi;
                float p = v * (1.0f - s);
                float q = v * (1.0f - f * s);
                float t = v * (1.0f - (1.0f - f) * s);
                switch (hi)
                {
                    case 0: R = v; G = t; B = p; break;
                    case 1: R = q; G = v; B = p; break;
                    case 2: R = p; G = v; B = t; break;
                    case 3: R = p; G = q; B = v; break;
                    case 4: R = t; G = p; B = v; break;
                    case 5: R = v; G = p; B = q; break;
                }
            }
            return System.Drawing.Color.FromArgb((int)(a * 255), (int)(R * 255), (int)(G * 255), (int)(B * 255));
        }

        /// <summary>
        /// HSV模型转换为RGB模型
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static HsvaColorF RgbToHsv(int r, int g, int b, int a = 255)
        {
            ValueRevision.ToRange(ref r, 0, 255);
            ValueRevision.ToRange(ref g, 0, 255);
            ValueRevision.ToRange(ref b, 0, 255);
            ValueRevision.ToRange(ref a, 0, 255);
            return RgbToHsv(System.Drawing.Color.FromArgb(a, r, g, b));
        }
        /// <summary>
        /// HSV模型转换为RGB模型
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static HsvaColorF RgbToHsv(System.Drawing.Color color)
        {
            float H, S, V;
            // RGB色值转换到[0, 1]
            float R = color.R / 255f;
            float G = color.G / 255f;
            float B = color.B / 255f;
            float CMax = ValueRevision.Max(R, G, B);
            float CMin = ValueRevision.Min(R, G, B);
            float Delta = CMax - CMin;
            // 色相计算
            if (Delta == 0)
            {
                H = 0;
            }
            else if (CMax == R)
            {
                H = 60 * (G - B) / Delta;
            }
            else if (CMax == G)
            {
                H = 60 * (B - R) / Delta + 120;
            }
            else if (CMax == B)
            {
                H = 60 * (R - G) / Delta + 240;
            }
            else
            {
                H = 0;
            }
            H %= 360;
            if (H < 0)
            {
                H += 360;
            }
            // 饱和度计算
            if (CMax == 0)
            {
                S = 0;
            }
            else
            {
                S = Delta / CMax;
            }
            // 明度计算
            V = CMax;
            return new HsvaColorF()
            {
                H = H,
                S = S,
                V = V,
                A = color.A / 255f,
            };
        }
        /// <summary>
        /// 改变输入RGB颜色的明度, 并输出为RGB颜色
        /// </summary>
        /// <param name="color"></param>
        /// <param name="value">明度</param>
        /// <returns></returns>
        public static System.Drawing.Color ChangeValue(System.Drawing.Color color, float value)
        {
            HsvaColorF hsv = RgbToHsv(color);
            hsv.V = value;
            return HsvToRgb(hsv);
        }
        /// <summary>
        /// 改变输入RGB颜色的明度, 并输出为RGB颜色
        /// </summary>
        /// <param name="color"></param>
        /// <param name="valueChangeFunc"></param>
        /// <returns></returns>
        public static System.Drawing.Color ChangeValue(System.Drawing.Color color, Func<float, float> valueChangeFunc)
        {
            HsvaColorF hsv = RgbToHsv(color);
            hsv.V = valueChangeFunc.Invoke(hsv.V);
            return HsvToRgb(hsv);
        }
    }
}
