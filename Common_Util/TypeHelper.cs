using Common_Util.Exceptions.General;
using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using static System.Collections.Specialized.BitVector32;

namespace Common_Util
{
    /// <summary>
    /// 类型相关帮助类
    /// </summary>
    public static class TypeHelper
    {
        /// <summary>
        /// 判断是否内置类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsBuiltInType(Type type)
        {
            return (type == typeof(object) || Type.GetTypeCode(type) != TypeCode.Object);
        }


        /// <summary>
        /// 检查是否包含无参公共构造函数
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool ExistNonParamPublicConstructor(Type type)
        {
            bool result = false;
            ConstructorInfo[] infoArray = type.GetConstructors();
            foreach (ConstructorInfo info in infoArray)
            {
                if (info.IsPublic && info.GetParameters().Length == 0)
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// 检查输入的泛型类型是否是指定泛型定义
        /// </summary>
        /// <remarks>
        /// 判断 <paramref name="checkType"/> 是否一个原型为 <paramref name="definition"/> 的泛型类型 <br/>
        /// 如果是实现了该泛型原型的另一类型, 则会返回 <see langword="false"/>, 例如: <br/>
        /// 现有泛型原型: interface A&lt;T&gt; {} <br/>
        /// 1. 如果检查类型是: A&lt;<see langword="int"/>&gt;, 则将返回 <see langword="true"/> <br/>
        /// 2. 定义类型 class B : A&lt;<see langword="int"/>&gt;, 传入检查类型: B, 则将返回 <see langword="false"/> <br/>
        /// 3. 定义类型 class C&lt;T&gt; : A&lt;T&gt;, 传入检查类型: C&lt;<see langword="int"/>&gt;, 则将返回 <see langword="false"/> <br/>
        /// </remarks>
        /// <param name="checkType">将被检查的类型</param>
        /// <param name="definition">泛型定义</param>
        /// <returns></returns>
        public static bool GenericTypeIsDefinitionFrom(
            Type checkType, 
            Type definition)
        {
            if (checkType.IsGenericType
                && !checkType.IsGenericTypeDefinition
                && definition.IsGenericTypeDefinition)
            {// 输入类型是泛型 并且 输入泛型定义是泛型定义
                return checkType.GetGenericTypeDefinition() == definition;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 检查输入的泛型类型是否实现指定泛型接口
        /// </summary>
        /// <remarks>
        /// 仅当传入类型 <paramref name="checkType"/> 是泛型类型 (非泛型定义), 且实现的所有接口中, 存在任一泛型接口的原型是 <paramref name="definition"/> 时, 返回 <see langword="true"/>
        /// </remarks>
        /// <param name="checkType">将被检查的类型</param>
        /// <param name="definition">泛型定义</param>
        /// <returns></returns>
        public static bool GenericTypeIsImplementDefinition(
            Type checkType,
            Type definition)
        {
            if (checkType.IsGenericType
                && !checkType.IsGenericTypeDefinition
                && definition.IsGenericTypeDefinition)
            {// 输入类型是泛型 并且 输入泛型定义是泛型定义
                return Array.Exists(
                    checkType.GetInterfaces(),
                    i => i.IsGenericType && i.GetGenericTypeDefinition() == definition); ;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 检查输入类型实现的接口中, 是否存在原型为指定泛型定义的接口
        /// </summary>
        /// <param name="checkType">将被检查的类型</param>
        /// <param name="definition">泛型定义</param>
        /// <returns></returns>
        public static bool ExistInterfaceIsDefinitionFrom(
            Type checkType,
            Type definition)
        {
            return Array.Exists(
                checkType.GetInterfaces(),
                i => i.IsGenericType && i.GetGenericTypeDefinition() == definition);
        }

        /// <summary>
        /// 检查输入类型实现的接口中, 是否存在原型为指定泛型定义的接口, 并将其中首个符合此条件的接口赋值到 <paramref name="firstMatchInterface"/>
        /// </summary>
        /// <param name="checkType"></param>
        /// <param name="definition"></param>
        /// <param name="firstMatchInterface"></param>
        /// <returns></returns>
        public static bool ExistInterfaceIsDefinitionFrom(
            Type checkType, Type definition, [NotNullWhen(true)] out Type? firstMatchInterface)
        {
            Type[] interfaces = checkType.GetInterfaces();
            foreach (var iType in interfaces)
            {
                if (iType.IsGenericType && iType.GetGenericTypeDefinition() == definition) 
                {
                    firstMatchInterface = iType;
                    return true;
                }
            }
            firstMatchInterface = null;
            return false;
        }
        /// <summary>
        /// 检查输入类型实现的接口中, 是否存在原型为指定泛型定义的接口, 并将其中符合此条件的接口全部作为数组赋值到 <paramref name="matchInterface"/>
        /// </summary>
        /// <param name="checkType"></param>
        /// <param name="definition"></param>
        /// <param name="matchInterface"></param>
        /// <returns></returns>
        public static bool ExistInterfaceIsDefinitionFrom(
            Type checkType, Type definition, out Type[] matchInterface)
        {
            Type[] interfaces = checkType.GetInterfaces();
            List<Type> matches = new(interfaces.Length);
            foreach (var iType in interfaces)
            {
                if (iType.IsGenericType && iType.GetGenericTypeDefinition() == definition)
                {
                    matches.Add(iType);
                    break;
                }
            }
            matchInterface = matches.ToArray();
            return matches.Count > 0;
        }

        /// <summary>
        /// 调用类型的公共无参构造方法, 返回由此创建的对象, 如果类型没有公共无参构造方法, 返回null
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object? InvokePublicNoParamConsturctor(Type type)
        {
            if (type != null && type.IsClass && type.HavePublicEmptyCtor())
            {
                return Activator.CreateInstance(type);
                /*ConstructorInfo c = type.GetConstructor(Array.Empty<Type>());
                if (c != null && c.IsPublic) 
                {
                    return c.Invoke(Array.Empty<object>());
                }
                */
            }
            return null;
        }

        /// <summary>
        /// 检查输入类型是否存在指定特性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="provider"></param>
        /// <param name="inherit">是否包含继承的特性</param>
        /// <returns></returns>
        public static bool CheckExistAttribute<T>(ICustomAttributeProvider provider, bool inherit = true)
            where T : System.Attribute
        {
            return provider != null && Array.Exists(
                provider.GetCustomAttributes(inherit),
                item => item.GetType() == typeof(T));
        }

        /// <summary>
        /// 检查输入类型是否存在指定特性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="provider"></param>
        /// <param name="attribute"></param>
        /// <param name="inherit">是否包含继承的特性</param>
        /// <returns></returns>
        public static bool CheckExistAttribute<T>(
            ICustomAttributeProvider provider, [NotNullWhen(true)] out T? attribute, bool inherit = true)
            where T : System.Attribute
        {
            if (provider != null) 
            {
                var attributes = provider.GetCustomAttributes(inherit);
                var obj = attributes.ToList().FirstOrDefault(item => item.GetType() == typeof(T));
                if (obj != null && obj is T t) 
                {
                    attribute = t;
                    return true;
                } 
            }
            attribute = null;
            return false;
        }


        #region 类型的单例对象
        private static Dictionary<Type, object> _singletonInstances 
            = new Dictionary<Type, object>();
        private static Dictionary<Type, object> _singletonTypeLocker 
            = new Dictionary<Type, object>();
        private static readonly object _singletonTypeGetLockerLocker = new object();
        private static object _getSingletonTypeLocker(Type type)
        {
            if (_singletonTypeLocker.ContainsKey(type))
            {
                return _singletonTypeLocker[type];
            }
            else
            {
                lock (_singletonTypeGetLockerLocker)
                {
                    if (_singletonTypeLocker.ContainsKey(type))
                    {
                        return _singletonTypeLocker[type];
                    }
                    else
                    {
                        object obj = new object();
                        _singletonTypeLocker.Add(type, obj);
                        return obj;
                    }
                }
            }
        }

        /// <summary>
        /// 取得指定类型的单例对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="TypeNotSupportedException"></exception>
        public static T Singleton<T>()
        {
            Type type = typeof(T);
            if (_singletonInstances.ContainsKey(type))
            {
                return (T)_singletonInstances[type];
            }
            else
            {
                lock (_getSingletonTypeLocker(type))
                {
                    if (_singletonInstances.ContainsKey(type))
                    {
                        return (T)_singletonInstances[type];
                    }
                    else
                    {
                        ConstructorInfo[] ctors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (ctors.Length == 0)
                        {
                            throw new TypeNotSupportedException(type, $"未找到构造函数");
                        }
                        ConstructorInfo? matchCtor = ctors.FirstOrDefault(i => i.GetParameters().Length == 0);
                        if (matchCtor == null)
                        {
                            throw new TypeNotSupportedException(type, $"未找到无参构造函数");
                        }
                        object instance;
                        if (matchCtor.IsPublic)
                        {
                            instance = Activator.CreateInstance(type)!;
                        }
                        else
                        {
                            instance = matchCtor.Invoke(null);
                        }
                        _singletonInstances.Add(type, instance);
                        return (T)instance;
                    }

                }
            }
        }
        #endregion

        #region 程序集遍历
        /// <summary>
        /// 遍历当前域内的所有类型
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Type> CurrentDomainAllType()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var Types = assembly.GetTypes();
                foreach (var type in Types)
                {
                    yield return type;
                }
            }
        }
        /// <summary>
        /// 遍历当前域内的所有类型, 对每一个类型执行输入的内容
        /// </summary>
        /// <param name="action"></param>
        public static void ForeachCurrentDomainType(Action<Type> action)
        {
            foreach (var type in CurrentDomainAllType())
            {
                action.Invoke(type);
            }
        }
        /// <summary>
        /// 遍历当前域内的所有类型, 对每一个类型执行输入的内容, 由返回值决定是否中断
        /// </summary>
        /// <param name="func">返回值: <see langword="true"/>:中断遍历</param>
        public static void ForeachCurrentDomainType(Func<Type, bool> func)
        {
            bool breakFlag;
            foreach (var type in CurrentDomainAllType())
            {
                breakFlag = func.Invoke(type);
                if (breakFlag) return;
            }
        }

        #endregion

        
    }
}
