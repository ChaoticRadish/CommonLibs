using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaoticKit.Module.Config
{
    /// <summary>
    /// 动态配置读取器的基类: 统一处理别名解析与值的类型转换, 派生类只需要提供原始值
    /// </summary>
    /// <remarks>
    /// 取值流程: 解析别名 → 由派生类取出原始值 → 转换为目标类型. <br/>
    /// 值转换遵循 <see cref="ConfigStringHelper"/> 的配置字符串规则, 规则详见 <see cref="DynamicConfig.TryConvertValue"/>. <br/>
    /// 原始值取不到、原始值为 <see langword="null"/>, 或转换失败时, 都会返回调用方传入的默认值,
    /// 也就是"静默降级"而不抛异常. <br/>
    /// 需要接入自定义配置来源 (例如数据库、远端配置中心) 时, 继承本类并重写 <see cref="TryGetRawValue"/> 即可
    /// </remarks>
    public abstract class DynamicConfigBase : IDynamicConfig
    {
        private readonly IReadOnlyDictionary<string, string> _aliases;

        /// <summary>
        /// 构造动态配置读取器
        /// </summary>
        /// <param name="aliases">
        /// 别名表: 取值的键会先在别名表中查找, 命中时改用映射到的键. <br/>
        /// 例如 <c>{ ["host"] = "Server", ["port"] = "Port" }</c>
        /// </param>
        /// <param name="keyComparer">
        /// 键与别名的比较器, 传 <see langword="null"/> 时使用 <see cref="StringComparer.OrdinalIgnoreCase"/>
        /// </param>
        protected DynamicConfigBase(IReadOnlyDictionary<string, string>? aliases = null, StringComparer? keyComparer = null)
        {
            KeyComparer = keyComparer ?? StringComparer.OrdinalIgnoreCase;
            _aliases = aliases is null
                ? new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(KeyComparer))
                : new ReadOnlyDictionary<string, string>(BuildDictionary(aliases, KeyComparer));
        }

        /// <summary>
        /// 当前使用的键与别名比较器
        /// </summary>
        public StringComparer KeyComparer { get; }

        /// <summary>
        /// 以指定比较器把键值对集合重建为字典
        /// </summary>
        /// <remarks>
        /// 被比较器判定为相同的键 (例如仅大小写不同) 出现多次时, 后出现的会覆盖先出现的, 不会抛异常
        /// </remarks>
        /// <typeparam name="TValue">值的类型</typeparam>
        /// <param name="source">键值对集合</param>
        /// <param name="comparer">键的比较器</param>
        /// <returns>重建后的字典</returns>
        protected static Dictionary<string, TValue> BuildDictionary<TValue>(IEnumerable<KeyValuePair<string, TValue>> source, StringComparer comparer)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(comparer);

            var dictionary = new Dictionary<string, TValue>(comparer);
            foreach (var pair in source)
            {
                dictionary[pair.Key] = pair.Value;
            }
            return dictionary;
        }

        /// <summary>
        /// 解析别名: 命中别名表时返回映射到的键, 否则原样返回
        /// </summary>
        /// <param name="key">调用方传入的键</param>
        /// <returns>实际用于取值的键</returns>
        protected string ResolveKey(string key)
        {
            return _aliases.TryGetValue(key, out var mapped) ? mapped : key;
        }

        /// <summary>
        /// 取出指定键的原始值, 不做类型转换
        /// </summary>
        /// <param name="targetType">
        /// 调用方期望的值类型, 供"需要按类型取值"的来源使用 (例如把配置节整体绑定为对象)
        /// </param>
        /// <param name="resolvedKey">已经过别名解析的键</param>
        /// <param name="rawValue">取到的原始值</param>
        /// <returns>是否取到值</returns>
        protected abstract bool TryGetRawValue(Type targetType, string resolvedKey, out object? rawValue);

        /// <inheritdoc/>
        public object? GetValue(Type targetType, string key, object? defaultValue = null)
        {
            ArgumentNullException.ThrowIfNull(targetType);
            ArgumentNullException.ThrowIfNull(key);

            if (!TryGetRawValue(targetType, ResolveKey(key), out var rawValue) || rawValue is null)
            {
                return defaultValue;
            }

            return DynamicConfig.TryConvertValue(rawValue, targetType, out var converted) ? converted : defaultValue;
        }
    }
}
