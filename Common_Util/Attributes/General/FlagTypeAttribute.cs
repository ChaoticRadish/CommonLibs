using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Attributes.General
{
    /// <summary>
    /// 用来标志类型的Attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class FlagTypeAttribute : System.Attribute
    {
        /// <summary>
        /// 标志的指定类型
        /// </summary>
        public Type TargetType { get; set; }
    }
}
