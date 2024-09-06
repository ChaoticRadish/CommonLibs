using Common_Util.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Attributes.General
{
    /// <summary>
    /// 层级规则
    /// </summary>
    public class LayerComponentBaseLongRuleAttribute : Attribute
    {
        public LayerComponentBaseLongRuleAttribute(params string[] rules)
        {
            Rules = rules;
        }

        public string[] Rules { get; }

        public LayerComponentBaseLong.Rule Rule { get => new LayerComponentBaseLong.Rule(Rules); }
    }
}
