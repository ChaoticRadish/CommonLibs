using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Module.DynamicIL
{
    public static class DuckType
    {
        /// <summary>
        /// 判断类型是否符合鸭子标准时的规则
        /// </summary>
        [Flags]
        public enum MatchingRule : int
        {
            /// <summary>
            /// 精准匹配. 要求类型与方法的泛型参数, 方法的形参, 返回值等的类型与鸭子标准相同
            /// </summary>
            Exact = 0b0001,
            /// <summary>
            /// 可分配. 要求类型与方法的泛型参数, 方法的形参, 返回值等的类型在使用时隐式转换即可满足需求
            /// </summary>
            /// <remarks>
            /// 形参 => 形参类型可<b>被分配</b>为鸭子标准的对应形参类型<br/>
            /// -------- i. 当需检查方法的形参具有默认值时, 允许缺省 <br/>
            /// -------- ii. 当需检查方法的形参数量少于鸭子标准时, 且少的这部分形参对应的鸭子标准形参不是 <see langword="out"/> 参数时, 允许抛弃 <br/>
            /// -------- iii. 泛型形参对应的泛型参数索引和泛型约束, 与鸭子标准对应的泛型形参相同时, 即视为等价 <br/>
            /// 返回值 => 返回值类型可<b>分配</b>到鸭子标准的返回值类型, <br/>
            /// -------- i. 当鸭子标准没有返回值时, 将视为匹配 <br/>
            /// </remarks>
            Assignable = 0b0010,
        }

        #region 判断

        /// <summary>
        /// 判断 <typeparamref name="T"/> 是否看着像 <typeparamref name="TDuck"/>
        /// </summary>
        /// <typeparam name="T">需要判断的类型</typeparam>
        /// <typeparam name="TDuck">特定的接口, 即鸭子的定义, 可以被当做鸭子的标准</typeparam>
        /// <param name="rule">判断规则</param>
        /// <returns></returns>
        public static bool LooksLike<T, TDuck>(MatchingRule rule = MatchingRule.Exact)
        {
            return LooksLike(typeof(T), typeof(TDuck), rule);
        }
        /// <summary>
        /// 判断 <paramref name="type"/> 是否看着像 <paramref name="duckInterface"/>
        /// </summary>
        /// <param name="type">需要判断的类型</param>
        /// <param name="duckInterface">特定的接口, 即鸭子的定义, 可以被当做鸭子的标准</param>
        /// <param name="rule">判断规则</param>
        /// <returns></returns>
        public static bool LooksLike(Type type, Type duckInterface, MatchingRule rule = MatchingRule.Exact)
        {
            if (!duckInterface.IsInterface)
                throw new ArgumentException("鸭子标准的定义类型必须是接口! ", nameof(duckInterface));

            throw new NotImplementedException();
        }
        /// <summary>
        /// 判断一个方法是否看着像鸭子标准中的方法
        /// </summary>
        /// <param name="method"></param>
        /// <param name="duckStandard"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        private static bool looksLike(MethodInfo method, MethodInfo duckStandard, MatchingRule rule)
        {
            if (method.Name != duckStandard.Name) return false;

            var matchingMode = (MatchingRule)((int)rule & 0b1111);
            var params1 = method.GetParameters();
            var params2 = method.GetParameters();
            switch (matchingMode)
            {
                case MatchingRule.Exact:
                    if (method.ReturnType != duckStandard.ReturnType) return false;
                    if (!params1.SequenceEqual(params2, lazyParameterInfoEC.Value)) return false;  // 检查两者形参列表的顺序, 类型均是否一致
                    break;
                case MatchingRule.Assignable:
                    if (duckStandard.ReturnType != typeof(void) // 无返回值时, 需要检查的类型允许返回任意类型, 所以只检查有返回值的情况
                        && !method.ReturnType.IsAssignableTo(duckStandard.ReturnType)   // 方法返回值无法提供给鸭子接口作为返回值
                        )
                    {
                        return false;
                    }
                    int pCountMax = Math.Max(params1.Length, params2.Length);
                    for (int i =  0; i < pCountMax; i++)
                    {
                        var param1 = i < params1.Length ? params2[i] : null;
                        var param2 = i < params2.Length ? params2[i] : null;
                        if (param1 == null && param2 == null) continue;
                        else if (param1 == null && param2 != null)
                        {
                            if (param2.IsOut) return false; // 形参列表少于鸭子标准的形参列表, 且少的形参出现了 out 参数
                            else continue;
                        }
                        else if (param1 != null && param2 == null)
                        {
                            if (param1.HasDefaultValue) continue;
                            else return false;
                        }
                        else
                        {

                        }
                        
                    }
                    break;
                default:    // 未知的匹配模式统一返回失败
                    return false;
            }

            throw new NotImplementedException();

        }

        /// <summary>
        /// 判断两个形参是否等价, 如果是泛型形参, 则约束条件相同即可
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private static bool isEquivalent(ParameterInfo a, ParameterInfo b)
        {
            if (a.IsOut != b.IsOut) return false;
            if (a.ParameterType.IsGenericParameter != b.ParameterType.IsGenericParameter) return false;
            if (a.ParameterType.IsGenericParameter)
            {
                return ReflectionHelper.GenericParameterHasSameConstraints(a.ParameterType, b.ParameterType);
            }
            else
            {
                return a.ParameterType == b.ParameterType;
            }
        }
        private readonly static Lazy<IEqualityComparer<ParameterInfo>> lazyParameterInfoEC = new(() =>
        {
            return EqualityComparer<ParameterInfo>.Create(
                (a, b) =>
                {
                    if (a != null && b != null)
                    {
                        return isEquivalent(a, b);
                    }
                    else
                    {
                        return a?.ParameterType == b?.ParameterType;
                    }
                }, 
                p => p.GetHashCode());
        });
        #endregion

        //public static bool TryWrapper<TInterface>(object obj, [NotNullWhen(true)] out TInterface? duck)
        //{
        //    if (!typeof(TInterface).IsInterface) 
        //        throw new ArgumentException($"泛型参数 {nameof(TInterface)} 必须是一个接口", nameof(TInterface));
        //    if (obj == null)
        //    {
        //        duck = default;
        //        return false;
        //    }
        //} 
    }
}
