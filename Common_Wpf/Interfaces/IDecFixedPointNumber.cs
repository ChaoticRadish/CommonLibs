using Common_Util.Data.Structure.Value;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Wpf.Interfaces
{
    public interface IDecFixedPointNumber
    {
        /// <summary>
        /// 显示值
        /// </summary>
        DecFixedPointNumber? ShowingValue { get; set; }
    }
}
