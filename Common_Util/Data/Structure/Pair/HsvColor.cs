using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Structure.Pair
{

    /// <summary>
    /// 单精度 HSV 色彩模型
    /// </summary>
    public struct HsvaColorF
    {
        /// <summary>
        /// 色相, 值域 [0, 360)
        /// </summary>
        public float H { get; set; }
        /// <summary>
        /// 饱和度, 值域 [0, 1]
        /// </summary>
        public float S { get; set; }
        /// <summary>
        /// 明度, 值域 [0, 1]
        /// </summary>
        public float V { get; set; }
        /// <summary>
        /// 透明度, 值域 [0, 1]
        /// </summary>
        public float A { get; set; }

        public override string ToString()
        {
            return $"H:{H}, S:{S}, V:{V}, A:{A}";
        }
    }
}
