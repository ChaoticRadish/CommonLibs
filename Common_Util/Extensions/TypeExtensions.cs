using System;
using System.Collections.Generic;
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
    }
}
