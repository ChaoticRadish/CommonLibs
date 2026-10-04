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
    /// 配置节到动态配置读取器的适配扩展
    /// </summary>
    public static class DynamicConfigConfigurationExtensions
    {
        /// <summary>
        /// 把配置节适配为 <see cref="IDynamicConfig"/>
        /// </summary>
        /// <param name="section">配置节, 例如 <c>configuration.GetSection("Mail")</c></param>
        /// <param name="aliases">
        /// 别名表: 取值的键会先在别名表中查找, 命中时改用映射到的键. <br/>
        /// 例如 <c>{ ["host"] = "Server", ["port"] = "Port" }</c>
        /// </param>
        /// <param name="keyComparer">
        /// 键与别名的比较器, 传 <see langword="null"/> 时使用 <see cref="StringComparer.OrdinalIgnoreCase"/>
        /// </param>
        /// <returns>以该配置节为来源的动态配置读取器</returns>
        /// <remarks>
        /// 取值规则: 键指向标量叶子时按目标类型转换; 键指向子节时, 按目标类型绑定整个子节
        /// (支持自定义类与结构体); 键不存在或无法转换时返回默认值
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="section"/> 为 <see langword="null"/></exception>
        public static IDynamicConfig AsDynamicConfig(this IConfiguration section, IReadOnlyDictionary<string, string>? aliases = null, StringComparer? keyComparer = null)
        {
            ArgumentNullException.ThrowIfNull(section);

            return new ConfigurationDynamicConfig(section, aliases, keyComparer);
        }
    }
}
