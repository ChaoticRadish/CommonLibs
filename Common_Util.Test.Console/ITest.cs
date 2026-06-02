using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Test.Console
{
    public interface ITest
    {
        /// <summary>
        /// 准备运行环境
        /// </summary>
        void Setup();
        /// <summary>
        /// 运行测试
        /// </summary>
        void Run();
        /// <summary>
        /// 运行结束后, 输出测试总结等
        /// </summary>
        void Finish();
    }
}
