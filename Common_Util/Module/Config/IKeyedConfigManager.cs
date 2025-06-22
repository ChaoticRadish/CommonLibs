using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Module.Config
{
    /// <summary>
    /// 接口: 键值式的配置管理器
    /// </summary>
    /// <typeparam name="TKey">键的类型</typeparam>
    public interface IKeyedConfigManager<TKey> : IConfigManager<IKeyedConfigReadWriteImpl<TKey>>
    {
        #region 加载与保存
        /// <summary>
        /// 尝试加载指定键值的配置对象
        /// </summary>
        /// <param name="key"></param>
        /// <param name="implName">配置读写实现的名字, 如果为 <see langword="null"/>, 则采用 <see cref="IConfigManager.DefaultImpl"/></param>
        /// <returns></returns>
        bool TryLoad(TKey key, string? implName, [NotNullWhen(true)] out object? config) 
        {
            var impl = GetImpl(implName);
            if (impl == null)
            {
                config = null;
                return false;
            }
            else
            {
                return impl.TryLoadConfig(key, out config);
            }
        }
        /// <summary>
        /// 加载指定键值的配置对象
        /// </summary>
        /// <param name="key"></param>
        /// <param name="implName">配置读写实现的名字, 如果为 <see langword="null"/>, 则采用 <see cref="IConfigManager.DefaultImpl"/></param>
        /// <returns></returns>
        object? Load(TKey key, string? implName = null)
        {
            return TryLoad(key, implName, out var config) ? config : null;
        }
        /// <summary>
        /// 使用指定名字的配置读写, 将当前缓存中指定键值的配置对象保存起来
        /// </summary>
        /// <param name="key"></param>
        /// <param name="implName">配置读写实现的名字, 如果为 <see langword="null"/>, 则采用 <see cref="IConfigManager.DefaultImpl"/></param>
        /// <returns></returns>
        bool Save(TKey key, object config, string? implName = null)
        {
            var impl = GetImpl(implName);
            if (impl == null)
            {
                return false;
            }
            else return impl.SaveConfig(key, config);
        }

        #endregion

    }

}
