using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ChaoticKit.Module.Config
{
    /// <summary>
    /// 动态配置读取器 (<see cref="IDynamicConfig"/>) 的静态工厂
    /// </summary>
    /// <remarks>
    /// 提供若干现成的配置来源; 需要自定义来源时继承 <see cref="DynamicConfigBase"/> 并重写
    /// <see cref="DynamicConfigBase.TryGetRawValue"/> 即可. <br/>
    /// 基于配置节 (Microsoft.Extensions.Configuration) 的来源由 ChaoticKit.Microsoft.Configuration 项目提供. <br/>
    /// 所有来源的取值都按 <see cref="ConfigStringHelper"/> 的配置字符串规则做类型转换, 规则详见 <see cref="TryConvertValue"/>
    /// </remarks>
    public static class DynamicConfig
    {
        #region 创建

        /// <summary>
        /// 包装一个普通对象, 使其公开的可读属性可以按键读取
        /// </summary>
        /// <param name="target">要包装的对象</param>
        /// <param name="aliases">
        /// 别名表: 取值的键会先在别名表中查找, 命中时改用映射到的属性名. <br/>
        /// 例如 <c>{ ["host"] = "Server", ["port"] = "Port" }</c>
        /// </param>
        /// <param name="keyComparer">
        /// 键与别名的比较器, 传 <see langword="null"/> 时使用 <see cref="StringComparer.OrdinalIgnoreCase"/>
        /// </param>
        /// <returns>以该对象的属性为来源的动态配置读取器</returns>
        /// <remarks>
        /// 只读取公开实例属性; 字段与索引器不参与读取
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="target"/> 为 <see langword="null"/></exception>
        public static IDynamicConfig FromObject(object target, IReadOnlyDictionary<string, string>? aliases = null, StringComparer? keyComparer = null)
            => new ObjectDynamicConfig(target, aliases, keyComparer);

        /// <summary>
        /// 包装一个字符串字典, 使其值可以按键读取
        /// </summary>
        /// <param name="dictionary">字符串值字典, 例如反序列化得到的配置表</param>
        /// <param name="aliases">别名表, 含义同 <see cref="FromObject"/></param>
        /// <param name="keyComparer">键与别名的比较器, 传 <see langword="null"/> 时使用 <see cref="StringComparer.OrdinalIgnoreCase"/></param>
        /// <returns>以该字典为来源的动态配置读取器 (字符串值会按目标类型转换)</returns>
        /// <exception cref="ArgumentNullException"><paramref name="dictionary"/> 为 <see langword="null"/></exception>
        public static IDynamicConfig FromDictionary(IReadOnlyDictionary<string, string?> dictionary, IReadOnlyDictionary<string, string>? aliases = null, StringComparer? keyComparer = null)
        {
            ArgumentNullException.ThrowIfNull(dictionary);
            return new DictionaryDynamicConfig(ToObjectPairs(dictionary), aliases, keyComparer);
        }

        /// <summary>
        /// 包装一个已经带类型的值字典, 使其值可以按键读取
        /// </summary>
        /// <param name="dictionary">
        /// 任意值字典, 例如 JSON 反序列化结果、数据行转换结果等; 值已经是目标类型时会直接命中, 不做转换
        /// </param>
        /// <param name="aliases">别名表, 含义同 <see cref="FromObject"/></param>
        /// <param name="keyComparer">键与别名的比较器, 传 <see langword="null"/> 时使用 <see cref="StringComparer.OrdinalIgnoreCase"/></param>
        /// <returns>以该字典为来源的动态配置读取器</returns>
        /// <exception cref="ArgumentNullException"><paramref name="dictionary"/> 为 <see langword="null"/></exception>
        public static IDynamicConfig FromDictionary(IReadOnlyDictionary<string, object?> dictionary, IReadOnlyDictionary<string, string>? aliases = null, StringComparer? keyComparer = null)
        {
            ArgumentNullException.ThrowIfNull(dictionary);
            return new DictionaryDynamicConfig(dictionary, aliases, keyComparer);
        }

        /// <summary>
        /// 包装一个 <see cref="NameValueCollection"/>, 例如 <see cref="System.Configuration.ConfigurationManager.AppSettings"/>
        /// </summary>
        /// <param name="collection">名称/值集合</param>
        /// <param name="aliases">别名表, 含义同 <see cref="FromObject"/></param>
        /// <param name="keyComparer">键与别名的比较器, 传 <see langword="null"/> 时使用 <see cref="StringComparer.OrdinalIgnoreCase"/></param>
        /// <returns>以该集合为来源的动态配置读取器</returns>
        /// <remarks>
        /// 同一个键存在多个值时, 与 <see cref="NameValueCollection"/> 的索引器一致, 取到逗号连接的字符串
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> 为 <see langword="null"/></exception>
        public static IDynamicConfig FromNameValueCollection(NameValueCollection collection, IReadOnlyDictionary<string, string>? aliases = null, StringComparer? keyComparer = null)
        {
            ArgumentNullException.ThrowIfNull(collection);
            return new DictionaryDynamicConfig(ToObjectPairs(collection), aliases, keyComparer);
        }

        /// <summary>
        /// 把字符串值字典投影为任意值字典
        /// </summary>
        private static IEnumerable<KeyValuePair<string, object?>> ToObjectPairs(IReadOnlyDictionary<string, string?> dictionary)
        {
            foreach (var pair in dictionary)
            {
                yield return KeyValuePair.Create<string, object?>(pair.Key, pair.Value);
            }
        }

        /// <summary>
        /// 把名称/值集合投影为任意值字典
        /// </summary>
        private static IEnumerable<KeyValuePair<string, object?>> ToObjectPairs(NameValueCollection collection)
        {
            foreach (var key in collection.AllKeys)
            {
                if (key is null) continue;
                yield return KeyValuePair.Create<string, object?>(key, collection[key]);
            }
        }

        #endregion

        #region 值转换

        /// <summary>
        /// 尝试把 <paramref name="rawValue"/> 转换为 <paramref name="targetType"/> 类型
        /// </summary>
        /// <param name="rawValue">原始值, 可以为 <see langword="null"/></param>
        /// <param name="targetType">目标类型</param>
        /// <param name="converted">转换结果</param>
        /// <returns>是否转换成功</returns>
        /// <remarks>
        /// 与字符串有关的转换统一走 <see cref="ConfigStringHelper"/> 的配置字符串规则, 依次为: <br/>
        /// 1. 类型已经匹配时直接返回, 不做转换 <br/>
        /// 2. 目标类型是字符串时, 按 <see cref="ConfigStringHelper.TryObj2ConfigValue"/> 转换
        /// (支持 <see cref="ChaoticKit.Data.Constraint.IStringConveying{TSelf}"/>、枚举、<see cref="Type"/>、
        /// 数组与列表等) <br/>
        /// 3. 原始值是字符串时, 按
        /// <see cref="ConfigStringHelper.TryConfigValue2Obj(string?, Type, out object?, ConfigStringHelper.ConfigValue2ObjOptions?)"/> 转换 <br/>
        /// 4. 前面没有成功时, 才使用通用转换器: <see cref="TypeDescriptor"/> 转换器 → <see cref="Convert.ChangeType(object, Type)"/> <br/>
        /// 也就是说, 配置字符串规则判失败时不会取它的回退值 (例如 <see cref="object.ToString"/> 或目标类型的默认值),
        /// 而是继续用后续方案, 因此任意对象不会被压成不可逆的字符串; 反过来, 配置字符串规则没有覆盖的类型
        /// (例如 <see cref="TimeSpan"/>) 仍可由通用转换器兜底, 而枚举这类会被配置字符串规则严格判定的类型,
        /// 在规则判失败后仍可能被宽松的 <see cref="TypeDescriptor"/> 转换器接受
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="targetType"/> 为 <see langword="null"/></exception>
        public static bool TryConvertValue(object? rawValue, Type targetType, out object? converted)
        {
            ArgumentNullException.ThrowIfNull(targetType);

            if (rawValue is null)
            {
                converted = null;
                return !targetType.IsValueType || Nullable.GetUnderlyingType(targetType) is not null;
            }

            if (targetType.IsInstanceOfType(rawValue))
            {
                converted = rawValue;
                return true;
            }

            // 目标类型是字符串: 走配置字符串规则
            if (targetType == typeof(string))
            {
                return TryConvertToConfigString(rawValue, out converted);
            }

            // 原始值是字符串: 先走配置字符串规则, 转换不出来再用通用转换器兜底
            if (rawValue is string text && TryConvertFromConfigString(text, targetType, out converted))
            {
                return true;
            }

            return TryConvertByTypeConverter(rawValue, targetType, out converted);
        }

        /// <summary>
        /// 把对象转换为配置字符串 (对象 → 字符串)
        /// </summary>
        /// <param name="rawValue">原始值, 不能为 <see langword="null"/></param>
        /// <param name="converted">转换结果</param>
        /// <returns>是否转换成功</returns>
        /// <remarks>
        /// 规则来自 <see cref="ConfigStringHelper.TryObj2ConfigValue"/>, 其中
        /// <see cref="ChaoticKit.Data.Constraint.IStringConveying{TSelf}"/> 的实现类型按约定的字符串形式输出. <br/>
        /// 该规则无法表达该对象时返回 <see langword="false"/>, 不会退回 <see cref="object.ToString"/>
        /// </remarks>
        private static bool TryConvertToConfigString(object rawValue, out object? converted)
        {
            try
            {
                if (ConfigStringHelper.TryObj2ConfigValue(rawValue, out var text))
                {
                    converted = text;
                    return converted is not null;
                }
            }
            catch
            {
                // 实现有问题的 IStringConveying 等情况: 视为转换失败
            }

            converted = null;
            return false;
        }

        /// <summary>
        /// 把配置字符串转换为目标类型 (字符串 → 对象)
        /// </summary>
        /// <param name="text">配置字符串</param>
        /// <param name="targetType">目标类型</param>
        /// <param name="converted">转换结果</param>
        /// <returns>是否转换成功</returns>
        /// <remarks>
        /// 规则来自 <see cref="ConfigStringHelper.TryConfigValue2Obj(string?, Type, out object?, ConfigStringHelper.ConfigValue2ObjOptions?)"/>,
        /// 其中 <see cref="ChaoticKit.Data.Constraint.IStringConveying{TSelf}"/> 的实现类型按约定的字符串形式还原. <br/>
        /// 该规则转换不出来时返回 <see langword="false"/>, 不会取目标类型的默认值, 以便调用方换用其它方案
        /// </remarks>
        private static bool TryConvertFromConfigString(string text, Type targetType, out object? converted)
        {
            try
            {
                if (ConfigStringHelper.TryConfigValue2Obj(text, targetType, out var value))
                {
                    converted = value;
                    return converted is not null;
                }
            }
            catch
            {
                // 实现有问题的 IStringConveying, 或列表元素无法放入 (例如 List<int> 中解析出 null) 等情况: 视为转换失败
            }

            converted = null;
            return false;
        }

        /// <summary>
        /// 通用转换: <see cref="TypeDescriptor"/> 转换器 → <see cref="Convert.ChangeType(object, Type)"/>
        /// </summary>
        /// <param name="rawValue">原始值, 不能为 <see langword="null"/></param>
        /// <param name="targetType">目标类型</param>
        /// <param name="converted">转换结果</param>
        /// <returns>是否转换成功</returns>
        private static bool TryConvertByTypeConverter(object rawValue, Type targetType, out object? converted)
        {
            try
            {
                var converter = TypeDescriptor.GetConverter(targetType);
                if (converter.CanConvertFrom(rawValue.GetType()))
                {
                    converted = converter.ConvertFrom(rawValue);
                    return true;
                }
            }
            catch
            {
                // 转换器自身抛异常时, 继续尝试后面的方式
            }

            try
            {
                // Convert.ChangeType 不接受可空类型, 这里先取到其基础类型
                converted = Convert.ChangeType(rawValue, Nullable.GetUnderlyingType(targetType) ?? targetType);
                return true;
            }
            catch
            {
                converted = null;
                return false;
            }
        }

        #endregion

        #region 实现

        /// <summary>
        /// 以对象的公开可读属性为来源的动态配置读取器
        /// </summary>
        private sealed class ObjectDynamicConfig : DynamicConfigBase
        {
            private readonly object _target;
            private readonly Dictionary<string, PropertyInfo> _propertyCache;

            public ObjectDynamicConfig(object target, IReadOnlyDictionary<string, string>? aliases, StringComparer? keyComparer)
                : base(aliases, keyComparer)
            {
                ArgumentNullException.ThrowIfNull(target);

                _target = target;
                _propertyCache = new Dictionary<string, PropertyInfo>(KeyComparer);

                foreach (var property in target.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    // 索引器属性的取值需要额外参数, 不纳入可读取范围
                    if (property.CanRead && property.GetIndexParameters().Length == 0)
                    {
                        _propertyCache[property.Name] = property;
                    }
                }
            }

            protected override bool TryGetRawValue(Type targetType, string resolvedKey, out object? rawValue)
            {
                if (_propertyCache.TryGetValue(resolvedKey, out var property))
                {
                    rawValue = property.GetValue(_target);
                    return true;
                }

                rawValue = null;
                return false;
            }
        }

        /// <summary>
        /// 以键值对集合为来源的动态配置读取器
        /// </summary>
        private sealed class DictionaryDynamicConfig : DynamicConfigBase
        {
            private readonly IReadOnlyDictionary<string, object?> _values;

            public DictionaryDynamicConfig(IEnumerable<KeyValuePair<string, object?>> values, IReadOnlyDictionary<string, string>? aliases, StringComparer? keyComparer)
                : base(aliases, keyComparer)
            {
                ArgumentNullException.ThrowIfNull(values);

                _values = new ReadOnlyDictionary<string, object?>(BuildDictionary(values, KeyComparer));
            }

            protected override bool TryGetRawValue(Type targetType, string resolvedKey, out object? rawValue)
                => _values.TryGetValue(resolvedKey, out rawValue);
        }

        #endregion
    }
}
