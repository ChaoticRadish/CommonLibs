using Common_Util.Attributes.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Enums
{
    /// <summary>
    /// 开关状态枚举 
    /// <para>3状态版本, 变化过程不区分方向, 例:</para>
    /// <para> 开 => 变化中 => 关; 关 => 变化中 => 开;</para>
    /// <para>两个过程中的 "变化中" 是相同的</para>
    /// </summary>
    public enum SwitchState01Enum : sbyte
    {
        [EnumDesc("打开")]
        Open = 1,
        [EnumDesc("变化中")]
        Changing = 0,
        [EnumDesc("关闭")]
        Close = -1,
    }
}
