using System;
using System.Collections.Generic;
using System.Text;

namespace Common_Util.Attributes.General
{
    /// <summary>
    /// 默认值属性
    /// </summary>
    public class DefaultValueAttribute : System.Attribute
    {
        /// <summary>
        /// 默认值
        /// </summary>
        /// <param name="s"></param>
        public DefaultValueAttribute(string s)
        {
            ValueString = s;
        }
        public DefaultValueAttribute(string s, string debugValue)
        {
#if DEBUG
            ValueString = debugValue;
#else
            ValueString = s;
#endif
        }
        /// <summary>
        /// 通过字符串数组构造对象, 其中每一项都是一组键值对
        /// <para>示例: "AA:7486456", "BB:0.4431"</para>
        /// </summary>
        /// <param name="pairs"></param>
        public DefaultValueAttribute(string[] pairs)
        {
            if (pairs != null && pairs.Length > 0)
            {
                Dictionary<string, string> map = new Dictionary<string, string>();
                foreach (string s in pairs)
                {
                    if (string.IsNullOrEmpty(s)) continue;
                    // 取得分隔符的位置
                    int splitIndex = s.IndexOf(':');
                    if (splitIndex < 0) continue;

                    // 键 与 值
                    string key = s.Substring(0, splitIndex);
                    string value = s.Substring(splitIndex + 1);
                    if (map.ContainsKey(key))
                    {
                        map[key] = value;
                    }
                    else
                    {
                        map.Add(key, value);
                    }
                }
                StringBuilder builder = new StringBuilder();
                builder.Append("{");
                int index = 0;
                foreach (string key in map.Keys)
                {
                    builder.Append("'").Append(key).Append("'")
                        .Append(':')
                        .Append("'").Append(map[key]).Append("'");
                    if (index < map.Count - 1)
                    {
                        builder.Append(',');
                    }
                    index++;
                }
                builder.Append("}");

                ValueString = builder.ToString();
            }
        }
        public DefaultValueAttribute(int i) : this(i.ToString()) { }
        public DefaultValueAttribute(bool b) : this(b.ToString()) { }
        public DefaultValueAttribute(float v) : this(v.ToString()) { }
        public DefaultValueAttribute(double v) : this(v.ToString()) { }
        public DefaultValueAttribute(long v) : this(v.ToString()) { }
        public DefaultValueAttribute(int i, int debugValue) : this(i.ToString(), debugValue.ToString()) { }
        public DefaultValueAttribute(bool b, bool debugValue) : this(b.ToString(), debugValue.ToString()) { }
        public DefaultValueAttribute(float v, float debugValue) : this(v.ToString(), debugValue.ToString()) { }
        public DefaultValueAttribute(double v, double debugValue) : this(v.ToString(), debugValue.ToString()) { }
        public DefaultValueAttribute(long v, long debugValue) : this(v.ToString(), debugValue.ToString()) { }


        #region 属性
        /// <summary>
        /// 值字符串
        /// </summary>
        public string? ValueString { get; private set; }
        #endregion

        /// <summary>
        /// 作为int值
        /// </summary>
        /// <returns></returns>
        public int AsInt(int defaultValue = default)
        {
            if (string.IsNullOrEmpty(ValueString)) return defaultValue;
            if (int.TryParse(ValueString, out int result))
            {
                return result;
            }
            else
            {
                return defaultValue;
            }
        }
        /// <summary>
        /// 尝试作为int值
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool TryAsInt(out int result)
        {
            if (string.IsNullOrEmpty(ValueString))
            {
                result = default;
            } 
            else if (int.TryParse(ValueString, out result))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 尝试作为double值
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool TryAsDouble(out double result)
        {
            if (string.IsNullOrEmpty(ValueString))
            {
                result = default;
            }
            else if (double.TryParse(ValueString, out result))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 尝试作为float值
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool TryAsFloat(out float result)
        {
            if (string.IsNullOrEmpty(ValueString))
            {
                result = default;
            }
            else if (float.TryParse(ValueString, out result))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 尝试作为long值
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool TryAsLong(out long result)
        {
            if (string.IsNullOrEmpty(ValueString))
            {
                result = default;
            }
            else if (long.TryParse(ValueString, out result))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 设置的默认值字符串转换为特定的类型返回
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T? AsValue<T>(T? defaultValue = default)
        {
            if (typeof(T) == typeof(string))
            {
                return (T?)(object?)ValueString;
            }
            object? obj = AsValue(typeof(T), null);
            return obj != null ? (T)obj : defaultValue;
        }
        /// <summary>
        /// 设置的默认值字符串转换为特定的类型返回
        /// </summary>
        /// <param name="type"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public object? AsValue(Type type, object? defaultValue = null)
        {
            if (type == typeof(string))
            {
                return ValueString;
            }
            else if (string.IsNullOrEmpty(ValueString))
            {// 空值判断
                return defaultValue;
            }
            var obj = Common_Util.String.StringConvertHelper.Convert(ValueString, type, out bool isSuccess);
            return isSuccess ? obj : defaultValue;
        }

        /// <summary>
        /// 将设定的默认值字符串以 Json 字符串的形式转换为特定值返回
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T? AsJson<T>(T? defaultValue = default)
        {
            if (string.IsNullOrEmpty(ValueString)) return defaultValue;
            return String.JsonHelper.Deserialize(ValueString, defaultValue);
        }
    }
}
