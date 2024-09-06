using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Winform.Enums
{
    /// <summary>
    /// 简易状态
    /// </summary>
    public enum SimpleStateEnum : int
    {
        /// <summary>
        /// 选中
        /// </summary>
        Checked,
        /// <summary>
        /// 未选中
        /// </summary>
        UnChecked,
        /// <summary>
        /// 只读
        /// </summary>
        ReadOnly,
        /// <summary>
        /// 停用
        /// </summary>
        Disable,
    }
}
