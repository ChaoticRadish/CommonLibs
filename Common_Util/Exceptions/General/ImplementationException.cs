using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Exceptions.General
{
    /// <summary>
    /// 代码实现存在问题的异常
    /// </summary>
    public class ImplementationException : Exception
    {
        public ImplementationException() : base() { }
        public ImplementationException(string message) : base(message) { }
    }
}
