using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Enums
{
    /// <summary>
    /// 极性
    /// </summary>
    /// <remarks>
    /// 通用的枚举类型, 区分三种情况: 1. 积极; 2. 消极; 3. 不明确
    /// </remarks>
    public enum Polarity : sbyte
    {
        /// <summary>
        /// 积极
        /// </summary>
        /// <remarks>
        /// 积极的行为或结果, 比如: 是/确定/完成...
        /// </remarks>
        Positive = 1,
        /// <summary>
        /// 消极
        /// </summary>
        /// <remarks>
        /// 消极的行为或结果, 比如: 否/拒绝/取消... 
        /// </remarks>
        Negative = -1,
        /// <summary>
        /// 不明确, 混沌的
        /// </summary>
        /// <remarks>
        /// 不明确的行为或结果, 比如没有做出选择, 而是直接关闭了窗口, 或者 ALT+F4 之类的强制结束的操作等
        /// </remarks>
        Chaos = 0,
    }

    public static class ResolutionExtensions
    {
        /// <summary>
        /// 转换为布尔值
        /// </summary>
        /// <param name="resolution"></param>
        /// <returns></returns>
        public static bool? ToBool(this Polarity resolution)
        {
            return resolution switch
            {
                Polarity.Positive => true,
                Polarity.Negative => false,
                Polarity.Chaos => null,
                _ => null,
            };
        }
        public static bool? ToBool(this Polarity? resolution)
        {
            return resolution switch
            {
                Polarity.Positive => true,
                Polarity.Negative => false,
                Polarity.Chaos => null,
                _ => null,
            };
        }
    }
}
