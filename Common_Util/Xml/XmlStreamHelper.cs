using Common_Util.Attributes.Xml;
using Common_Util.Data.Constraint;
using Common_Util.Extensions;
using Common_Util.IO;
using Common_Util.String;
using System;
using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using static System.Net.Mime.MediaTypeNames;

namespace Common_Util.Xml
{
    /// <summary>
    /// XML 流式读写的帮助类
    /// </summary>
    public static class XmlStreamHelper
    {
        static XmlStreamHelper()
        {
        }

        private const string ATTRIBUTE_NAME_ISNULL = nameof(Nullable<int>.HasValue);
        private const string ATTRIBUTE_NAME_EXTRA_TYPE = "Extra.Type";
        private const string ELEMENT_NAME_VALUE = nameof(Nullable<int>.Value);

        private const string EXTRA_ELEMENT_NAME_TEXT = "Text";
        private const string EXTRA_ELEMENT_NAME_BASE64_DATA = "Base64";
        private const string EXTRA_ELEMENT_NAME_KEY_VALUES = "Map";

        #region 写入

        /// <summary>
        /// 在写入器的当前位置, 写入一个表示传入对象的节点. 
        /// <para>官方库中的 XML Attribute 只是拿来复用了, 实际与官方用法会有出入</para>
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="obj"></param>
        /// <param name="nodeName">节点名字, 使用值: 传入值 = 传入null => 注解寻找相关信息 = 未能找到 => 类型名</param>
        /// <param name="writeElementTag">是否写入表示传入对象的元素标签, 将视情况取传入值或类型名等</param>
        public static void Write<T>(XmlWriter writer, T obj, string? nodeName = null, bool writeElementTag = true, ExtraPropertyElementCollection? extraProperty = null)
        {
            ArgumentNullException.ThrowIfNull(obj);
            Write(writer, obj.GetType(), obj, nodeName, writeElementTag, extraProperty);
        }

        /// <summary>
        /// 在写入器的当前位置, 写入一个表示传入对象的节点. 
        /// <para>官方库中的 XML Attribute 只是拿来复用了, 实际与官方用法会有出入</para>
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="obj"></param>
        /// <param name="nodeName">节点名字, 使用值: 传入值 = 传入null => 注解寻找相关信息 = 未能找到 => 类型名</param>
        /// <param name="writeElementTag">是否写入表示传入对象的元素标签, 将视情况取传入值或类型名等</param>
        public static void Write(XmlWriter writer, Type type, object? obj, string? nodeName = null, bool writeElementTag = true, ExtraPropertyElementCollection? extraProperty = null)
        {
            nodeName ??= getNodeName(type);

            if (writeElementTag)
            {
                writer.WriteStartElement(nodeName);
            }

            if (obj != null)
            {
                if (isTextNode(type))
                {
                    writer.WriteString(asString(obj));
                }
                else if (_特殊处理类型Dic.ContainsKey(type))
                {
                    _特殊处理类型Dic[type].writeFunc(writer, obj);
                }
                else if (type.IsNullable(out var targetType))
                {
                    bool hasValue = (bool)type.GetProperty(nameof(Nullable<int>.HasValue))!.GetValue(obj)!;
                    object? value = type.GetProperty(nameof(Nullable<int>.Value))!.GetValue(obj);

                    writer.WriteStartAttribute(ATTRIBUTE_NAME_ISNULL);
                    writer.WriteValue(hasValue);
                    writer.WriteEndAttribute();

                    Write(writer, targetType!, value, ELEMENT_NAME_VALUE, true);
                }
                else
                {
                    var defines = tidyClassXmlDefines(type);

                    if (writeElementTag)
                    {
                        foreach (PropertyInfo property in defines.Attributes.Keys)
                        {
                            string tagStr = defines.NodeNameToAttributesProperty.First(i => i.Value == property).Key;
                            object? value = property.GetValue(obj, null);
                            var attr = defines.Attributes[property];

                            writer.WriteStartAttribute(tagStr);
                            if (property.ExistCustomAttribute<XmlTextValueAttribute>((attr) =>
                            {
                                writeXmlTextValue(writer, value, attr);
                            })) { }
                            else
                            {
                                writer.WriteValue(value ?? string.Empty);
                            }

                            writer.WriteEndAttribute();
                        }
                    }

                    foreach (PropertyInfo property in defines.Elements.Keys)
                    {
                        string tagStr = defines.NodeNameToElementProperty.First(i => i.Value == property).Key;
                        object? value = property.GetValue(obj, null);
                        var attr = defines.Elements[property];

                        bool typeTag = !property.ExistCustomAttribute<XmlNoTypeTagAttribute>();

                        property.ExistCustomAttribute<XmlCommentAttribute>(attr =>
                        {
                            writer.WriteComment(attr.Text);
                        });

                        writer.WriteStartElement(tagStr);
                        if (value != null || property.PropertyType.IsNullable())
                        {
                            if (property.PropertyType.IsPrimitive)
                            {
                                writer.WriteValue(value!);
                            }
                            else if (property.ExistCustomAttribute<XmlTextValueAttribute>((attr) =>
                            {
                                writeXmlTextValue(writer, value, attr);
                            })) { }
                            else
                            {
                                if (property.PropertyType != typeof(string) 
                                    && property.PropertyType != typeof(byte[])
                                    && value is IEnumerable enumerable)
                                {
                                    foreach (var element in enumerable)
                                    {
                                        var temp = element ?? ValueHelper.DefaultValue(property.PropertyType);
                                        if (temp != null)
                                        {
                                            Write(writer, temp.GetType(), temp, writeElementTag: typeTag);
                                        }
                                    }
                                }
                                else
                                {
                                    value ??= ValueHelper.DefaultValue(property.PropertyType);
                                    Write(writer, property.PropertyType, value, writeElementTag: typeTag);
                                }
                            }
                        }
                        writer.WriteEndElement();
                    }

                    foreach (PropertyInfo property in defines.Arrays.Keys)
                    {
                        string tagStr = defines.NodeNameToArrayProperty.First(i => i.Value == property).Key;
                        object? value = property.GetValue(obj, null);
                        var attr = defines.Arrays[property];


                        property.ExistCustomAttribute<XmlCommentAttribute>(attr =>
                        {
                            writer.WriteComment(attr.Text);
                        });

                        writer.WriteStartElement(tagStr);

                        if (value != null)
                        {
                            IEnumerable enumerable = (IEnumerable)value;
                            foreach (var element in enumerable)
                            {
                                Write(writer, element.GetType(), element);
                            }
                        }

                        writer.WriteEndElement();
                    }
                }
            }

            if (extraProperty != null && extraProperty.Any())
            {
                var defines = tidyClassXmlDefines(type);
                foreach (string key in extraProperty.Keys)
                {
                    if (defines.NodeNameToElementProperty.ContainsKey(key) || defines.NodeNameToArrayProperty.ContainsKey(key))
                    {
                        throw new Exception($"额外属性元素出现与原有属性重复的键值! {key}");
                    }

                    var item = extraProperty[key];

                    writer.WriteStartElement(key);

                    switch (item.Type)
                    {
                        case ExtraPropertyElementDataType.Text:
                            {
                                Write(writer, typeof(string), item.Text, EXTRA_ELEMENT_NAME_TEXT, true);
                            }
                            break;
                        case ExtraPropertyElementDataType.StreamAsBase64Data:
                            {
                                writer.WriteStartElement(EXTRA_ELEMENT_NAME_BASE64_DATA);
                                if (item.Datas != null)
                                {
                                    byte[] buffer = new byte[1024];
                                    int i;
                                    while ((i = item.Datas.Read(buffer, 0, buffer.Length)) > 0)
                                    {
                                        writer.WriteBase64(buffer, 0, i);
                                    }
                                }
                                writer.WriteEndElement();
                            }
                            break;
                        case ExtraPropertyElementDataType.KeyValues:
                            {
                                writer.WriteStartElement(EXTRA_ELEMENT_NAME_KEY_VALUES);
                                if (item.KeyValues != null)
                                {
                                    foreach (var pair in item.KeyValues)
                                    {
                                        writer.WriteStartElement(pair.Key);
                                        if (pair.Value != null)
                                        {
                                            Write(writer, pair.Value.GetType(), pair.Value, null, true);
                                        }
                                        writer.WriteEndElement();
                                    }
                                }
                                writer.WriteEndElement();
                            }
                            break;
                    }

                    writer.WriteEndElement();
                }
            }

            if (writeElementTag)
            {
                writer.WriteEndElement();
            }
        }

        #endregion

        #region 读取

        /// <summary>
        /// 读取读取器当前所在节点层级, 根据传入参数, 找到第一个元素节点或读取当前节点, 根据节点名读取为对应类型的值
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="caseSensitive">节点名是否区分大小写</param>
        /// <param name="readCurrentNode">是否直接读取当前节点</param>
        /// <returns></returns>
        public static object? ReadValue(XmlReader reader, bool caseSensitive = true, bool readCurrentNode = true)
        {
            return _readValue(reader, out _, out _, caseSensitive, readCurrentNode);
        }
        private static object? _readValue(XmlReader reader, out bool readEnd, out int endDepth, bool caseSensitive = true, bool readCurrentNode = true)
        {
            /* readEnd: 读取时, 是否已经读取到当前节点的末尾
             * endDepth: 读取结束时的当前读取深度 (相对开始读取时)
             */
            readEnd = false;
            endDepth = 0;
            object? output = null;
            if (readCurrentNode)
            {

                // 读取当前节点
                if (reader.NodeType != XmlNodeType.Element) return null;

                string tagName = reader.Name;
                Type? matchType = getMappingType(tagName, caseSensitive);
                if (matchType != null)
                {
                    output = _readAs(reader, matchType);
                    readEnd = true;
                }
                return output;
            }
            else
            {
                // 找到第一个元素节点
                if (reader.NodeType == XmlNodeType.Element && reader.IsEmptyElement)
                {
                    return null;
                }
                int _tempDepth = 0;
                bool isStop = readToElementEndOrStop(reader, (nodeType, depth) =>
                {
                    if (depth == 1 && nodeType == XmlNodeType.Element)
                    {
                        string tagName = reader.Name;
                        Type? matchType = getMappingType(tagName, caseSensitive);
                        if (matchType != null)
                        {
                            output = _readAs(reader, matchType);
                        }
                        else
                        {
                            _tempDepth = depth;
                        }
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                });

                readEnd = !isStop;
                if (isStop)
                {
                    endDepth = _tempDepth;
                }
                else
                {
                    endDepth = 0;
                }
            }
            return output;
        }
        /// <summary>
        /// 读取读取器当前所在节点层级, 找到第一个节点名与传入节点名相符的元素, 将其读取为指定类型的对象
        /// <para>官方库中的 XML Attribute 只是拿来复用了, 实际与官方用法会有出入</para>
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="type"></param>
        /// <param name="nodeName">节点名字, 使用值: 传入值 = 传入null => 注解寻找相关信息 = 未能找到 => 类型名</param>
        /// <param name="existElementTag">是否存在标识传入对象的元素标签, 将视情况根据传入值或类型名等做查找</param>
        /// <param name="needReadToElementEnd">是否读取结束后, 继续读取到当前元素的终止位置</param>
        /// <param name="extraPropertyArgs">预期出现的额外属性, 此方法的实现仅支持非文本类型, 非特殊处理类型, 非可空类型(<see cref="Nullable{T}"/>)的类型</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static object? ReadAs(XmlReader reader, Type type, string? nodeName = null, bool existElementTag = true, bool needReadToElementEnd = false, ExtraPropertyElementReadSetting? extraPropertyArgs = null)
        {
            if (!type.HavePublicEmptyCtor() && !type.IsNullable())
            {
                throw new InvalidOperationException($"目标类型需要有公共无参构造函数, 目标类型: {type.Name}");
            }


            if (!existElementTag)
            {
                if (reader.NodeType != XmlNodeType.Element && reader.NodeType != XmlNodeType.Text)
                {
                    throw new InvalidOperationException($"当前节点不是 {XmlNodeType.Element} ");
                }
            }

            nodeName ??= getNodeName(type);

            if (existElementTag)
            {
                object? output = null;

                if (!reader.IsEmptyElement)
                {

                    bool isStop = readToElementEndOrStop(reader, (nodeType, depth) =>
                    {
                        if (nodeType == XmlNodeType.Element && reader.Name == nodeName)
                        {
                            output = _readAs(reader, type, extraPropertyArgs);
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    });
                    if (!isStop)
                    {
                        throw new Exception("读取到当前所在元素的结尾后, 仍没有找到节点名相符的节点");
                    }
                    if (needReadToElementEnd)
                    {
                        readToElementEnd(reader, null);
                    }
                }
                return output;
            }
            else
            {
                object? output = _readAs(reader, type, extraPropertyArgs);

                return output;
            }

        }
        private static object? _readAs(XmlReader reader, Type type, ExtraPropertyElementReadSetting? extraPropertyArgs = null)
        {
            bool isEmptyElement = reader.IsEmptyElement;

            if (isTextNode(type))
            {
                string foundStr = readXmlText(reader);

                return fromString(type, foundStr);
            }
            else if (_特殊处理类型Dic.TryGetValue(type, out var _特殊处理类型value))
            {
                return _特殊处理类型value.readFunc(reader);
            }
            else if (type.IsNullable(out var targetType))
            {
                bool hasValue = false;
                object? value = null;

                string? hasValueStr = reader.GetAttribute(ATTRIBUTE_NAME_ISNULL);
                if (hasValueStr != null && bool.TryParse(hasValueStr, out hasValue))
                {
                    readToElementEnd(reader, (nodeType, depth) =>
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element:
                                if (depth == 0)
                                {
                                    if (reader.Name == ELEMENT_NAME_VALUE)
                                    {
                                        value = ReadAs(reader, targetType!, needReadToElementEnd: true);
                                    }
                                }
                                break;
                        }
                    });
                }

                return value;
            }
            else
            {
                object output = Activator.CreateInstance(type) ?? throw new InvalidOperationException($"未能实例化对象! 目标类型: {type.Name}");

                _XmlDefines defines = tidyClassXmlDefines(type);

                Dictionary<string, string> attributes = [];

                for (int i = 0; i < reader.AttributeCount; i++)
                {
                    reader.MoveToAttribute(i);

                    attributes.Add(reader.Name, reader.Value);

                    if (defines.NodeNameToAttributesProperty.TryGetValue(reader.Name, out var property))
                    {

                        object? value = null;
                        if (property.ExistCustomAttribute<XmlTextValueAttribute>(attr =>
                        {
                            value = xmlTextAs(property.PropertyType, reader.Value, attr);
                        })) { }
                        else
                        {
                            value = fromString(property.PropertyType, reader.Value);
                        }

                        property.SetValue(output, value);
                    }
                }
                extraPropertyArgs?.appendAfterReadAttributes(attributes);

                if (!isEmptyElement)
                {
                    readToElementEnd(reader,
                        (nodeType, depth) =>
                        {
                            string elementTag = reader.Name;
                            switch (nodeType)
                            {
                                case XmlNodeType.Element:

                                    if (depth > 1)
                                    {
                                        // 忽略不是期望类型属性的子节点
                                    }
                                    else
                                    {
                                        PropertyInfo? property;
                                        if (defines.NodeNameToElementProperty.TryGetValue(elementTag, out property))
                                        {
                                            // Element 属性

                                            bool typeTag = !property.PropertyType.IsPrimitive && !property.ExistCustomAttribute<XmlNoTypeTagAttribute>();

                                            object? value = null;
                                            if (property.PropertyType.IsPrimitive)
                                            {
                                                value = ReadAs(reader, property.PropertyType, existElementTag: typeTag, needReadToElementEnd: true);
                                            }
                                            else if (property.ExistCustomAttribute<XmlTextValueAttribute>((attr) =>
                                            {
                                                value = xmlTextAs(property.PropertyType, readXmlText(reader), attr);
                                            })) { }
                                            else
                                            {
                                                value = ReadAs(reader, property.PropertyType, 
                                                    existElementTag: typeTag, needReadToElementEnd: true);

                                            }


                                            property.SetValue(output, value);
                                        }
                                        else if (defines.NodeNameToArrayProperty.TryGetValue(elementTag, out property))
                                        {
                                            // Array 属性

                                            var interfaces = property.PropertyType.GetInterfaces();
                                            var ilistInterface = interfaces.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>));

                                            IList? list = null;
                                            Type? itemType = null;

                                            if (property.PropertyType.IsArray)
                                            {
                                                itemType = property.PropertyType.GetElementType() ?? throw new InvalidOperationException($"数组类型的属性 {property.Name} 未能取得元素类型");

                                                Type listType = typeof(List<>).MakeGenericType(itemType);

                                                object value = Activator.CreateInstance(listType) ?? throw new InvalidOperationException($"为属性 {property.Name} 实例化列表对象失败");

                                                list = (IList)value;
                                            }
                                            else if (property.PropertyType == typeof(IEnumerable) 
                                                    || property.PropertyType == typeof(ICollection) 
                                                    || property.PropertyType == typeof(IList))
                                            {
                                                itemType = typeof(object);
                                                list = new List<object>();
                                            }
                                            else if (ilistInterface != null)
                                            {
                                                var gArgs = ilistInterface.GetGenericArguments();
                                                if (gArgs.Length == 0) throw new Exception($"属性 {property.Name} 未能取得实现的 IList<> 接口泛型参数");

                                                itemType = gArgs[0];

                                                object value;

                                                value = Activator.CreateInstance(property.PropertyType) ?? throw new InvalidOperationException($"为属性 {property.Name} 实例化对象失败");

                                                list = (IList)value;
                                            }
                                            else if (property.PropertyType.IsGenericType && property.PropertyType.DeclaringType == typeof(IEnumerable<>))
                                            {

                                                var gArgs = property.PropertyType.GetGenericArguments();
                                                if (gArgs.Length == 0) throw new Exception($"属性 {property.Name} 未能取得实现的 IEnumerable<> 接口泛型参数");

                                                itemType = gArgs[0];

                                                Type listType = typeof(List<>).MakeGenericType(itemType);

                                                object value = Activator.CreateInstance(listType) ?? throw new InvalidOperationException($"为属性 {property.Name} 实例化列表对象失败");

                                                list = (IList)value;
                                            }

                                            if (list != null && itemType != null)
                                            {
                                                readToElementEnd(reader, (_nodeType, _depth) =>
                                                {
                                                    switch (_nodeType)
                                                    {
                                                        case XmlNodeType.Element:
                                                            if (_depth > 1)
                                                            {
                                                                // 忽略不是期望类型属性的子节点
                                                            }
                                                            else
                                                            {
                                                                object? value = null;
                                                                if (itemType == typeof(object))
                                                                {
                                                                    value = ReadValue(reader, true, true);  // 尝试读取当前节点为数据时, 没读取成功的情况, 都是不会读取任何东西, 导致读取深度改变的
                                                                                                            // 所以不用使用读取到节点结束点的方法, 来校正深度
                                                                }
                                                                else if (reader.Name == getNodeName(itemType))
                                                                {
                                                                    value = ReadAs(reader, itemType, existElementTag: false);   // 元素标签已在这里被读取, 所以调用时传入 false
                                                                }
                                                                list.Add(value);
                                                            }
                                                            break;
                                                    }

                                                });
                                            }

                                            if (property.PropertyType.IsArray)
                                            {
                                                var arr = Array.CreateInstance(itemType!, list!.Count);

                                                for (int i = 0; i < list.Count; i++)
                                                {
                                                    arr.SetValue(list[i], i);
                                                }
                                                property.SetValue(output, arr);
                                            }
                                            else
                                            {
                                                property.SetValue(output, list);
                                            }
                                        }
                                        else if (extraPropertyArgs != null && extraPropertyArgs.Contains(elementTag))
                                        {
                                            _readExtraProperty(reader, elementTag, extraPropertyArgs);
                                        }
                                    }

                                    break;
                                case XmlNodeType.EndElement:
                                    break;
                            };
                        });
                }


                return output;
            }

        }

        private static void _readExtraProperty(XmlReader reader, string elementTag, ExtraPropertyElementReadSetting extraPropertyArgs)
        {
            readToElementEnd(reader,
                (_nodeType, _depth) =>
                {
                    bool leftElement = false;
                    string _elementTag = reader.Name;
                    if (_depth == 1 && _nodeType == XmlNodeType.Element)
                    {
                        switch (_elementTag)
                        {
                            case EXTRA_ELEMENT_NAME_TEXT:
                                if (extraPropertyArgs.ReadText != null)
                                {
                                    if (reader.IsEmptyElement)
                                    {
                                        extraPropertyArgs.ReadText(elementTag, null);
                                    }
                                    else
                                    {
                                        string text = reader.ReadString();
                                        extraPropertyArgs.ReadText(elementTag, text);
                                    }
                                }
                                break;
                            case EXTRA_ELEMENT_NAME_BASE64_DATA:
                                if (extraPropertyArgs.ReadStream != null)
                                {
                                    if (reader.IsEmptyElement)
                                    {
                                        var tempFile = TempFileHelper.NewTempFile();
                                        using (var stream = tempFile.OpenStream()) { }
                                        var dispose = extraPropertyArgs.ReadStream(elementTag, tempFile);
                                        if (dispose)
                                        {
                                            tempFile.Dispose();
                                        }
                                    }
                                    else
                                    {
                                        byte[] buffer = new byte[1024];
                                        int i = 0;
                                        var tempFile = TempFileHelper.NewTempFile();
                                        using (var stream = tempFile.OpenStream())
                                        {
                                            while ((i = reader.ReadElementContentAsBase64(buffer, 0, 1024)) > 0)
                                            {
                                                stream.Write(buffer, 0, i);
                                                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == EXTRA_ELEMENT_NAME_BASE64_DATA)
                                                {
                                                    leftElement = true;
                                                }
                                            }
                                        }
                                        var dispose = extraPropertyArgs.ReadStream(elementTag, tempFile);
                                        if (dispose)
                                        {
                                            tempFile.Dispose();
                                        }
                                    }
                                }
                                break;
                            case EXTRA_ELEMENT_NAME_KEY_VALUES:
                                if (extraPropertyArgs.ReadKeyValues != null)
                                {
                                    var dic = _readExtraPropeartyAsKeyValues(reader);
                                    extraPropertyArgs.ReadKeyValues(elementTag, dic);
                                }
                                break;
                        }
                    }
                    return leftElement;

                });
        }
        private static Dictionary<string, object?> _readExtraPropeartyAsKeyValues(XmlReader reader)
        {
            Dictionary<string, object?> output = [];
            if (!reader.IsEmptyElement)
            {
                readToElementEnd(reader,
                    (_nodeType, _depth) =>
                    {
                        string _elementTag = reader.Name;
                        if (_depth == 1 && _nodeType == XmlNodeType.Element)
                        {
                            var value = _readValue(reader, out bool readEnd, out int endDepth, true, false);
                            if (!readEnd)
                            {
                                // 如果没有读取结束, 读取到当前节点的末尾, 每深入一层, 增加一次读取到末尾
                                for (int i = 0; i <= endDepth; i++)
                                {
                                    readToElementEnd(reader, null);
                                }
                            }
                            output.Add(_elementTag, value);
                        }
                    });
            }
            return output;
        }

        /// <summary>
        /// 一直读取, 直到找到名字与传入参数相同的元素节点, 或读取到当前所在层级结束处
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="elementName">目标元素名</param>
        /// <param name="depth">查询深度, 如果不为 null, 则在查找时, 只查找相对调用时的深度与传入值相等的元素</param>
        /// <returns></returns>
        public static bool ReadUntilFindElementNode(XmlReader reader, string elementName, int? depth = null)
        {
            // 提前读取结束, 说明找到了想要查找的节点
            return readToElementEndOrStop(reader, (nodeType, _depth) =>
            {
                if (nodeType == XmlNodeType.Element
                    && (depth == null || depth == _depth) 
                    && reader.Name == elementName)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }, endWhenNone: true);
        }

        /// <summary>
        /// 尝试在当前元素节点上, 寻找属性名与输入值相等的属性, 将其值读取为指定类型, 如果没有找到, 则返回 false
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <param name="attributeName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TryReadAttribute<T>(XmlReader reader, string attributeName, out T? value)
        { 
            bool b = TryReadAttribute(reader, attributeName, typeof(T), out object? obj);
            value = obj == null ? default : (T)obj;
            return b;
        }
        /// <summary>
        /// 尝试在当前元素节点上, 寻找属性名与输入值相等的属性, 将其值读取为指定类型, 如果没有找到, 则返回 false
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="attributeName"></param>
        /// <param name="targetType"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TryReadAttribute(XmlReader reader, string attributeName, Type targetType, out object? value)
        {
            value = null;
            if (reader.NodeType != XmlNodeType.Element)
            {
                return false;   // 当前节点不是元素节点
            }
            if (!reader.HasAttributes)
            {
                return false;
            }
            bool found = false;
            for (int i = 0; i < reader.AttributeCount; i++)
            {
                reader.MoveToAttribute(i);
                if (reader.Name == attributeName)
                {
                    value = fromString(targetType, reader.Value);
                    found = true;
                    break;
                }
            }
            reader.MoveToAttribute(0);

            return found;
        }
        #endregion


        private static string getNodeName(Type type)
        {
            string? output = null;
            type.ExistCustomAttribute<XmlRootAttribute>(attr => { output = attr.ElementName; });
            if (output == null)
            {
                output = type.Name;
                if (type.IsArray)
                {
                    output = output.Replace("[]", "Array");
                }
                else if (type.IsGenericType)
                {
                    StringBuilder sb = new();

                    string gName = type.GetGenericTypeDefinition().Name;
                    sb.Append(gName.Substring(0, gName.IndexOf('`')));
                    foreach (var arg in type.GetGenericArguments())
                    {
                        sb.Append('_').Append(arg.Name);
                    }

                    output = sb.ToString();
                }
            }
            return output;
        }


        #region 纯文本节点

        private static bool isTextNode(Type type)
        {
            if (type.ExistCustomAttribute<XmlTextNodeAttribute>())
            {
                return true;
            }
            else
            {
                if (type.IsPrimitive) return true;
                if (type == typeof(string)) return true;
                else if (type == typeof(byte[])) return true;
                else if (type.IsAssignableTo(typeof(IStringConveying))) return true;
                else if (type.IsEnum) return true;
                else return false;
            }
        }

        private static string? asString(object obj)
        {
            if (obj is IStringConveying stringConveying)
            {
                return stringConveying.ConvertToString();
            }
            else if (obj is byte[] bs)
            {
                return Convert.ToBase64String(bs);
            }
            else
            {
                return obj.ToString();
            }
        }
        private static object? fromString(Type type, string str)
        {
            if (type.IsNullable(out var targetType))
            {
                if (str.IsEmpty()) return null;

                type = targetType!;
            }
            if (type.IsAssignableTo(typeof(IStringConveying)))
            {
                return StringConveyingHelper.FromString(type, str);
            }
            else if (type == typeof(byte[]))
            {
                return Convert.FromBase64String(str);
            }
            else 
            {
                return StringConvertHelper.Convert(str, type);
            }
        }
        #endregion

        #region 读写纯文本

        private static void writeXmlTextValue(XmlWriter writer, object? value, XmlTextValueAttribute attr)
        {
            if (value is DateTime dt)
            {
                writer.WriteString(dt.ToString(attr.DateTimeFormat));
            }
            else
            {
                writeXmlTextValue(writer, value);
            }
        }
        private static void writeXmlTextValue(XmlWriter writer, object? value)
        {
            if (value == null)
            {
                writer.WriteString(string.Empty);
            }
            else
            {
                writer.WriteString(asString(value));
            }
        }

        private static string readXmlText(XmlReader reader)
        {
            string foundStr = string.Empty;

            if (!reader.IsEmptyElement)
            {
                readToElementEnd(reader, (nodeType, depth) =>
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Text:
                            foundStr = reader.ReadString();
                            break;
                    }
                });
            }
            return foundStr;
        }
        private static object? xmlTextAs(Type type, string text, XmlTextValueAttribute attr)
        {
            if (type == typeof(DateTime))
            {
                if (attr.DateTimeFormat == null)
                {
                    return DateTime.Parse(text);
                }
                else
                {
                    return DateTime.ParseExact(text, attr.DateTimeFormat, null);
                }
            }
            else
            {
                return fromString(type, text);
            }
        }

        #endregion

        #region 读取流程方法

        //private static int testIndex = 0;
        private static void readToElementEnd(XmlReader reader, Func<XmlNodeType, int, bool>? action)
        {
            //int _testIndex = Interlocked.Increment(ref testIndex);
            //Console.WriteLine(_testIndex + "start");

            int depth = 0;
            bool end = false;
            bool leftElement = false;   // 记录 action 返回值, 即在 action 中, 是否离开了所在节点
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        //Console.Write($"{_testIndex} Element {reader.Name}");
                        depth++;
                        if (reader.IsEmptyElement)
                        {
                            //Console.WriteLine($"    Empty");
                        }
                        else
                        {
                            //Console.WriteLine($"    depth++ => {depth}");
                        }
                        break;
                }
                // Console.WriteLine($"{reader.NodeType} reader.Name  => {reader.Name}");
                if (action != null)
                {
                    leftElement = action.Invoke(reader.NodeType, depth);
                }
                if (leftElement || (reader.NodeType == XmlNodeType.Element && reader.IsEmptyElement)) 
                {
                    depth--;
                }
                switch (reader.NodeType)
                {
                    case XmlNodeType.EndElement:
                        //Console.Write($"{_testIndex} EndElement {reader.Name}");
                        if (depth == 0)
                        {
                            end = true;
                            //Console.WriteLine($"   End");
                        }
                        else
                        {
                            depth--;
                            //Console.WriteLine($"    depth-- => {depth}");
                        }
                        break;
                }
                if (end)
                {
                    break;
                }
            }

            if (!end)
            {
                throw new InvalidOperationException("未找到当前节点的结束标签");
            }

            // Console.WriteLine(_testIndex + "end");
        }
        //private static int testIndex = 0;
        private static void readToElementEnd(XmlReader reader, Action<XmlNodeType, int>? action)
        {
            readToElementEnd(reader, 
                action == null ? 
                null 
                :
                (nodeType, depth) =>
                {
                    action(nodeType, depth);
                    return false;
                });
        }
        /// <summary>
        /// 读取, 直到当前元素结束或提前停止
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="action">返回值: true => 提前停止</param>
        /// <param name="endWhenNone">如果读取到 <see cref="XmlNodeType.None"/>, 是否视为结束</param>
        /// <returns>true => 提前停止了</returns>
        /// <exception cref="InvalidOperationException"></exception>
        private static bool readToElementEndOrStop(XmlReader reader, Func<XmlNodeType, int, bool> action, bool endWhenNone = false)
        {
            int depth = 0;
            bool end = false;
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.IsEmptyElement)
                        {
                        }
                        else
                        {
                            depth++;
                        }
                        break;
                }
                bool stop = action.Invoke(reader.NodeType, depth);
                if (stop)
                {
                    return true;
                }
                switch (reader.NodeType)
                {
                    case XmlNodeType.EndElement:
                        if (depth == 0)
                        {
                            end = true;
                        }
                        else
                        {
                            depth--;
                        }
                        break;
                }
                if (end)
                {
                    break;
                }
            }

            if (!endWhenNone && !end)
            {
                throw new InvalidOperationException("未找到当前节点的结束标签");
            }
            return false;
        }

        #endregion

        #region 类名与元素名的映射 
        private static readonly Dictionary<Type, HashSet<string>> typeTagNames = new()
        {
            { typeof(int), ["Int", "Int32", "int", "Integer", "I"] },
            { typeof(short), ["Short", "Int16", "short" ] },
            { typeof(byte), ["Byte", "byte" ] },
            { typeof(long), ["Long", "Int64", "long" ] },
            { typeof(float), ["Float", "float", "F"] },
            { typeof(double), ["Double", "double", "D"] },
            { typeof(bool), ["Boolean", "bool", "b"] },
            { typeof(string), ["String", "str", "string", "S"] },
        }; 

        private static HashSet<string> getTypeTagNamesSet(Type type)
        {
            HashSet<string>? set;
            if (typeTagNames.TryGetValue(type, out set)) { }
            else
            {
                set = new();
                typeTagNames.Add(type, set);
            }
            return set;
        }

        private static Type? getMappingType(string tagName, bool caseSensitive)
        {
            if (caseSensitive)
            {
                return typeTagNames.FirstOrDefault(i => i.Value.Contains(tagName)).Key;
            }
            else
            {
                tagName = tagName.ToLower();
                return typeTagNames.FirstOrDefault(i => i.Value.Any(s => s.ToLower() == tagName)).Key;
            }
        }

        /// <summary>
        /// 添加类型与标签名的映射, 将在 <see cref="ReadValue(XmlReader, bool)"/> 中使用
        /// </summary>
        /// <param name="type"></param>
        /// <param name="names"></param>
        public static void AddTypeTagNameMapping(Type type, params string[] names)
        {
            var set = getTypeTagNamesSet(type);

            foreach (string name in names)
            {
                set.Add(name);
            }

        }
        /// <summary>
        /// 添加类型与标签名的映射, 将使用类型的对应的默认节点名作为映射值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void AddTypeTagNameMapping<T>()
        {
            AddTypeTagNameMapping(typeof(T), getNodeName(typeof(T)));
        }
        /// <summary>
        /// 添加类型与标签名的映射
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="names"></param>
        public static void AddTypeTagNameMapping<T>(params string[] names)
        {
            AddTypeTagNameMapping(typeof(T), names);
        }

        /// <summary>
        /// 移除类型与标签名的映射, 将在 <see cref="ReadValue(XmlReader, bool)"/> 中使用
        /// </summary>
        /// <param name="type"></param>
        /// <param name="names"></param>
        public static void RemoveTypeTagNameMapping(Type type, params string[] names)
        {
            var set = getTypeTagNamesSet(type);

            foreach (string name in names)
            {
                set.Remove(name);
            }
        }

        /// <summary>
        /// 移除类型与标签名的映射, 将从映射名字集合中, 移除类型的对应的默认节点名 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void RemoveTypeTagNameMapping<T>()
        {
            RemoveTypeTagNameMapping(typeof(T), getNodeName(typeof(T)));
        }
        /// <summary>
        /// 移除类型与标签名的映射
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="names"></param>
        public static void RemoveTypeTagNameMapping<T>(params string[] names)
        {
            RemoveTypeTagNameMapping(typeof(T), names);
        }
        #endregion

        #region XML相关声明整理
        private struct _XmlDefines()
        {
            public Dictionary<PropertyInfo, XmlAttributeAttribute> Attributes = [];
            public Dictionary<string, PropertyInfo> NodeNameToAttributesProperty = [];
            public Dictionary<PropertyInfo, XmlElementAttribute?> Elements = [];
            public Dictionary<string, PropertyInfo> NodeNameToElementProperty = [];
            public Dictionary<PropertyInfo, XmlArrayAttribute?> Arrays = [];
            public Dictionary<string, PropertyInfo> NodeNameToArrayProperty = [];
        }
        private static _XmlDefines tidyClassXmlDefines(Type type)
        {
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            _XmlDefines output = new();

            foreach (PropertyInfo property in properties)
            {
                if (property.ExistCustomAttribute<XmlIgnoreAttribute>()) continue;

                if (property.ExistCustomAttribute<XmlAttributeAttribute>((attr) =>
                {
                    output.Attributes.Add(property, attr);
                    output.NodeNameToAttributesProperty.Add(attr.AttributeName.WhenEmptyDefault(property.Name), property);
                })) continue;

                if (property.ExistCustomAttribute<XmlElementAttribute>((attr) =>
                {
                    output.Elements.Add(property, attr);
                    output.NodeNameToElementProperty.Add(attr.ElementName.WhenEmptyDefault(property.Name), property);
                })) continue;

                if (property.ExistCustomAttribute<XmlArrayAttribute>((attr) =>
                {
                    output.Arrays.Add(property, attr);
                    output.NodeNameToArrayProperty.Add(attr.ElementName.WhenEmptyDefault(property.Name), property);
                })) continue;

                if (property.PropertyType.IsAssignableTo(typeof(IList))
                    || property.PropertyType == typeof(IEnumerable)
                    || property.PropertyType == typeof(ICollection)
                    || (property.PropertyType.IsGenericType && property.PropertyType.DeclaringType == typeof(IEnumerable<>)))
                {
                    output.Arrays.Add(property, null);
                    output.NodeNameToArrayProperty.Add(property.Name, property);
                    continue;
                }

                output.Elements.Add(property, null);
                output.NodeNameToElementProperty.Add(property.Name, property);
            }
            return output;
        }

        #endregion


        #region 特殊处理类型
        private const string DATETIME_DEFAULT_FORMAT = "yyyy-MM-dd HH:mm:ss:fff";
        private static Dictionary<Type, (Action<XmlWriter, object> writeFunc, Func<XmlReader, object?> readFunc)> _特殊处理类型Dic = new()
        {
            {
                typeof(DateTime),
                (
                    writeFunc: (writer, obj) =>
                    {
                        if (obj is DateTime dt)
                        {
                            writer.WriteStartAttribute(nameof(DateTime.Ticks));
                            writer.WriteValue(dt.Ticks);
                            writer.WriteEndAttribute();
                            writer.WriteString(dt.ToString(DATETIME_DEFAULT_FORMAT));
                        }
                    },
                    readFunc: (reader) =>
                    {
                        string? ticks = reader.GetAttribute(nameof(DateTime.Ticks));

                        readToElementEnd(reader, null);

                        if (long.TryParse(ticks, out long l))
                        {
                            return new DateTime(l);
                        }
                        else
                        {
                            return default(DateTime);
                        }
                    }
                )
            },
        };
        #endregion

        #region 额外属性元素

        /* 写: 额外属性元素将在其他属性写入完成后, 再作写入. 
         *     需传入 ExtraPropertyElementCollection, 且对象不能是任意集合类型, 传入集合中的元素不能出现名字与准备写入的对象中, 非 Attribute 的属性名字相同的项
         *     将遍历元素, 使用其 Key 值作为元素标签名, 并拥有一个名字为常量 ATTRIBUTE_NAME_EXTRA_TYPE 对应值的 Attribute, 标识该额外属性的类型
         * 
         * 读: 需传入可能出现的额外属性元素标签名, 
         *     准备读取的数据类型不能是任意集合类型, 传入的额外元素名, 不能出现与准备写入的对象中, 非 Attribute 的属性名字相同的项
         *     读取时如果遇到额外属性元素名, 将会尝试读取其数据类型, 再根据不同类型做不同读取方式, 如果未能获取到, 则抛出异常
         * 
         */

        /// <summary>
        /// <para>Key => 元素标签名</para>
        /// <para>Value => 元素数据</para>
        /// </summary>
        public class ExtraPropertyElementCollection : Dictionary<string, ExtraPropertyElementData>
        {
        }
        /// <summary>
        /// 额外属性元素读取设定
        /// <para>指示读取方法读取元素节点的过程中, 需要读取读取哪些节点名匹配的节点, 即相对于的处理操作</para>
        /// </summary>
        public class ExtraPropertyElementReadSetting : List<string>
        {
            public ExtraPropertyElementReadSetting()
            {

            }
            public ExtraPropertyElementReadSetting(IEnumerable<string> keys) : base(keys)
            {

            }
            
            public delegate IEnumerable<string> AppendExtraPropertyElementHandler(ReadOnlyDictionary<string, string> attributes);
            /// <summary>
            /// 在读取节点上的属性值结束后, 追加更多的额外属性元素的节点名
            /// </summary>
            public AppendExtraPropertyElementHandler? AppendAfterReadAttributes;

            internal void appendAfterReadAttributes(Dictionary<string, string> attributes)
            {
                IEnumerable<string>? keys = AppendAfterReadAttributes?.Invoke(attributes.AsReadOnly());
                if (keys != null)
                {
                    AddRange(keys);
                }
            }


            public delegate void TextHandler(string key, string? text);
            /// <summary>
            /// 读取到文本类型的额外属性时, 调用此处理方法
            /// </summary>
            public TextHandler? ReadText;

            /// <summary>
            /// 数据流处理方法的委托
            /// </summary>
            /// <param name="key"></param>
            /// <param name="tempfile"></param>
            /// <returns>是否是否释放临时文件, true => 释放</returns>
            public delegate bool StreamHandler(string key, TempFileHelper.TempFile tempfile);
            /// <summary>
            /// 读取到数据流类型的额外属性时, 调用此处理方法
            /// </summary>
            public StreamHandler? ReadStream;

            public delegate void KeyValuesHandler(string key, Dictionary<string, object?>? keyValues);
            /// <summary>
            /// 读取到键值对类型的额外属性时, 调用此处理方法
            /// </summary>
            public KeyValuesHandler? ReadKeyValues;
        }

        public enum ExtraPropertyElementDataType
        {
            /// <summary>
            /// 文本
            /// </summary>
            Text,
            /// <summary>
            /// 数据流 (记录为 base64编码)
            /// </summary>
            StreamAsBase64Data,
            /// <summary>
            /// 键值对 (将使用帮助类记录的映射关系来读写, 一个类型对应多个标签名时, 使用第一个标签名)
            /// </summary>
            KeyValues,
        }
        public class ExtraPropertyElementData
        {
            public ExtraPropertyElementDataType Type { get; set; }

            public string? Text { get; set; }

            public Stream? Datas { get; set; }
            
            public Dictionary<string, object?>? KeyValues { get; set; }


            #region 隐式转换
            public static implicit operator ExtraPropertyElementData(string text)
            {
                return new ExtraPropertyElementData()
                {
                    Type = ExtraPropertyElementDataType.Text,
                    Text = text,
                };
            }
            public static implicit operator ExtraPropertyElementData(Stream stream)
            {
                return new ExtraPropertyElementData()
                {
                    Type = ExtraPropertyElementDataType.StreamAsBase64Data,
                    Datas = stream,
                };
            }
            public static implicit operator ExtraPropertyElementData(Dictionary<string, object?>? dic)
            {
                return new ExtraPropertyElementData()
                {
                    Type = ExtraPropertyElementDataType.KeyValues,
                    KeyValues = dic,
                };
            }
            #endregion
        }

        #endregion
    }

}