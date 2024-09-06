using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Module
{
    /// <summary>
    /// 完整信息获取器接口
    /// </summary>
    public interface IFullInfoGetter
    {
        string GetFullInfo(object obj);
    }
}
