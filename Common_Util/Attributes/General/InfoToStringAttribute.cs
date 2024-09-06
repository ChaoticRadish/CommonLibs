using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Attributes.General
{
    /// <summary>
    /// 标记一个东西如果要获取信息, 尽量使用 <see cref="object.ToString"/> 来获取信息
    /// </summary>
    public class InfoToStringAttribute : System.Attribute
    {
    }
}
