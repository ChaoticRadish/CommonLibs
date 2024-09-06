using Common_Util.Attributes.General;
using Common_Util.Extensions;
using Common_Util.Module.Config;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Module.Config
{
    /// <summary>
    /// 配置读写类的实现
    /// </summary>
    public interface IConfigReadWriteImpl
    {
        /// <summary>
        /// 获取输入类型的配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetConfig<T>() where T : new();
        /// <summary>
        /// 保存输入类型的配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="config">配置数据对象</param>
        void SaveConfig<T>(T config) where T : new();
    }

    /// <summary>
    /// 标注使用哪个配置读写实例的特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ConfigReadWriteImplAttribute : System.Attribute
    {
        public ConfigReadWriteImplAttribute(string name)
        {
            Name = name;
        }
        public ConfigReadWriteImplAttribute(object enumObj)
        {
            Name = enumObj.ToString() ?? string.Empty;
        }


        public string Name { get; }
    }

    /// <summary>
    /// 基于<see cref="ConfigurationManager"/>的配置帮助类, Debug模式下会优先读取DEBUG前缀的键值对
    /// </summary>
    public class ConfigurationManagerReadWriteImpl : IConfigReadWriteImpl
    {


        public T GetConfig<T>() where T : new()
        {
            Type t = typeof(T);
            T output = new T();

            PropertyInfo[] properties = t.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (property.SetMethod == null) continue;
                string? value = Get(t, property);
                if (value == null) continue;
                object? obj = ConfigStringHelper.ConfigValue2Obj(value, property.PropertyType);
                if (obj == null) continue;
                property.SetValue(output, obj);
            }
            return output;
        }

        public void SaveConfig<T>(T config) where T : new()
        {
            Type t = typeof(T);
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            PropertyInfo[] properties = t.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (property.SetMethod == null) continue;

                string key;
                string? value = ConfigStringHelper.Obj2ConfigValue(property.GetValue(config));
#if DEBUG
                key = GetNodeKey(t, property, true);
#else
                key = GetNodeKey(t, property, false);
#endif
                var element = configuration.AppSettings.Settings[key];
                if (element == null)
                {
                    configuration.AppSettings.Settings.Add(key, value);
                }
                else
                {
                    element.Value = value;
                }
            }
            configuration.Save();
            ConfigurationManager.RefreshSection("appSetting");
        }


        #region 读取
        /// <summary>
        /// 取得节点的键
        /// </summary>
        /// <param name="configType"></param>
        /// <param name="property"></param>
        /// <param name="debug">debug节点</param>
        /// <returns></returns>
        private static string GetNodeKey(Type configType, PropertyInfo property, bool debug = false)
        {
            return $"{(debug ? "DEBUG:" : "")}{configType.Name}.{property.Name}";
        }

        private string? Get(Type configType, PropertyInfo property)
        {
            string? output;

            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
#if DEBUG
            output = configuration.AppSettings.Settings[GetNodeKey(configType, property, true)]?.Value;
            output ??= configuration.AppSettings.Settings[GetNodeKey(configType, property, false)]?.Value;
#else
            output = configuration.AppSettings.Settings[GetNodeKey(configType, property, false)]?.Value;
#endif
            if (output == null)
            {
                property.ExistCustomAttribute<DefaultValueAttribute>((att) =>
                {
                    output = att.ValueString;
                });
            }
            return output;
        }
        #endregion
    }
}
