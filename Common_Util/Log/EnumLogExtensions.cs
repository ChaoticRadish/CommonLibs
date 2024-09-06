using Common_Util.Attributes.General;
using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Log
{
    public class EnumLogHelper<T> where T : Enum
    {
        private static bool initDone = false;
        private static readonly Dictionary<T, EnumLogConfig<T>> Configs = new Dictionary<T, EnumLogConfig<T>>();
        private static readonly Dictionary<T, DealFunc> DealFuncs = new Dictionary<T, DealFunc>();
        private static Func<T, LogData, LogData?>? DefaultDealFunc;

        internal struct DealFunc
        {
            public object? Target { get; set; }
            public MethodInfo Method { get; set; }

            public static implicit operator DealFunc((object Target, MethodInfo Method) funcArgs)
            {
                return new DealFunc()
                {
                    Method = funcArgs.Method,
                    Target = funcArgs.Target,
                };
            }
            public static implicit operator DealFunc(Func<LogData, LogData?> func)
            {
                return new DealFunc()
                {
                    Method = func.Method,
                    Target = func.Target,
                };
            }
        }

        /// <summary>
        /// 初始器初始方法可以被识别到的名字
        /// </summary>
        private static readonly List<string> InitiatorInitMethodNames = new List<string>()
        {
            "init", "初始化",
        }.Select(i => i.ToLower()).ToList();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Init()
        {
            lock (_initLocker)
            {
                if (initDone) return;
                _init();
            }
        }
        private static readonly object _initLocker = new();
        private static void _init()
        {
            Type type = typeof(T);

            FieldInfo[] fields = type.GetFields();
            foreach (var field in fields)
            {
                if (field.ExistCustomAttribute<LoggerAttribute>(out var att))
                {
                    if (att == null) continue;
                    object? obj = field.GetValue(null);
                    if (obj == null)
                    {
                        continue;
                    }
                    T value = (T)obj;
                    EnumLogConfig<T> config = new EnumLogConfig<T>()
                    {
                        Category = att.Category,
                        SubCategory = att.SubCategory,
                        Enable = att.Enable,
                        Value = value,
                    };
                    Configs.Add(value, config);
                }

            }
            if (type.ExistCustomAttribute<LoggerInitiatorAttribute>(out var att_initiator))
            {
                if (att_initiator != null && att_initiator.InitiatorType != null)
                {
                    MethodInfo[] methods = att_initiator.InitiatorType.GetMethods(BindingFlags.Public | BindingFlags.Static);
                    foreach (MethodInfo method in methods)
                    {
                        if (method.GetParameters().Length != 0)
                        {
                            continue;
                        }
                        if (InitiatorInitMethodNames.Contains(method.Name.ToLower()))
                        {
                            if (!method.ExistCustomAttribute<InitMethodAttribute>(out var att_initMethod)
                                || att_initMethod == null
                                || !att_initMethod.Ignore)
                            {
                                method.Invoke(null, null);
                            }
                        }
                        else if (method.ExistCustomAttribute<InitMethodAttribute>(out var att_initMethod))
                        {
                            if (att_initMethod != null && !att_initMethod.Ignore)
                            {
                                method.Invoke(null, null);
                            }
                        }
                    }
                }

            }

            initDone = true;
        }

        /// <summary>
        /// 取得指定枚举值对应的配置
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static EnumLogConfig<T>? GetConfig(T value)
        {
            if (!initDone)
            {
                Init();
            }
            return Configs.ContainsKey(value) ? Configs[value] : null;
        }

        internal static DealFunc? GetDealFunc(T value)
        {
            return DealFuncs.ContainsKey(value) ? DealFuncs[value] : (DealFunc?)null;
        }
        internal static Func<T, LogData, LogData?>? GetDefaultDealFunc()
        {
            return DefaultDealFunc;
        }
        /// <summary>
        /// 设置特定枚举值使用指定的处理函数
        /// </summary>
        /// <param name="value"></param>
        /// <param name="func"></param>
        public static void SetDealFunc(T value, Func<LogData, LogData?> func)
        {
            if (DealFuncs.ContainsKey(value))
            {
                DealFuncs[value] = func;
            }
            else
            {
                DealFuncs.Add(value, func);
            }
        }
        /// <summary>
        /// 设置默认情况下的处理函数
        /// </summary>
        /// <param name="func"></param>
        public static void SetDealFunc(Func<T, LogData, LogData?> func)
        {
            DefaultDealFunc = func;
        }
    }
    internal struct EnumLogConfig<T>
    { 
        public T Value { get; set; }

        public bool Enable { get; set; }

        // public string Level { get; set; }

        public string Category { get; set; }

        public string SubCategory { get; set; }
    }
    public static class EnumLogExtensions
    {

        public static void InitEnumLogHelper<T>()
            where T : Enum
        {
            EnumLogHelper<T>.Init();
        }


        public static void SetDealFunc<T>(this T @enum, Func<LogData, LogData?> func)
            where T : Enum
        {
            EnumLogHelper<T>.SetDealFunc(@enum, func);
        }
        private static LogData? Deal<T>(T @enum, LogData data)
            where T : Enum
        {
            var method = EnumLogHelper<T>.GetDealFunc(@enum);
            if (method == null)
            {
                var defaultMethod = EnumLogHelper<T>.GetDefaultDealFunc();
                return defaultMethod?.Invoke(@enum, data) ?? data;
            }
            else
            {
                try
                {
                    object? obj = method.Value.Method.Invoke(method.Value.Target, new object[] { data });
                    if (obj == null) return null;
                    if (obj is LogData logData)
                    {
                        return logData;
                    }
                    else
                    {
                        return data;
                    }
                }
                catch
                {
                    return data;
                }
            }
        }


        public static void Info<T>(this T @enum, string message) 
            where T : Enum
        {
            EnumLogConfig<T>? config = EnumLogHelper<T>.GetConfig(@enum);
            if (config == null || !config.Value.Enable) return;
            LogData? data = LogData.CreateNonTrace(nameof(Info), config.Value.Category, config.Value.SubCategory, message, null);
            data = Deal(@enum, data);
            if (data != null)
            {
                GlobalLoggerManager.Log(data);
            }
        }
        public static void Debug<T>(this T @enum, string message)
            where T : Enum
        {
            EnumLogConfig<T>? config = EnumLogHelper<T>.GetConfig(@enum);
            if (config == null || !config.Value.Enable) return;
            LogData? data = LogData.CreateNonTrace(nameof(Debug), config.Value.Category, config.Value.SubCategory, message, null);
            data = Deal(@enum, data);
            if (data != null)
            {
                GlobalLoggerManager.Log(data);
            }
        }
        public static void Warning<T>(this T @enum, string message, Exception? ex = null, bool logTrack = false)
            where T : Enum
        {
            Warning(@enum, message, ex, logTrack, 1);
        }
        public static void Warning<T>(this T @enum, string message, Exception? ex, bool logTrack, int depth)
            where T : Enum
        {
            EnumLogConfig<T>? config = EnumLogHelper<T>.GetConfig(@enum);
            if (config == null || !config.Value.Enable) return;
            LogData? data;
            if (logTrack)
            {
                data = LogData.Create(nameof(Warning), config.Value.Category, config.Value.SubCategory, message, ex, depth + 1);
            }
            else
            {
                data = LogData.CreateNonTrace(nameof(Warning), config.Value.Category, config.Value.SubCategory, message, ex);
            }
            data = Deal(@enum, data);
            if (data != null)
            {
                GlobalLoggerManager.Log(data);
            }
        }
        
        
        public static void Error<T>(this T @enum, string message, Exception? ex = null)
            where T : Enum
        {
            Error(@enum, message, ex, 1);
        }
        public static void Error<T>(this T @enum, string message, Exception? ex, int depth)
            where T : Enum
        {
            EnumLogConfig<T>? config = EnumLogHelper<T>.GetConfig(@enum);
            if (config == null || !config.Value.Enable) return;
            LogData? data = LogData.Create(nameof(Error), config.Value.Category, config.Value.SubCategory, message, ex, depth + 1);
            data = Deal(@enum, data);
            if (data != null)
            {
                GlobalLoggerManager.Log(data);
            }
        }


        public static void Fatal<T>(this T @enum, string message, Exception? ex = null)
            where T : Enum
        {
            Fatal(@enum, message, ex, 1);
        }
        public static void Fatal<T>(this T @enum, string message, Exception? ex, int depth)
            where T : Enum
        {
            EnumLogConfig<T>? config = EnumLogHelper<T>.GetConfig(@enum);
            if (config == null || !config.Value.Enable) return;
            LogData? data = LogData.Create(nameof(Fatal), config.Value.Category, config.Value.SubCategory, message, ex, depth + 1);
            data = Deal(@enum, data);
            if (data != null)
            {
                GlobalLoggerManager.Log(data);
            }
        }
    }
}
