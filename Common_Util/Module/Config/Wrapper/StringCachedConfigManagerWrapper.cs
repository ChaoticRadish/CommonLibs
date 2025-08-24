using Common_Util.Data.Struct;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Module.Config.Wrapper
{
    public readonly struct StringCachedConfigManagerWrapper<TCompatibleKey> : ICachedConfigManager<TCompatibleKey>
    {
        private readonly ICachedConfigManager<TCompatibleKey>? cachedManager;
        private readonly Func<TCompatibleKey, string>? convert2StrFunc;
        private readonly Func<string, TCompatibleKey>? convert2CompatibleFunc;
        private readonly ICachedConfigManager<string>? stringKeyedManager;
        /// <summary>
        /// 模式: 0:未配置; 1:类型; 2:字符串;
        /// </summary>
        private readonly ModeEnum useMode;
        private enum ModeEnum
        {
            Compatible,
            String,
        }
        private ICachedConfigManager usingManager
        {
            get 
            {
                return useMode switch
                {
                    ModeEnum.Compatible => cachedManager!,
                    ModeEnum.String => stringKeyedManager!,
                    _ => throw InvalidMode(),
                };
            }
        }


        public StringCachedConfigManagerWrapper(ICachedConfigManager<TCompatibleKey> cachedManager, Func<TCompatibleKey, string> convert2StrFunc, Func<string, TCompatibleKey> convert2CompatibleFunc)
        {
            this.cachedManager = cachedManager;
            this.convert2StrFunc = convert2StrFunc;
            this.convert2CompatibleFunc = convert2CompatibleFunc;
            stringKeyedManager = null;
            useMode = ModeEnum.Compatible;
        }
        public StringCachedConfigManagerWrapper(ICachedConfigManager<string> stringKeyedManager, Func<TCompatibleKey, string>? convert2StrFunc = null, Func<string, TCompatibleKey>? convert2CompatibleFunc = null)
        {
            this.stringKeyedManager = stringKeyedManager;
            cachedManager = null;
            this.convert2StrFunc = convert2StrFunc;
            this.convert2CompatibleFunc = convert2CompatibleFunc;
            useMode = ModeEnum.String;
        }

        private readonly StringKeyedConfigReadWriteImplWrapper<TCompatibleKey> ConvertFromBase(IConfigReadWriteImpl impl)
        {
            return impl switch
            {
                StringKeyedConfigReadWriteImplWrapper<TCompatibleKey> wrapper => wrapper,
                IKeyedConfigReadWriteImpl<TCompatibleKey> typeImpl => 
                    new(typeImpl,
                        convert2StrFunc ?? throw MissingConvertFuncException(),
                        convert2CompatibleFunc ?? throw MissingConvertFuncException()),
                IKeyedConfigReadWriteImpl<string> strImpl => new(strImpl),
                _ => throw InvalidMode()
            };
        }
        private Exception MissingConvertFuncException()
        {
            return new InvalidOperationException("需配置键值转换函数以支持读写实现实例的包装") 
            {
                Data = {
                    ["Convert2StrFunc"] = convert2StrFunc != null ? "not null" : "null",
                    ["Convert2CompatibleFunc"] = convert2CompatibleFunc != null ? "not null" : "null",
                } 
            };
        }


        #region 接口转换
        private readonly struct StringKeyedInterfaceWrapper(StringCachedConfigManagerWrapper<TCompatibleKey> wrapper) : ICachedConfigManager<string>
        {
            private readonly StringCachedConfigManagerWrapper<TCompatibleKey> wrapper = wrapper;

            public IEnumerable<string> AllKeys => wrapper.AllStringKeys;

            public IEnumerable<KeyValuePair<string, object>> AllCaches => wrapper.AllStringKeyCachePairs;

            public bool CacheProtecting => wrapper.CacheProtecting;

            public IReadOnlyDictionary<string, IConfigReadWriteImpl> Impls => wrapper.Impls;

            public IConfigReadWriteImpl DefaultImpl => wrapper.DefaultImpl;

            public void ClearCaches() => wrapper.ClearCaches();

            public object Clone(object config) => wrapper.Clone(config);

            public bool IsAvailable(IConfigReadWriteImpl impl) => wrapper.IsAvailable(impl);

            public bool IsLoaded(string key) => wrapper.IsLoaded(key);

            public void SetCacheProtect(bool b) => wrapper.SetCacheProtect(b);

            public void SetDefaultImpl<TImpl>(TImpl impl) where TImpl : IConfigReadWriteImpl
                => wrapper.SetDefaultImpl(impl);

            public bool TryAdd(string key, object config)
                => wrapper.TryAdd(key, config);

            public bool TryAddImpl(string name, IConfigReadWriteImpl impl)
                => wrapper.TryAddImpl(name, impl);

            public bool TryGet(string key, [NotNullWhen(true)] out object? config)
                => wrapper.TryGet(key, out config);

            public bool TryGetImpl(string name, [NotNullWhen(true)] out IConfigReadWriteImpl? impl)
                => wrapper.TryGetImpl(name, out impl);

            public bool TryRemove(string key, [NotNullWhen(true)] out object? config)
                => wrapper.TryRemove(key, out config);

            public bool TryRemoveImpl(string name, MaybeNull<IConfigReadWriteImpl> comparisonImpl, [NotNullWhen(true)] out IConfigReadWriteImpl? impl)
                => wrapper.TryRemoveImpl(name, comparisonImpl, out impl);

            public bool TrySetImpl(string name, IConfigReadWriteImpl impl)
                => wrapper.TrySetImpl(name, impl);

            public bool TryUpdate(string key, object config)
                => wrapper.TryUpdate(key, config);

            public bool TryUpdateImpl(string name, IConfigReadWriteImpl impl)
                => wrapper.TryUpdateImpl(name, impl);
        }
        #endregion

        #region 接口实现

        #region 共用签名部分
        public bool CacheProtecting => usingManager.CacheProtecting;

        public void SetCacheProtect(bool b) => usingManager.SetCacheProtect(b);

        public void ClearCaches() => usingManager.ClearCaches();

        public object Clone(object config) => usingManager.Clone(config);

        public bool IsAvailable(IConfigReadWriteImpl impl) => usingManager.IsAvailable(StringKeyedConfigReadWriteImplWrapper<TCompatibleKey>.UnWrapper(impl));

        public IReadOnlyDictionary<string, IConfigReadWriteImpl> Impls
        {
            get
            {
                var _this = this;
                return usingManager.Impls.ToDictionary(i => i.Key, i => (IConfigReadWriteImpl)_this.ConvertFromBase(i.Value));
            }
        }

        public IConfigReadWriteImpl DefaultImpl => ConvertFromBase(usingManager.DefaultImpl);

        public void SetDefaultImpl<TImpl>(TImpl impl) where TImpl : IConfigReadWriteImpl
        {
            usingManager.SetDefaultImpl(StringKeyedConfigReadWriteImplWrapper<TCompatibleKey>.UnWrapper(impl));
        }

        public bool TryGetImpl(string name, [NotNullWhen(true)] out IConfigReadWriteImpl? impl)
        {
            if (!usingManager.TryGetImpl(name, out impl))
            {
                impl = null;
                return false;
            }
            else
            {
                impl = ConvertFromBase(impl);
                return true;
            }
        }

        public bool TryAddImpl(string name, IConfigReadWriteImpl impl)
        {
            impl = StringKeyedConfigReadWriteImplWrapper<TCompatibleKey>.UnWrapper(impl);
            return useMode switch
            {
                ModeEnum.String => ((ICachedConfigManager<string>)usingManager).TryAddImpl(name, impl),
                ModeEnum.Compatible => ((ICachedConfigManager<TCompatibleKey>)usingManager).TryAddImpl(name, impl),
                _ => throw InvalidMode(),
            };
        }

        public bool TryUpdateImpl(string name, IConfigReadWriteImpl impl)
        {
            impl = StringKeyedConfigReadWriteImplWrapper<TCompatibleKey>.UnWrapper(impl);
            return useMode switch
            {
                ModeEnum.String => ((ICachedConfigManager<string>)usingManager).TryUpdateImpl(name, impl),
                ModeEnum.Compatible => ((ICachedConfigManager<TCompatibleKey>)usingManager).TryUpdateImpl(name, impl),
                _ => throw InvalidMode(),
            };
        }

        public bool TrySetImpl(string name, IConfigReadWriteImpl impl)
        {
            impl = StringKeyedConfigReadWriteImplWrapper<TCompatibleKey>.UnWrapper(impl);
            return useMode switch
            {
                ModeEnum.String => ((ICachedConfigManager<string>)usingManager).TrySetImpl(name, impl),
                ModeEnum.Compatible => ((ICachedConfigManager<TCompatibleKey>)usingManager).TrySetImpl(name, impl),
                _ => throw InvalidMode(),
            };
        }

        public bool TryRemoveImpl(string name, MaybeNull<IConfigReadWriteImpl> comparisonImpl, [NotNullWhen(true)] out IConfigReadWriteImpl? impl)
        {
            if (comparisonImpl.HasValue)
            {
                comparisonImpl = new MaybeNull<IConfigReadWriteImpl>(StringKeyedConfigReadWriteImplWrapper<TCompatibleKey>.UnWrapper(comparisonImpl.Value));
            }
            bool result = useMode switch
            {
                ModeEnum.String => ((ICachedConfigManager<string>)usingManager).TryRemoveImpl(name, comparisonImpl, out impl),
                ModeEnum.Compatible => ((ICachedConfigManager<TCompatibleKey>)usingManager).TryRemoveImpl(name, comparisonImpl, out impl),
                _ => throw InvalidMode(),
            };
            if (result && impl != null)
            {
                impl = ConvertFromBase(impl);
            }
            return result;
        }


        #endregion

        #region 兼容键值类型
        public IEnumerable<TCompatibleKey> AllKeys
        {
            get
            {
                var _this = this;
                return useMode switch
                {
                    ModeEnum.String => ((ICachedConfigManager<string>)usingManager).AllKeys.Select(k => _this.convert2CompatibleFunc!(k)),
                    ModeEnum.Compatible => ((ICachedConfigManager<TCompatibleKey>)usingManager).AllKeys,
                    _ => throw InvalidMode(),
                };
            }
        }

        public IEnumerable<KeyValuePair<TCompatibleKey, object>> AllCaches 
        {
            get
            {
                var _this = this;
                return useMode switch
                {
                    ModeEnum.String => ((ICachedConfigManager<string>)usingManager).AllCaches.Select(i => KeyValuePair.Create(_this.convert2CompatibleFunc!(i.Key), i.Value)),
                    ModeEnum.Compatible => ((ICachedConfigManager<TCompatibleKey>)usingManager).AllCaches,
                    _ => throw InvalidMode(),
                };
            } 
        }


        public bool IsLoaded(TCompatibleKey key)
        {
            return useMode switch
            {
                ModeEnum.String => ((ICachedConfigManager<string>)usingManager).IsLoaded(convert2StrFunc!(key)),
                ModeEnum.Compatible => ((ICachedConfigManager<TCompatibleKey>)usingManager).IsLoaded(key),
                _ => throw InvalidMode(),
            };
        }

        public bool TryGet(TCompatibleKey key, [NotNullWhen(true)] out object? config)
        {
            return useMode switch
            {
                ModeEnum.String => ((ICachedConfigManager<string>)usingManager).TryGet(convert2StrFunc!(key), out config),
                ModeEnum.Compatible => ((ICachedConfigManager<TCompatibleKey>)usingManager).TryGet(key, out config),
                _ => throw InvalidMode(),
            };
        }

        public bool TryAdd(TCompatibleKey key, object config)
        {
            return useMode switch
            {
                ModeEnum.String => ((ICachedConfigManager<string>)usingManager).TryAdd(convert2StrFunc!(key), config),
                ModeEnum.Compatible => ((ICachedConfigManager<TCompatibleKey>)usingManager).TryAdd(key, config),
                _ => throw InvalidMode(),
            };
        }

        public bool TryUpdate(TCompatibleKey key, object config)
        {
            return useMode switch
            {
                ModeEnum.String => ((ICachedConfigManager<string>)usingManager).TryUpdate(convert2StrFunc!(key), config),
                ModeEnum.Compatible => ((ICachedConfigManager<TCompatibleKey>)usingManager).TryUpdate(key, config),
                _ => throw InvalidMode(),
            };
        }

        public bool TryRemove(TCompatibleKey key, [NotNullWhen(true)] out object? config)
        {
            return useMode switch
            {
                ModeEnum.String => ((ICachedConfigManager<string>)usingManager).TryRemove(convert2StrFunc!(key), out config),
                ModeEnum.Compatible => ((ICachedConfigManager<TCompatibleKey>)usingManager).TryRemove(key, out config),
                _ => throw InvalidMode(),
            };
        }

        #endregion

        #region 字符串键值

        public IEnumerable<string> AllStringKeys
        {
            get
            {
                var _this = this;
                return useMode switch
                {
                    ModeEnum.String => ((ICachedConfigManager<string>)usingManager).AllKeys,
                    ModeEnum.Compatible => ((ICachedConfigManager<TCompatibleKey>)usingManager).AllKeys.Select(k => _this.convert2StrFunc!(k)),
                    _ => throw InvalidMode(),
                };
            }
        }

        public IEnumerable<KeyValuePair<string, object>> AllStringKeyCachePairs
        {
            get
            {
                var _this = this;
                return useMode switch
                {
                    ModeEnum.String => ((ICachedConfigManager<string>)usingManager).AllCaches,
                    ModeEnum.Compatible => ((ICachedConfigManager<TCompatibleKey>)usingManager).AllCaches.Select(i => KeyValuePair.Create(_this.convert2StrFunc!(i.Key), i.Value)),
                    _ => throw InvalidMode(),
                };
            }
        }

        public bool IsLoaded(string key)
        {
            return useMode switch
            {
                ModeEnum.String => ((ICachedConfigManager<string>)usingManager).IsLoaded(key),
                ModeEnum.Compatible => ((ICachedConfigManager<TCompatibleKey>)usingManager).IsLoaded(convert2CompatibleFunc!(key)),
                _ => throw InvalidMode(),
            };
        }

        public bool TryGet(string key, [NotNullWhen(true)] out object? config)
        {
            return useMode switch
            {
                ModeEnum.String => ((ICachedConfigManager<string>)usingManager).TryGet(key, out config),
                ModeEnum.Compatible => ((ICachedConfigManager<TCompatibleKey>)usingManager).TryGet(convert2CompatibleFunc!(key), out config),
                _ => throw InvalidMode(),
            };
        }

        public bool TryAdd(string key, object config)
        {
            return useMode switch
            {
                ModeEnum.String => ((ICachedConfigManager<string>)usingManager).TryAdd(key,  config),
                ModeEnum.Compatible => ((ICachedConfigManager<TCompatibleKey>)usingManager).TryAdd(convert2CompatibleFunc!(key), config),
                _ => throw InvalidMode(),
            };
        }

        public bool TryUpdate(string key, object config)
        {
            return useMode switch
            {
                ModeEnum.String => ((ICachedConfigManager<string>)usingManager).TryUpdate(key, config),
                ModeEnum.Compatible => ((ICachedConfigManager<TCompatibleKey>)usingManager).TryUpdate(convert2CompatibleFunc!(key), config),
                _ => throw InvalidMode(),
            };
        }

        public bool TryRemove(string key, [NotNullWhen(true)] out object? config)
        {
            return useMode switch
            {
                ModeEnum.String => ((ICachedConfigManager<string>)usingManager).TryRemove(key, out config),
                ModeEnum.Compatible => ((ICachedConfigManager<TCompatibleKey>)usingManager).TryRemove(convert2CompatibleFunc!(key), out config),
                _ => throw InvalidMode(),
            };
        }

        #endregion

        #endregion

        #region 异常
        private readonly Exception InvalidMode() => new InvalidOperationException("当前使用模式无效") { Data = { ["Mode"] = useMode } };

        #endregion
    }
}
