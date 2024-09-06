using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Exceptions.General
{
    /// <summary>
    /// 不可能分支异常
    /// </summary>
    /// <remarks>
    /// 代码走到了一个理论上不可能发生的分支
    /// </remarks>
    public sealed class ImpossibleForkException : Exception
    {
        public ImpossibleForkException() : base("代码走到了一个理论上不可能发生的分支! ") { }
    }
}
