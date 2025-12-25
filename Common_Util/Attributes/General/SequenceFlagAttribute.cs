using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Attributes.General
{
    /// <summary>
    /// 顺序标记
    /// </summary>
    /// <param name="sequence"></param>
    [AttributeUsage(AttributeTargets.All)]
    public class SequenceFlagAttribute : Attribute
    {
        public SequenceFlagAttribute() 
        {
            Sequence = null;
        }
        public SequenceFlagAttribute(int sequence) 
        {
            Sequence = sequence;
        }

        /// <summary>
        /// 顺序值
        /// </summary>
        public int? Sequence { get; } 


    }
}
