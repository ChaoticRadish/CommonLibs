using ChaoticKit.Module.Config;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaoticKit.Microsoft.Configuration
{
    /// <summary>
    /// 以配置节 (Microsoft.Extensions.Configuration) 为来源的动态配置读取器
    /// </summary>
    /// <remarks>
    /// 取值规则: 键指向标量叶子 (字符串、数字、布尔等) 时取出字符串, 交由
    /// <see cref="DynamicConfigBase"/> 按目标类型转换; 键指向子节时, 按目标类型绑定整个子节;
    /// 键不存在时由基类返回默认值
    /// </remarks>
    internal sealed class ConfigurationDynamicConfig : DynamicConfigBase
    {
        private readonly IConfiguration _section;

        /// <summary>
        /// 构造以配置节为来源的动态配置读取器
        /// </summary>
        /// <param name="section">配置节, 例如 <c>configuration.GetSection("Mail")</c></param>
        /// <param name="aliases">别名表: 取值的键会先在别名表中查找, 命中时改用映射到的键</param>
        /// <param name="keyComparer">
        /// 键与别名的比较器, 传 <see langword="null"/> 时使用 <see cref="StringComparer.OrdinalIgnoreCase"/>
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="section"/> 为 <see langword="null"/></exception>
        public ConfigurationDynamicConfig(IConfiguration section, IReadOnlyDictionary<string, string>? aliases = null, StringComparer? keyComparer = null)
            : base(aliases, keyComparer)
        {
            ArgumentNullException.ThrowIfNull(section);

            _section = section;
        }

        /// <inheritdoc/>
        protected override bool TryGetRawValue(Type targetType, string resolvedKey, out object? rawValue)
        {
            var section = _section.GetSection(resolvedKey);

            // 标量叶子: 直接取出字符串, 由基类按目标类型转换
            var value = section.Value;
            if (value is not null)
            {
                rawValue = value;
                return true;
            }

            if (!section.Exists())
            {
                rawValue = null;
                return false;
            }

            // 不是标量叶子: 按目标类型绑定整个子节, 支持自定义类与结构体
            try
            {
                rawValue = section.Get(targetType);
            }
            catch
            {
                // 无法绑定时, 视为没有取到值
                rawValue = null;
            }

            return rawValue is not null;
        }
    }
}
