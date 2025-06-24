using Common_Util.Data.Struct;
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
        /// 配置读写实现字典
        /// </summary>
        /// <remarks>
        /// 键值为配置读写实现的名称, 不允许为 <see langword="null"/>
        /// </remarks>
        IReadOnlyDictionary<string, IConfigReadWriteImpl> Impls { get; }

        /// <summary>
        /// 判断是否包含指定名字的读写实现
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        bool HasImpl(string name) => Impls.ContainsKey(name);
        /// <summary>
        /// 判断 <paramref name="impl"/> 是否此配置管理器可用的配置读写实现
        /// </summary>
        /// <returns></returns>
        bool IsAvailable(IConfigReadWriteImpl impl);

        /// <summary>
        /// 默认的配置读写实现
        /// </summary>
        IConfigReadWriteImpl DefaultImpl { get; }

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
        /// 尝试设置配置读写实现
        /// </summary>
        /// <remarks>
        /// 如果指定名字的实现不存在, 则添加, 反之则覆盖
        /// </remarks>
        /// <param name="name"></param>
        /// <param name="impl"></param>
        /// <returns>添加或更新操作是否成功</returns>
        bool TrySetImpl(string name, IConfigReadWriteImpl impl);
        /// <summary>
        /// 移除配置读写实现
        /// </summary>
        /// <param name="name"></param>
        /// <param name="comparisonImpl">比较实例, 如果不是 <see cref="MaybeNull{IConfigReadWriteImpl}.Null"/>, 则仅在对应值与此相同时执行移除操作</param>
        /// <param name="impl">成功移除掉的配置读写实现</param>
        /// <returns>如果已有并移除了, 则返回 <see langword="true"/>, 如果不存在对应名字的读写实现, 则返回 <see langword="false"/></returns>
        bool TryRemoveImpl(string name, MaybeNull<IConfigReadWriteImpl> comparisonImpl, [NotNullWhen(true)] out IConfigReadWriteImpl? impl);

        #region 默认实现
        /// <summary>
        /// 尝试获取配置读写实现
        /// </summary>
        /// <param name="enum"></param>
        /// <param name="impl">取得的配置读写实现</param>
        /// <returns></returns>
        bool TryGetImpl(Enum @enum, [NotNullWhen(true)] out IConfigReadWriteImpl? impl) => TryGetImpl(@enum.ToString(), out impl);
        /// <summary>
        /// 尝试设置配置读写实现
        /// </summary>
        /// <remarks>
        /// 如果指定名字的实现不存在, 则添加, 反之则覆盖
        /// </remarks>
        /// <param name="enum"></param>
        /// <param name="impl"></param>
        /// <returns>添加或更新操作是否成功</returns>
        bool TrySetImpl(Enum @enum, IConfigReadWriteImpl impl) => TrySetImpl(@enum.ToString(), impl);
        /// <summary>
        /// 检查并设置配置读写实现
        /// </summary>
        /// <remarks>
        /// 如果指定名字的实现不存在, 则添加, 反之则覆盖
        /// </remarks>
        /// <param name="name"></param>
        /// <param name="impl"></param>
        void SetImpl(string name, IConfigReadWriteImpl impl)
        {
            if (!IsAvailable(impl)) throw new NotSupportedException("配置读写实现不可用于此配置管理器");
            if (!TrySetImpl(name, impl)) throw new InvalidOperationException("设置配置读写实现失败");
        }
        /// <summary>
        /// 检查并设置配置读写实现
        /// </summary>
        /// <remarks>
        /// 如果指定名字的实现不存在, 则添加, 反之则覆盖
        /// </remarks>
        /// <param name="enum"></param>
        /// <param name="impl"></param>
        void SetImpl(Enum @enum, IConfigReadWriteImpl impl) => SetImpl(@enum.ToString(), impl);


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
        /// 移除配置读写实现
        /// </summary>
        /// <param name="name"></param>
        /// <param name="impl">成功移除掉的配置读写实现</param>
        /// <returns>如果已有并移除了, 则返回 <see langword="true"/>, 如果不存在对应名字的读写实现, 则返回 <see langword="false"/></returns>
        bool TryRemoveImpl(string name, [NotNullWhen(true)] out IConfigReadWriteImpl? impl) => TryRemoveImpl(name, MaybeNull<IConfigReadWriteImpl>.Null, out impl);
        #endregion

        #endregion
    }
    /// <summary>
    /// 接口: 使用 <typeparamref name="TImpl"/> 类型的读写实现的配置管理器
    /// </summary>
    /// <remarks>
    /// 默认实现下, 增删改均调用 <see cref="IConfigManager"/> 实现的接口, 操作其中类型可以分配给 <typeparamref name="TImpl"/> 的部分
    /// </remarks>
    public interface IConfigManager<TImpl> : IConfigManager
        where TImpl : IConfigReadWriteImpl
    {
        #region 读写实现方式

        /// <summary>
        /// 判断是否包含指定名字的读写实现
        /// </summary>
        /// <remarks>
        /// 默认实现从 <see cref="IConfigManager.Impls"/> 判断是否存在, 且类型为 <typeparamref name="TImpl"/>
        /// </remarks>
        /// <param name="name"></param>
        /// <returns></returns>
        new bool HasImpl(string name) => ((IConfigManager)this).Impls.TryGetValue(name, out var exist) && exist is TImpl;

        /// <summary>
        /// 配置读写实现字典
        /// </summary>
        /// <remarks>
        /// 键值为配置读写实现的名称, 不允许为 <see langword="null"/>. 默认实现为从基类取得类型可分配给 <typeparamref name="TImpl"/> 的项
        /// </remarks>
        new IReadOnlyDictionary<string, TImpl> Impls 
        {
            get
            {
                var baseImpls = ((IConfigManager)this).Impls;
                return baseImpls.Where(i => i.Value is TImpl).ToDictionary(kvp => kvp.Key, kvp => (TImpl)kvp.Value);
            } 
        }

        /// <summary>
        /// 默认的配置读写实现
        /// </summary>
        new TImpl DefaultImpl { get => (TImpl)((IConfigManager)this).DefaultImpl; }

        /// <summary>
        /// 尝试获取配置读写实现
        /// </summary>
        /// <param name="name"></param>
        /// <param name="impl">取得的配置读写实现</param>
        /// <returns></returns>
        bool TryGetImpl(string name, [NotNullWhen(true)] out TImpl? impl)
        {
            if (TryGetImpl(name, out IConfigReadWriteImpl? got) && got is TImpl tGot)
            {
                impl = tGot;
                return true;
            }
            else
            {
                impl = default;
                return false;
            }
        }
        /// <summary>
        /// 尝试添加配置读写实现
        /// </summary>
        /// <remarks>
        /// 默认实现下会调用 <see cref="IConfigManager.TryAddImpl(string, IConfigReadWriteImpl)"/> 添加, 如果对应名称已存在, 且类型不为 <typeparamref name="TImpl"/>, 也会添加失败
        /// </remarks>
        /// <param name="name"></param>
        /// <param name="impl"></param>
        /// <remarks>添加成功与否</remarks>
        bool TryAddImpl(string name, TImpl impl) => ((IConfigManager)this).TryAddImpl(name, impl);
        /// <summary>
        /// 尝试更新配置读写实现, 仅在存在时可更新
        /// </summary>
        /// <param name="name"></param>
        /// <param name="comparisonImpl">如果此值不为 <see cref="MaybeNull{TImpl}.Null"/>, 则仅在名字对应值与此相等时执行更新</param>
        /// <param name="impl"></param>
        /// <returns>更新成功与否</returns>
        bool TryUpdateImpl(string name, MaybeNull<TImpl> comparisonImpl, TImpl impl) => ((IConfigManager)this).TryUpdateImpl(name, impl);
        /// <summary>
        /// 尝试设置配置读写实现
        /// </summary>
        /// <remarks>
        /// 如果指定名字的实现不存在, 则添加, 反之则覆盖
        /// </remarks>
        /// <param name="name"></param>
        /// <param name="impl"></param>
        /// <returns>添加或更新操作是否成功</returns>
        bool TrySetImpl(string name, TImpl impl) => ((IConfigManager)this).TrySetImpl(name, impl);
        /// <summary>
        /// 移除配置读写实现
        /// </summary>
        /// <param name="name"></param>
        /// <param name="impl">成功移除掉的配置读写实现</param>
        /// <returns>如果已有并移除了, 则返回 <see langword="true"/>, 如果不存在对应名字的读写实现, 则返回 <see langword="false"/></returns>
        bool TryRemoveImpl(string name, [NotNullWhen(true)] out TImpl? impl)
        {
            if (TryGetImpl(name, out IConfigReadWriteImpl? got) && got is TImpl tGot)
            {
                if (TryRemoveImpl(name, new MaybeNull<IConfigReadWriteImpl>(tGot), out got))
                {
                    impl = (TImpl)got;
                    return true;
                }
            }
            impl = default;
            return false;
        }
        /// <summary>
        /// 移除配置读写实现
        /// </summary>
        /// <param name="name"></param>
        /// <param name="comparisonImpl">比较实例, 如果不是 <see cref="MaybeNull{TImpl}.Null"/>, 则仅在对应值与此相同时执行移除操作</param>
        /// <param name="impl">成功移除掉的配置读写实现</param>
        /// <returns>如果已有并移除了, 则返回 <see langword="true"/>, 如果不存在对应名字的读写实现, 则返回 <see langword="false"/></returns>
        bool TryRemoveImpl(string name, MaybeNull<TImpl> comparisonImpl, [NotNullWhen(true)] out TImpl? impl)
        {
            if (!comparisonImpl.HasValue)
            {
                return TryRemoveImpl(name, out impl);
            }
            else if (TryRemoveImpl(name, new MaybeNull<IConfigReadWriteImpl>(comparisonImpl.Value), out var got))
            {
                impl = (TImpl)got;
                return true;
            }
            else
            {
                impl = default;
                return false;
            }
        }


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
                if (TryGetImpl(name, out TImpl? impl)) return impl;
                else return default;
            }
        }
        /// <summary>
        /// 尝试更新配置读写实现, 仅在存在时可更新
        /// </summary>
        /// <param name="name"></param>
        /// <param name="impl"></param>
        /// <returns>更新成功与否</returns>
        bool TryUpdateImpl(string name, TImpl impl) => TryUpdateImpl(name, MaybeNull<TImpl>.Null, impl);

        #endregion


    }

}
