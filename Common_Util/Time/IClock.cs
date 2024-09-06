using System;
using System.Collections.Generic;
using System.Text;

namespace Common_Util.Time
{
    public interface IClock
    {
        /// <summary>
        /// 已流逝时间 (单位: 秒)
        /// </summary>
        public double ElapseTime { get; }

    }
}
