using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Module.Config
{
    public static class IConfigManagerExtensions
    {
        /// <summary>
        /// 获取配置读写实现
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IConfigReadWriteImpl? GetImpl(this IConfigManager manager, string name)
        {
            ArgumentNullException.ThrowIfNull(manager);
            if (manager.TryGetImpl(name, out var config))
            {
                return config;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 设置配置读写实现
        /// </summary>
        /// <remarks>
        /// 如果指定名字的实现不存在, 则添加, 反之则覆盖
        /// </remarks>
        /// <param name="manager"></param>
        /// <param name="name"></param>
        /// <param name="impl"></param>
        /// <returns>添加或更新操作是否成功</returns>
        public static bool SetImpl(this IConfigManager manager, string name, IConfigReadWriteImpl impl)
        {
            ArgumentNullException.ThrowIfNull(manager);
            if (manager.HasImpl(name))
            {
                return manager.TryUpdateImpl(name, impl);
            }
            else
            {
                return manager.TryAddImpl(name, impl);
            }
        }
    }
}
