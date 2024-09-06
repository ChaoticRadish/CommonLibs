using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Winform.Interfaces
{
    /// <summary>
    /// 控件列表接口
    /// </summary>
    public interface IControlList
    {
        /// <summary>
        /// 清空
        /// </summary>
        void Clear();
        /// <summary>
        /// 更新列表项的尺寸及位置
        /// </summary>
        void UpdateItemBound();

        Point InnerAreaOffset { get; }
    }
}
