using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Winform.Win32
{
    /// <summary>
    /// 事件常量 (Winuser.h)
    /// <para>文档: https://learn.microsoft.com/zh-cn/windows/win32/winauto/event-constants</para>
    /// </summary>
    public enum EventEnum
    {
        /// <summary>
        /// 可能的最低事件值。
        /// </summary>
        EVENT_MIN = 0x00000001,
        /// <summary>
        /// 可能的最高事件值。
        /// </summary>
        EVENT_MAX = 0x7FFFFFFF,

        /// <summary>
        /// <para>一个对象被创建</para> 
        /// <para>An object has been created</para> 
        /// </summary>
        EVENT_OBJECT_CREATE = 0x8000,
        /// <summary>
        /// <para>一个对象的 Name 属性被修改</para> 
        /// <para>An object's Name property has changed</para> 
        /// </summary>
        EVENT_OBJECT_NAMECHANGE = 0x800C,
    }
}
