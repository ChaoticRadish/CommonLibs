using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Common_Util.GDI
{
    /// <summary>
    /// 颜色值转换
    /// </summary>
    public static class ColorConvert
    {
        static ColorConvert()
        {
            LoadColors();
        }

        private static void LoadColors()
        {
            Type type = typeof(Color);
            PropertyInfo[] propertyInfos = type.GetProperties(BindingFlags.Public | BindingFlags.Static);
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                if (propertyInfo.PropertyType == type)
                {
                    ColorDic.Add(propertyInfo.Name, (Color)propertyInfo.GetValue(null)!);
                }
            }
        }


        #region 颜色表
        private static Dictionary<string, Color> ColorDic = new();
        #endregion


        /// <summary>
        /// 将字符串转换为颜色值
        /// <para>转换顺序(优先级): </para>
        /// <para>1. 以#号开头的字符串, 根据长度判断是 #RRGGBB 格式还是 #AARRGGBB 转换</para>
        /// <para>2. 从静态的颜色字典中获取颜色字符串对应的颜色</para>
        /// </summary>
        /// <param name="colorStr"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static Color FromString(string colorStr, Color? defaultValue = null)
        {
            if (defaultValue == null) { defaultValue = Color.White; }
            if (string.IsNullOrWhiteSpace(colorStr)) { return defaultValue.Value; }
            colorStr = colorStr.Trim();

            if (colorStr.StartsWith('#'))
            {
                try
                {
                    if (colorStr.Length == 1 + 6)   // rgb颜色
                    {
                        byte r = Convert.ToByte(colorStr.Substring(1, 2), 16);
                        byte g = Convert.ToByte(colorStr.Substring(3, 2), 16);
                        byte b = Convert.ToByte(colorStr.Substring(5, 2), 16);
                        return Color.FromArgb(r, g, b);
                    }
                    else if (colorStr.Length == 1 + 8)  // argb颜色
                    {
                        byte a = Convert.ToByte(colorStr.Substring(1, 2), 16);
                        byte r = Convert.ToByte(colorStr.Substring(1, 2), 16);
                        byte g = Convert.ToByte(colorStr.Substring(3, 2), 16);
                        byte b = Convert.ToByte(colorStr.Substring(5, 2), 16);
                        return Color.FromArgb(a, r, g, b);
                    }
                }
                catch
                {
                    return defaultValue.Value;
                }
            }
            else
            {
                if (ColorDic.ContainsKey(colorStr))
                {
                    return ColorDic[colorStr];
                }
            }

            return defaultValue.Value;
        }
    }
}
