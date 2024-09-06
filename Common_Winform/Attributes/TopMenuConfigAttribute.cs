using Common_Util.Attributes.General;
using Common_Util.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Winform.Attributes
{
    /// <summary>
    /// 菜单项配置
    /// </summary>
    public class TopMenuConfigAttribute : LayerComponentBaseLongRuleAttribute
    {
        public TopMenuConfigAttribute(params string[] rules) : base(rules) { }
    }
}
