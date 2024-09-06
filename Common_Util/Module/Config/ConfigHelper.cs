using Common_Util;
using Common_Util.Data.Constraint;
using Common_Util.Extensions;
using Common_Util.String;
using System;
using System.Collections;
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
        #region 实现方式
        /// <summary>
        /// 配置读写实现列表
        /// </summary>
        private static Dictionary<string, IConfigReadWriteImpl> Impls = [];

        /// <summary>
        /// 默认的配置读写实现
        /// </summary>
        public static IConfigReadWriteImpl DefaultImpl { get; private set; } = new ConfigurationManagerReadWriteImpl();

        /// <summary>
        /// 设置默认的配置读写实现
        /// </summary>
        /// <param name="impl"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void SetDefaultImpl(IConfigReadWriteImpl impl)
        {
            if (impl == null) throw new ArgumentNullException(nameof(impl));
            DefaultImpl = impl;
        }

        /// <summary>
        /// 配置配置读写的实现
        /// </summary>
        /// <param name="name"></param>
        /// <param name="impl"></param>
        public static void SetImpl(string name, IConfigReadWriteImpl impl)
        {
            Impls.Set(name, impl);
        }
        /// <summary>
        /// 配置配置读写的实现, 使用枚举值作为名字
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumObj"></param>
        /// <param name="impl"></param>
        public static void SetImpl(Enum enumObj, IConfigReadWriteImpl impl)
        {
            Impls.Set(enumObj.ToString(), impl);
        }
        public static IConfigReadWriteImpl? GetImpl(string name)
        {
            if (Impls.TryGetValue(name, out IConfigReadWriteImpl? value))
            {
                return value;
            }
            else
            {
                return null;
            }
        }
        public static IConfigReadWriteImpl? GetImpl(Enum enumObj)
        {
            return GetImpl(enumObj.ToString());
        }


        #endregion

        #region 状态
        /// <summary>
        /// 使已有的配置对象不能被从字典中移除, 只能载入新的配置对象到缓存的字典中
        /// <para>如保存配置时, 将只会保存到实现对应的位置(比如文件, 数据库中), 而不会替换缓存字典中的对象</para>
        /// </summary>
        public static bool OnlyAllowAdd
        {
            get
            {
                return _onlyAddConfig;
            }
            set
            {
                _onlyAddConfig = value;
                if (_onlyAddConfig)
                {
                    ClearCloneCache();
                }
            }
        }
        private static bool _onlyAddConfig = false;

        #endregion

        /// <summary>
        /// 配置数据对象字典, 存放最后一次保存的配置
        /// </summary>
        private static Dictionary<Type, object> Configs = new Dictionary<Type, object>();
        /// <summary>
        /// 当前缓存的所有配置
        /// </summary>
        public static Type[] ConfigTypes { get => Configs.Keys.ToArray(); }

        private static readonly object GetConfigLocker = new object();

        /// <summary>
        /// 清空配置对象的缓存
        /// </summary>
        public static void ClearCache()
        {
            if (OnlyAllowAdd) return;
            Configs.Clear();
        }

        /// <summary>
        /// 读取一个配置信息对象, 使用类所设置的配置读写实现
        /// </summary>
        /// <param name="type"></param>
        /// <param name="getClone">获取克隆对象</param>
        public static object? GetConfig(Type type, bool getClone = false)
        {
            Type thisType = typeof(ConfigHelper);
            MethodInfo method = thisType.GetMethod(nameof(GetConfig), [typeof(bool)])!;
            MethodInfo temp = method.MakeGenericMethod(type);
            return temp.Invoke(null, [getClone]);
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
            lock (GetConfigLocker)
            {
                Type t = typeof(T);
                T output;
                if (Configs.TryGetValue(t, out object? value))
                {
                    output = (T)value;
                }
                else
                {
                    // 需要使用的配置读写实现
                    IConfigReadWriteImpl impl = DefaultImpl;
                    t.ExistCustomAttribute<ConfigReadWriteImplAttribute>((attr) =>
                    {
                        if (Impls.TryGetValue(attr.Name, out IConfigReadWriteImpl? value))
                        {
                            impl = value;
                        }
                    });
                    // 读取, 写入对象
                    output = impl.GetConfig<T>();

                    Configs.Add(t, output!);
                }

                if (OnlyAllowAdd || getClone)
                {
                    return Clone(output) ?? throw new Exception("克隆异常! 克隆配置信息对象得到null! ");
                }
                else
                {
                    return output;
                }
            }
        }

        #region 克隆配置对象
        /// <summary>
        /// 清空克隆缓存
        /// </summary>
        private static void ClearCloneCache()
        {
            //CloneCache_ConfigJson.Clear();
            // 暂不使用克隆缓存
        }
        //private static Dictionary<Type, string> CloneCache_ConfigJson = new Dictionary<Type, string>();

        /// <summary>
        /// 克隆配置文件的对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configObj"></param>
        /// <returns></returns>
        private static T? Clone<T>(T configObj)
            where T : new()
        {
            // 配置类型深拷贝应该这样子就够用了
            // 20230815 实测这样子效率会很低, 因为会频繁获取这个克隆, 简单优化一下
            //Type type = typeof(T);
            //string json;
            //if (CloneCache_ConfigJson.ContainsKey(type))
            //{
            //    json = CloneCache_ConfigJson[type];
            //}
            //else
            //{
            //    json = configObj.ToJson();
            //    CloneCache_ConfigJson.Add(type, json);
            //}
            //return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);

            object? obj = CloneObj(configObj);
            return obj == null ? default : (T)obj;
        }

        /// <summary>
        /// 简单实现深拷贝
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static object? CloneObj(object? obj)
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
                        arrayDest.SetValue(CloneObj(itemSource), i);
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
                        listDest.Add(CloneObj(itemSource));
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

        /// <summary>
        /// 判断指定配置类是否已经被加载到内存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool IsLoaded<T>()
            where T : new()
        {
            return Configs.ContainsKey(typeof(T));
        }
        /// <summary>
        /// 保存一个配置信息, 使用类所设置的配置读写实现
        /// </summary>
        /// <param name="type"></param>
        public static void SaveConfig(Type type, object config)
        {
            Type thisType = typeof(ConfigHelper);
            MethodInfo method = thisType.GetMethod(nameof(_saveConfig), BindingFlags.Static | BindingFlags.NonPublic)!;
            MethodInfo temp = method.MakeGenericMethod(type);
            temp.Invoke(null, [config]);
        }
        /// <summary>
        /// 保存一个配置信息, 使用类所设置的配置读写实现
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static void SaveConfig<T>(T config)
            where T : new()
        {
            _saveConfig(config);
        }
        private static void _saveConfig<T>(T config)
            where T : new()
        {
            SaveConfig(config, string.Empty);
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
            Type t = typeof(T);

            IConfigReadWriteImpl? impl = null;
            // 需要使用的配置读写实现
            if (string.IsNullOrEmpty(implName))
            {
                impl = DefaultImpl;
                t.ExistCustomAttribute<ConfigReadWriteImplAttribute>((attr) =>
                {
                    if (Impls.TryGetValue(attr.Name, out IConfigReadWriteImpl? value))
                    {
                        impl = value;
                    }
                });
            }
            else
            {
                if (Impls.TryGetValue(implName, out IConfigReadWriteImpl? value))
                {
                    impl = value;
                }
                if (impl == null)
                {
                    throw new ArgumentException("未找到指定的配置读写实现", nameof(implName));
                }
            }

            SaveConfig(config, impl);
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
        /// <summary>
        /// 使用指定实现保存配置信息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="config"></param>
        /// <param name="impl"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void SaveConfig<T>(T config, IConfigReadWriteImpl impl)
            where T : new()
        {
            // 检查
            if (impl == null)
            {
                throw new ArgumentNullException(nameof(impl));
            }
            Type t = typeof(T);
            if (!OnlyAllowAdd)
            {
                // 移除 "最后保存配置" 字典中暂存的配置信息
                Configs.Remove(t);
            }
            // 保存
            impl.SaveConfig<T>(config);
        }




    }
}
