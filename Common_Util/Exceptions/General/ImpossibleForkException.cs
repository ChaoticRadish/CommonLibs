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
        private const string fixedString = "代码走到了一个理论上不可能发生的分支! ";

        public ImpossibleForkException() : base(fixedString) { }

        /// <summary>
        /// 实例化时, 附带额外的文本信息, 一般用来描述为什么是不可能发生的分支
        /// </summary>
        /// <param name="message"></param>
        public ImpossibleForkException(string message) : base($"{fixedString}{message}") { }
    }
}
