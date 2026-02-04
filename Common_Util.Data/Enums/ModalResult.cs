using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Enums
{
    /// <summary>
    /// 模态结果类型枚举值
    /// </summary>
    public enum ModalResult
    {
        Custom,
        Ok,
        Cancel,
        Yes,
        No,
        /// <summary>
        /// 中止
        /// </summary>
        Abort,
        Retry,
        Ignore,
        Continue,
        Chaos,
    }

    public static class ModalResultExtensions
    {
        /// <summary>
        /// 取得模态结果枚举值对应的默认中文名
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static string DefaultChineseName(this ModalResult result)
        {
            return result switch
            {
                ModalResult.Ok => "确定",
                ModalResult.Cancel => "取消",
                ModalResult.Yes => "是",
                ModalResult.No => "否",
                ModalResult.Chaos => "不明确",
                ModalResult.Abort => "中止",
                ModalResult.Retry => "重试",
                ModalResult.Ignore => "忽略",
                ModalResult.Continue => "继续",
                ModalResult.Custom => string.Empty,
                _ => string.Empty,
            };
        }
        /// <summary>
        /// 取得模态结果枚举值对应的默认极性
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static Polarity DefaultPolarity(this ModalResult result)
        {
            return result switch
            {
                ModalResult.Ok => Polarity.Positive,
                ModalResult.Yes => Polarity.Positive,
                ModalResult.Retry => Polarity.Positive,
                ModalResult.Continue => Polarity.Positive,

                ModalResult.No => Polarity.Negative,
                ModalResult.Cancel => Polarity.Negative,
                ModalResult.Abort => Polarity.Negative,
                ModalResult.Ignore => Polarity.Negative,

                ModalResult.Chaos => Polarity.Chaos,
                ModalResult.Custom => Polarity.Chaos,

                _ => Polarity.Chaos,
            };
        }
    }
}
