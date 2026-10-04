using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaoticKit.Module.Config
{
    /// <summary>
    /// 接口: 动态配置读取器
    /// </summary>
    /// <remarks>
    /// 用于把调用方与配置的实际存放位置解耦: 调用方只关心"键"和"期望类型", 不需要知道配置是来自普通对象、
    /// 字典, 还是配置节 (见 ChaoticKit.Microsoft.Configuration 项目中的 AsDynamicConfig 扩展方法). <br/>
    /// 与 <see cref="IConfigManager"/> 的分工: <see cref="IConfigManager"/> 管理的是配置对象的读写实现与缓存,
    /// 读取单位是一个配置类实例; 本接口的读取单位是单个键值, 适合表达零散参数、可选覆盖项与别名映射
    /// </remarks>
    public interface IDynamicConfig
    {
        /// <summary>
        /// 按键读取配置值, 需要时转换为 <paramref name="targetType"/> 类型
        /// </summary>
        /// <param name="targetType">期望的值类型</param>
        /// <param name="key">配置的键</param>
        /// <param name="defaultValue">
        /// 未取到值, 或取到的值无法转换为 <paramref name="targetType"/> 时返回的默认值;
        /// 引用类型可以传 <see langword="null"/>
        /// </param>
        /// <returns>取到的配置值; 未取到或转换失败时返回 <paramref name="defaultValue"/></returns>
        /// <remarks>
        /// 取到的值会按 <see cref="ConfigStringHelper"/> 的配置字符串规则转换为 <paramref name="targetType"/>,
        /// 因此 <see cref="ChaoticKit.Data.Constraint.IStringConveying{TSelf}"/> 等由该规则支持的类型可以直接读取. <br/>
        /// 泛型取值、尝试取值与实例填充见 <see cref="DynamicConfigExtensions"/>
        /// </remarks>
        object? GetValue(Type targetType, string key, object? defaultValue = null);
    }
}
