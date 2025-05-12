using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Interfaces.UI
{
    /// <summary>
    /// 进度显示器
    /// </summary>
    public interface IProcessShower
    {
        /// <summary>
        /// 当前进度, 值域 [0, 1], 1 表示已完成, 进度 100%
        /// </summary>
        double Process { get; set; }
    }

    public static class ProcessShowerHelper
    {
        public static readonly IProcessShower Empty = new ProcessShower01();


        private struct ProcessShower01 : IProcessShower
        {
            public double Process { get; set; }
        }
    }
}
