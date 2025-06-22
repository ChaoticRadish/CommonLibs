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
        #region 基础操作
        /// <summary>
        /// 拷贝配置对象
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        object Clone(object config);

        #endregion

        #region 读写实现方式

        /// <summary>
        /// 默认的配置读写实现
        /// </summary>
        IConfigReadWriteImpl DefaultImpl { get; }

        /// <summary>
        /// 判断 <paramref name="impl"/> 是否此配置管理器可用的配置读写实现
        /// </summary>
        /// <returns></returns>
        bool IsAvailable(IConfigReadWriteImpl impl);
        /// <summary>
        /// 设置 <paramref name="impl"/> 为当前的默认读写实现
        /// </summary>
        /// <typeparam name="TImpl"></typeparam>
        /// <param name="impl"></param>
        void SetDefaultImpl<TImpl>(TImpl impl) where TImpl : IConfigReadWriteImpl;
        /// <summary>
        /// 检查传入的 <paramref name="impl"/> 是否可用, 如果可用则设置为当前的默认读写实现
        /// </summary>
        /// <typeparam name="TImpl"></typeparam>
        /// <param name="impl"></param>
        /// <returns></returns>
        bool TrySetDefaultImpl<TImpl>(TImpl impl) where TImpl : IConfigReadWriteImpl
        {
            if (!IsAvailable(impl)) return false;
            else
            {
                SetDefaultImpl(impl);
            }
            return true;
        }
        #endregion
    }
    /// <summary>
    /// 接口: 配置管理器
    /// </summary>
    public interface IConfigManager<TImpl> : IConfigManager
        where TImpl : IConfigReadWriteImpl
    {
        #region 读写实现方式
        /// <summary>
        /// 配置读写实现字典
        /// </summary>
        /// <remarks>
        /// 键值为配置读写实现的名称, 不允许为 <see langword="null"/>
        /// </remarks>
        IReadOnlyDictionary<string, TImpl> Impls { get; }
        /// <summary>
        /// 默认的配置读写实现
        /// </summary>
        new TImpl DefaultImpl { get => (TImpl)((IConfigManager)this).DefaultImpl; }
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
        bool TryGetImpl(string name, [NotNullWhen(true)] out TImpl? impl);

        /// <summary>
        /// 尝试添加配置读写实现
        /// </summary>
        /// <param name="name"></param>
        /// <param name="impl"></param>
        /// <remarks>添加成功与否</remarks>
        bool TryAddImpl(string name, TImpl impl);
        /// <summary>
        /// 尝试更新配置读写实现, 仅在存在时可更新
        /// </summary>
        /// <param name="name"></param>
        /// <param name="impl"></param>
        /// <returns>更新成功与否</returns>
        bool TryUpdateImpl(string name, TImpl impl);
        /// <summary>
        /// 移除配置读写实现
        /// </summary>
        /// <param name="name"></param>
        /// <param name="impl">成功移除掉的配置读写实现</param>
        /// <returns>如果已有并移除了, 则返回 <see langword="true"/>, 如果不存在对应名字的读写实现, 则返回 <see langword="false"/></returns>
        bool TryRemoveImpl(string name, [NotNullWhen(true)] out TImpl? impl);

        #region 默认实现的 Get / Set 方法
        /// <summary>
        /// 获取配置读写实现
        /// </summary>
        /// <param name="name">
        /// 实现的名字, 如果为 <see langword="null"/>, 将返回 <see cref="DefaultImpl"/>; 如果不存在对应名字的实现, 则返回 <see langword="null"/>
        /// </param>
        /// <returns></returns>
        TImpl? GetImpl(string? name = null)
        {
            if (name == null) return DefaultImpl;
            else
            {
                if (TryGetImpl(name, out var impl)) return impl;
                else return default;
            }
        }
        /// <summary>
        /// 尝试获取可以分配给 <typeparamref name="TWannaImpl"/> 的配置读写实现
        /// </summary>
        /// <typeparam name="TWannaImpl">期望的实现类型</typeparam>
        /// <param name="name"></param>
        /// <param name="impl"></param>
        /// <returns></returns>
        bool TryGetImpl<TWannaImpl>(string name, [NotNullWhen(true)] out TWannaImpl? impl)
        {
            if (TryGetImpl(name, out var exist) && exist is TWannaImpl tImpl)
            {
                impl = tImpl;
                return true;
            }
            else
            {
                impl = default;
                return false;
            }
        }

        /// <summary>
        /// 设置配置读写实现
        /// </summary>
        /// <remarks>
        /// 如果指定名字的实现不存在, 则添加, 反之则覆盖
        /// </remarks>
        /// <param name="name"></param>
        /// <param name="impl"></param>
        /// <returns>添加或更新操作是否成功</returns>
        bool SetImpl(string name, TImpl impl)
        {
            if (HasImpl(name))
            {
                return TryUpdateImpl(name, impl);
            }
            else
            {
                return TryAddImpl(name, impl);
            }
        }
        #endregion

        #endregion


    }

}
