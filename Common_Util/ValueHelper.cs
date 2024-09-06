using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Reflection;
using System.Text;
    
namespace Common_Util
{
    /// <summary>
    /// 值帮助类
    /// </summary>
    public static class ValueHelper
    {
        /// <summary>
        /// True值字符串
        /// </summary>
        public readonly static List<string> TrueStrings = new List<string>
        {
            "是", "是的", "正确", "没错", "1", "true", "ture", "y", "yes", "t", "一",
        };
        /// <summary>
        /// False值字符串
        /// </summary>
        public readonly static List<string> FalseStrings = new List<string>
        {
            "不", "不是", "错误", "0", "false", "flase", "fales", "n", "f", "no", "nope", "",
        };
        /// <summary>
        /// 判断输入的值是否为True值的字符串
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsTrueString(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            if (TrueStrings.Contains(input.Trim().ToLower()))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// 返回指定结构体类型的默认值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T StructDefaultValue<T>() where T : struct
        {
            return default;
        }
        private static MethodInfo methodInfoStructDefaultValue_T
        {
            get
            {
                if (_methodInfoStructDefaultValue_T == null)
                {
                    var methods = typeof(ValueHelper).GetMethods(BindingFlags.Public | BindingFlags.Static);
                    foreach (var method in methods.Where(i => i.Name == nameof(StructDefaultValue) && i.GetParameters().Length == 0))
                    {
                        if (method.IsGenericMethod && method.GetGenericArguments().Length == 1)
                        {
                            _methodInfoStructDefaultValue_T = method;
                        }
                    }
                }
                return _methodInfoStructDefaultValue_T!;
            }
        }
        private static MethodInfo? _methodInfoStructDefaultValue_T;


        /// <summary>
        /// 返回指定类型的默认值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T? DefaultValue<T>()
        {
            return default;
        }
        private static MethodInfo methodInfoDefaultValue_T
        {
            get
            {
                if (_methodInfoDefaultValue_T == null)
                {
                    var methods = typeof(ValueHelper).GetMethods(BindingFlags.Public | BindingFlags.Static);
                    foreach (var method in methods.Where(i => i.Name == nameof(DefaultValue) && i.GetParameters().Length == 0))
                    {
                        if (method.IsGenericMethod && method.GetGenericArguments().Length == 1)
                        {
                            _methodInfoDefaultValue_T = method;
                        }
                    }
                }
                return _methodInfoDefaultValue_T!;
            }
        }
        private static MethodInfo? _methodInfoDefaultValue_T;

        /// <summary>
        /// 返回指定结构体类型的默认值
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object StructDefaultValue(Type type)
        {
            var method = methodInfoStructDefaultValue_T.MakeGenericMethod(type);
            return method.Invoke(null, null) ?? throw new Exception("获取结构体默认值异常取得 null 值! ");
        }
        /// <summary>
        /// 返回指定类型的默认值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static object? DefaultValue(Type type)
        {
            var method = methodInfoDefaultValue_T.MakeGenericMethod(type);
            return method.Invoke(null, null);
        }

    }
}
