
namespace Common_Util.Maths
{
    public static class ColorCalcUtil
    {
        /// <summary>
        /// HSL颜色转换为RGB颜色
        /// </summary>
        /// <param name="h"></param>
        /// <param name="s"></param>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static void HSL2RGB(float h, float s, float l, out float r, out float g, out float b)
        {
            float num = ((!(l < 0.5f)) ? ((1f - (2f * l - 1f)) * s) : ((1f + (2f * l - 1f)) * s));
            float m = l - 0.5f * num;
            FromHCM(h, num, m, out r, out g, out b);
        }

        /// <summary>
        /// HSV颜色转换为RGB颜色
        /// </summary>
        /// <param name="h"></param>
        /// <param name="s"></param>
        /// <param name="v"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static void HSV2RGB(float h, float s, float v, out float r, out float g, out float b)
        {
            float num = v * s;
            float m = v - num;
            FromHCM(h, num, m, out r, out g, out b);
        }

        private static void FromHCM(float h, float c, float m, out float r, out float g, out float b)
        {
            if (h < 0f || h >= 1f)
            {
                h -= MathF.Floor(h);
            }

            float num = h * 6f;
            float num2 = c * (1f - MathF.Abs(MathF.IEEERemainder(num, 2f) - 1f));
            r = 0f;
            g = 0f;
            b = 0f;
            if (num < 2f)
            {
                b = 0f;
                if (num < 1f)
                {
                    g = num2;
                    r = c;
                }
                else
                {
                    g = c;
                    r = num2;
                }
            }
            else if (num < 4f)
            {
                r = 0f;
                if (num < 3f)
                {
                    g = c;
                    b = num2;
                }
                else
                {
                    g = num2;
                    b = c;
                }
            }
            else
            {
                g = 0f;
                if (num < 5f)
                {
                    r = num2;
                    b = c;
                }
                else
                {
                    r = c;
                    b = num2;
                }
            }

            r += m;
            g += m;
            b += m;
        }
    }
}
