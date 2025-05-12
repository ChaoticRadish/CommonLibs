using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Extensions
{
    public static class ValueTypeExtensions
    {
        /// <summary>
        /// 限制并返回输入值到区间 [left, right] 间
        /// </summary>
        /// <param name="value"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static int Range(this int value, int left, int right = int.MaxValue)
        {
            if (value < left) return left;
            else if (value > right) return right;
            else return value;
        }

        /// <summary>
        /// 将double值转换为普通的字符串 (会避免转换成科学计数法)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToNormalString(this double value)
        {
            return value.ToString("0.###############"); // double值只能保存15位数(整数+小数总共15位), 所以15个'#'就够了
        }

        /// <summary>
        /// 根据bool值返回字符串
        /// </summary>
        /// <param name="b"></param>
        /// <param name="trueStr"></param>
        /// <param name="falseStr"></param>
        /// <returns></returns>
        public static string? ToString(this bool b, string? trueStr, string? falseStr = null)
        {
            return b ? trueStr : falseStr;
        }

        #region 遍历
        /// <summary>
        /// 将 <paramref name="total"/> 分为最大长度不超过 <paramref name="segmentMaxLength"/> 的 n 个片段
        /// </summary>
        /// <param name="total">被分段的总量, 如果小于或等于 0, 则会返回空的遍历器</param>
        /// <param name="segmentMaxLength">必须大于 0 </param>
        /// <returns>返回遍历这些片段起始索引和长度的遍历器</returns>
        public static IEnumerable<(long start, long length)> Segment(this long total, long segmentMaxLength)
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(segmentMaxLength, 0);
            if (total <= 0) yield break;

            long count = total / segmentMaxLength;
            long over = total % segmentMaxLength;
            if (over > 0) count += 1;
            for (long i = 0; i < count; i++)
            {
                long start = i * segmentMaxLength;
                long length = (i < count - 1) ? segmentMaxLength : (total - start);
                yield return (start, length);
            }

        }
        #endregion
    }
}
