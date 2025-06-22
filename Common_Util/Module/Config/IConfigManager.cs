using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Module.Config
{
    /// <summary>
    /// 接口: 配置管理器
    /// </summary>
    public interface IConfigManager
    {
        #region 读写实现方式
        /// <summary>
        /// 配置读写实现字典
        /// </summary>
        /// <remarks>
        /// 键值为配置读写实现的名称, 不允许为 <see langword="null"/>
        /// </remarks>
        IReadOnlyDictionary<string, IConfigReadWriteImpl> Impls { get; }
        /// <summary>
        /// 默认的配置读写实现
        /// </summary>
        IConfigReadWriteImpl DefaultImpl { get; }
        /// <summary>
        /// 判断是否包含指定名字的读写实现
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        bool HasImpl(string name);
        /// <summary>
        /// 尝试获取配置读写实现
        /// </summary>
        /// <param name="name"></param>
        /// <param name="impl">取得的配置读写实现</param>
        /// <returns></returns>
        bool TryGetImpl(string name, [NotNullWhen(true)] out IConfigReadWriteImpl? impl);
        /// <summary>
        /// 尝试添加配置读写实现
        /// </summary>
        /// <param name="name"></param>
        /// <param name="impl"></param>
        /// <remarks>添加成功与否</remarks>
        bool TryAddImpl(string name, IConfigReadWriteImpl impl);
        /// <summary>
        /// 尝试更新配置读写实现, 仅在存在时可更新
        /// </summary>
        /// <param name="name"></param>
        /// <param name="impl"></param>
        /// <returns>更新成功与否</returns>
        bool TryUpdateImpl(string name, IConfigReadWriteImpl impl);
        /// <summary>
        /// 移除配置读写实现
        /// </summary>
        /// <param name="name"></param>
        /// <param name="impl">成功移除掉的配置读写实现</param>
        /// <returns>如果已有并移除了, 则返回 <see langword="true"/>, 如果不存在对应名字的读写实现, 则返回 <see langword="false"/></returns>
        bool TryRemoveImpl(string name, [NotNullWhen(true)] out IConfigReadWriteImpl? impl);

        #endregion

        #region 配置对象缓存

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
    }

    /// <summary>
    /// 接口: 基于配置对象类型的配置管理器
    /// </summary>
    public interface ITypeConfigManager : IConfigManager
    {
        #region 加载与保存

        /// <summary>
        /// 判断指定类型的配置对象是否已经被加载到缓存
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool IsLoaded(Type type);

        /// <summary>
        /// 加载指定类型的配置对象
        /// </summary>
        /// <param name="type"></param>
        /// <param name="implName">配置读写实现的名字, 如果为 <see langword="null"/>, 则采用 <see cref="IConfigManager.DefaultImpl"/></param>
        /// <returns></returns>
        object Load(Type type, string? implName = null);
        /// <summary>
        /// 使用指定名字的配置读写, 将当前缓存中指定类型的配置对象保存起来
        /// </summary>
        /// <param name="type"></param>
        /// <param name="implName">配置读写实现的名字, 如果为 <see langword="null"/>, 则采用 <see cref="IConfigManager.DefaultImpl"/></param>
        /// <returns></returns>
        bool Save(Type type, string? implName = null);

        #endregion

        #region 配置对象缓存

        /// <summary>
        /// 尝试获取缓存中指定类型的配置对象
        /// </summary>
        /// <param name="type"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        bool TryGet(Type type, [NotNullWhen(true)] out object? config);
        /// <summary>
        /// 尝试获取缓存中指定类型的配置对象的拷贝
        /// </summary>
        /// <param name="type"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        bool TryGetClone(Type type, [NotNullWhen(true)] out object? config);

        /// <summary>
        /// 取得当前缓存中的所有配置对象
        /// </summary>
        /// <returns></returns>
        IEnumerable<object> GetCaches();

        /// <summary>
        /// 尝试添加指定类型的配置对象到缓存
        /// </summary>
        /// <param name="type"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        bool TryAdd(Type type, object config);
        /// <summary>
        /// 尝试更新指定类型的配置对象到缓存
        /// </summary>
        /// <remarks>
        /// 处于保护状态 <see cref="IConfigManager.CacheProtecting"/> 时, 不允许更新
        /// </remarks>
        /// <param name="type"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        bool TryUpdate(Type type, object config);
        /// <summary>
        /// 尝试从缓存移除指定类型的配置对象
        /// </summary>
        /// <remarks>
        /// 处于保护状态 <see cref="IConfigManager.CacheProtecting"/> 时, 不允许移除
        /// </remarks>
        /// <param name="type"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        bool TryRemove(Type type, [NotNullWhen(true)] out object config);

        #endregion

    }
}
