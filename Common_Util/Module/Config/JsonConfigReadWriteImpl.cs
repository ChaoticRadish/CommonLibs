using Common_Util.Attributes.General;
using Common_Util.Data.Constraint;
using Common_Util.Extensions;
using Common_Util.String;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Common_Util.Module.Config
{
    /// <summary>
    /// 使用 <see cref="System.Text.Json"/> 实现的配置读写实现
    /// </summary>
    public class JsonConfigReadWriteImpl : IConfigReadWriteImpl
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isFile">true: 指定文件; false: 指定文件夹</param>
        public JsonConfigReadWriteImpl(string path, bool isFile = false)
        {
            SavePath = path;
            PathIsFile = isFile;

            if (isFile)
            {
                FileInfo file = new FileInfo(path);
                if (file.Directory == null)
                {
                    throw new Exception("未能找到文件对应的文件夹");
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

        public T GetConfig<T>() where T : new()
        {
            Type t = typeof(T);
            T output = new T();

            #region 使用文件
            string path = PathIsFile ? SavePath : $"{SavePath}/{t.Name}.json";
            #endregion

            #region 读取
            Dictionary<string, JsonElement> values = new Dictionary<string, JsonElement>();

            JsonDocumentOptions docOptions = new JsonDocumentOptions()
            {
                CommentHandling = JsonCommentHandling.Skip,
            };

            if (File.Exists(path))
            {
                try
                {
                    string jsonStr = File.ReadAllText(path);
                    JsonDocument doc = JsonDocument.Parse(jsonStr, docOptions);
                    foreach (var item in doc.RootElement.EnumerateObject())
                    {
                        values.Add(item.Name, item.Value);
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
                    JsonDocument doc = JsonDocument.Parse(jsonStr, docOptions);
                    foreach (var item in doc.RootElement.EnumerateObject())
                    {
                        values.Set(item.Name, item.Value);
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
                if (values.ContainsKey(property.Name))
                {
                    value = Convert(values[property.Name], property.PropertyType);
                }
                if (value == null)
                {
                    // 取特性的默认值
                    property.ExistCustomAttribute<DefaultValueAttribute>((att) =>
                    {
                        value = StringConvertHelper.Convert(att.ValueString, property.PropertyType);
                    });
                }
                if (value == null) { continue; }

                property.SetValue(
                    output, value);

            }
            #endregion

            return output;
        }

        private object? Convert(JsonElement jElement, Type targetType)
        {
            MethodInfo method = GetType().GetMethod(nameof(ConvertImpl), BindingFlags.NonPublic | BindingFlags.Instance)!;
            method = method.MakeGenericMethod(targetType);
            return method.Invoke(this, new object[] { jElement });
        }
        private object? ConvertImpl<T>(JsonElement jElement)
        {
            try
            {
                Type type = typeof(T);
                switch (jElement.ValueKind)
                {
                    case JsonValueKind.Object:
                        {
                            var obj = Activator.CreateInstance(type);
                            if (obj == null) return null;
                            PropertyInfo[] properties = type.GetProperties();
                            foreach (var item in jElement.EnumerateObject())
                            {
                                var property = properties.FirstOrDefault(i => i.Name == item.Name);
                                if (property == null || property.SetMethod == null) continue;
                                object? pValue = Convert(item.Value, property.PropertyType);
                                property.SetValue(obj, pValue);
                            }
                            return obj;
                        }
                    case JsonValueKind.Array:
                        //return String.JsonHelper.Deserialize<T>(jElement.ToString());
                        if (type.IsList())
                        {
                            var listObj = Activator.CreateInstance(type);
                            if (listObj == null) return null;
                            else
                            {
                                IList list = (IList)listObj;
                                Type itemType = type.GenericTypeArguments[0];
                                foreach (var item in jElement.EnumerateArray())
                                {
                                    list.Add(Convert(item, itemType));
                                }
                                return list;
                            }
                        }
                        else
                        {
                            Type itemType = type.GenericTypeArguments[0];
                            Array array = Array.CreateInstance(itemType, jElement.GetArrayLength());

                            int index = 0;
                            foreach (var item in jElement.EnumerateArray())
                            {
                                array.SetValue(Convert(item, itemType), index++);
                                index++;
                            }
                            return array;
                        }
                    case JsonValueKind.String:
                        if (needConvertToString(type))
                        {
                            string? str = jElement.GetString();
                            return ConfigStringHelper.ConfigValue2Obj(str, type);
                        }
                        else
                        {
                            return jElement.GetString();
                        }
                    case JsonValueKind.Number:
                        if (type.IsEnum)
                        {
                            string? str = jElement.GetString();
                            return str == null ? null : EnumHelper.Convert(type, str);
                        }
                        else
                        {
                            return StringConvertHelper.Convert(jElement.ToString(), type);
                        }
                    case JsonValueKind.True:
                        return true;
                    case JsonValueKind.False:
                        return false;
                    case JsonValueKind.Undefined:
                    case JsonValueKind.Null:
                        return null;
                    default:
                        {
                            string? str = jElement.GetString();
                            return StringConvertHelper.Convert(str, type);
                        }

                }
            }
            catch //(Exception ex)
            {
                throw;
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
                if (type.GetConstructor(Type.EmptyTypes) == null) continue;

                MethodInfo temp = method.MakeGenericMethod(type);
                temp.Invoke(this, new object[] { });
            }
        }

        /// <summary>
        /// 初始化配置文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void InitConfig<T>() where T : new()
        {
            T config = new T();
            _saveConfig(
                config,
                (property) =>
                {
                    object? obj = null;
                    // 取特性的默认值
                    property.ExistCustomAttribute<DefaultValueAttribute>((att) =>
                    {
                        obj = ConfigStringHelper.ConfigValue2Obj(att.ValueString, property.PropertyType);
                    });

                    obj ??= property.GetValue(config);
                    if (obj == null && property.PropertyType == typeof(string))
                    {
                        obj = "";
                    }
                    return obj;
                },
                (path) =>
                {
                    return File.Exists(path);
                });
        }

        public void SaveConfig<T>(T config) where T : new()
        {
            _saveConfig(
                config,
                (property) =>
                {
                    return property.GetValue(config);
                },
                (path) =>
                {
                    return false;
                });

        }

        /// <summary>
        /// 保存输入类型的配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="config"></param>
        /// <param name="getValueFunc">获取属性对应值的方法</param>
        /// <param name="needCancelFunc">判断是否需要取消保存的方法, 参数为准备保存的路径</param>
        private void _saveConfig<T>(T config, Func<PropertyInfo, object?> getValueFunc, Func<string, bool> needCancelFunc) where T : new()
        {
            Type t = typeof(T);


            string path = PathIsFile ? SavePath : $"{SavePath}/{t.Name}.json";
#if DEBUG
            path = CreateDebugFilePath(path);
#endif
            if (needCancelFunc.Invoke(path))
            {
                return;
            }

            // 获取配置值
            JsonNodeOptions nodeOptions = new JsonNodeOptions()
            {
                PropertyNameCaseInsensitive = false,
            };
            JsonObject root = new JsonObject(nodeOptions);

            PropertyInfo[] properties = t.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                // object value = property.GetValue(config);
                object? value = getValueFunc(property);
                //if (value == null) { continue; }
                //obj.Add(property.Name, JToken.FromObject(value));
                if (value == null)
                {
                    root.Add(property.Name, null);
                }
                else
                {
                    root.Add(property.Name, _toJsonObject(property.PropertyType, value));
                }
            }

            using (System.IO.FileStream fs = new FileStream(path, FileMode.Create))
            //using (System.IO.StreamWriter sw = new StreamWriter(fs))
            using (Utf8JsonWriter writer = new Utf8JsonWriter(fs, new JsonWriterOptions()
            {
                Indented = true,
            })) 

            root.WriteTo(writer, new JsonSerializerOptions()
            {
                WriteIndented = true,
            });
            //string str = Newtonsoft.Json.JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings()
            //{
            //    NullValueHandling = NullValueHandling.Include
            //});
            //string str = obj.ToString(Newtonsoft.Json.Formatting.Indented);
            //sw.Write(str);
        }

        private JsonNode? _toJsonObject(Type type, object? value)
        {
            if (value == null)
            {
                return null;
            }
            else if (type == typeof(short))
            {
                return (JsonNode)(short)value;
            }
            else if (type == typeof(ushort))
            {
                return (JsonNode)(ushort)value;
            }
            else if (type == typeof(int))
            {
                return (JsonNode)(int)value;
            }
            else if (type == typeof(uint))
            {
                return (JsonNode)(uint)value;
            }
            else if (type == typeof(long))
            {
                return (JsonNode)(long)value;
            }
            else if (type == typeof(ulong))
            {
                return (JsonNode)(ulong)value;
            }
            else if (type == typeof(float))
            {
                return (JsonNode)(float)value;
            }
            else if (type == typeof(double))
            {
                return (JsonNode)(double)value;
            }
            else if (type == typeof(bool))
            {
                return (JsonNode)(bool)value;
            }
            else if (type == typeof(string))
            {
                return (string)value!;
            }
            else if (needConvertToString(type))
            {
                return ConfigStringHelper.Obj2ConfigValue(value);
            }
            else if (type.IsList() || type.IsArray)
            {
                var itemType = type.GetGenericArguments()[0];
                JsonArray obj = new JsonArray();
                IList list = (IList)value;
                foreach (var item in list)
                {
                    obj.Add(_toJsonObject(itemType, item));
                }
                return obj;
            }
            else
            {
                JsonObject obj = new JsonObject();
                PropertyInfo[] properties = type.GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    object? pValue = property.GetValue(value);
                    if (pValue == null)
                    {
                        obj.Add(property.Name, null);
                    }
                    else
                    {
                        obj.Add(property.Name, _toJsonObject(property.PropertyType, pValue));
                    }
                }
                return obj;
            }

        }


        private bool needConvertToString(Type type)
        {
            return type.IsEnum || type == typeof(Type) || typeof(IStringConveying).IsAssignableFrom(type);
        }

        #endregion


    }
}
