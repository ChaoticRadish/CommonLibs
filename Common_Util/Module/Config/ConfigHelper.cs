using Common_Util;
using Common_Util.Data.Constraint;
using Common_Util.Extensions;
using Common_Util.String;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Module.Config
{

    public static class ConfigHelper
    {
        #region 共享

        /// <summary>
        /// 全局共享的配置管理器, 未设置的情况下会返回 <see cref="Default"/>
        /// </summary>
        public static IConfigManager Shared 
        { 
            get => shared ?? Default;
            set => shared = value;
        }
        private static IConfigManager? shared;
        /// <summary>
        /// 默认的配置管理器
        /// </summary>
        public static IConfigManager Default => lazyDefault.Value;
        private readonly static Lazy<IConfigManager> lazyDefault = new(() => new ConfigManager());

        #endregion

        #region 静态调用共享的配置管理器, 随时允许调用
        /// <summary>
        /// 设置 <paramref name="impl"/> 为当前的默认读写实现
        /// </summary>
        /// <param name="impl"></param>
        public static void SetDefaultImpl(IConfigReadWriteImpl impl)
        {
            Shared.TrySetDefaultImpl(impl);
        }

        #endregion

        #region 静态调用共享的配置管理器, 仅在配置了基于类型或字符串的键值式配置管理器时可用

        #region 状态
        /// <summary>
        /// <see cref="Shared"/> 的配置管理器类型是否支持在帮助类的静态方法中使用
        /// </summary>
        private static bool SharedStaticCallingSupport
        {
            get
            {
                return Shared is ICachedConfigManager<Type>
                    || Shared is ICachedConfigManager<string>;
            }
        }
        /// <summary>
        /// 检查 <see cref="SharedStaticCallingSupport"/>, 如果不支持, 则抛出异常
        /// </summary>
        /// <exception cref="NotSupportedException"></exception>
        private static void CheckSharedStaticCallingSupport()
        {
            if (!SharedStaticCallingSupport) throw new NotSupportedException("当前共享配置读写管理器不支持帮助类静态调用")
            { 
                Data = {
                    ["Shared"] = Shared.GetType().FullName,
                } 
            };
        }
        /// <summary>
        /// 使已有的配置对象不能被从字典中移除, 只能载入新的配置对象到缓存的字典中
        /// </summary>
        /// <remarks>
        /// 如保存配置时, 将只会保存到实现对应的位置(比如文件, 数据库中), 而不会替换缓存字典中的对象 <br/>
        /// 实际上会访问与修改 <see cref="Shared"/> 的 <see cref="ICachedConfigManager.CacheProtecting"/> (当然前提是 <see cref="SharedStaticCallingSupport"/> )
        /// </remarks>
        public static bool OnlyAllowAdd
        {
            get
            {
                CheckSharedStaticCallingSupport();
                return Shared is ICachedConfigManager cachedCM && cachedCM.CacheProtecting;
            }
            set
            {
                CheckSharedStaticCallingSupport(); 
                if (Shared is ICachedConfigManager cachedCM)
                {
                    cachedCM.SetCacheProtect(value);
                }
            }
        }

        #endregion

        #region 缓存
        /// <summary>
        /// 清空配置对象的缓存
        /// </summary>
        public static void ClearCache()
        {
            CheckSharedStaticCallingSupport();

            if (Shared is ICachedConfigManager cachedConfigManager)
            {
                cachedConfigManager.ClearCaches();
            }
        }

        #endregion
        #region 读写实现
        private static ConcurrentDictionary<Type, string?> typeUseImplName = [];
        private static string? GetUseImplName(Type type)
        {
            return typeUseImplName.GetOrAdd(type, t =>
            {
                if (t.ExistCustomAttribute<ConfigReadWriteImplAttribute>(out var attr))
                {
                    return attr.Name;
                }
                else
                {
                    return null;
                }
            });
        }

        #endregion

        #region 读取配置

        /// <summary>
        /// 判断指定配置类是否已经被加载到内存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool IsLoaded<T>()
            where T : new()
        {
            CheckSharedStaticCallingSupport();

            Type type = typeof(T);
            if (Shared is ICachedConfigManager<Type> typeCM)
            {
                return typeCM.IsLoaded(type);
            }
            else if (Shared is ICachedConfigManager<string> stringCM)
            {
                return stringCM.IsLoaded(
                    type.FullName ?? throw new InvalidOperationException("无法取得类型的完整名称")
                    {
                        Data = { ["Type"] = type, }
                    });
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 读取一个配置信息对象, 使用类所设置的配置读写实现
        /// </summary>
        /// <param name="type"></param>
        /// <param name="getClone">获取克隆对象</param>
        public static object? GetConfig(Type type, bool getClone = false)
        {
            CheckSharedStaticCallingSupport();

            if (Shared is ICachedConfigManager<Type> typeCM)
            {
                return typeCM.Get(type, true, GetUseImplName(type), getClone);
            }
            else if (Shared is ICachedConfigManager<string> stringCM)
            {
                return stringCM.Get(
                    type.FullName ?? throw new InvalidOperationException("无法取得类型的完整名称")
                    {
                        Data = { ["Type"] = type, }
                    },
                    true, GetUseImplName(type), getClone);
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 读取一个配置信息对象, 使用类所设置的配置读写实现
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="getClone">获取克隆对象</param>
        /// <returns></returns>
        public static T GetConfig<T>(bool getClone = false)
            where T : new()
        {
            var output = GetConfig(typeof(T), getClone) ?? throw new InvalidOperationException("读取配置信息对象失败");
            return (T)output;
        }


        #endregion

        #region 保存
        /// <summary>
        /// 保存一个配置信息, 使用类所设置的配置读写实现
        /// </summary>
        /// <param name="type"></param>
        public static void SaveConfig(Type type, object config, string? implName = null)
        {
            ArgumentNullException.ThrowIfNull(config);
            CheckSharedStaticCallingSupport();

            implName ??= GetUseImplName(type);
            if (Shared is ICachedConfigManager<Type> typeCM)
            {
                if (!typeCM.Save(type, config, GetUseImplName(type)))
                {
                    goto Failure;
                }
            }
            else if (Shared is ICachedConfigManager<string> stringCM)
            {
                if (!stringCM.Save(
                    type.FullName ?? throw new InvalidOperationException("无法取得类型的完整名称")
                    {
                        Data = { ["Type"] = type, }
                    },
                    config, GetUseImplName(type)))
                {
                    goto Failure;
                }
            }
            return;
        Failure:
            throw new InvalidOperationException("保存配置信息失败")
            {
                Data = 
                {
                    ["Type"] = type,
                    ["Config"] = config,
                    ["UseImplName"] = implName,
                }
            };
        }
        /// <summary>
        /// 保存一个配置信息, 使用类所设置的配置读写实现
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static void SaveConfig<T>(T config)
            where T : new()
        {
            ArgumentNullException.ThrowIfNull(config);
            SaveConfig(typeof(T), config, null);
        }
        /// <summary>
        /// 使用指定实现保存配置信息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="config"></param>
        /// <param name="implName"></param>
        public static void SaveConfig<T>(T config, string implName)
            where T : new()
        {
            ArgumentNullException.ThrowIfNull(config);
            SaveConfig(typeof(T), config, implName);
        }
        /// <summary>
        /// 使用指定实现保存配置信息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="config"></param>
        /// <param name="implName"></param>
        public static void SaveConfig<T>(T config, Enum implName)
            where T : new()
        {
            SaveConfig(config, implName.ToString());
        }

        #endregion

        #endregion




    }
}
