using Common_Util.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_WpfWinformMix.Winform
{
    public static class UnifiedConverter
    {
        /// <summary>
        /// 将 Winform 对话框结果转换为通用的模态结果枚举
        /// </summary>
        /// <param name="dialogResult"></param>
        /// <returns></returns>
        public static ModalResult ToModalResult(DialogResult dialogResult)
        {
            return dialogResult switch
            {
                DialogResult.None => ModalResult.Chaos,
                DialogResult.OK => ModalResult.Ok,
                DialogResult.Cancel => ModalResult.Cancel,
                DialogResult.Abort => ModalResult.Abort,
                DialogResult.Retry => ModalResult.Retry,
                DialogResult.Ignore => ModalResult.Ignore,
                DialogResult.Yes => ModalResult.Yes,
                DialogResult.No => ModalResult.No,
                DialogResult.TryAgain => ModalResult.Retry,
                DialogResult.Continue => ModalResult.Continue,
                _ => ModalResult.Chaos
            };
        }

        /// <summary>
        /// 将通用的模态结果枚举转换为 Winform 对话框结果
        /// </summary>
        /// <param name="modalResult"></param>
        /// <returns></returns>
        public static DialogResult ToDialogResult(ModalResult modalResult)
        {
            return modalResult switch
            {
                ModalResult.Ok => DialogResult.OK,
                ModalResult.Cancel => DialogResult.Cancel,
                ModalResult.Yes => DialogResult.Yes,
                ModalResult.No => DialogResult.No,
                ModalResult.Abort => DialogResult.Abort,
                ModalResult.Retry => DialogResult.Retry,
                ModalResult.Ignore => DialogResult.Ignore,
                ModalResult.Continue => DialogResult.Continue,

                ModalResult.Custom => DialogResult.None,
                ModalResult.Chaos => DialogResult.None,
                _ => DialogResult.None,
            };
        }
    }
}
