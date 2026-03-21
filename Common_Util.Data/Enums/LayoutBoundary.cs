using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Enums
{
    /// <summary>
    /// 布局边界
    /// </summary>
    public enum LayoutBoundary
    {
        /// <summary>
        /// 边界由内容决定 (无界)
        /// </summary>
        Content,
        /// <summary>
        /// 边界由容器决定 (有界)
        /// </summary>
        Container,

        /// <summary>
        /// 视口尺寸。类似于 <see cref="Container"/>，但特指可见区域。
        /// </summary>
        /// <remarks>
        /// 通常用于处理虚拟化滚动列表，表示大小由当前屏幕可见范围决定。
        /// </remarks>
        Viewport,
        /// <summary>
        /// 固定尺寸。无论内容多少或容器多大，均保持设定的固定宽高。
        /// </summary>
        Fixed,
        /// <summary>
        /// 比例填充。无论内容多少，始终填满父容器的剩余空间。
        /// </summary>
        /// <remarks>
        /// (类似于 WPF 中 "1*" 概念)
        /// </remarks>
        Fill,
        /// <summary>
        /// 屏幕尺寸。
        /// </summary>
        /// <remarks>
        /// 忽略父容器限制，强制匹配当前显示器/屏幕的工作区尺寸。
        /// 适用于全屏模式或弹窗蒙层场景。
        /// </remarks>
        Screen,
    }
}
