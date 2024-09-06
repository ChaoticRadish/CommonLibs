using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Maths
{
    /// <summary>
    /// 数值比较帮助类
    /// </summary>
    public static class CompareHelper
    {
        /// <summary>
        /// 验证数值大小, 如果与形参不对应, 将交换位置到正确的形参上
        /// </summary>
        /// <param name="smaller">执行完成后, 此引用将是较小值</param>
        /// <param name="bigger">执行完成后, 此引用将是较大值</param>
        public static void JudgeBigger(ref int smaller, ref int bigger)
        {
            if (smaller == bigger) return;
            (smaller, bigger) = (Math.Min(smaller, bigger), Math.Max(smaller, bigger));
        }
        /// <summary>
        /// 验证数值大小, 如果与形参不对应, 将交换位置到正确的形参上
        /// </summary>
        /// <param name="smaller">执行完成后, 此引用将是较小值</param>
        /// <param name="bigger">执行完成后, 此引用将是较大值</param>
        public static void JudgeBigger(ref uint smaller, ref uint bigger)
        {
            if (smaller == bigger) return;
            (smaller, bigger) = (Math.Min(smaller, bigger), Math.Max(smaller, bigger));
        }
        /// <summary>
        /// 验证数值大小, 如果与形参不对应, 将交换位置到正确的形参上
        /// </summary>
        /// <param name="smaller">执行完成后, 此引用将是较小值</param>
        /// <param name="bigger">执行完成后, 此引用将是较大值</param>
        public static void JudgeBigger(ref byte smaller, ref byte bigger)
        {
            if (smaller == bigger) return;
            (smaller, bigger) = (Math.Min(smaller, bigger), Math.Max(smaller, bigger));
        }
        /// <summary>
        /// 验证数值大小, 如果与形参不对应, 将交换位置到正确的形参上
        /// </summary>
        /// <param name="smaller">执行完成后, 此引用将是较小值</param>
        /// <param name="bigger">执行完成后, 此引用将是较大值</param>
        public static void JudgeBigger(ref sbyte smaller, ref sbyte bigger)
        {
            if (smaller == bigger) return;
            (smaller, bigger) = (Math.Min(smaller, bigger), Math.Max(smaller, bigger));
        }
        /// <summary>
        /// 验证数值大小, 如果与形参不对应, 将交换位置到正确的形参上
        /// </summary>
        /// <param name="smaller">执行完成后, 此引用将是较小值</param>
        /// <param name="bigger">执行完成后, 此引用将是较大值</param>
        public static void JudgeBigger(ref short smaller, ref short bigger)
        {
            if (smaller == bigger) return;
            (smaller, bigger) = (Math.Min(smaller, bigger), Math.Max(smaller, bigger));
        }
        /// <summary>
        /// 验证数值大小, 如果与形参不对应, 将交换位置到正确的形参上
        /// </summary>
        /// <param name="smaller">执行完成后, 此引用将是较小值</param>
        /// <param name="bigger">执行完成后, 此引用将是较大值</param>
        public static void JudgeBigger(ref ushort smaller, ref ushort bigger)
        {
            if (smaller == bigger) return;
            (smaller, bigger) = (Math.Min(smaller, bigger), Math.Max(smaller, bigger));
        }
        /// <summary>
        /// 验证数值大小, 如果与形参不对应, 将交换位置到正确的形参上
        /// </summary>
        /// <param name="smaller">执行完成后, 此引用将是较小值</param>
        /// <param name="bigger">执行完成后, 此引用将是较大值</param>
        public static void JudgeBigger(ref long smaller, ref long bigger)
        {
            if (smaller == bigger) return;
            (smaller, bigger) = (Math.Min(smaller, bigger), Math.Max(smaller, bigger));
        }
        /// <summary>
        /// 验证数值大小, 如果与形参不对应, 将交换位置到正确的形参上
        /// </summary>
        /// <param name="smaller">执行完成后, 此引用将是较小值</param>
        /// <param name="bigger">执行完成后, 此引用将是较大值</param>
        public static void JudgeBigger(ref ulong smaller, ref ulong bigger)
        {
            if (smaller == bigger) return;
            (smaller, bigger) = (Math.Min(smaller, bigger), Math.Max(smaller, bigger));
        }
        /// <summary>
        /// 验证数值大小, 如果与形参不对应, 将交换位置到正确的形参上
        /// </summary>
        /// <param name="smaller">执行完成后, 此引用将是较小值</param>
        /// <param name="bigger">执行完成后, 此引用将是较大值</param>
        public static void JudgeBigger(ref float smaller, ref float bigger)
        {
            if (smaller == bigger) return;
            (smaller, bigger) = (Math.Min(smaller, bigger), Math.Max(smaller, bigger));
        }
        /// <summary>
        /// 验证数值大小, 如果与形参不对应, 将交换位置到正确的形参上
        /// </summary>
        /// <param name="smaller">执行完成后, 此引用将是较小值</param>
        /// <param name="bigger">执行完成后, 此引用将是较大值</param>
        public static void JudgeBigger(ref double smaller, ref double bigger)
        {
            if (smaller == bigger) return;
            (smaller, bigger) = (Math.Min(smaller, bigger), Math.Max(smaller, bigger));
        }
        /// <summary>
        /// 验证数值大小, 如果与形参不对应, 将交换位置到正确的形参上
        /// </summary>
        /// <param name="smaller">执行完成后, 此引用将是较小值</param>
        /// <param name="bigger">执行完成后, 此引用将是较大值</param>
        public static void JudgeBigger(ref decimal smaller, ref decimal bigger)
        {
            if (smaller == bigger) return;
            (smaller, bigger) = (Math.Min(smaller, bigger), Math.Max(smaller, bigger));
        }
    }
}
