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
                _ => string.Empty,
            };
        }
    }
}
