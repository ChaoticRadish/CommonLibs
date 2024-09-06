using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Enums
{
    /// <summary>
    /// 追加新项到已有集合时, 出现冲突时的处理方式
    /// </summary>
    public enum AppendConflictDealMode : int
    {
        /// <summary>
        /// 使用追加项去覆盖已有的
        /// </summary>
        Override = 1,
        /// <summary>
        /// 忽略追加项
        /// </summary>
        Ignore = 2,
        /// <summary>
        /// 弹出异常
        /// </summary>
        Exception = 3,

    }
}
