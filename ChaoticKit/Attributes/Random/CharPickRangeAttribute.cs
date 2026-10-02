using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaoticKit.Attributes.Random
{
    /// <summary>
    /// 字符选取区间
    /// </summary>
    public class CharPickRangeAttribute(params char[] chars) : Attribute
    {
        public char[] Chars { get; } = chars;
    }
}
