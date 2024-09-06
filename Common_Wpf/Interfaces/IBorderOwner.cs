using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Wpf.Interfaces
{
    /// <summary>
    /// 接口: 拥有边框的东西
    /// </summary>
    public interface IBorderOwner
    {
        /// <summary>
        /// 边框可见
        /// </summary>
        bool BorderVisibility { get; set;}


    }
}
