using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Module.Config
{
    /// <summary>
    /// 配置读写类的实现
    /// </summary>
    public interface IConfigReadWriteImpl
    {
    }
    /// <summary>
    /// 基于键值加卸载对应配置的配置读写类的实现
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IKeyedConfigReadWriteImpl<TKey> : IConfigReadWriteImpl
    {
        /// <summary>
        /// 获取输入键值的配置
        /// </summary>
        /// <returns></returns>
        bool TryLoadConfig(TKey key, [NotNullWhen(true)] out object? config);
        /// <summary>
        /// 保存输入类型的配置
        /// </summary>
        /// <param name="config">配置数据对象</param>
        bool SaveConfig(TKey key, object config);
    }
    /// <summary>
    /// 以类型作为键值的 <see cref="IKeyedConfigReadWriteImpl{TKey}"/>
    /// </summary>
    /// <remarks>
    /// 键对应的配置对象, 类型与键相同
    /// </remarks>
    public interface ITypeKeyedConfigReadWriteImpl : IKeyedConfigReadWriteImpl<Type>
    {
        /// <summary>
        /// 类型是否有效
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool IsAvailable(Type type);

        /// <summary>
        /// 接口默认实现: 获取输入类型的配置, 获取失败时抛出异常
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T DefaultLoadConfig<T>()
        {
            Type type = typeof(T);
            if (!IsAvailable(type)) throw new InvalidOperationException("目标类型无效")
            {
                Data = { ["Type"] = type }
            };
            if (TryLoadConfig(type, out var config))
            {
                if (config is T tConfig) return tConfig;
                else 
                {
                    throw new InvalidOperationException("加载得到的对象类型与期望类型不同! ")
                    {
                        Data = 
                        {
                            ["Target"] = type,
                            ["Loaded"] = config?.GetType()
                        }
                    };
                }
            }
            else
            {
                throw new InvalidOperationException("加载配置失败")
                {
                    Data = { ["Type"] = type }
                };
            }
        }
        /// <summary>
        /// 接口默认实现: 保存输入类型的配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="config">配置数据对象</param>
        void DefaultSaveConfig<T>(T config)
        {
            Type type = typeof(T);
            if (!IsAvailable(type)) throw new InvalidOperationException("目标类型无效")
            {
                Data = { ["Type"] = type }
            };
            if (!SaveConfig(type, config!))
            {
                throw new InvalidOperationException("保存配置失败")
                {
                    Data = { ["Type"] = type }
                };
            }
        }
    }

}
