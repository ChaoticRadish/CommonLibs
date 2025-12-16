using Common_Util.Data.Converter.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common_Util.Data.Structure.Pair
{
    /// <summary>
    /// 单字节 RGB 色彩模型
    /// </summary>
    [JsonConverter(typeof(RgbaColorBJsonConverter))]
    public struct RgbaColorB
    {
        private uint Data;

        /// <summary>
        /// 红色值, 值域 [0, 255]
        /// </summary>
        public byte R 
        {
            readonly get => (byte)((Data >> 24) & 0xFF); 
            set => Data = (uint)(value << 24) | (Data & 0x00FFFFFF);
        }
        /// <summary>
        /// 绿色值, 值域 [0, 255]
        /// </summary>
        public byte G
        {
            readonly get => (byte)((Data >> 16) & 0xFF);
            set => Data = (uint)(value << 16) | (Data & 0xFF00FFFF);
        }
        /// <summary>
        /// 蓝色值, 值域  [0, 255]
        /// </summary>
        public byte B
        {
            readonly get => (byte)((Data >> 8) & 0xFF);
            set => Data = (uint)(value << 8) | (Data & 0xFFFF00FF);
        }
        /// <summary>
        /// 透明度, 值域 [0, 255]
        /// </summary>
        public byte A
        {
            readonly get => (byte)((Data >> 0) & 0xFF);
            set => Data = (uint)(value << 0) | (Data & 0xFFFFFF00);
        }

        #region 隐式转换
        public static implicit operator RgbaColorB(uint color)
        {
            return new RgbaColorB()
            {
                Data = color
            };
        }
        public static implicit operator uint(RgbaColorB color)
        {
            return color.Data;
        }

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

        public override string ToString()
        {
            return $"R:{R}, G:{G}, B:{B}, A:{A}";
        }
    }
    public class RgbaColorBJsonConverter : StructToUintJsonConverter<RgbaColorB>
    {
        public override uint Convert(RgbaColorB obj) => obj;

        public override RgbaColorB Convert(uint value) => value;
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

        public override string ToString()
        {
            return $"R:{R}, G:{G}, B:{B}, A:{A}";
        }
    }
}
