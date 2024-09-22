using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Extensions
{
    public static class TypeExtensions
    {

        #region 枚举类型检查
        /// <summary>
        /// 判断类型是否枚举类型
        /// </summary>
        /// <param name="type"></param>
        /// <param name="allowNullable">是否允许可空的枚举类型</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEnum(this Type type, bool allowNullable = true)
        {
            return type.IsEnum || (allowNullable && type.IsNullable(out var t) && t!.IsEnum);
        }
        #endregion

        #region 集合/列表等类型的检查/判断
        /// <summary>
        /// 判断类型是否列表
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsList(this Type type)
        {
            if (type != null && type.IsGenericType)
            {
                return type.BaseFrom(typeof(IList<>));
            }

            return false;
        }
        /// <summary>
        /// 判断类型是否可枚举类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsEnumerable(this Type type)
        {
            if (type != null && type.IsGenericType)
            {
                return type.BaseFrom(typeof(IEnumerable<>));
            }

            return false;
        }

        /// <summary>
        /// 判断类型是否字典
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsDictionary(this Type type)
        {
            if (type != null && type.IsGenericType)
            {
                return type.BaseFrom(typeof(IDictionary<,>));
            }

            return false;
        }
        #endregion

        #region 可空类型的相关方法
        /// <summary>
        /// 判断传入类型是否可空类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullable(this Type type)
        {
            return NullableTarget(type) != null;
        }
        /// <summary>
        /// 判断传入类型是否为可空类型
        /// </summary>
        /// <param name="type"></param>
        /// <param name="targetType">如果是可空类型, 此值为目标类型</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullable(this Type type, out Type? targetType)
        {
            return (targetType = NullableTarget(type)) != null;
        }
        /// <summary>
        /// 取得可空类型的目标类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns>null => 不是可空类型</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type? NullableTarget(this Type type)
        {
            if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Nullable<>)) return null;
            else
            {
                var args = type.GetGenericArguments();
                if (args.Length == 0) return null;
                return args[0];
            }
        }
        #endregion

        #region 查找静态属性或方法等
        /// <summary>
        /// 查找类型为 <typeparamref name="TTarget"/> 的静态公共属性并获取其值
        /// </summary>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="type"></param>
        /// <param name="name">想要查找的属性名</param>
        /// <param name="value">静态属性的值</param>
        /// <returns></returns>
        public static bool TryFindPublicStaticProperty<TTarget>(this Type type, string name, [NotNullWhen(true)] out TTarget? value)
        {
            PropertyInfo? property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Static);
            if (property == null)
            {
                value = default;
                return false;
            }
            if (property.PropertyType != typeof(TTarget))
            {
                value = default;
                return false;
            }
            var obj = property.GetValue(null);
            if (obj is TTarget target)
            {
                value = target;
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }
        /// <summary>
        /// 查找返回类型为 <paramref name="returnType"/> 的无参非泛型静态公共方法
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name">想要查找的方法名</param>
        /// <param name="returnType">期望的返回值类型</param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static bool TryFindPublicStaticMethod(this Type type, string name, Type returnType, [NotNullWhen(true)] out MethodInfo? method)
        {
            method = type.GetMethod(name, BindingFlags.Public | BindingFlags.Static);
            if (method == null)
            {
                return false;
            }
            else if (method.ReturnType != returnType)
            {
                method = null;
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 查找没有返回值的无参非泛型静态公共方法, 并调用它
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="type"></param>
        /// <param name="name">想要查找的方法名</param>
        /// <returns>调用成功时, 返回 <see langword="true"/></returns>
        public static bool TryFindPublicStaticMethodAndInvoke(this Type type, string name)
        {
            MethodInfo? method = type.GetMethod(name, BindingFlags.Public | BindingFlags.Static);
            if (method == null)
            {
                return false;
            }
            else if (method.ReturnType != typeof(void))
            {
                return false;
            }
            method.Invoke(null, null);
            return true;
        }

        /// <summary>
        /// 查找返回类型为 <typeparamref name="TReturn"/> 的无参非泛型静态公共方法, 并调用它, 获取其返回值
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="type"></param>
        /// <param name="name">想要查找的方法名</param>
        /// <param name="returnValue">方法返回值</param>
        /// <returns>成功调用目标方法, 且取得预期类型的返回值时, 返回 <see langword="true"/></returns>
        public static bool TryFindPublicStaticMethodAndInvoke<TReturn>(this Type type, string name, [NotNullWhen(true)] out TReturn? returnValue)
        {
            MethodInfo? method = type.GetMethod(name, BindingFlags.Public | BindingFlags.Static);
            if (method == null)
            {
                returnValue = default;
                return false;
            }
            if (method.ReturnType != typeof(TReturn))
            {
                returnValue = default;
                return false;
            }
            var obj = method.Invoke(null, null);
            if (obj is TReturn target)
            {
                returnValue = target;
                return true;
            }
            else
            {
                returnValue = default;
                return false;
            }
        }
        #endregion

        /// <summary>
        /// 检查输入类型是否包含无参公共构造方法
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool HavePublicEmptyCtor(this Type type)
        {
            return !type.IsAbstract && (
                type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic).Length == 0 || type.GetConstructor(Type.EmptyTypes) != null);
        }

        /// <summary>
        /// 判断类型是否基于指定类型
        /// </summary>
        /// <param name="type"></param>
        /// <param name="baseType"></param>
        /// <returns></returns>
        public static bool BaseFrom(this Type type, Type baseType)
        {
            if (type == null)
            {
                return false;
            }

            if (baseType.IsGenericTypeDefinition && type.IsGenericType && !type.IsGenericTypeDefinition)
            {
                type = type.GetGenericTypeDefinition();
            }

            if (type == baseType)
            {
                return true;
            }

            if (baseType.IsAssignableFrom(type))
            {
                return true;
            }

            bool flag = false;
            if (baseType.IsInterface && baseType.FullName != null)
            {
                if (type.GetInterface(baseType.FullName) != null)
                {
                    flag = true;
                }
                else if (type.GetInterfaces().Any((Type e) => (!e.IsGenericType || !baseType.IsGenericTypeDefinition) ? (e == baseType) : (e.GetGenericTypeDefinition() == baseType)))
                {
                    flag = true;
                }
            }

            if (!flag && type.Assembly.ReflectionOnly)
            {
                while (!flag && type != typeof(object))
                {
                    if (type != null)
                    {
                        if (type.FullName == baseType.FullName && type.AssemblyQualifiedName == baseType.AssemblyQualifiedName)
                        {
                            flag = true;
                        }

                        if (type.BaseType == null)
                        {
                            continue;
                        }
                        type = type.BaseType;
                    }
                }
            }

            return flag;
        }

        /// <summary>
        /// 尝试获取索引为 <paramref name="index"/> 的泛型参数, 如果失败则抛出异常
        /// </summary>
        /// <param name="type"></param>
        /// <param name="index">泛型参数索引</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Type GetGenericArgument(this Type type, int index = 0)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);

            if (type.IsGenericType)
            {
                var gArgs = type.GetGenericArguments();
                if (gArgs.Length == 0) throw new InvalidOperationException("取得空泛型参数数组! ");

                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, gArgs.Length);

                return gArgs[index];
            }
            else
            {
                throw new InvalidOperationException("传入类型不是泛型类型! ");
            }
        }


    }
}
