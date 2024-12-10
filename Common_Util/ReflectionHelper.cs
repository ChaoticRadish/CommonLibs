using Common_Util.Exceptions.General;
using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util
{
    /// <summary>
    /// 反射帮助类
    /// </summary>
    public static class ReflectionHelper
    {
        /// <summary>
        /// 判断方法的输入参数, 参数类型, 返回类型, 是否均一致
        /// </summary>
        /// <param name="method"></param>
        /// <param name="paramTypes">null值可代表无参</param>
        /// <param name="returnType"></param>
        /// <returns></returns>
        public static bool IsMatch(MethodInfo method, Type[]? paramTypes = null, Type? returnType = null)
        {
            return _isMatch_ParamCount(method, paramTypes) && _isMatch_ParamType(method, paramTypes) && _isMatch_ReturnType(method, returnType);
        }


        /// <summary>
        /// 判断方法的输入参数, 参数类型, 是否均一致
        /// </summary>
        /// <param name="method"></param>
        /// <param name="paramTypes">null值可代表无参</param>
        /// <returns></returns>
        public static bool IsMatchAnyReturn(MethodInfo method, Type[]? paramTypes = null)
        {
            return _isMatch_ParamCount(method, paramTypes) && _isMatch_ParamType(method, paramTypes);
        }


        private static bool _isMatch_ParamCount(MethodInfo method, Type[]? paramTypes = null)
        {
            ParameterInfo[] paramters = method.GetParameters();
            if (paramters.Length > 0)
            {
                if (paramTypes == null || paramters.Length != paramTypes.Length)
                {
                    return false;
                }
            }
            else
            {
                if (paramTypes != null && paramTypes.Length > 0)
                {
                    return false;
                }
            }
            return true;
        }
        private static bool _isMatch_ParamType(MethodInfo method, Type[]? paramTypes = null)
        {
            ParameterInfo[] paramters = method.GetParameters();
            if (paramTypes != null)
            {
                for (int i = 0; i < paramters.Length; i++)
                {
                    if (paramters[i].ParameterType != paramTypes[i])
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        private static bool _isMatch_ReturnType(MethodInfo method, Type? returnType)
        {
            if (!(returnType == method.ReturnType || (returnType == null && method.ReturnType == typeof(void))))
            {
                return false;
            }
            return true;
        }

        #region 等价比较
        /// <summary>
        /// 比较传入的两个可能包含泛型形参的类型是否等价
        /// </summary>
        /// <remarks>
        /// 传入的两个类型都为 <see langword="null"/> 时, 会视为等价 (相等), 返回 <see langword="true"/>
        /// </remarks>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <param name="normalTypeEquivalentCompareFunc">
        /// 比较两个相同位置的普通类型是否等价, 其中第一个参数来自于 <paramref name="t1"/>, 第二个来自于 <paramref name="t2"/> <br/>
        /// 当此参数为 <see langword="null"/> 时, 使用 <see cref="Type.Equals(Type?)"/> 比较是否相同, 仅相同时等价
        /// </param>
        /// <param name="gTypeDefinitionEquivalentCompareFunc">
        /// 比较两个相同位置的泛型类型的泛型定义是否等价, 其中第一个参数来自于 <paramref name="t1"/>, 第二个来自于 <paramref name="t2"/> <br/>
        /// 当此参数为 <see langword="null"/> 时, 使用 <see cref="Type.Equals(Type?)"/> 比较是否相同, 仅相同时等价
        /// </param>
        /// <param name="gParamEquivalentCompareFunc">
        /// 比较两个相同位置的泛型形参是否等价, 其中第一个参数来自于 <paramref name="t1"/>, 第二个来自于 <paramref name="t2"/> <br/>
        /// 当此参数为 <see langword="null"/> 时, 使用 <see cref="GenericParameterHasSameConstraints(Type, Type)"/> 比较是否具有相同的约束条件, 约束条件相同则相同
        /// </param>
        /// <returns></returns>
        public static bool IsEquivalent(Type? t1, Type? t2, 
            Func<Type, Type, bool>? normalTypeEquivalentCompareFunc = null,
            Func<Type, Type, bool>? gTypeDefinitionEquivalentCompareFunc = null,
            Func<Type, Type, bool>? gParamEquivalentCompareFunc = null)
        {
            if (t1 == null && t2 == null) return true;  // 均为 null, 等价
            if (t1 == null ^ t2 == null) return false;  // 只有一个为 null, 不等价

            var enumerable1 = PreorderGenericParameterTree(t1!, true);
            var enumerable2 = PreorderGenericParameterTree(t2!, true);
            if (enumerable1.Count() != enumerable2.Count()) return false;   // 可遍历数量不同, 结构必然不等价

            normalTypeEquivalentCompareFunc ??= defaultNormalTypeEquivalentCompareFunc;
            gTypeDefinitionEquivalentCompareFunc ??= defaultGenericTypeDefinitionEquivalentCompareFunc;
            gParamEquivalentCompareFunc ??= GenericParameterHasSameConstraints;


            Type[] t1GArgs, t2GArgs;    // 泛型参数列表的临时对象
            Type t1Definition, t2Definition;    // 泛型定义的临时对象
            foreach ((Type? temp1, Type? temp2) in (enumerable1, enumerable2).UntilAllAway())
            {
                ImpossibleForkException.ImpossibleNull(temp1);
                ImpossibleForkException.ImpossibleNull(temp2);

                if (temp1.IsGenericType && temp2.IsGenericType)
                {
                    t1GArgs = temp1.GetGenericArguments();
                    t2GArgs = temp2.GetGenericArguments();
                    if (t1GArgs.Length != t2GArgs.Length) return false; // 数量不等, 结构必然不等价
                    t1Definition = temp1.GetGenericTypeDefinition();
                    t2Definition = temp2.GetGenericTypeDefinition();
                    if (!gTypeDefinitionEquivalentCompareFunc!(t1Definition, t2Definition))
                    {
                        // 两个类型的泛型定义不等价
                        return false;
                    }
                }
                else if (!temp1.IsGenericType && !temp2.IsGenericType)
                {
                    if (temp1.IsGenericParameter && temp2.IsGenericParameter)
                    {
                        if (!gParamEquivalentCompareFunc!(temp1, temp2))
                        {
                            // 两个泛型形参不等价
                            return false;
                        }
                    }
                    else if (!temp1.IsGenericParameter && !temp2.IsGenericParameter)
                    {
                        if (!normalTypeEquivalentCompareFunc!(temp1, temp2))
                        {
                            // 两个普通类型不等价
                            return false;
                        }
                    }
                    else
                    {
                        // 一个是泛型形参, 一个不是, 必然不等价
                        return false;
                    }
                }
                else
                {
                    // 一个是普通类型, 一个是泛型类型, 必然不等价
                    return false;
                }
            }
            // 经过检查, 没有出现不等价
            return true;
        }

        private static bool defaultNormalTypeEquivalentCompareFunc(Type t1, Type t2)
        {
            return t1.Equals(t2);
        }
        private static bool defaultGenericTypeDefinitionEquivalentCompareFunc(Type t1, Type t2)
        {
            return t1.Equals(t2);
        }

        #endregion

        #region 泛型参数

        /// <summary>
        /// 判断传入的两个直接使用泛型类型参数作为参数类型的形参是否具有相同的约束条件
        /// </summary>
        /// <remarks>
        /// 相当于调用重载 <see cref="GenericParameterHasSameConstraints(Type, Type, Func{Type[], Type[], bool}?)"/> 时, 不传入自定义约束条件比较方法
        /// </remarks>
        /// <param name="gParam1"></param>
        /// <param name="gParam2"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]  
        public static bool GenericParameterHasSameConstraints(Type gParam1, Type gParam2)
        {
            return GenericParameterHasSameConstraints(gParam1, gParam2, null);
        }
        /// <summary>
        /// 判断传入的两个直接使用泛型类型参数作为参数类型的形参是否具有相同的约束条件
        /// </summary>
        /// <param name="gParam1"></param>
        /// <param name="gParam2"></param>
        /// <param name="customConstraintsCompaper">
        /// 自定义约束条件的比较方法, 第一个参数对应 <paramref name="gParam1"/> 的约束条件, 第二个参数对应 <paramref name="gParam2"/> 的约束条件. <br/>
        /// 如果为 <see langword="null"/>, 采用 <see cref="IEnumerableExtensions.DisorderEquals{T}(IEnumerable{T}, IEnumerable{T}, IEqualityComparer{T}?)"/> 比较是否无序相等
        /// </param>
        /// <returns></returns>
        public static bool GenericParameterHasSameConstraints(
            Type gParam1, Type gParam2, 
            Func<Type[], Type[], bool>? customConstraintsCompaper = null)
        {
            if (!gParam1.IsGenericParameter)
            {
                throw new ArgumentException($"传入参数 {gParam1} 不是泛型形参", nameof(gParam1));
            }
            if (!gParam2.IsGenericParameter)
            {
                throw new ArgumentException($"传入参数 {gParam2} 不是泛型形参", nameof(gParam2));
            }
            if (gParam1 == gParam2) return true;

            var constraints1 = gParam1.GetGenericParameterConstraints();
            var constraints2 = gParam2.GetGenericParameterConstraints();

            return customConstraintsCompaper == null ? 
                constraints1.DisorderEquals(constraints2)
                : 
                customConstraintsCompaper(constraints1, constraints2);
        }

        /// <summary>
        /// 以先根次序遍历类型中所有的类型参数
        /// </summary>
        /// <param name="type"></param>
        /// <param name="includeSelf">遍历时是否包含自身</param>
        /// <returns></returns>
        public static IEnumerable<Type> PreorderGenericParameterTree(Type type, bool includeSelf = true)
        {
            Stack<(Type type, IEnumerator<Type> gParmas)> stack = new();
            bool back = false;   // 是否正在向下一级移动
            (Type type, IEnumerator<Type> gParmas) current = (type, ((IEnumerable<Type>)type.GetGenericArguments()).GetEnumerator());
            stack.Push(current);
            while (stack.Count != 0) 
            {
                if (!back)
                {
                    if (stack.Count == 1 && !includeSelf) 
                    { 
                        // 遍历不包含传入的类型自身
                    }
                    else
                    {
                        yield return current.type;
                    }

                }

                if (current.gParmas.MoveNext())
                {
                    var next = current.gParmas.Current;
                    current = (next, ((IEnumerable<Type>)next.GetGenericArguments()).GetEnumerator());
                    stack.Push(current);
                    back = false;
                }
                else
                {
                    stack.Pop();
                    if (stack.Count == 0)
                    {
                        break;
                    }
                    current = stack.Peek();
                    back = true;
                }
            }
        }

        #endregion
    }
}
