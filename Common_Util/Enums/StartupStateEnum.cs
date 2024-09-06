using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Enums
{
    /// <summary>
    /// 启动状态枚举
    /// </summary>
    public enum StartupStateEnum : byte
    {
        Waiting = 0,
        Starting = 1,
        Success = 2,
        Failure = 4,
    }
}
