using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data
{
    /// <summary>
    /// 位操作的帮助类
    /// </summary>
    public static class BitHelper
    {
        /// <summary>
        /// 标记比特位位置的一些 <see cref="byte"/>
        /// </summary>
        /// <remarks>
        /// 例如: 第四个元素 (索引 3 ), 为标记 <see langword="byte"/> 从低位开始数起, 第 4 位 bit 的标记值 <br/>
        /// 可以以类似以下方式使用 
        /// <code>
        /// byte b = ...;
        /// int bitValue = (b &amp; BitLocation[3]) &gt; 0 ? 1 : 0; 
        /// // bitValue 为 b 第 4 位 bit 的值 
        /// </code>
        /// </remarks>
        public readonly static byte[] BitLocation =
        [
            0b_0000_0001,
            0b_0000_0010,
            0b_0000_0100,
            0b_0000_1000,
            0b_0001_0000,
            0b_0010_0000,
            0b_0100_0000,
            0b_1000_0000,
        ];
        /// <summary>
        /// 标记比特位小端范围的一些 <see langword="byte"/>
        /// </summary>
        /// <remarks>
        /// 例如: 第五个元素 (索引 4 ), 为标记 <see langword="byte"/> 从低位开始数起, 从首位到第 4 位 (含) 的标记值 <br/>
        /// 可以按索引值等于想要获取的位数来快速记忆 <br/>
        /// 可以以类似以下方式使用 
        /// <code>
        /// byte b = ...;
        /// byte value = (byte)((b &amp; <see cref="BitLittleRange"/>[4])); 
        /// // value 为 b 从小端开始计数, 前 4 位的值
        /// </code>
        /// </remarks>
        public readonly static byte[] BitLittleRange =
        [
            0b_0000_0000,
            0b_0000_0001,
            0b_0000_0011,
            0b_0000_0111,
            0b_0000_1111,
            0b_0001_1111,
            0b_0011_1111,
            0b_0111_1111,
            0b_1111_1111,
        ];
        /// <summary>
        /// 标记比特位大端范围的一些 <see langword="byte"/>
        /// </summary>
        /// <remarks>
        /// 例如: 第五个元素 (索引 4 ), 为标记 <see langword="byte"/> 从高位开始数起, 从首位到第 4 位 (含) 的标记值 <br/>
        /// 可以按索引值等于想要获取的位数来快速记忆 <br/>
        /// 可以以类似以下方式使用 
        /// <code>
        /// byte b = ...;
        /// byte value = (byte)((b &amp; <see cref="BitBigRange"/>[4])); 
        /// // value 为 b 从大端开始计数, 前 4 位的值 
        /// </code>
        /// </remarks>
        public readonly static byte[] BitBigRange =
        [
            0b_0000_0000,
            0b_1000_0000,
            0b_1100_0000,
            0b_1110_0000,
            0b_1111_0000,
            0b_1111_1000,
            0b_1111_1100,
            0b_1111_1110,
            0b_1111_1111,
        ];
    }
}
