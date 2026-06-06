using ChaoticKit.Attributes.General;
using ChaoticKit.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaoticKit.Winform.Attributes
{
    /// <summary>
    /// 菜单项配置
    /// </summary>
    public class TopMenuConfigAttribute : LayerComponentBaseLongRuleAttribute
    {
        public TopMenuConfigAttribute(params string[] rules) : base(rules) { }
    }
}
