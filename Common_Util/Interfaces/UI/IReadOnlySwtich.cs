using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Interfaces.UI
{
    /// <summary>
    /// 可切换只读状态
    /// </summary>
    public interface IReadOnlySwtich
    {
        bool ReadOnly { get; set; }
    }
}
