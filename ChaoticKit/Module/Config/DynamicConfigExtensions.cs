using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ChaoticKit.Module.Config
{
    /// <summary>
    /// <see cref="IDynamicConfig"/> 的扩展方法
    /// </summary>
    public static class DynamicConfigExtensions
    {
        /// <summary>
        /// 泛型取值
        /// </summary>
        /// <typeparam name="T">期望的值类型</typeparam>
        /// <param name="config">动态配置读取器</param>
        /// <param name="key">配置的键</param>
        /// <param name="defaultValue">未取到值, 或取到的值无法转换为 <typeparamref name="T"/> 时返回的默认值</param>
        /// <returns>取到的配置值, 或 <paramref name="defaultValue"/></returns>
        /// <remarks>
        /// 如果配置来源没有按目标类型转换 (例如自定义实现直接返回了原始对象), 这里会再兜底转换一次
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="config"/> 为 <see langword="null"/></exception>
        public static T? GetValue<T>(this IDynamicConfig config, string key, T? defaultValue = default)
        {
            ArgumentNullException.ThrowIfNull(config);

            var raw = config.GetValue(typeof(T), key, defaultValue);
            if (raw is null)
            {
                return defaultValue;
            }

            if (raw is T typed)
            {
                return typed;
            }

            return DynamicConfig.TryConvertValue(raw, typeof(T), out var converted) && converted is T tConverted
                ? tConverted
                : defaultValue;
        }

        /// <summary>
        /// 尝试取值
        /// </summary>
        /// <typeparam name="T">期望的值类型</typeparam>
        /// <param name="config">动态配置读取器</param>
        /// <param name="key">配置的键</param>
        /// <param name="value">取到的配置值, 未取到时为 <see langword="default"/></param>
        /// <returns>是否取到了值 (取到的值为 <see langword="null"/> 时视为未取到)</returns>
        /// <exception cref="ArgumentNullException"><paramref name="config"/> 为 <see langword="null"/></exception>
        public static bool TryGetValue<T>(this IDynamicConfig config, string key, [NotNullWhen(true)] out T? value)
        {
            ArgumentNullException.ThrowIfNull(config);

            var raw = config.GetValue(typeof(T), key, null);
            if (raw is null)
            {
                value = default;
                return false;
            }

            if (raw is T typed)
            {
                value = typed;
                return true;
            }

            if (DynamicConfig.TryConvertValue(raw, typeof(T), out var converted) && converted is T tConverted)
            {
                value = tConverted;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// 按属性名逐项取值, 创建一个填充好的 <typeparamref name="T"/> 实例
        /// </summary>
        /// <typeparam name="T">具有公开无参构造函数且含可写属性的类型</typeparam>
        /// <param name="config">动态配置读取器</param>
        /// <returns>填充后的实例</returns>
        /// <remarks>
        /// 只处理"可读且可写"的属性 (含 init 访问器), 跳过索引器; <br/>
        /// 取不到值或无法转换的属性会被跳过并保留其默认值, 不会抛异常, 因此返回的实例可能是部分填充的
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="config"/> 为 <see langword="null"/></exception>
        public static T CreateInstance<T>(this IDynamicConfig config) where T : new()
        {
            ArgumentNullException.ThrowIfNull(config);

            var instance = new T();
            foreach (var property in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!property.CanRead || !property.CanWrite || property.GetIndexParameters().Length > 0)
                {
                    continue;
                }

                var value = config.GetValue(property.PropertyType, property.Name);
                if (value is null)
                {
                    continue;
                }

                if (property.PropertyType.IsInstanceOfType(value))
                {
                    property.SetValue(instance, value);
                    continue;
                }

                if (DynamicConfig.TryConvertValue(value, property.PropertyType, out var converted)
                    && converted is not null
                    && property.PropertyType.IsInstanceOfType(converted))
                {
                    property.SetValue(instance, converted);
                }
            }

            return instance;
        }
    }
}
