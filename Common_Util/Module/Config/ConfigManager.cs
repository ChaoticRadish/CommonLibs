using Common_Util.Data.Struct;
using Common_Util.Extensions;
using Common_Util.Module.Concurrency;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Module.Config
{
    /// <summary>
    /// 配置管理器的默认实现
    /// </summary>
    public class ConfigManager : ITypeCachedConfigManager
    {
        public IReadOnlyDictionary<string, IKeyedConfigReadWriteImpl<Type>> Impls => impls;
        private readonly ConcurrentDictionary<string, IKeyedConfigReadWriteImpl<Type>> impls = [];

        public IKeyedConfigReadWriteImpl<Type> DefaultImpl 
        { 
            get => settedDefaultImpl ?? defaultImpl;
            set => settedDefaultImpl = value;
        }
        private IKeyedConfigReadWriteImpl<Type>? settedDefaultImpl;
        private readonly IKeyedConfigReadWriteImpl<Type> defaultImpl = new ConfigurationManagerReadWriteImpl();

        IConfigReadWriteImpl IConfigManager.DefaultImpl => DefaultImpl;

        IReadOnlyDictionary<string, IConfigReadWriteImpl> IConfigManager.Impls => impls.ToDictionary(kvp => kvp.Key, kvp => (IConfigReadWriteImpl)kvp.Value);

        public bool IsAvailable(IConfigReadWriteImpl impl)
        {
            if (impl == null) return false;
            else
            {
                return impl.GetType().IsAssignableTo(typeof(IKeyedConfigReadWriteImpl<Type>));
            }
        }
        public void SetDefaultImpl<TImpl>(TImpl impl) where TImpl : IConfigReadWriteImpl
        {
            DefaultImpl = (IKeyedConfigReadWriteImpl<Type>)impl;
        }

        public bool HasImpl(string name)
        {
            name ??= string.Empty;
            return impls.ContainsKey(name);
        }

        #region 基于 ConcurrentDictionary<string, IKeyedConfigReadWriteImpl<Type>> 实现操作方法

        /// <summary>
        /// 添加与更新操作的守卫对象
        /// </summary>
        /// <remarks>
        /// 执行这两个操作时不允许执行比较移除 (<see cref="TryRemoveImpl(string, MaybeNull{IKeyedConfigReadWriteImpl{Type}}, out IKeyedConfigReadWriteImpl{Type}?)"/>), 
        /// 执行移除时也将阻止添加与更新 <br/>
        /// 暂且不针对键加锁, 这样子够用了
        /// </remarks>
        private readonly WorkCountGuard implDicIUGuard = new();
        private static TimeSpan implDicIUWaitGuardTimeout = TimeSpan.FromSeconds(5);
        private readonly object compareRemoveLocker = new();
        public bool TryGetImpl(string name, [NotNullWhen(true)] out IKeyedConfigReadWriteImpl<Type>? impl)
        {
            return impls.TryGetValue(name, out impl);
        }
        public bool TryAddImpl(string name, IKeyedConfigReadWriteImpl<Type> impl)
        {
            using var token = implDicIUGuard.TryBeginWork(implDicIUWaitGuardTimeout);
            if (!token.GetSuccess) return false;

            return impls.TryAdd(name, impl);
        }
        public bool TryUpdateImpl(string name, MaybeNull<IKeyedConfigReadWriteImpl<Type>> comparisonValue, IKeyedConfigReadWriteImpl<Type> impl)
        {
            using var token = implDicIUGuard.TryBeginWork(implDicIUWaitGuardTimeout);
            if (!token.GetSuccess) return false;

            IKeyedConfigReadWriteImpl<Type> comparisonImpl;
            if (comparisonValue.HasValue) comparisonImpl = comparisonValue.Value;
            else
            {
                if (!impls.TryGetValue(name, out var exist))
                    return false;
                else
                    comparisonImpl = exist;
            }
            return impls.TryUpdate(name, impl, comparisonImpl);
        }
        public bool TrySetImpl(string name, IKeyedConfigReadWriteImpl<Type> impl)
        {
            using var token = implDicIUGuard.TryBeginWork(implDicIUWaitGuardTimeout);
            if (!token.GetSuccess) return false;

            impls.AddOrUpdate(name, impl, (_, _) => impl);
            return true;
        }
        public bool TryRemoveImpl(string name, [NotNullWhen(true)] out IKeyedConfigReadWriteImpl<Type>? impl)
        {
            return impls.TryRemove(name, out impl);
        }

        public bool TryRemoveImpl(string name, MaybeNull<IKeyedConfigReadWriteImpl<Type>> comparisonImpl, [NotNullWhen(true)] out IKeyedConfigReadWriteImpl<Type>? impl)
        {
            if (!comparisonImpl.HasValue) return impls.TryRemove(name, out impl);
            else
            {
                using var token = implDicIUGuard.TryAcquireLock(implDicIUWaitGuardTimeout);
                if (!token.GetSuccess)
                {
                    impl = default;
                    return false;
                }

                lock (compareRemoveLocker)
                {
                    if (impls.TryGetValue(name, out var exists) && exists == comparisonImpl.Value)
                    {
                        if (impls.TryRemove(name, out impl))
                        {
                            return true;
                        }
                    }
                }
                impl = default;
                return false;
            }
        }


        public bool TryGetImpl(string name, [NotNullWhen(true)] out IConfigReadWriteImpl? impl)
        {
            if (TryGetImpl(name, out IKeyedConfigReadWriteImpl<Type>? exist))
            {
                impl = exist;
                return true;
            }
            else
            {
                impl = default;
                return false;
            }
        }

        public bool TryAddImpl(string name, IConfigReadWriteImpl impl)
        {
            if (!IsAvailable(impl)) return false;
            else return TryAddImpl(name, (IKeyedConfigReadWriteImpl<Type>)impl);
        }

        public bool TryUpdateImpl(string name, IConfigReadWriteImpl impl)
        {
            if (!IsAvailable(impl)) return false;
            else return TryUpdateImpl(name, (IKeyedConfigReadWriteImpl<Type>)impl);
        }

        public bool TrySetImpl(string name, IConfigReadWriteImpl impl)
        {
            if (!IsAvailable(impl)) return false;
            else return TrySetImpl(name, (IKeyedConfigReadWriteImpl<Type>)impl);
        }

        public bool TryRemoveImpl(string name, MaybeNull<IConfigReadWriteImpl> comparisonImpl, [NotNullWhen(true)] out IConfigReadWriteImpl? impl)
        {
            if (comparisonImpl.HasValue)
            {
                if (IsAvailable(comparisonImpl.Value) && 
                    TryRemoveImpl(name, 
                        new MaybeNull<IKeyedConfigReadWriteImpl<Type>>((IKeyedConfigReadWriteImpl<Type>)comparisonImpl.Value), 
                        out IKeyedConfigReadWriteImpl<Type>? exist))
                {
                    impl = exist;
                    return true;
                }
            }
            else
            {
                if (TryRemoveImpl(name,
                    MaybeNull<IKeyedConfigReadWriteImpl<Type>>.Null,
                    out IKeyedConfigReadWriteImpl<Type>? exist))
                {
                    impl = exist;
                    return true;
                }
            }
            impl = default;
            return false;
        }

        #endregion


        #region 克隆
        public object Clone(object config)
        {
            return cloneObj(config)!;
        }
        /// <summary>
        /// 通过反射 + 属性拷贝实现的对象拷贝
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static object? cloneObj(object? obj)
        {
            if (obj == null) return null;
            Type type = obj.GetType();
            if (type.IsValueType || type == typeof(string))
            {
                return obj;
            }
            object? output = Activator.CreateInstance(type);
            if (output == null) return null;
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo property in properties)
            {
                if (!property.CanWrite) continue;
                if (property.PropertyType.IsArray)
                {
                    object? arraySourceObj = property.GetValue(obj);
                    if (arraySourceObj == null) continue;
                    Array arraySource = (Array)arraySourceObj;
                    var elementType = property.PropertyType.GetElementType();
                    if (elementType == null)
                    {
                        continue;
                    }
                    Array arrayDest = Array.CreateInstance(elementType, arraySource.Length);
                    for (int i = 0; i < arraySource.Length; i++)
                    {
                        object? itemSource = arraySource.GetValue(i);
                        arrayDest.SetValue(cloneObj(itemSource), i);
                    }
                    property.SetValue(output, arrayDest);
                }
                else if (property.PropertyType.IsList())
                {
                    object? listSourceObj = property.GetValue(obj);
                    if (listSourceObj == null) continue;
                    IList listSource = (IList)listSourceObj;
                    if (Activator.CreateInstance(property.PropertyType) is not IList listDest) continue;
                    for (int i = 0; i < listSource.Count; i++)
                    {
                        object? itemSource = listSource[i];
                        listDest.Add(cloneObj(itemSource));
                    }
                    property.SetValue(output, listDest);
                }
                else
                {
                    object? value = property.GetValue(obj);
                    if (value == null) continue;
                    property.SetValue(output, value);
                }
            }
            return output;
        }
        #endregion


        #region 缓存

        #region 状态
        public bool CacheProtecting { get; private set; } = false;

        public void SetCacheProtect(bool b)
        {
            CacheProtecting = b;
        }

        #endregion

        private readonly ConcurrentDictionary<Type, object> caches = [];

        public IEnumerable<Type> AllKeys => caches.Keys;

        public IEnumerable<KeyValuePair<Type, object>> AllCaches => caches;



        public bool IsLoaded(Type key)
        {
            return caches.ContainsKey(key);
        }

        public bool TryGet(Type key, [NotNullWhen(true)] out object? config)
        {
            return caches.TryGetValue(key, out config);
        }

        public bool TryAdd(Type key, object config)
        {
            return caches.TryAdd(key, config);
        }

        public bool TryUpdate(Type key, object config)
        {
            if (CacheProtecting) return false;
            if (!TryGet(key, out var exist))
            {
                return false;
            }
            else
            {
                if (exist == config) return true;
                return caches.TryUpdate(key, config, exist);
            }
        }

        public bool TryRemove(Type key, [NotNullWhen(true)] out object? config)
        {
            if (CacheProtecting)
            {
                config = null;
                return false;
            }
            return caches.TryRemove(key, out config);
        }

        public void ClearCaches()
        {
            if (CacheProtecting) return;
            caches.Clear();
        }



        #endregion
    }
}
