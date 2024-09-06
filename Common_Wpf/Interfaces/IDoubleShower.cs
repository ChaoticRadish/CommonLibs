using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Wpf.Interfaces
{
    /// <summary>
    /// 接口: 可以显示一个Double值的东西
    /// </summary>
    public interface IDoubleShower
    {
        /// <summary>
        /// 显示值
        /// </summary>
        double? ShowingValue { get; set; }
    }
}
