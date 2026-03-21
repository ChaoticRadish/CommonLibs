using Common_Util.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Struct.Modal
{
    public readonly struct ModalResultButtonOption : ITagModalResult
    {
        public readonly static ModalResultButtonOption[] OkCancelOptions
            = [
                new() 
                {
                    Result = ModalResult.Ok,
                    IsDefault = true,
                    DialogResult = Polarity.Positive,
                },
                new()
                {
                    Result = ModalResult.Cancel,
                    IsCancel = true,
                    DialogResult = Polarity.Negative,
                }];

        /// <summary>
        /// 模态结果的类型
        /// </summary>
        public ModalResult Result { get; init; }
        /// <summary>
        /// 显示文本
        /// </summary>
        public string ShowText { get; init; }
        /// <summary>
        /// 关联标签值
        /// </summary>
        public object? Tag { get; init; }
        /// <summary>
        /// 是否默认按钮
        /// </summary>
        public bool IsDefault { get; init; }
        /// <summary>
        /// 是否取消按钮
        /// </summary>
        public bool IsCancel { get; init; }
        /// <summary>
        /// 对话结果
        /// </summary>
        /// <remarks>
        /// 映射到对话框的结果
        /// </remarks>
        public Polarity DialogResult { get; init; }

        public override string ToString()
        {
            return $"[{Result}]{ShowText}{(Tag == null ? "" : $" <{Tag}> ")}";
        }
    }

}
