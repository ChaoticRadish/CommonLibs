using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common_Util.String
{
    public static class JsonHelper
    {
        static JsonHelper()
        {
            Implement = new DefaultJsonConverter();
            //Console.WriteLine("设置JsonHelper.Implement: " + nameof(DefaultJsonConverter));
        }
        #region 实现方法
        /// <summary>
        /// 当前 <see cref="JsonHelper"/> 所使用的Json转换实现, 默认使用 <see cref="DefaultJsonConverter"/>
        /// </summary>
        public static IJsonConverter? Implement { get; set; }

        /// <summary>
        /// 使用指定类型的实例覆盖 <see cref="Common_Util.String.JsonHelper.Implement"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void SetImplement<T>() where T : IJsonConverter
        {
            SetImplement(typeof(T));
        }

        /// <summary>
        /// 使用指定类型的实例覆盖 <see cref="Common_Util.String.JsonHelper.Implement"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void SetImplement(Type type)
        {
            if (!type.IsAssignableTo(typeof(IJsonConverter)))
            {
                throw new ArgumentException("输入类型未继承" + nameof(IJsonConverter));
            }
            ConstructorInfo? ctor = type.GetConstructor(Array.Empty<Type>());
            if (ctor != null)
            {
                Implement = (IJsonConverter?)Activator.CreateInstance(type);
                return;
            }
            throw new ArgumentException("输入类型没有支持的构造方法! " + nameof(IJsonConverter));
        }
        #endregion

        /// <summary>
        /// 将输入对象序列化为Json字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="oneLine">是否格式化为单行</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">未设置Json转换的实现类型, 或者其为null</exception>
        public static string? Serialize(object obj, bool oneLine = false)
        {
            if (Implement == null) throw new NotImplementedException("未设置Json序列化与反序列化的实现! ");
            return Implement.Serialize(obj, oneLine);
        }
        /// <summary>
        /// 将输入的字符串反序列化为指定类型的对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonStr"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">未设置Json转换的实现类型, 或者其为null</exception>
        public static T? Deserialize<T>(string jsonStr, T? defaultValue = default)
        {
            if (Implement == null) throw new NotImplementedException("未设置Json序列化与反序列化的实现! ");
            try
            {
                object? obj = Implement.Deserialize(typeof(T), jsonStr);
                return obj == null ? default(T) : (T)obj;
            }
            catch
            {
                return defaultValue;
            }
        }
        public static object? Deserialize(Type targetType, string jsonStr, object? defaultValue = default)
        {
            if (Implement == null) throw new NotImplementedException("未设置Json序列化与反序列化的实现! ");
            try
            {
                return Implement.Deserialize(targetType, jsonStr);
            }
            catch
            {
                return defaultValue;
            }
        }
    }

    /// <summary>
    /// Json字符串与对象的转换接口
    /// </summary>
    public interface IJsonConverter
    {
        string? Serialize(object obj, bool oneLine);

        object? Deserialize(Type targetType, string jsonStr);
    }

    /// <summary>
    /// 默认的Json转换实现, 使用 <see cref="System.Text.Json"/> 实现
    /// </summary>
    public class DefaultJsonConverter : IJsonConverter
    {
        public object? Deserialize(Type targetType, string jsonStr)
        {
            //Console.WriteLine("DefaultJsonConverter.Deserialize");
            return System.Text.Json.JsonSerializer.Deserialize(jsonStr, targetType);
        }

        public string? Serialize(object obj, bool oneLine)
        {
            //Console.WriteLine("DefaultJsonConverter.Serialize");
            if (obj == null) return null;
            return System.Text.Json.JsonSerializer.Serialize(obj, obj.GetType(), new System.Text.Json.JsonSerializerOptions()
            {
                WriteIndented = !oneLine
            });
        }
    }
}
