using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Wrapped
{
    /// <summary>
    /// 显示文本值的包装器
    /// <para>当自定义显示值为 null 时, 使用特定方法从数据值取得实际值</para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ShowTextWrapper<T>
    {
        public ShowTextWrapper(T? value, string? showText = null)
        {
            Value = value;
            _showText = showText;
        }
        public ShowTextWrapper() { }


        /// <summary>
        /// 数据值
        /// </summary>
        public T? Value { get; set; }

        public Type ValueType { get => typeof(T); }

        public string ShowText { get => _showText ?? ConvertFunc?.Invoke(Value) ?? GetShowText(); }
        /// <summary>
        /// 自定义显示值, 最优先使用
        /// </summary>
        private string? _showText;
        /// <summary>
        /// 设置自定义显示文本, 如果为空则取消设置
        /// </summary>
        /// <param name="showText"></param>
        public void CustomShowText(string? showText)
        {
            _showText = showText;
        }

        /// <summary>
        /// 从数据值取得显示文本值的方法, 次优先使用
        /// </summary>
        public Func<T?, string>? ConvertFunc { get; set; }

        /// <summary>
        /// 从数据值取得显示文本值的方法, 最低优先级
        /// </summary>
        /// <returns></returns>
        protected virtual string GetShowText()
        {
            return Value?.ToString() ?? string.Empty;
        }

        public override string ToString()
        {
            return ShowText;
        }

        #region 隐式转换
        public static implicit operator T?(ShowTextWrapper<T>? wrapper)
        {
            return wrapper == null ? default : wrapper.Value;
        }
        public static implicit operator ShowTextWrapper<T>(T? property)
        {
            return new(property);
        }
        public static implicit operator ShowTextWrapper<T>((T? property, string showText) obj)
        {
            return new(obj.property, obj.showText);
        }
        public static implicit operator ShowTextWrapper<T>((string showText, T? property) obj)
        {
            return new(obj.property, obj.showText);
        }
        #endregion
    }

    public static class ShowTextWrapperExtensions
    {
        /// <summary>
        /// 向列表添加一项 (添加到尾部)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TWrapper"></typeparam>
        /// <param name="list"></param>
        /// <param name="obj"></param>
        /// <param name="showText"></param>
        /// <returns></returns>
        public static IList<TWrapper> AppendWrapper<T, TWrapper>(this IList<TWrapper> list, T? obj, string? showText = null)
            where TWrapper : ShowTextWrapper<T>, new()
        {
            TWrapper wrapper = new TWrapper() { Value = obj };
            wrapper.CustomShowText(showText);
            list.Add(wrapper);
            return list;
        }

    }

    public static class EnumShowTextWrapperHelper
    {
        #region 创建列表
        /// <summary>
        /// 遍历枚举类型, 创建包装器的列表
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <typeparam name="TWrapper"></typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"><see cref=""/></exception>
        public static IList<TWrapper> CreateList<TEnum, TWrapper>()
            where TWrapper : EnumShowTextWrapper, new()
        {
            Type type = typeof(TEnum);
            if (!type.IsEnum(true))
            {
                throw new InvalidOperationException($"泛型类型 {nameof(TEnum)} 必须是枚举类型");
            }
            List<TWrapper> wrappers = [];

            EnumHelper.ForEach(type, (obj) =>
            {
                wrappers.Add(new() { Value = (Enum)obj });
            });

            return wrappers;

        }

        #endregion
    }

    /// <summary>
    /// 枚举值的显示文本值的包装器, 限制类型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EnumShowTextWrapper<T> : EnumShowTextWrapper
    {
        public EnumShowTextWrapper(T? value, string? showText = null) : base((Enum?)(object?)value, showText) { }
        public EnumShowTextWrapper() : base() { }

        #region 类型检查
        static EnumShowTextWrapper()
        {
            Type type = typeof(T);
            if (!type.IsEnum(true))
            {
                throw new InvalidOperationException($"泛型类型 {nameof(T)} 必须是枚举类型");
            }
        }
        #endregion

        public new T? Value
        {
            get => (T?)(object?)base.Value;
            set => base.Value = (Enum?)(object?)value;
        }

        public static IList<EnumShowTextWrapper<T>> CreateList()
        {
            return EnumShowTextWrapperHelper.CreateList<T, EnumShowTextWrapper<T>>();
        }


        #region 隐式转换
        public static implicit operator T?(EnumShowTextWrapper<T> wrapper)
        {
            return wrapper == null ? default : wrapper.Value;
        }
        public static implicit operator EnumShowTextWrapper<T>(T? property)
        {
            return new(property);
        }
        public static implicit operator EnumShowTextWrapper<T>((T? property, string showText) obj)
        {
            return new(obj.property, obj.showText);
        }
        public static implicit operator EnumShowTextWrapper<T>((string showText, T? property) obj)
        {
            return new(obj.property, obj.showText);
        }
        #endregion
    }

    /// <summary>
    /// 枚举值的显示文本值的包装器 (不限制类型)
    /// </summary>
    public class EnumShowTextWrapper : ShowTextWrapper<Enum>
    {
        public EnumShowTextWrapper(Enum? value, string? showText = null) : base(value, showText) { }
        public EnumShowTextWrapper() : base() { }


        protected Enum? EnumValue
        {
            get => (Enum?)(object?)Value;
        }
        protected override string GetShowText()
        {
            return EnumValue?.GetDesc() ?? string.Empty;
        }

        #region 隐式转换
        public static implicit operator Enum?(EnumShowTextWrapper wrapper)
        {
            return wrapper == null ? default : wrapper.Value;
        }
        public static implicit operator EnumShowTextWrapper(Enum? property)
        {
            return new(property);
        }
        public static implicit operator EnumShowTextWrapper((Enum? property, string showText) obj)
        {
            return new(obj.property, obj.showText);
        }
        public static implicit operator EnumShowTextWrapper((string showText, Enum? property) obj)
        {
            return new(obj.property, obj.showText);
        }
        #endregion
    }
}
