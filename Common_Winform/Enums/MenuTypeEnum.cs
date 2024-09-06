using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Winform.Enums
{
    public enum MenuTypeEnum
    {
        /// <summary>
        /// 顶部菜单, 一般不带图标, 放在窗口顶部的菜单中, 通常包含子菜单
        /// </summary>
        Top,

        /// <summary>
        /// 头部菜单, 一般较大个, 划分不同大模块, 一般不含子菜单
        /// </summary>
        Head,

        /// <summary>
        /// 右键菜单
        /// </summary>
        RightButton,
    }
}
