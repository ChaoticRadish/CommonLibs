using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Attributes.Random
{
    /// <summary>
    /// 生成随机对象时, 忽略本属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoreRandomAttribute : Attribute
    {
    }
}
