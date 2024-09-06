using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Interfaces.Owner
{
    /// <summary>
    /// 拥有标签的某个东西
    /// <para>标签可以是任意值, 包含 null 值</para>
    /// </summary>
    public interface ITagsOwner
    {
        /// <summary>
        /// 拥有的标签
        /// </summary>
        public IEnumerable<string?> Tags { get; }
    }
}
