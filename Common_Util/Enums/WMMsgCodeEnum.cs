using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Enums
{
    /// <summary>
    /// WM消息代码的枚举
    /// </summary>
    public enum WMMsgCodeEnum : int
    {
        WM_PAINT = 0xF,
        WM_NCPAINT = 0x85,

        WM_CTLCOLOREDIT = 0x133,

        WM_SETFOCUS = 0x7,
        WM_KILLFOCUS = 0x8,

        WM_SETFONT = 0x30,

        WM_MOUSEMOVE = 0x200,
        WM_LBUTTONDOWN = 0x201,
        WM_RBUTTONDOWN = 0x204,
        WM_MBUTTONDOWN = 0x207,
        WM_LBUTTONUP = 0x202,
        WM_RBUTTONUP = 0x205,
        WM_MBUTTONUP = 0x208,
        WM_LBUTTONDBLCLK = 0x203,
        WM_RBUTTONDBLCLK = 0x206,
        WM_MBUTTONDBLCLK = 0x209,

        WM_KEYDOWN = 0x0100,
        WM_KEYUP = 0x0101,
        WM_CHAR = 0x0102,
    }
}
