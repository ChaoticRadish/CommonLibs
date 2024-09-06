using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Enums
{
    /// <summary>
    /// 常用的四向枚举
    /// </summary>
    public enum FourWayEnum : byte
    {
        [Common_Util.Attributes.General.EnumDesc("上")]
        Up = 0,
        [Common_Util.Attributes.General.EnumDesc("下")]
        Down = 1,
        [Common_Util.Attributes.General.EnumDesc("左")]
        Left = 2,
        [Common_Util.Attributes.General.EnumDesc("右")]
        Right = 3,
    }
}
