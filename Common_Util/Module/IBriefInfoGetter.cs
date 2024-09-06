using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Module
{
    /// <summary>
    /// 简略信息获取器接口
    /// <para>注: 简略信息一般仅有一行</para>
    /// </summary>
    public interface IBriefInfoGetter
    {
        string GetBriefInfo(object obj);
    }
}
