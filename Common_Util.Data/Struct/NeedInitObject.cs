using Common_Util.Exceptions.General;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Struct
{
    /// <summary>
    /// 接口: 一个需要赋初始值的对象
    /// </summary>
    public interface INeedInitObject
    {
        /// <summary>
        /// 是否已经赋初值
        /// </summary>
        bool Inited { get; }
        /// <summary>
        /// 当前是否拥有值
        /// </summary>
        bool HasValue { get; }

        /// <summary>
        /// <see langword="get"/> => 如果未赋初始值, 将抛出异常; <see langword="set"/> => 如果已经有初始值, 将抛出异常
        /// </summary>
        object Value { get; set; }

    }

    /// <summary>
    /// 接口: 一个需要赋 <typeparamref name="T"/> 类型初始值的对象
    /// </summary>
    public interface INeedInitObject<T> : INeedInitObject
    {
        /// <summary>
        /// <see langword="get"/> => 如果未赋初始值, 将抛出异常; <see langword="set"/> => 如果已经有初始值, 将抛出异常
        /// </summary>
        new T Value { get; set; }

    }

    /// <summary>
    /// 用于包装一个需要赋初始值的对象的包装器, 线程不安全! 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class NeedInitObject<T> : INeedInitObject, INeedInitObject<T>
    {
        /// <summary>
        /// 实例化一个未有初值的包装器
        /// </summary>
        public NeedInitObject()
        {
            Inited = false;
        }
        /// <summary>
        /// 实例化一个已有初值的包装器
        /// </summary>
        /// <param name="initValue"></param>
        public NeedInitObject(T initValue)
        {
            Inited = true;
            value = initValue;
        }

        public bool Inited { get; private set; }
        public bool HasValue => value != null;

        #region 数据

        object INeedInitObject.Value { get => Value!; set => Value = (T)value; }
        /// <summary>
        /// <see langword="get"/> => 如果未赋初始值, 将抛出异常; <see langword="set"/> => 如果已经有初始值, 将抛出异常
        /// </summary>
        public T Value
        {
            get
            {
                if (Inited) return value ?? throw new ImpossibleForkException("已经赋值, 但拥有的值却是 null! ");
                else throw new InvalidOperationException($"未赋值! 目标类型: {typeof(T)}");
            }
            set
            {
                if (Inited) throw new InvalidOperationException("已具有初始值, 无法再次赋值! ");
                if (value == null) throw new InvalidOperationException("此类型的值不接受空值! ");
                this.value = value;
                Inited = true;
            }
        }
        private T? value;
        #endregion


        #region 隐式转换
        public static implicit operator NeedInitObject<T>(T initValue)
        {
            return new(initValue);
        }
        public static implicit operator T(NeedInitObject<T> wrapper)
        {
            return wrapper.Value;
        }
        #endregion

        public override string ToString()
        {
            return $"NeedInitObject<{typeof(T).Name}>_{(Inited ? "Inited" : "WaitingValue")}_[{value?.ToString() ?? "null"}]";
        }

    }

    public static class NeedInitObject
    {
        /// <summary>
        /// 使用传入对象作为初始值, 创建一个 <see cref="NeedInitObject{T}"/>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static INeedInitObject Create(object obj)
        {
            ArgumentNullException.ThrowIfNull(obj);
            Type objType = obj.GetType();
            Type wrapperType = typeof(NeedInitObject<>).MakeGenericType(objType);
            var constructor = wrapperType.GetConstructor([objType]) ?? throw new ImpossibleForkException($"未能取得使用初始值作为唯一一个形参的构造函数");
            return (INeedInitObject)constructor.Invoke([obj]);
        }

        /// <summary>
        /// 判断 <paramref name="property"/> 类型是否 <see cref="INeedInitObject"/>, 如果是, 则将 <paramref name="value"/> 设置到 <paramref name="obj"/> 的 <paramref name="property"/> 上
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="property"></param>
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueToProperty(object obj, PropertyInfo property, object value)
        {
            if (property.PropertyType.IsAssignableTo(typeof(INeedInitObject)))
            {
                var propertyValue = property.GetValue(obj);
                if (propertyValue is INeedInitObject wrapper)
                {
                    wrapper.Value = value;  
                }
            }
            else
            {
                throw new InvalidOperationException($"传入属性不实现接口 {typeof(INeedInitObject).Name}");
            }
        }
    }
}
