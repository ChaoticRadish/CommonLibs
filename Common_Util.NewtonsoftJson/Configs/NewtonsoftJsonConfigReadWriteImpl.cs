using Common_Util.Attributes.General;
using Common_Util.Interfaces;
using Common_Util.Module;
using Common_Util.Extensions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Common_Util.Data.Constraint;
using Common_Util.Module.Config;
using System.Diagnostics.CodeAnalysis;

namespace Common_Util.Module.Config
{
    /// <summary>
    /// 使用 <see cref="Newtonsoft.Json.JsonConvert"/> 实现的配置读写实现
    /// </summary>
    public class NewtonsoftJsonConfigReadWriteImpl : TypeKeyedConfigReadWriteImplBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isFile">true: 指定文件; false: 指定文件夹</param>
        public NewtonsoftJsonConfigReadWriteImpl(string path, bool isFile = false)
        {
            SavePath = path;
            PathIsFile = isFile;

            if (isFile)
            {
                FileInfo file = new FileInfo(path);
                if (file.Directory == null)
                {
                    throw new Exception("未能取得文件的文件夹信息! ");
                }
                if (!file.Directory.Exists)
                {
                    file.Directory.Create();
                }
            }
            else
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                if (!dir.Exists)
                {
                    dir.Create();
                }
            }
        }

        #region 文件
        public bool PathIsFile { get; }

        /// <summary>
        /// 配置数据保存路径
        /// </summary>
        public string SavePath { get; }

        //private const string DebugFileExtension = "debug.json";

        /// <summary>
        /// 生成Debug模式下专用配置文件路径
        /// </summary>
        /// <param name="defaultPath"></param>
        /// <returns></returns>
        private string CreateDebugFilePath(FileInfo defaultFile)
        {
            return Path.ChangeExtension(defaultFile.FullName, "debug" + defaultFile.Extension);
        }
        private string CreateDebugFilePath(string defaultFile) 
        {
            return CreateDebugFilePath(new FileInfo(defaultFile));
        }
        #endregion


        #region 读

        public override bool TryLoadConfig(Type key, [NotNullWhen(true)] out object? config)
        {
            Type t = key;
            object output = CreateInstance(t);

            #region 使用文件
            string path = PathIsFile ? SavePath : $"{SavePath}/{t.Name}.json";
            #endregion

            #region 读取
            Dictionary<string, JToken?> values = new Dictionary<string, JToken?>();

            JsonLoadSettings loadSetting = new JsonLoadSettings()
            {
                CommentHandling = CommentHandling.Ignore,
            };

            if (File.Exists(path))
            {
                try
                {
                    string jsonStr = File.ReadAllText(path);
                    JObject jobj = JObject.Parse(jsonStr, loadSetting);
                    foreach (var item in jobj)
                    {
                        values.Add(item.Key, item.Value);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"读取文件 {path} 发生异常", ex);
                }
            }

#if DEBUG
            string debugFile = CreateDebugFilePath(path);
            if (File.Exists(debugFile))
            {
                try
                {
                    string jsonStr = File.ReadAllText(debugFile);
                    JObject jobj = JObject.Parse(jsonStr, loadSetting);
                    foreach (var item in jobj)
                    {
                        if (values.ContainsKey(item.Key))
                        {
                            values[item.Key] = item.Value;
                        }
                        else
                        {
                            values.Add(item.Key, item.Value);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"读取文件 {path} 发生异常", ex);
                }
            }
#endif

            #endregion

            #region 赋值
            PropertyInfo[] properties = t.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (property.SetMethod == null) { continue; }
                object? value = null;
                if (values.TryGetValue(property.Name, out JToken? jToken))
                {
                    if (jToken != null)
                    {
                        value = Convert(jToken, property.PropertyType);
                    }
                }
                if (value == null)
                {
                    // 取特性的默认值
                    property.ExistCustomAttribute<DefaultValueAttribute>((att) =>
                    {
                        value = ConfigStringHelper.ConfigValue2Obj(att.ValueString, property.PropertyType);
                    });
                }
                if (value == null) { continue; }

                property.SetValue(
                    output, value);

            }
            #endregion

            config = output;
            return true;
        }

        private object? Convert(JToken jToken, Type targetType)
        {
            MethodInfo method = GetType().GetMethod(nameof(ConvertImpl), BindingFlags.NonPublic | BindingFlags.Instance)!;
            method = method.MakeGenericMethod(targetType);
            return method.Invoke(this, [jToken]);
        }
        private object? ConvertImpl<T>(JToken jToken)
        {
            Type type = typeof(T);
            switch (jToken.Type)
            {
                case JTokenType.Null:
                    return null;
                case JTokenType.Object:
                case JTokenType.Array:
                    return JsonConvert.DeserializeObject<T>(jToken.ToString());
                default:
                    if (needConvertToString(type))
                    {
                        string? str = jToken.Value<string>();
                        return ConfigStringHelper.ConfigValue2Obj(str, type);
                    }
                    else
                    {
                        return jToken.Value<T>();
                    }
            }
        }

        #endregion

        #region 写
        /// <summary>
        /// 初始化输入类型的配置文件
        /// </summary>
        /// <param name="types">准备初始化的配置类型, 需要符合 new() 约束</param>
        public void InitConfig(params Type[] types)
        {
            Type thisType = GetType();
            MethodInfo method = thisType.GetMethod(nameof(InitConfig), Type.EmptyTypes)!;

            foreach (Type type in types)
            {
                if (!IsAvailable(type)) continue;

                object config = CreateInstance(type);
                _saveConfig(
                    type,
                    config,
                    (property) =>
                    {
                        object? obj = null;
                        // 取特性的默认值
                        property.ExistCustomAttribute<DefaultValueAttribute>((att) =>
                        {
                            obj = ConfigStringHelper.ConfigValue2Obj(att.ValueString, property.PropertyType);
                        });
                        if (obj == null)
                        {
                            obj = property.GetValue(config);
                        }
                        if (obj == null && property.PropertyType == typeof(string))
                        {
                            obj = string.Empty;
                        }
                        return obj;
                    },
                    File.Exists);
            }
        }

        /// <summary>
        /// 初始化配置文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void InitConfig<T>() where T : new()
        {
            InitConfig(typeof(T));
        }

        public override bool SaveConfig(Type key, object config)
        {
            _saveConfig(
                key,
                config,
                (property) =>
                {
                    return property.GetValue(config);
                },
                (path) =>
                {
                    return false;
                });
            return true;
        }

        /// <summary>
        /// 保存输入类型的配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="config"></param>
        /// <param name="getValueFunc">获取属性对应值的方法</param>
        /// <param name="needCancelFunc">判断是否需要取消保存的方法, 参数为准备保存的路径</param>
        private void _saveConfig(Type key, object config, Func<PropertyInfo, object?> getValueFunc, Func<string, bool> needCancelFunc) 
        {
            Type t = key;


            string path = PathIsFile ? SavePath : $"{SavePath}/{t.Name}.json";
#if DEBUG
            path = CreateDebugFilePath(path);
#endif
            if (needCancelFunc.Invoke(path))
            {
                return;
            }

            // 获取配置值
            JObject obj = new JObject();
            PropertyInfo[] properties = t.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                // object value = property.GetValue(config);
                object? value = getValueFunc(property);
                //if (value == null) { continue; }
                //obj.Add(property.Name, JToken.FromObject(value));
                if (value == null)
                {
                    obj.Add(property.Name, null);
                }
                else if (needConvertToString(property.PropertyType))
                {
                    obj.Add(property.Name, JToken.FromObject(ConfigStringHelper.Obj2ConfigValue(value) ?? string.Empty));
                }
                else 
                {
                    obj.Add(property.Name, JToken.FromObject(value));
                }
            }

            using System.IO.FileStream fs = new(path, FileMode.Create);
            using System.IO.StreamWriter sw = new(fs);
            //string str = Newtonsoft.Json.JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings()
            //{
            //    NullValueHandling = NullValueHandling.Include
            //});
            string str = obj.ToString(Newtonsoft.Json.Formatting.Indented);
            sw.Write(str);
        }

        private bool needConvertToString(Type type)
        {
            return type.IsEnum || type == typeof(Type) || StringConveyingHelper.ConvertibleCheck(type);
        }

        #endregion


    }
}
