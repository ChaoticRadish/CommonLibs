using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Wpf.Interfaces
{
    /// <summary>
    /// 接口: 可以显示一个Float值的东西
    /// </summary>
    public interface IFloatShower
    {
        /// <summary>
        /// 显示值
        /// </summary>
        float? ShowingValue { get; set; }
    }
}
