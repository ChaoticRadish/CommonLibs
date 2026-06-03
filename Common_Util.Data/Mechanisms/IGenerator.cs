using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Mechanisms
{
    /// <summary>
    /// 对象生成器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IGenerator<TArgs, T>
    {
        /// <summary>
        /// 生成对象
        /// </summary>
        /// <returns></returns>
        /// <param name="args">生成参数</param>
        T Generating(TArgs args);
    }
}
