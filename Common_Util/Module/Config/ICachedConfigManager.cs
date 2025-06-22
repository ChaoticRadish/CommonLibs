using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Module.Config
{
    public interface ICachedConfigManager : IConfigManager
    {
        #region 状态

        /// <summary>
        /// 当前是否处于保护状态
        /// </summary>
        /// <remarks>
        /// 处于保护状态时, 缓存着的配置对象将无法被移除或覆盖, 但是允许添加 (或加载) 新的配置对象
        /// </remarks>
        bool CacheProtecting { get; }
        /// <summary>
        /// 设置当前是否启用保护状态
        /// </summary>
        /// <param name="b"></param>
        void SetCacheProtect(bool b);

        #endregion

        /// <summary>
        /// 清空所有缓存着的配置对象
        /// </summary>
        void ClearCaches();

    }

    /// <summary>
    /// 接口: 缓存配置对象的配置管理器
    /// </summary>
    /// <remarks>
    /// 键值与配置对象是一对一关系
    /// </remarks>
    /// <typeparam name="TKey">配置对象的键值</typeparam>
    public interface ICachedConfigManager<TKey> : IKeyedConfigManager<TKey>, ICachedConfigManager
    {


        #region 缓存数据
        /// <summary>
        /// 取得当前缓存中的所有键值
        /// </summary>
        IEnumerable<TKey> AllKeys { get; }
        /// <summary>
        /// 取得当前缓存中的所有配置对象
        /// </summary>
        /// <returns></returns>
        IEnumerable<KeyValuePair<TKey, object>> AllCaches { get; }
        /// <summary>
        /// 取得当前缓存中的所有配置对象的拷贝
        /// </summary>
        /// <returns></returns>
        IEnumerable<KeyValuePair<TKey, object>> AllCachesClone
            => AllCaches.Select(kvp => KeyValuePair.Create(kvp.Key, Clone(kvp.Value)));

        #endregion

        /// <summary>
        /// 判断指定键值的配置对象是否已经被加载到缓存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool IsLoaded(TKey key);


        /// <summary>
        /// 尝试获取缓存中指定键值的配置对象
        /// </summary>
        /// <param name="key"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        bool TryGet(TKey key, [NotNullWhen(true)] out object? config);

        /// <summary>
        /// 尝试添加指定键值的配置对象到缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        bool TryAdd(TKey key, object config);
        /// <summary>
        /// 尝试更新指定键值的配置对象到缓存
        /// </summary>
        /// <remarks>
        /// 处于保护状态 <see cref="IConfigManager.CacheProtecting"/> 时, 不允许更新
        /// </remarks>
        /// <param name="key"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        bool TryUpdate(TKey key, object config);
        /// <summary>
        /// 尝试从缓存移除指定键值的配置对象
        /// </summary>
        /// <remarks>
        /// 处于保护状态 <see cref="IConfigManager.CacheProtecting"/> 时, 不允许移除
        /// </remarks>
        /// <param name="key"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        bool TryRemove(TKey key, [NotNullWhen(true)] out object? config);

        #region 默认实现的 Get / Set 方法
        /// <summary>
        /// 获取缓存中指定键值的配置对象
        /// </summary>
        /// <param name="key"></param>
        /// <param name="loadIfNotLoaded">如果未加载指定键值的配置, 是否加载</param>
        /// <param name="useImplName">加载配置时使用的读写实现的名字</param>
        /// <param name="getClone">取得配置对象时, 是否取得其拷贝, 而不是其引用</param>
        /// <returns></returns>
        object? Get(TKey key, bool loadIfNotLoaded = false, string? useImplName = null, bool getClone = false)
        {
            object? output;
            if (IsLoaded(key))
            {
                if (TryGet(key, out object? config)) output = config;
                else return null;
            }
            else
            {
                if (loadIfNotLoaded)
                {
                    var loaded = Load(key, useImplName);
                    if (loaded != null && TryAdd(key, loaded)) output = loaded;
                    else return null;
                }
                else return null;
            }
            if (getClone && output != null)
            {
                return Clone(output);
            }
            else
            {
                return output;
            }
        }
        /// <summary>
        /// 设置指定键值的配置对象到缓存
        /// </summary>
        /// <remarks>
        /// 如果指定键值的配置对象不存在, 则添加, 反之则覆盖 
        /// (<see cref="IConfigManager.CacheProtecting"/> 状态下不会覆盖, 而是返回 <see langword="false"/>)
        /// </remarks>
        /// <param name="key"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        bool Set(TKey key, object config)
        {
            if (IsLoaded(key))
            {
                if (CacheProtecting)
                {
                    return false;
                }
                return TryUpdate(key, config);
            }
            else return TryAdd(key, config);
        }
        #endregion

    }


    /// <summary>
    /// 接口: 使用类型作为键的 <see cref="ICachedConfigManager{TKey}"/>
    /// </summary>
    /// <remarks>
    /// 键对应的配置对象, 类型与键相同
    /// </remarks>
    public interface ITypeCachedConfigManager : ICachedConfigManager<Type>
    {
        #region 默认实现
        /// <summary>
        /// 获取缓存中指定类型的配置对象
        /// </summary>
        /// <param name="loadIfNotLoaded">如果未加载指定键值的配置, 是否加载</param>
        /// <param name="useImplName">加载配置时使用的读写实现的名字</param>
        /// <param name="getClone">取得配置对象时, 是否取得其拷贝, 而不是其引用</param>
        /// <returns>
        /// 当未获取到配置对象或取得的配置对象不是 <typeparamref name="T"/> 时, 会返回 <see langword="default"/>, 所以在类型是不能为 <see langword="null"/> 的类型时, 需要另外注意一下
        /// </returns>
        T? Get<T>(bool loadIfNotLoaded = false, string? useImplName = null, bool getClone = false)
        {
            var output = Get(typeof(T), loadIfNotLoaded, useImplName, getClone);
            return output == null ? default : (output is T tOut ? tOut : default);
        }

        /// <summary>
        /// 设置指定类型的配置对象到缓存
        /// </summary>
        /// <remarks>
        /// 如果指定类型的配置对象不存在, 则添加, 反之则覆盖 
        /// (<see cref="IConfigManager.CacheProtecting"/> 状态下不会覆盖, 而是返回 <see langword="false"/>)
        /// </remarks>
        /// <param name="config"></param>
        /// <returns></returns>
        bool Set<T>(T config)
        {
            // 外部传入可能为 null 的值时, 编译器会警告, 这里就不检查了
            return Set(typeof(T), config!);
        }
        #endregion
    }
    /// <summary>
    /// 接口: 使用字符串作为键的 <see cref="ICachedConfigManager{TKey}"/>
    /// </summary>
    public interface IStringCachedConfigManager : ICachedConfigManager<string> { }
}
