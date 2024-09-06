using Common_Util.Attributes.General;
using Common_Util.Data.Constraint;
using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Odbc;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Common_Util.Module.Config
{
    /// <summary>
    /// 使用 <see cref="XmlDocument"/> 实现的配置读写实现 
    /// </summary>
    public class XmlConfigReadWriteImpl : IConfigReadWriteImpl
    {
        #region 常量
        private const string NODENAME_ROOT = "Root";
        private const string NODENAME_ITEM = "Item";
        private const string ATTRIBUTE_PROPERTYNAME = "Property";
        private const string ATTRIBUTE_NAME = "Name";
        private const string ATTRIBUTE_DESC = "Desc";

        private const string DEBUG_VALUE = "DebugValue";
        private const string RELEASE_VALUE = "Value";
        #endregion


        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isFile">true: 指定文件; false: 指定文件夹</param>
        public XmlConfigReadWriteImpl(string path, bool isFile = false)
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

        #endregion



        #region 读
        public T GetConfig<T>() where T : new()
        {
            Type t = typeof(T);
            T output = new();

            #region 使用文件
            string path = PathIsFile ? SavePath : $"{SavePath}/{t.Name}.xml";
            #endregion

            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(path);
            }
            catch
            {
                return output;
            }

            if (doc.DocumentElement == null) return output;

            if (doc.DocumentElement.Name == NODENAME_ROOT)
            {
                Dictionary<string, XmlNode> nodes = new Dictionary<string, XmlNode>();

                foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                {
                    if (node.NodeType != XmlNodeType.Element || node.Name != NODENAME_ITEM)
                    {
                        continue;
                    }
                    string? propertyName = node.Attributes?[ATTRIBUTE_PROPERTYNAME]?.Value;
                    if (!string.IsNullOrEmpty(propertyName))
                    {
                        nodes.Add(propertyName, node);
                    }
                }
                PropertyInfo[] properties = t.GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    object? obj = null;
                    if (nodes.TryGetValue(property.Name, out XmlNode? value))
                    {
                        obj = Parse(value, property.PropertyType);
                    }
                    if (obj == null)
                    {
                        // 取特性的默认值
                        property.ExistCustomAttribute<DefaultValueAttribute>((att) =>
                        {
                            obj = ConfigStringHelper.ConfigValue2Obj(att.ValueString, property.PropertyType);
                        });
                    }
                    if (obj == null) continue;
                    property.SetValue(output, obj);
                }
            }

            return output;
        }
        /// <summary>
        /// 将节点转换为指定类型的对象
        /// </summary>
        /// <param name="node"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        protected object? Parse(XmlNode node, Type type)
        {
            object? debug = null;
            object? release = null;
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType != XmlNodeType.Element)
                {
                    continue;
                }
                switch (child.Name)
                {
#if DEBUG
                    case DEBUG_VALUE:
                        debug = ParseValue(child, type);
                        break;
#endif
                    case RELEASE_VALUE:
                        release = ParseValue(child, type);
                        break;
                }
            }
            return debug ?? release;
        }
        protected object? ParseValue(XmlNode node, Type type)
        {
            //if (type.IsValueType || type == typeof(string) || type.IsEnum)
            //{ 
            //    return StringConverter.Convert(node.InnerText, type);
            //}
            using StringReader reader = new StringReader(node.InnerXml);
            try
            {
                if (needConvertToString(type))
                {
                    return ConfigStringHelper.ConfigValue2Obj(node.InnerText, type);
                }
                else
                {
                    return new XmlSerializer(type).Deserialize(reader);
                }
            }
            catch
            {
                return null;
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
                temp.Invoke(this, []);
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
                        obj = string.Empty;
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



        private void _saveConfig<T>(T config, Func<PropertyInfo, object?> getValueFunc, Func<string, bool> needCancelFunc) where T : new()
        {
            Type t = typeof(T);

            #region 使用文件
            string path = PathIsFile ? SavePath : $"{SavePath}/{t.Name}.xml";
            #endregion

            if (needCancelFunc.Invoke(path))
            {
                return;
            }

            XmlDocument doc = new XmlDocument();

            XmlNode? root = doc.AppendChild(doc.CreateElement(NODENAME_ROOT));
            if (root == null)
            {
                // 也许要抛出一下异常? 
                return;
            }

            PropertyInfo[] properties = t.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (property.SetMethod == null) continue;

                object? value = getValueFunc(property);
                if (value == null) continue;
                XmlNode node = CreateNode(doc, value, property.PropertyType);
                node.Attributes!.SetNamedItem(CreateAttribute(doc, ATTRIBUTE_PROPERTYNAME, property.Name));
                property.ExistCustomAttribute<NameDescAttribute>((attr) =>
                {
                    node.Attributes.SetNamedItem(CreateAttribute(doc, ATTRIBUTE_NAME, attr.Name));
                    node.AppendChild(CreateDescNode(doc, ATTRIBUTE_DESC, attr.Desc));
                });
                if (node == null) continue;
                root.AppendChild(node);
            }

            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    using (XmlWriter write = XmlWriter.Create(fs, new XmlWriterSettings()
                    {
                        Indent = true,
                        OmitXmlDeclaration = true,
                    }))
                    {
                        doc.Save(write);
                    }
                }
            }
            catch (Exception)
            {
                // 也许需要输出点日志 或将其抛出? 
            }
        }



        protected XmlElement CreateNode(XmlDocument doc, object obj, Type targetType)
        {
            XmlElement node = doc.CreateElement(NODENAME_ITEM, null);
#if DEBUG
            node.AppendChild(CreateValueNode(doc, obj, targetType, DEBUG_VALUE));
#else
            node.AppendChild(CreateValueNode(doc, obj, targetType, RELEASE_VALUE));
#endif

            return node;
        }
        protected XmlElement CreateValueNode(XmlDocument doc, object obj, Type targetType, string nodeName)
        {
            Type type = targetType;
            XmlElement output;
            if (needConvertToString(type)) 
            {
                output = CreateNormalValueNode(doc, ConfigStringHelper.Obj2ConfigValue(obj) ?? string.Empty, nodeName);
            }
            else
            {
                output = CreateNormalValueNode(doc, obj, nodeName);
            }
            return output;
        }

        protected XmlElement CreateNormalValueNode(XmlDocument doc, object obj, string nodeName)
        {
            XmlSerializer serializer = new XmlSerializer(obj.GetType());
            XmlElement valueNode = doc.CreateElement(nodeName);
            StringBuilder stringBuilder = new StringBuilder();
            using (StringWriter sw = new StringWriter(stringBuilder))
            {
                using (XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings()
                {
                    OmitXmlDeclaration = true,
                    //Indent = true,
                }))
                {
                    XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
                    namespaces.Add(string.Empty, string.Empty);

                    serializer.Serialize(writer, obj, namespaces); ;
                    // stringBuilder.AppendLine();
                    // valueNode.InnerXml = stringBuilder.ToString();
                    // valueNode.AppendChild(doc.CreateTextNode(stringBuilder.ToString()));
                    using (StringReader sr = new StringReader(stringBuilder.ToString()))
                    {
                        using (XmlReader reader = XmlReader.Create(sr))
                        {
                            var node = doc.ReadNode(reader);
                            if (node != null)
                            {
                                valueNode.AppendChild(node);

                            }
                        }
                    }
                }
            }
            return valueNode;
        }
        protected XmlElement CreateStringValueNode(XmlDocument doc, string str, string nodeName)
        {
            XmlElement valueNode = doc.CreateElement(nodeName);
            valueNode.InnerText = str;
            return valueNode;
        }

        protected XmlAttribute CreateAttribute(XmlDocument doc, string key, string value)
        {
            XmlAttribute node = doc.CreateAttribute(key);
            node.Value = value;
            return node;
        }
        protected XmlElement CreateDescNode(XmlDocument doc, string key, string value)
        {
            XmlElement node = doc.CreateElement(key);
            if (!string.IsNullOrEmpty(value))
            {
                node.InnerText = value;
            }
            return node;
        }
        #endregion

        private bool needConvertToString(Type type)
        {
            return type.IsEnum || type == typeof(Type) || typeof(IStringConveying).IsAssignableFrom(type);
        }


    }
}
