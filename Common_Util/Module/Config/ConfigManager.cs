using Common_Util.Extensions;
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


        public bool TryAddImpl(string name, IKeyedConfigReadWriteImpl<Type> impl)
        {
            name ??= string.Empty;
            return impls.TryAdd(name, impl);
        }

        public bool TryGetImpl(string name, [NotNullWhen(true)] out IKeyedConfigReadWriteImpl<Type>? impl)
        {
            name ??= string.Empty;
            return impls.TryGetValue(name, out impl);
        }

        public bool TryRemoveImpl(string name, [NotNullWhen(true)] out IKeyedConfigReadWriteImpl<Type>? impl)
        {
            name ??= string.Empty;
            return impls.TryRemove(name, out impl);
        }

        public bool TryUpdateImpl(string name, IKeyedConfigReadWriteImpl<Type> impl)
        {
            name ??= string.Empty;
            if (!TryGetImpl(name, out var exist))
            {
                return false;
            }
            else
            {
                if (exist == impl) return true;
                return impls.TryUpdate(name, impl, exist);
            }
        }

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

        IConfigReadWriteImpl IConfigManager.DefaultImpl => DefaultImpl;

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
