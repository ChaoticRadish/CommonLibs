using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaoticKit.Wpf.Interfaces
{
    /// <summary>
    /// 接口: 可以显示一个 <see cref="Decimal"/> 值的东西
    /// </summary>
    public interface IDecimalShower
    {
        /// <summary>
        /// 显示值
        /// </summary>
        decimal? ShowingValue { get; set; }
    }
}
