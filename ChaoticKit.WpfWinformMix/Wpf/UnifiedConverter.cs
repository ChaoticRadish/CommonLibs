using ChaoticKit.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaoticKit.WpfWinformMix.Wpf
{
    public static class UnifiedConverter
    {
        /// <summary>
        /// 将 WPF 对话框结果转换为通用的模态结果枚举
        /// </summary>
        /// <param name="dialogResult"></param>
        /// <returns></returns>
        public static ModalResult ToModalResult(bool? dialogResult)
        {
            return dialogResult switch
            {
                true => ModalResult.Ok,
                false => ModalResult.Cancel,
                null => ModalResult.Chaos,
            };
        }
    }
}
