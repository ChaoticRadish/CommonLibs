using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Exceptions.General
{
    /// <summary>
    /// 专门用来表示某东西未初始化的异常
    /// </summary>
    public class NotInitializedException : Exception
    {
        public NotInitializedException() : base() { }
        public NotInitializedException(string msg) : base(msg) { }

        public static NotImplementedException MemberNotInit([CallerMemberName] string caller = "")
        {
            return new NotImplementedException(caller.IsEmpty() ? "当前成员未未初始化" : $"{caller} 未初始化");
        }
    }
}
