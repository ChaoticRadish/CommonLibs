using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Interfaces.Behavior
{
    /// <summary>
    /// 具有加卸载行为的某个东西
    /// </summary>
    public interface ILoadable
    {
        /// <summary>
        /// 加载
        /// </summary>
        void Load();
        /// <summary>
        /// 卸载
        /// </summary>
        void Unload();
    }
}
