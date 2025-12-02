using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Structure.Pair
{
    /// <summary>
    /// 单字节 RGB 色彩模型
    /// </summary>
    public struct RgbaColorB
    {
        /// <summary>
        /// 红色值, 值域 [0, 255)
        /// </summary>
        public byte R { get; set; }
        /// <summary>
        /// 绿色值, 值域 [0, 255)
        /// </summary>
        public byte G { get; set; }
        /// <summary>
        /// 蓝色值, 值域  [0, 255)
        /// </summary>
        public byte B { get; set; }
        /// <summary>
        /// 透明度, 值域 [0, 255)
        /// </summary>
        public byte A { get; set; }

        #region 隐式转换
        public static implicit operator System.Drawing.Color(RgbaColorB colorB)
        {
            return System.Drawing.Color.FromArgb(colorB.A, colorB.R, colorB.G, colorB.B);
        }
        public static implicit operator RgbaColorB(System.Drawing.Color color)
        {
            return new RgbaColorB()
            {
                R = color.R,
                G = color.G,
                B = color.B,
                A = color.A
            };
        }
        #endregion
    }

    /// <summary>
    /// 单精度 RGB 色彩模型
    /// </summary>
    public struct RgbaColorF
    {
        /// <summary>
        /// 红色值, 值域 [0, 1]
        /// </summary>
        public float R { get; set; }
        /// <summary>
        /// 绿色值, 值域 [0, 1]
        /// </summary>
        public float G { get; set; }
        /// <summary>
        /// 蓝色值, 值域 [0, 1]
        /// </summary>
        public float B { get; set; }
        /// <summary>
        /// 蓝色值, 值域 [0, 1]
        /// </summary>
        public float A { get; set; }
    }
}
