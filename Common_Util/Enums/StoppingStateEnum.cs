using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Enums
{
    public enum StoppingStateEnum : byte
    {
        Waiting = 0,
        Stopping = 1,
        Success = 2,
        Failure = 4,
    }
}
