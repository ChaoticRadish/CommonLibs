using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Attributes.General
{
    /// <summary>
    /// 标记是否初始化方法
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class InitMethodAttribute : Attribute
    {
        public bool Ignore { get; set; } = false;
    }
}
