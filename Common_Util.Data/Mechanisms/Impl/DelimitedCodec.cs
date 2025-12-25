using Common_Util.Attributes.General;
using Common_Util.Data.Constraint;
using Common_Util.Data.Mechanisms.Extensions.Channel;
using Common_Util.Data.Struct;
using Common_Util.Extensions;
using Common_Util.Module.Reflection;
using Common_Util.String;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Common_Util.Data.Mechanisms.Impl
{
    /// <summary>
    /// 分隔符编解码器, 序列化后的数据仅包含属性数据, 适用于结构简单的数据类型
    /// </summary>
    /// <remarks>
    /// 序列化过程: 获取类型的带顺序标记 <see cref="SequenceFlagAttribute"/> 的可与字符串互转的可读可写属性, <br/> 
    /// 逐一转换为字符串拼接起来, 不同属性使用特定分隔符分隔 <br/>
    /// 处理的属性类型必须满足其中之一:  <br/>
    /// 1. 基础类型或实现 <see cref="Common_Util.Data.Constraint.IStringConveying{TSelf}"/> <br/> 的类型
    /// 2. 集合元素符合条件 1. 的集合类型, 且集合类型是 <see cref="IEnumerable{T}"/>, <see cref="ICollection{T}"/>, <see cref="IList{T}"/> 或 <see cref="IList{T}"/> 具有公共无参构造方法的实现类型
    /// </remarks>
    /// <param name="delimitChar">分隔符, 需要是基础类型不会出现的字符</param>
    /// <param name="nullFlag">空对象标记, 允许任意值</param>
    /// <param name="notNullFlag">非空对象标记, 允许任意值</param>
    public sealed class DelimitedCodec(
        char delimitChar = DelimitedCodec.DEFAULT_DELIMIT_CHAR,
        char nullFlag = DelimitedCodec.DEFAULT_NULL_FLAG_CHAR,
        char notNullFlag = DelimitedCodec.DEFAULT_NOTNULL_FLAG_CHAR
        ) : ICodec<string>, IStreamCodec<char>
    {
        public static DelimitedCodec Shared { get; } = new();

        public const char DEFAULT_DELIMIT_CHAR = '|';
        public const char DEFAULT_NULL_FLAG_CHAR = '-';
        public const char DEFAULT_NOTNULL_FLAG_CHAR = '+';

        private readonly char usingDelimitChar = delimitChar;
        private readonly char[] usingDelimitCharArray = [delimitChar];
        private readonly char nullFlag = nullFlag;
        private readonly char notNullFlag = notNullFlag;
        private readonly static Dictionary<Type, TypeSerializeInfos> checkCaches = [];
        private readonly struct TypeSerializeInfos
        {
            /// <summary>
            /// 是否适用于此编解码器
            /// </summary>
            /// <param name="needInnerCreateInstance">是否需要在编解码器内部创建实例</param>
            public readonly bool ValidType(bool needInnerCreateInstance)
            {
                return (HasPublicEmptyCtor || !needInnerCreateInstance)
                    && Properties.Length > 0;
            }
            /// <summary>
            /// 类型是否具有空构造方法
            /// </summary>
            public bool HasPublicEmptyCtor { get; init; }
            /// <summary>
            /// 有效或允许使用的
            /// </summary>
            public PropertyDetailInfo[] Properties { get; init; }

            /// <summary>
            /// 不适用原因描述
            /// </summary>
            public string? InvalidReason { get; init; }
        }
        private readonly struct PropertyDetailInfo
        {
            /// <summary>
            /// 属性
            /// </summary>
            public PropertyInfo Property { get; init; }
            /// <summary>
            /// 类型详情
            /// </summary>
            public TypeDetail TypeDetail { get; init; }

            /// <summary>
            /// 是否集合, 只允许是 <see cref="IEnumerable{T}"/>, <see cref="ICollection{T}"/>, <see cref="IList{T}"/> 或 <see cref="IList{T}"/> 具有公共无参构造方法的实现类型
            /// </summary>
            [MemberNotNullWhen(true, nameof(CollectionElementType), nameof(CreateCollectionType))]
            public bool IsCollection { get; init; }
            /// <summary>
            /// 集合元素类型
            /// </summary>
            public TypeDetail? CollectionElementType { get; init; }
            /// <summary>
            /// 是否数组
            /// </summary>
            public bool IsArray { get; init; }
            /// <summary>
            /// 是否集合接口, 即 <see cref="IEnumerable{T}"/>, <see cref="ICollection{T}"/>, <see cref="IList{T}"/> 中的其中之一
            /// </summary>
            public bool IsCollectionInterface { get; init; }
            /// <summary>
            /// 是否列表接口具体的实现类型
            /// </summary>
            public bool IsListImplClass { get; init; }
            /// <summary>
            /// 序列化时需要创建的集合类型
            /// </summary>
            public Type? CreateCollectionType { get; init; }


        }
        private readonly struct TypeDetail
        {
            public readonly Type UnderlyingType { get; init; }
            public readonly bool CanBeNull { get; init; }
            public readonly TypeCategory Category { get; init; }
        }
        /// <summary>
        /// 类型的类别
        /// </summary>
        /// <remarks>
        /// 分为两大类: <br/>
        /// 1. 单一对象, 非负数的枚举值, 需要再细分类型; <br/>
        /// 2. 结构对象, 负数枚举值, 包含集合, 列表这些, 不取得细分类型, 统一归为 <see cref="StructureData"/>
        /// </remarks>
        private enum TypeCategory
        {
            StructureData = -1,

            ShortData = 0,
            String = 1, 
            StringConveying = 2,
        }


        #region 判断
        private static TypeSerializeInfos? _getTypeSerializeInfos(Type type) 
        {
            if (checkCaches.TryGetOrAdd(type, () => _checkValid(type), out var serializeInfos))
            {
                return serializeInfos;
            }
            else
            {
                return null;
            }
        }
        private static TypeSerializeInfos _checkValid(Type type)
        {
            string? invalidReason = null;
            bool hasPublicEmptyCtor = type.HavePublicEmptyCtor();
            var properties = MemberSequenceHelper.GetOrderedProperties(type)
                .Select(p => _canConvert(p.Member))
                .Where(p => p.canConvert)
                .Select(p => p.detail)
                .ToArray();

            if (!hasPublicEmptyCtor)
            {
                invalidReason = "没有公共无参构造方法";
            }
            else if (properties.Length == 0)
            {
                invalidReason = "没有有效的可转换属性";
            }
            return new()
            {
                HasPublicEmptyCtor = hasPublicEmptyCtor,
                Properties = properties,
                InvalidReason = invalidReason,
            };
        }
        /// <summary>
        /// 判断能否转换, 可以的话返回属性转换详情信息
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        private static (bool canConvert, PropertyDetailInfo detail) _canConvert(PropertyInfo property)
        {
            if (!property.CanRead || !property.CanWrite) return (false, default);

            TypeDetail propertyTypeDetail = _getTypeDetail(property.PropertyType);
            Type underlyingType = propertyTypeDetail.UnderlyingType;
            bool isCollection = false;
            TypeDetail? collectionElementType = null;
            Type? createdCollectionType = null;
            bool isArray = false;
            bool isCollectionInterface = false;
            bool isListImplClass = false;

            if (propertyTypeDetail.Category == TypeCategory.StructureData)
            {
                if (underlyingType.IsArray)
                {
                    var elementType = underlyingType.GetElementType();
                    if (elementType == null) return (false, default);
                    collectionElementType = _getTypeDetail(elementType);
                    if (collectionElementType.Value.Category == TypeCategory.StructureData) return (false, default);

                    isCollection = true;
                    isArray = true;
                    createdCollectionType = underlyingType;
                }
                else if (TypeHelper.ExistInterfaceIsDefinitionFrom(underlyingType, typeof(IList<>), out Type? firstMatchInterface)
                    && underlyingType.HavePublicEmptyCtor())
                {
                    var gArgs = firstMatchInterface.GetGenericArguments();
                    if (gArgs.Length != 1) return (false, default);
                    collectionElementType = _getTypeDetail(gArgs[0]);
                    if (collectionElementType.Value.Category == TypeCategory.StructureData) return (false, default);

                    isCollection = true;
                    isListImplClass = true;
                    createdCollectionType = underlyingType;
                }
                else if (TypeHelper.GenericTypeIsDefinitionFrom(underlyingType, typeof(IEnumerable<>))
                    || TypeHelper.GenericTypeIsDefinitionFrom(underlyingType, typeof(ICollection<>))
                    || TypeHelper.GenericTypeIsDefinitionFrom(underlyingType, typeof(IList<>)))
                {
                    var gArgs = underlyingType.GetGenericArguments();
                    if (gArgs.Length != 1) return (false, default);
                    collectionElementType = _getTypeDetail(gArgs[0]);
                    if (collectionElementType.Value.Category == TypeCategory.StructureData) return (false, default);

                    isCollection = true;
                    isCollectionInterface = true;
                    createdCollectionType = typeof(List<>).MakeGenericType(collectionElementType.Value.UnderlyingType);
                }
                else return (false, default);
            }
            
            return (true, new()
            {
                Property = property,
                TypeDetail = propertyTypeDetail,
                IsCollection = isCollection,
                CollectionElementType = collectionElementType,
                IsArray = isArray,
                IsCollectionInterface = isCollectionInterface,
                IsListImplClass = isListImplClass,
                CreateCollectionType = createdCollectionType,
            });
        }

        private static TypeDetail _getTypeDetail(Type type)
        {
            bool canBeNull = type.CanBeNull(out var underlyingType);
            TypeCategory category;
            if (underlyingType == typeof(string))
                category = TypeCategory.String;
            else if (TypeHelper.IsSimpleType(underlyingType))
                category = TypeCategory.ShortData;
            else if (StringConveyingHelper.ConvertibleCheck(underlyingType))
                category = TypeCategory.StringConveying;
            else 
                category = TypeCategory.StructureData;
            return new()
            {
                UnderlyingType = underlyingType,
                CanBeNull = canBeNull,
                Category = category,
            };
        }
        #endregion


        #region 实现

        #region 反序列化
        private OperationResult<object> Deserialize(Type type, IReadableChannel<char> channel)
        {
            var getInfos = _getTypeSerializeInfos(type);
            if (getInfos == null)
            {
                return "未能取得类型判断结果信息";
            }
            if (!getInfos.Value.ValidType(true))
            {
                return $"类型不适用于此编解码器: {getInfos.Value.InvalidReason}";
            }
            var infos = getInfos.Value;

            var payload = Activator.CreateInstance(type);
            if (payload == null) return "未能如预期创建负责数据实例";

            var reader = new CharSequenceReader(channel.AsEnumerable());

            StringBuilder buffer = new();
            foreach (var property in infos.Properties)
            {
                buffer.Clear();
                var handleResult = _deserializePropertyValue(property, reader, buffer);
                if (handleResult.IsFailure)
                    return handleResult;
                reader.ReadUntilNotIn(usingDelimitCharArray);

                var value = handleResult.Data;
                property.Property.SetValue(payload, value);
            }

            return (true, payload, "成功");
        }
        #region 属性处理的实现细节
        private OperationResult<object> _deserializePropertyValue(PropertyDetailInfo property, CharSequenceReader reader, StringBuilder buffer)
        {
            if (property.TypeDetail.CanBeNull)
            {
                var isNullResult = _deserializeTypeValue_IsNull(reader, buffer);
                if (isNullResult.IsFailure) return isNullResult.FailureReason("检查是否为空的标记失败");
                if (isNullResult.Data) return (true, null, "成功");
            }
            var propertyHandleResult = property.TypeDetail.Category switch
            {
                TypeCategory.ShortData => _deserializeTypeValue_ShortData(property.TypeDetail, reader, buffer),
                TypeCategory.String => _deserializeTypeValue_String(property.TypeDetail, reader, buffer),
                TypeCategory.StringConveying => _deserializeTypeValue_StringConveying(property.TypeDetail, reader, buffer),
                TypeCategory.StructureData => _deserializePropertyValue_StructureData(property, reader, buffer),
                _ => $"取得的类型处理分类 ({property.TypeDetail.Category}) 无效",
            };
            if (!propertyHandleResult)
                return propertyHandleResult.FailureReasonWithTitle($"属性 {property.Property.Name} 处理失败");

            return propertyHandleResult;
        }
        private OperationResult<object> _deserializeTypeValue_ShortData(TypeDetail typeDetail, CharSequenceReader reader, StringBuilder buffer)
        {
            buffer.Clear();
            reader.TryReadUntilAnyChar(usingDelimitCharArray, buffer, out _);
            var obj = _shortDataConvertFromString(typeDetail, buffer.ToString());
            return (true, obj, "成功");
        }
        private OperationResult<object> _deserializeTypeValue_String(TypeDetail typeDetail, CharSequenceReader reader, StringBuilder buffer)
        {
            var segmentResult = _deserializeTypeValue_LengthValueSegment(reader, buffer);
            if (segmentResult.IsFailure) return segmentResult.FailureReason("未知原因");
            return (true, segmentResult.Data.dataSegment, "成功");
        }
        private OperationResult<object> _deserializeTypeValue_StringConveying(TypeDetail typeDetail, CharSequenceReader reader, StringBuilder buffer)
        {
            var segmentResult = _deserializeTypeValue_LengthValueSegment(reader, buffer);
            if (segmentResult.IsFailure) return segmentResult.FailureReason("未知原因");
            string dataSegment = segmentResult.Data.dataSegment;
            var obj = StringConveyingHelper.FromString(typeDetail.UnderlyingType, dataSegment);
            return (true, obj, "成功");
        }
        private OperationResult<object> _deserializePropertyValue_StructureData(PropertyDetailInfo property, CharSequenceReader reader, StringBuilder buffer)
        {
            if (property.IsCollection)
            {
                var elementType = property.CollectionElementType.Value;

                var readCodeResult = _deserializeTypeValue_CountValue(reader, buffer);
                if (readCodeResult.IsFailure) return readCodeResult.FailureReason("未知错误");

                int count = readCodeResult.Data;

                Action<object?, int> addMethod;
                object output;
                if (property.IsArray)
                {
                    var array = Array.CreateInstance(elementType.UnderlyingType, count);
                    addMethod = array.SetValue;
                    output = array;
                }
                else
                {
                    var instance = Activator.CreateInstance(property.CreateCollectionType);
                    if (instance == null) return "创建集合类型实例失败";
                    var list = (IList)instance;
                    addMethod = (obj, index) => list.Add(obj);
                    output = list;
                }

                for (int index = 0; index < count; index++)
                {
                    object? item;

                    if (elementType.CanBeNull)
                    {
                        var isNullResult = _deserializeTypeValue_IsNull(reader, buffer);
                        if (isNullResult.IsFailure) return isNullResult.FailureReason("检查是否为空的标记失败");
                        if (isNullResult.Data) 
                        {
                            item = null;
                            goto GotItem;
                        }
                    }
                    var itemHandleResult = elementType.Category switch
                    {
                        TypeCategory.ShortData => _deserializeTypeValue_ShortData(elementType, reader, buffer),
                        TypeCategory.String => _deserializeTypeValue_String(elementType, reader, buffer),
                        TypeCategory.StringConveying => _deserializeTypeValue_StringConveying(elementType, reader, buffer),
                        _ => $"子项处理分类 ({elementType.Category}) 无效"
                    };
                    if (!itemHandleResult)
                        return itemHandleResult.FailureReasonWithTitle($"集合元素 {index} 处理失败");
                    item = itemHandleResult.Data;

                    GotItem:
                    addMethod(item, index);
                }

                return output;
            }
            else return $"集合处理类型是结构数据, 但不是集合类型";
        }


        #region 小操作的实现
        private OperationResult<bool> _deserializeTypeValue_IsNull(CharSequenceReader reader, StringBuilder buffer)
        {
            buffer.Clear();
            reader.ReadCountOrEnd(1, buffer);
            if (buffer.Length == 0) return "未能取得是否为空的标志";
            if (buffer[0] == nullFlag)
            {
                return (true, true, "成功");
            }
            else if (buffer[0] == notNullFlag)
            {
                return (true, false, "成功");
            }
            else
            {
                return  $"是否为空的标记无效: {buffer[0]}";
            }
        }
        private OperationResult<(int length, string dataSegment)> _deserializeTypeValue_LengthValueSegment(CharSequenceReader reader, StringBuilder buffer)
        {
            var countResult = _deserializeTypeValue_CountValue(reader, buffer);
            if (countResult.IsFailure) return countResult.FailureReason("读取长度信息失败");
            int length = countResult.Data;

            buffer.Clear();
            reader.ReadCountOrEnd(length, buffer);
            if (buffer.Length != length) return "实际读取到数量与数量标记不匹配";
            var segment = buffer.ToString();
            reader.ReadUntilNotIn(usingDelimitCharArray);
            buffer.Clear();
            return (length, segment);
        }
        private OperationResult<int> _deserializeTypeValue_CountValue(CharSequenceReader reader, StringBuilder buffer)
        {
            buffer.Clear();
            if (!reader.TryReadUntilAnyChar(usingDelimitCharArray, buffer, out _)) return "读取长度未读取到结束分隔符";
            string lengthStr = buffer.ToString();
            if (lengthStr.IsEmpty() || !int.TryParse(lengthStr, out int length)) return "读取到的长度字符串转换为数值失败";
            reader.ReadUntilNotIn(usingDelimitCharArray);
            buffer.Clear();
            return length;
        }
        #endregion

        #endregion

        public IOperationResult<TPayload> Deserialize<TPayload>(IReadableChannel<char> channel)
        {
            Type type = typeof(TPayload);
            var result = Deserialize(type, channel);
            if (result.IsSuccess)
            {
                result.DataImpossibleNull();
                return (OperationResult<TPayload>)(TPayload)result.Data;
            }
            else
            {
                return (OperationResult<TPayload>)result.FailureReason("未知原因");
            }
        }
        public IOperationResult<TPayload> Deserialize<TPayload>(string obj)
        {
            return Deserialize<TPayload>(obj.AsReadableChannel());
        }


        #endregion

        #region 序列化
        private OperationResult Serialize(Type type, object payload, IWritableChannel<char> channel)
        {
            var getInfos = _getTypeSerializeInfos(type);
            if (getInfos == null)
            {
                return "未能取得类型判断结果信息";
            }
            if (!getInfos.Value.ValidType(false))
            {
                return $"类型不适用于此编解码器: {getInfos.Value.InvalidReason}";
            }
            var infos = getInfos.Value;

            if (payload == null) return "传入的负载数据对象为空对象";

            foreach (var property in infos.Properties)
            {
                var value = property.Property.GetValue(payload);
                if (property.TypeDetail.CanBeNull)
                {
                    channel.Append(value == null ? nullFlag : notNullFlag);
                }
                if (value != null)
                {
                    var propertyHandleResult = property.TypeDetail.Category switch
                    {
                        TypeCategory.ShortData => _serializeTypeValue_ShortData(property.TypeDetail, value, channel),
                        TypeCategory.String => _serializeTypeValue_String(property.TypeDetail, value, channel),
                        TypeCategory.StringConveying => _serializeTypeValue_StringConveying(property.TypeDetail, value, channel),
                        TypeCategory.StructureData => _serializePropertyValue_StructureData(property, value, channel),
                        _ => $"取得的类型处理分类 ({property.TypeDetail.Category}) 无效"
                    };
                    if (!propertyHandleResult) 
                        return propertyHandleResult.FailureReasonWithTitle($"属性 {property.Property.Name} 处理失败");
                }
                channel.Append(usingDelimitChar);
            }

            return true;
        }
        #region 属性处理的实现细节

        private OperationResult _serializeTypeValue_ShortData(TypeDetail typeDetail, object value, IWritableChannel<char> channel)
        {
            var strValue = _shortDataConvertToString(typeDetail, value);
            channel.Append(strValue);
            return true;
        }
        private OperationResult _serializeTypeValue_String(TypeDetail typeDetail, object value, IWritableChannel<char> channel)
        { 
            string str = (string)value;
            return _serializeTypeValue_LengthValueSegment(str, channel);
        }
        private OperationResult _serializeTypeValue_StringConveying(TypeDetail typeDetail, object value, IWritableChannel<char> channel)
        {
            string str = StringConveyingHelper.ToString(typeDetail.UnderlyingType, value);
            return _serializeTypeValue_LengthValueSegment(str, channel);
        }
        private OperationResult _serializeTypeValue_LengthValueSegment(string value, IWritableChannel<char> channel)
        {
            channel.Append(value.Length.ToString()).Append(usingDelimitChar).Append(value);
            return true;
        }
        private OperationResult _serializePropertyValue_StructureData(PropertyDetailInfo property, object value, IWritableChannel<char> channel)
        {
            if (property.IsCollection)
            {
                var elementType = property.CollectionElementType.Value;

                var enumerable = (IEnumerable)value;
                int count;
                if (enumerable is ICollection collection) count = collection.Count;
                else
                {
                    var _enumerator = enumerable.GetEnumerator();
                    count = 0;
                    while (_enumerator.MoveNext()) count++;
                }

                channel.Append(count.ToString()).Append(usingDelimitChar);

                int index = 0;
                var enumerator = enumerable.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var item = enumerator.Current;

                    if (elementType.CanBeNull)
                    {
                        channel.Append(value == null ? nullFlag : notNullFlag);
                    }
                    if (item != null)
                    {
                        var itemHandleResult = elementType.Category switch
                        {
                            TypeCategory.ShortData => _serializeTypeValue_ShortData(elementType, item, channel),
                            TypeCategory.String => _serializeTypeValue_String(elementType, item, channel),
                            TypeCategory.StringConveying => _serializeTypeValue_StringConveying(elementType, item, channel),
                            _ => $"子项处理分类 ({elementType.Category}) 无效"
                        };
                        if (!itemHandleResult)
                            return itemHandleResult.FailureReasonWithTitle($"集合元素 {index} 处理失败");
                    }
                    if (index != count - 1) // 除了最后一项外, 均需要写入分隔符, 用于分隔不同元素
                    {
                        channel.Append(usingDelimitChar);
                    }
                    index++;
                }
                if (index != count) return $"集合取得的数量与实际遍历所得数量存在差异";

                return true;
            }
            else return $"集合处理类型是结构数据, 但不是集合类型";
        }
        #endregion

        public IOperationResult Serialize<TPayload>([DisallowNull] TPayload payload, IWritableChannel<char> channel)
        {
            return Serialize(typeof(TPayload), payload, channel);
        }
        public IOperationResult<string> Serialize<TPayload>([DisallowNull] TPayload payload)
        {
            StringBuilder builder = new();
            var result = Serialize(payload, builder.AsWritableChannel());
            if (result.IsFailure) return OperationResult<string>.Failure(result.FailureReason);
            return OperationResult<string>.Success(builder.ToString());
        }
        #endregion


        #region 短数据读写处理方式
        /* 短数据为常见的数据值类型, 直接采用配置字符串帮助类转换即可
         * 如果要压缩字符串总长度, 需要另做处理
         * */
        private static string _shortDataConvertToString(TypeDetail typeDetail, object value)
        {
            return Common_Util.Module.Config.ConfigStringHelper.Obj2ConfigValue(value) ?? throw new NullReferenceException("转换意外取得空字符串引用");
        }
        private static object _shortDataConvertFromString(TypeDetail typeDetail, string value)
        {
            return Common_Util.Module.Config.ConfigStringHelper.ConfigValue2Obj(value, typeDetail.UnderlyingType) ?? throw new NullReferenceException("转换意外取得空对象引用");
        }

        #endregion


        #endregion
    }
}
