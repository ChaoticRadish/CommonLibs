using ChaoticKit.Data.Structure.Pair;
using ChaoticKit.Exceptions.General;
using ChaoticKit.Extensions;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace ChaoticKit.Module.DynamicIL
{
    /// <summary>
    /// 鸭子类型的相关操作
    /// </summary>
    /// <remarks>
    /// 目的: 目标类型( <see langword="class"/> 或 <see langword="struct"/>, 也可以是一个接口对象, 在类型定义上可以不直接实现鸭子类型) -- 当做 --> 鸭子类型(接口) 来使用 <br/>
    /// 鸭子类型相当于一个协议, 也可被称为 "鸭子标准" <br/>
    /// 最低必要条件 (即 <see cref="MatchingRule.Assignable"/>): <br/>
    /// 1. 鸭子类型是一个接口 <br/>
    /// 2. 鸭子类型的每一个方法, 在目标类型中都有一个对应的方法 <br/>
    /// 3. 鸭子类型的每一个方法, 都需要在无额外数据输入的情况下, 能够调用目标类型中的对应方法 (如果目标类型形参具有默认值, 也可以不经由鸭子类型传入. 类型只要能够隐式转换为对应位置的参数即可) <br/>
    /// 4. 目标类型的这些对应的方法, 除了鸭子标准不要求返回值外, 其返回值需要能够作为鸭子类型方法的返回值返回 <br/>
    /// 5. 如果鸭子类型是一个泛型类型, 目标类型可以是泛型类型, 也可以是普通类型 (这些情况下需要以形参默认值的方式传递给目标类型的方法, 并且缺少的泛型类型不能作为目标类型方法的返回值).  <br/>
    /// 在相对应的方法中, 目标类型方法所需的泛型参数, 鸭子类型需要能够全部提供到位 <br/>
    /// 6. 如果鸭子类型不是一个泛型类型, 目标类型就必须是普通类型, 因为这种时候可能出现目标类型的方法要求一个泛型参数, 鸭子类型的方法却无法提供的情况 <br/>
    /// </remarks>
    public static class DuckType
    {
        /// <summary>
        /// 判断类型是否符合鸭子标准时的规则
        /// </summary>
        [Flags]
        public enum MatchingRule : int
        {
            /// <summary>
            /// 基本设置的枚举数据段落
            /// </summary>
            BaseSettingSegment = 0b1111,
            /// <summary>
            /// 精准匹配. 要求类型与方法的泛型参数, 方法的形参, 返回值等的类型与鸭子标准同构等价
            /// </summary>
            /// <remarks>
            /// 类型本身的泛型形参列表需要与鸭子标准的泛型形参列表同构等价
            /// </remarks>
            Exact = 0b0001,
            /// <summary>
            /// 可分配. 要求类型与方法的泛型参数, 方法的形参, 返回值等的类型在使用时隐式转换即可满足需求
            /// </summary>
            /// <remarks>
            /// 类型本身的泛型形参列表需要与鸭子标准的泛型形参列表相等, 因为指不定什么时候会用于输入值或输出值, 必须相等才能避免无法转换
            /// 形参 => 形参类型可<b>被分配</b>为鸭子标准的对应形参类型<br/>
            /// -------- i. 当需检查方法的形参具有默认值时, 允许缺省 <br/>
            /// -------- ii. 当需检查方法的形参数量少于鸭子标准时, 且少的这部分形参对应的鸭子标准形参不是 <see langword="out"/> 参数时, 允许抛弃 <br/>
            /// -------- iii. 泛型形参对应的泛型参数索引和泛型约束, 与鸭子标准对应的泛型形参相同时, 即视为等价 <br/>
            /// 返回值 => 返回值类型可<b>分配</b>到鸭子标准的返回值类型, <br/>
            /// -------- i. 当鸭子标准没有返回值时, 将视为匹配 <br/>
            /// </remarks>
            Assignable = 0b0010,

            #region 匹配设置
            /// <summary>
            /// 匹配设置的枚举数据段落
            /// </summary>
            /// <remarks>
            /// 搜索优先级为: <see cref="Match2Custom"/> &gt; <see cref="Match2Instance"/> &gt; <see cref="Match2Static"/>
            /// </remarks>
            MatchSettingSegment = 0b1111_0000,
            /// <summary>
            /// 鸭子标准中的方法匹配到静态方法
            /// </summary>
            Match2Static = 0b00001_0000,
            /// <summary>
            /// 鸭子标准中的方法匹配到实例方法
            /// </summary>
            Match2Instance = 0b0010_0000,
            /// <summary>
            /// 鸭子标准中的方法按自定义匹配方式匹配到特定的方法
            /// </summary>
            Match2Custom = 0b0100_0000,
            /// <summary>
            /// 找到匹配方法后, 另外再做一次检查, 未通过检查则抛出异常
            /// </summary>
            MatchThenCheck = 0b1000_0000,
            #endregion
        }

        #region 判断

        #region 规则
        /// <summary>
        /// 匹配规则的参数集合
        /// </summary>
        public struct MatchingRuleArgs
        {
            /// <summary>
            /// 匹配规则
            /// </summary>
            public MatchingRule Rule { get; set; }

            #region 寻找匹配方法的相关参数
            /// <summary>
            /// 寻找看着像的方法的寻找方法 
            /// </summary>
            public FindMethodDelegate? FindMethod { get; set; }
            /// <summary>
            /// 寻找看着像的方法的寻找方法时, 是否区分大小写
            /// </summary>
            public bool CaseSensitiveWhenFindMethod { get; set; }
            #endregion

            #region 泛型形参映射关系
            /// <summary>
            /// 自定义的取得对应映射关系的方法, 传入目标类型一方的信息以及所对应的泛型参数数组, 返回鸭子标准的信息
            /// </summary>
            /// <remarks>
            /// 如果为 <see langword="null"/>, 将使用 <see cref="GetGenericArgFromOtherDefaultImpl"/>
            /// </remarks>
            public GetGenericArgFromOtherDelegate? GenericArgMappingRule { get; set; }
            #endregion

            public static MatchingRuleArgs Default { get; } = new MatchingRuleArgs()
            {
                Rule = MatchingRule.Exact | MatchingRule.Match2Instance,

                FindMethod = null, // 默认条件不采用自定义, 故设为 null
                CaseSensitiveWhenFindMethod = true, // 默认情况下应区分大小写, 即方法名完全一致

                GenericArgMappingRule = null,
            };
        }
        /// <summary>
        /// 寻找看着像的方法的委托
        /// </summary>
        /// <param name="duckMethod">鸭子标准中的方法</param>
        /// <param name="instanceMethods">准备查找的公共实例方法集合</param>
        /// <param name="staticMethods">准备查找的公共静态方法集合</param>
        /// <param name="gParamMappingTidied">类型层面的泛型参数的映射关系</param>
        /// <returns>找到的对应方法, 如果为 <see langword="null"/>, 说明未能找到</returns>
        public delegate MethodInfo? FindMethodDelegate(MethodInfo duckMethod, MethodInfo[] instanceMethods, MethodInfo[] staticMethods, TypeGenericArgMappingTidied gParamMappingTidied);

        /// <summary>
        /// 自定义获取的方法委托: 由传入的另一方的泛型参数信息 <paramref name="otherGenericArgIndex"/>, 在发起获取动作的自身的泛型参数列表 <paramref name="selfGenericTypes"/>, 找到匹配项, 并以 <see cref="TypeGenericArgWithIndex"/> 形式返回
        /// </summary>
        /// <param name="selfGenericTypes">发起获取动作的自身的泛型形参和参数的列表</param>
        /// <param name="otherGenericArgIndex">另一方的泛型参数信息</param>
        /// <returns>当未能在 <paramref name="selfGenericTypes"/> 中找到相匹配的泛型参数时, 返回 <see langword="null"/></returns>
        public delegate TypeGenericArgWithIndex? GetGenericArgFromOtherDelegate((Type argType, Type paramType)[] selfGenericTypes, TypeGenericArgWithIndex otherGenericArgIndex);
        /// <summary>
        /// <see cref="GetGenericArgFromOtherDelegate"/> 委托的默认实现: 直接取相同位置索引的项, 未能取到则返回 <see langword="false"/>
        /// </summary>
        private static TypeGenericArgWithIndex? GetGenericArgFromOtherDefaultImpl((Type argType, Type paramType)[] selfGenericTypes, TypeGenericArgWithIndex otherGenericArgIndex)
        {
            if (otherGenericArgIndex.Index >= selfGenericTypes.Length) return null;
            return (otherGenericArgIndex.Index, selfGenericTypes[otherGenericArgIndex.Index]);
        }
        #endregion

        #region 方法寻找结果结构体
        /// <summary>
        /// 看着像的方法映射关系
        /// </summary>
        public struct LooksLikeMethod
        {
            /// <summary>
            /// 鸭子标准的类型
            /// </summary>
            public Type DuckInterface { get; set; }
            /// <summary>
            /// 鸭子标准中的方法
            /// </summary>
            public MethodInfo MethodInDuckInterface { get; set; }
            /// <summary>
            /// 对应的类型
            /// </summary>
            public Type TargetType { get; set; }
            /// <summary>
            /// 对应类型中的方法, 当对应类型中没有对应的方法时, 会是 <see langword="null"/>
            /// </summary>
            public MethodInfo? MethodInTargetType { get; set; }

            /// <summary>
            /// 在 <see cref="TargetType"/> 中找到了看着像 <see cref="MethodInDuckInterface"/> 的方法
            /// </summary>
            public readonly bool IsFound { get => MethodInTargetType != null; } 
        }
        #endregion

        /// <summary>
        /// 判断 <typeparamref name="T"/> 是否看着像 <typeparamref name="TDuck"/>
        /// </summary>
        /// <typeparam name="T">需要判断的类型</typeparam>
        /// <typeparam name="TDuck">特定的接口, 即鸭子的定义, 可以被当做鸭子的标准</typeparam>
        /// <param name="rule">判断规则, 如果为空, 则采用 <see cref="MatchingRuleArgs.Default"/></param>
        /// <returns></returns>
        public static bool LooksLike<T, TDuck>(MatchingRuleArgs? rule = null)
        {
            return LooksLike(typeof(T), typeof(TDuck), rule);
        }

        /// <summary>
        /// 判断 <typeparamref name="T"/> 是否看着像 <typeparamref name="TDuck"/>
        /// </summary>
        /// <typeparam name="TDuck">特定的接口, 即鸭子的定义, 可以被当做鸭子的标准</typeparam>
        /// <param name="type">需要判断的类型</typeparam>
        /// <param name="rule">判断规则, 如果为空, 则采用 <see cref="MatchingRuleArgs.Default"/></param>
        /// <returns></returns>
        public static bool LooksLike<TDuck>(Type type, MatchingRuleArgs? rule = null)
        {
            return LooksLike(type, typeof(TDuck), rule);
        }
        /// <summary>
        /// 判断 <paramref name="type"/> 是否看着像 <paramref name="duckInterface"/>
        /// </summary>
        /// <param name="type">需要判断的类型</param>
        /// <param name="duckInterface">特定的接口, 即鸭子的定义, 可以被当做鸭子的标准</param>
        /// <param name="rule">判断规则, 如果为空, 则采用 <see cref="MatchingRuleArgs.Default"/></param>
        /// <returns></returns>
        public static bool LooksLike(Type type, Type duckInterface, MatchingRuleArgs? rule = null)
        {
            return AllLooksLikeMethod(type, duckInterface, rule).All(m => m.IsFound);    // 所有的方法都可以找到相对应且看着像的方法
        }
        /// <summary>
        /// 获取 <paramref name="type"/> 中所有看着像 <paramref name="duckInterface"/> 中某一方法的方法
        /// </summary>
        /// <param name="type">被搜索方法的类型</param>
        /// <param name="duckInterface">鸭子标准的类型</param>
        /// <param name="rule">判断规则, 如果为空, 则采用 <see cref="MatchingRuleArgs.Default"/></param>
        /// <returns></returns>
        public static IEnumerable<LooksLikeMethod> AllLooksLikeMethod(Type type, Type duckInterface, MatchingRuleArgs? rule = null)
        {
            if (!duckInterface.IsInterface)
                throw new ArgumentException("鸭子标准的定义类型必须是接口! ", nameof(duckInterface));

            MatchingRuleArgs usingRule = rule ?? MatchingRuleArgs.Default;

            if (!IsTypeLooksLike(type, duckInterface, usingRule.GenericArgMappingRule ?? GetGenericArgFromOtherDefaultImpl, out var mappings))
            {
                yield break;
            }
            var mappingTidied = new TypeGenericArgMappingTidied(mappings);


            var instanceMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            var staticMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
            var dMethods = duckInterface.GetMethods(BindingFlags.Public | BindingFlags.Instance);   // 鸭子接口的实例方法
            foreach (var dMethod in dMethods)
            {
                var method = findLooksLikeMethod(dMethod, instanceMethods, staticMethods, mappingTidied, usingRule);
                yield return new LooksLikeMethod()
                {
                    DuckInterface = duckInterface,
                    MethodInDuckInterface = dMethod,
                    TargetType = type,
                    MethodInTargetType = method,
                };
            }
        }

        /// <summary>
        /// 在 <paramref name="instanceMethods"/> 或 <paramref name="staticMethods"/> 中查找与 <paramref name="duckMethod"/> 相对应的方法
        /// </summary>
        /// <param name="duckMethod">鸭子标准中的方法</param>
        /// <param name="instanceMethods">准备查找的公共实例方法集合</param>
        /// <param name="staticMethods">准备查找的公共静态方法集合</param>
        /// <param name="gParamMappingTidied">类型层面的泛型参数的映射关系</param>
        /// <param name="rule"></param>
        /// <returns>如果找到了相对应的方法, 返回该方法, 否则返回 <see langword="null"/></returns>
        private static MethodInfo? findLooksLikeMethod(MethodInfo duckMethod, MethodInfo[] instanceMethods, MethodInfo[] staticMethods, TypeGenericArgMappingTidied gParamMappingTidied, MatchingRuleArgs rule)
        {
            MethodInfo? output = null;

            // 寻找方法时比较字符串的参数
            StringComparison stringComparison = rule.CaseSensitiveWhenFindMethod ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;

            MatchingRule matchSetting = rule.Rule & MatchingRule.MatchSettingSegment;
            // 优先使用自定义的方法去寻找
            if ((matchSetting & MatchingRule.Match2Custom) > 0)
            {
                if (rule.FindMethod == null)
                {
                    throw new InvalidOperationException($"传入的查找规则中, 寻找对应方法的委托对象 {nameof(MatchingRuleArgs.FindMethod)} 为 null! ");
                }
                else
                {
                    output = rule.FindMethod.Invoke(duckMethod, instanceMethods, staticMethods, gParamMappingTidied);
                    // 查找到的方法再检查是否匹配
                    if (output != null && (matchSetting & MatchingRule.MatchThenCheck) > 0)
                    {
                        if (!IsMethodLookLike(output, duckMethod, gParamMappingTidied, rule))
                        {
                            throw new InvalidOperationException($"通过自定义匹配方式获取到方法后, 再次校验是否匹配时未通过检查! 寻找到的方法: {output}") ;
                        }
                    }
                }
            }
            // 未找到的情况下, 寻找实例方法
            if (output == null && (matchSetting & MatchingRule.Match2Instance) > 0)
            {
                foreach (var method in instanceMethods.Where(m => m.Name.Equals(duckMethod.Name, stringComparison)))
                {
                    // 遍历方法名符合条件的方法, 寻找第一个形参列表与返回值均看着像的方法
                    if (IsMethodLookLike(method, duckMethod, gParamMappingTidied, rule))
                    {
                        // 找到了匹配项
                        output = method;
                        break;
                    }
                }
            }
            // 仍未找到的情况下, 寻找静态方法
            if (output == null && (matchSetting & MatchingRule.Match2Static) > 0)
            {
                foreach (var method in staticMethods.Where(m => m.Name.Equals(duckMethod.Name, stringComparison)))
                {
                    // 遍历方法名符合条件的方法, 寻找第一个形参列表与返回值均看着像的方法
                    if (IsMethodLookLike(method, duckMethod, gParamMappingTidied, rule))
                    {
                        // 找到了匹配项
                        output = method;
                        break;
                    }
                }
            }

            return output;
        }


        /// <summary>
        /// 判断传入的目标方法是否看着像鸭子标准的方法
        /// </summary>
        /// <param name="targetMethod"></param>
        /// <param name="duckMethod"></param>
        /// <param name="gArgMappingTidied">类型层面的泛型参数的映射关系</param>
        /// <param name="rule"></param>
        /// <returns></returns>
        private static bool IsMethodLookLike(MethodInfo targetMethod, MethodInfo duckMethod, TypeGenericArgMappingTidied gArgMappingTidied, MatchingRuleArgs rule)
        {
            MatchingRule baseSetting = rule.Rule & MatchingRule.BaseSettingSegment;
            switch (baseSetting) 
            {
                case MatchingRule.Exact:
                    return _isMethodLookLike_Excat(targetMethod, duckMethod, gArgMappingTidied, rule);
                case MatchingRule.Assignable:
                    return _isMethodLookLike_Assignable(targetMethod, duckMethod, gArgMappingTidied, rule);
                default:
                    throw new NotImplementedException($"未实现的规则基础设置 {baseSetting}");
            }
        }
        private static bool _isMethodLookLike_Excat(MethodInfo targetMethod, MethodInfo duckMethod, TypeGenericArgMappingTidied gArgMappingTidied, MatchingRuleArgs rule)
        {
            Type tReturn = targetMethod.ReturnType;
            Type dReturn = duckMethod.ReturnType;


            // 整理判断泛型参数是否等价的比较方法
            if (!__isMethodLookLike_TidingGArgsEquivalentCompareFunc(targetMethod, duckMethod, gArgMappingTidied, rule, out var gArgEquivalentCompareFunc))
            {
                return false;
            }

            // 比较返回类型是否等价
            if (!ReflectionHelper.IsEquivalent(tReturn, dReturn, 
                normalTypeEquivalentCompareFunc: null,
                gTypeDefinitionEquivalentCompareFunc: null, 
                gParamEquivalentCompareFunc: gArgEquivalentCompareFunc))
            {
                return false;
            }

            // 比较形参列表是否同构等价
            ParameterInfo[] tParams = targetMethod.GetParameters();
            ParameterInfo[] dParams = duckMethod.GetParameters();

            if (tParams.Length != dParams.Length) return false; // 形参数量不等

            foreach (var (tP, dP) in (tParams, dParams).UntilAnyAway()) // 实际会同时结束
            {
                // 相同位置的形参是否等价
                if (!ReflectionHelper.IsEquivalent(tP.ParameterType, dP.ParameterType,
                    normalTypeEquivalentCompareFunc: null,
                    gTypeDefinitionEquivalentCompareFunc: null,
                    gParamEquivalentCompareFunc: gArgEquivalentCompareFunc))
                {
                    return false;
                }
            }

            return true;
        }
        private static bool _isMethodLookLike_Assignable(MethodInfo targetMethod, MethodInfo duckMethod, TypeGenericArgMappingTidied gArgMappingTidied, MatchingRuleArgs rule)
        {
            Type tReturn = targetMethod.ReturnType;
            Type dReturn = duckMethod.ReturnType;


            // 整理判断泛型参数是否等价的比较方法
            if (!__isMethodLookLike_TidingGArgsEquivalentCompareFunc(targetMethod, duckMethod, gArgMappingTidied, rule, out var gArgEquivalentCompareFunc))
            {
                return false;
            }

            // 比较返回类型是否等价
            if (dReturn == typeof(void))
            {
                // 鸭子标准不需要返回值
                goto ReturnTypeEquivalent;
            }
            if (tReturn.IsAssignableTo(dReturn))
            {
                // 目标类型的方法返回类型能够被赋值到鸭子标准方法的返回类型
                goto ReturnTypeEquivalent;
            }
            // 比较是否同构等价
            if (ReflectionHelper.IsEquivalent(tReturn, dReturn,
                normalTypeEquivalentCompareFunc: null,
                gTypeDefinitionEquivalentCompareFunc: null,
                gParamEquivalentCompareFunc: gArgEquivalentCompareFunc))
            {
                // 返回值类型同构等价
                goto ReturnTypeEquivalent;
            }
            return false;

            
        ReturnTypeEquivalent:

            // 比较形参列表是否同构等价
            ParameterInfo[] tParams = targetMethod.GetParameters();
            ParameterInfo[] dParams = duckMethod.GetParameters();

            foreach (var (tP, dP) in (tParams, dParams).UntilAllAway()) 
            { 
                if (tP != null && dP != null)
                {
                    // 相同位置的形参是否等价
                    if (!ReflectionHelper.IsEquivalent(tP.ParameterType, dP.ParameterType,
                        normalTypeEquivalentCompareFunc: null,
                        gTypeDefinitionEquivalentCompareFunc: null,
                        gParamEquivalentCompareFunc: gArgEquivalentCompareFunc))
                    {
                        return false;
                    }
                }
                else if (tP == null && dP != null)
                {
                    // 鸭子标准的形参数量多于目标类型的方法的, 这种情况下可以把前者的参数抛弃不使用
                    continue;
                }
                else if (tP != null && dP == null)
                {
                    // 目标类型的形参数量多于鸭子标准的方法的, 这种情况下前者必须有一个对应位置的默认值
                    if (tP.HasDefaultValue) continue;
                    else return false;
                }
                else 
                {
                    // 遍历项的两个元素值均为 null, 实际不会出现这种情况
                    throw new ImpossibleForkException($"使用 {nameof(IEnumerableExtensions.UntilAnyAway)} 遍历两个方法的形参列表出错, 出现遍历项两个元素均为 null 的情况");
                }
            }

            return true;
        }

        /// <summary>
        /// 整理判断泛型参数是否等价的比较方法, 整理失败时返回 <see langword="false"/>
        /// </summary>
        /// <returns></returns>
        private static bool __isMethodLookLike_TidingGArgsEquivalentCompareFunc(
            MethodInfo targetMethod, MethodInfo duckMethod, TypeGenericArgMappingTidied gArgMappingTidied, MatchingRuleArgs rule, 
            [NotNullWhen(true)] out Func<Type, Type, bool>? gArgEquivalentCompareFunc)
        {
            // 获取两个方法的泛型形参列表
            var tGArgs = targetMethod.GetGenericArguments();
            var tGParams = targetMethod.IsGenericMethod && !targetMethod.IsGenericMethodDefinition ?
                targetMethod.GetGenericMethodDefinition().GetGenericArguments() : tGArgs;
            var dGArgs = duckMethod.GetGenericArguments();
            var dGParams = duckMethod.IsGenericMethod && !duckMethod.IsGenericMethodDefinition ? 
                duckMethod.GetGenericMethodDefinition().GetGenericArguments() : dGArgs;
            // 整理泛型形参的映射关系, 包含泛型类型和泛型方法的泛型参数列表
            TypeGenericArgMappingTidied methodGenericArgMappingTidied;
            if (tGArgs.Length == 0 && dGArgs.Length == 0)
            {
                // 都没有泛型形参列表的话跳过整理
                methodGenericArgMappingTidied = new TypeGenericArgMappingTidied([]);
            }
            else
            {
                if (tryTidiedGenericArgsMappings(
                    (tGArgs, tGParams).UntilAnyAway().ToArray(), 
                    (dGArgs, dGParams).UntilAnyAway().ToArray(), 
                    rule.GenericArgMappingRule ?? GetGenericArgFromOtherDefaultImpl, gArgMappingTidied, out var mappingsList))
                {
                    methodGenericArgMappingTidied = new TypeGenericArgMappingTidied(mappingsList);
                }
                else
                {
                    // 整理泛型形参映射关系失败
                    gArgEquivalentCompareFunc = null;
                    return false;
                }

            }
            // 泛型形参是否等价的比较方法
            gArgEquivalentCompareFunc = (gPTarget, gPDuck) =>
            {
                // 同时在类型的泛型形参中被找到, 同时两者属于同一组映射关系
                return methodGenericArgMappingTidied.TryFromTargetArg(gPTarget, out var fromTarget)
                    && methodGenericArgMappingTidied.TryFromDuckArg(gPDuck, out var fromDuck)
                    && fromTarget.Value.IndexOnDuck == fromDuck.Value.IndexOnDuck
                    && fromTarget.Value.IndexOnTarget == fromDuck.Value.IndexOnTarget;
            };
            return true;
        }

        #region 类型是否看着像的实现


        /// <summary>
        /// 判断 <paramref name="type"/> 的类型定义本身是否看着像 <paramref name="duckInterface"/>, 出现泛型参数时, 按索引对应地去寻找
        /// </summary>
        /// <param name="type">目标类型</param>
        /// <param name="duckInterface">鸭子标准</param>
        /// <param name="customGenericParamMappings">泛型形参自定义固定映射关系</param>
        /// <param name="mappings">判断过程中顺带整理的, 由目标类型泛型参数映射到鸭子类型泛型参数的映射关系</param>
        /// <returns></returns>
        private static bool IsTypeLooksLike(Type type, Type duckInterface, CustomTypeGenericArgMapping[] customGenericParamMappings, out TypeGenericArgMapping[] mappings)
        {
            return IsTypeLooksLike(type, duckInterface, (duckGPs, targetGPIndex) =>
            {
                foreach (var item in customGenericParamMappings)
                {
                    if (item.IndexOnTargetType == targetGPIndex.Index)
                    {
                        if (duckGPs.Length > item.IndexOnDuckType)
                        {
                            return null;    // 映射的鸭子类型中的索引在可选范围之外
                        }
                        else
                        {
                            return new TypeGenericArgWithIndex()
                            {
                                Index = item.IndexOnDuckType,
                                ArgType = duckGPs[item.IndexOnDuckType].argType,
                                ParamType = duckGPs[item.IndexOnDuckType].paramType,
                            };
                        }
                    }
                }
                return null;
            }, out mappings);
        }
        /// <summary>
        /// 判断 <paramref name="type"/> 的类型定义本身是否看着像 <paramref name="duckInterface"/>
        /// </summary>
        /// <param name="type">目标类型</param>
        /// <param name="duckInterface">鸭子标准</param>
        /// <param name="genericParamMappings">泛型形参映射规则</param>
        /// <param name="mappings">判断过程中顺带整理的, 由目标类型泛型参数映射到鸭子类型泛型参数的映射关系</param>
        /// <returns></returns>
        private static bool IsTypeLooksLike(Type type, Type duckInterface, GetGenericArgFromOtherDelegate genericParamMappings, out TypeGenericArgMapping[] mappings)
        {
            if (!duckInterface.IsGenericType)
            {
                if (type.IsGenericType)
                {
                    // 鸭子标准不是泛型类型, 目标类型只能是普通类型
                    goto LooksNotLike;
                }
                else
                {
                    // 两者均是普通类型
                    goto TargetTypeIsNormal;
                }
            }
            else
            {
                if (!type.IsGenericType)
                {
                    // 目标类型是普通类型
                    goto TargetTypeIsNormal;
                }
                else
                {
                    var gArgs1 = type.GetGenericArguments();  // 目标类型的泛型参数列表
                    var gParams1 = type.IsGenericType && !type.IsGenericTypeDefinition ?
                        type.GetGenericTypeDefinition().GetGenericArguments() : gArgs1;
                    var gArgs2 = duckInterface.GetGenericArguments();
                    var gParams2 = duckInterface.IsGenericType && !type.IsGenericTypeDefinition ?
                        duckInterface.GetGenericTypeDefinition().GetGenericArguments() : gArgs2;

                    if (tryTidiedGenericArgsMappings(
                        (gArgs1, gParams1).UntilAnyAway().ToArray(), 
                        (gArgs2, gParams2).UntilAnyAway().ToArray(), 
                        genericParamMappings, null, out var mappingsList))
                    {
                        mappings = [.. mappingsList];
                        return true;
                    }
                    else
                    {
                        goto LooksNotLike;
                    }


                }
            }

        TargetTypeIsNormal:
            mappings = [];
            return true;

        LooksNotLike: // 看着不像的情况下的统一返回值
            mappings = [];
            return false;
        }


        #endregion

        #region 方法是否看着像的实现
        /// <summary>
        /// 判断一个方法是否看着像鸭子标准中的方法
        /// </summary>
        /// <param name="type"><paramref name="method"/> 所属的类型</param>
        /// <param name="method">准备检查方法</param>
        /// <param name="duckType"><paramref name="duckStandard"/> 所属的类型</param>
        /// <param name="duckStandard">对应鸭子标准中的方法</param>
        /// <param name="typeGParamMappings">类型上的泛型参数映射关系</param>
        /// <param name="rule"></param>
        /// <returns></returns>
        private static bool methodLooksLike(
            Type type, MethodInfo method,
            Type duckType, MethodInfo duckStandard,
            TypeGenericArgMapping[] typeGParamMappings, MatchingRule rule)
        {
            if (method.Name != duckStandard.Name) return false;

            var matchingMode = rule & MatchingRule.BaseSettingSegment;

            var params1 = method.GetParameters();
            var params2 = method.GetParameters();
            var gParams1 = method.GetGenericArguments();
            var gParams2 = method.GetGenericArguments();
            var arrReturnGParams1_Enumerable = ReflectionHelper
                .PreorderGenericParameterTree(method.ReturnType, true);
            var arrReturnGParams2_Enumerable = ReflectionHelper
                .PreorderGenericParameterTree(method.ReturnType, true);
            switch (matchingMode)
            {
                case MatchingRule.Exact:
                    {
                        var arrReturnGParams1_Any = arrReturnGParams1_Enumerable.Any();
                        var arrReturnGParams2_Any = arrReturnGParams2_Enumerable.Any();
                        // 两者泛型参数列表需要符合条件: 相同索引的参数, 拥有相同的约束
                        if (!gParams1.SequenceEqual(gParams2, lazyGenericArgEC.Value))
                        {
                            return false;
                        }
                        // 判断返回值
                        if (arrReturnGParams1_Any || arrReturnGParams2_Any)
                        {
                            // 均含泛型
                            // 啥都不做, 继续往下执行
                        }
                        else if (!arrReturnGParams1_Any || !arrReturnGParams2_Any)
                        {
                            // 不含泛型
                            if (method.ReturnType != duckStandard.ReturnType) return false;
                        }
                        else
                        {
                            // 一个含泛型一个不含泛型
                            return false;
                        }

                        if (!params1.SequenceEqual(params2, lazyParameterInfoEC.Value)) return false;  // 检查两者形参列表的顺序, 类型均是否一致
                    }
                    break;
                case MatchingRule.Assignable:
                    if (duckStandard.ReturnType != typeof(void) // 无返回值时, 需要检查的类型允许返回任意类型, 所以只检查有返回值的情况
                        && !method.ReturnType.IsAssignableTo(duckStandard.ReturnType)   // 方法返回值无法提供给鸭子接口作为返回值
                        )
                    {
                        return false;
                    }
                    int pCountMax = Math.Max(params1.Length, params2.Length);
                    for (int i = 0; i < pCountMax; i++)
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

        #endregion

        #region 映射关系结构体

        /// <summary>
        /// 泛型参数类型对象, 与其在类型定义上的位置索引
        /// </summary>
        public struct TypeGenericArgWithIndex
        {
            /// <summary>
            /// 在对应类型定义中的位置索引
            /// </summary>
            public int Index { get; set; }
            /// <summary>
            /// 从类型上取得的泛型参数类型
            /// </summary>
            public Type ArgType { get; set; }
            /// <summary>
            /// 从类型上取得的泛型形参类型
            /// </summary>
            public Type ParamType { get; set; }  

            public static implicit operator TypeGenericArgWithIndex((int index, Type aType, Type pType) arg)
            {
                return new TypeGenericArgWithIndex()
                {
                    Index = arg.index,
                    ArgType = arg.aType,
                    ParamType = arg.pType,
                };
            }
            public static implicit operator TypeGenericArgWithIndex((int index, (Type aType, Type pType) types) arg)
            {
                return new TypeGenericArgWithIndex()
                {
                    Index = arg.index,
                    ArgType = arg.types.aType,
                    ParamType = arg.types.pType,
                };
            }

            public static implicit operator TypeGenericArgWithIndex((Type aType, Type pType, int index) arg)
            {
                return new TypeGenericArgWithIndex()
                {
                    Index = arg.index,
                    ArgType = arg.aType,
                    ParamType = arg.pType,
                };
            }

        }
        /// <summary>
        /// 自定义类型泛型参数映射关系
        /// </summary>
        public struct CustomTypeGenericArgMapping
        {
            /// <summary>
            /// 泛型参数映射项在鸭子标准的类型定义中的位置索引
            /// </summary>
            public int IndexOnDuckType { get; set; }

            /// <summary>
            /// 泛型参数映射项在目标类型的类型定义中的位置索引
            /// </summary>
            public int IndexOnTargetType { get; set; }


            public static implicit operator CustomTypeGenericArgMapping((int indexOfDuckType, int indexOfTargetType) arg)
            {
                return new CustomTypeGenericArgMapping()
                {
                    IndexOnDuckType = arg.indexOfDuckType,
                    IndexOnTargetType = arg.indexOfTargetType,
                };
            }
        }
        /// <summary>
        /// 鸭子标准与目标类型的泛型形参映射关系
        /// </summary>
        public struct TypeGenericArgMapping
        {
            public TypeGenericArgMapping()
            {
                IndexOnDuck = 0;
                ArgOnDuck = typeof(object);
                ParamOnDuck = typeof(object);
                IndexOnTarget = 0;
                ArgOnTarget = typeof(object);
                ParamOnTarget = typeof(object);
            }
            public TypeGenericArgMapping(TypeGenericArgWithIndex duckOne, TypeGenericArgWithIndex targetOne)
            {
                (IndexOnDuck, ArgOnDuck, ParamOnDuck) = (duckOne.Index, duckOne.ArgType, duckOne.ParamType);
                (IndexOnTarget, ArgOnTarget, ParamOnTarget) = (targetOne.Index, targetOne.ArgType, targetOne.ParamType);
            }

            #region 映射内容

            /// <summary>
            /// 泛型参数映射项在鸭子标准的类型定义中的位置索引
            /// </summary>
            public int IndexOnDuck { get; set; }
            /// <summary>
            /// 鸭子标准上取得的泛型参数类型
            /// </summary>
            public Type ArgOnDuck { get; set; }
            /// <summary>
            /// 鸭子标准上取得的泛型形参类型
            /// </summary>
            public Type ParamOnDuck { get; set; }

            /// <summary>
            /// 泛型参数映射项在目标类型的类型定义中的位置索引
            /// </summary>
            public int IndexOnTarget { get; set; }
            /// <summary>
            /// 目标类型上取得的泛型参数类型
            /// </summary>
            public Type ArgOnTarget { get; set; }
            /// <summary>
            /// 目标类型上去的的泛型形参类型
            /// </summary>
            public Type ParamOnTarget { get; set; }

            #endregion

            public readonly override string ToString()
            {
                return $"[Duck:{IndexOnDuck}]{ArgOnDuck} <=> [Target:{IndexOnTarget}]{ArgOnTarget}";
            }
        }
        /// <summary>
        /// 整理有序的鸭子标准与目标类型的泛型形参映射关系 
        /// </summary>
        /// <remarks>
        /// 需要优化: 存了四个字典对象, 不是很好
        /// </remarks>
        public struct TypeGenericArgMappingTidied : IEnumerable<TypeGenericArgMapping>
        {
            private ReadOnlyDictionary<Type, TypeGenericArgMapping> DuckArgToMapping;
            private ReadOnlyDictionary<Type, TypeGenericArgMapping> TargetArgToMapping;
            private ReadOnlyDictionary<Type, TypeGenericArgMapping> DuckParamToMapping;
            private ReadOnlyDictionary<Type, TypeGenericArgMapping> TargetParamToMapping;

            internal TypeGenericArgMappingTidied(IEnumerable<TypeGenericArgMapping> mappings)
            {
                Dictionary<Type, TypeGenericArgMapping> duckGenericArgToMapping = [];
                Dictionary<Type, TypeGenericArgMapping> targetGenericArgToMapping = [];
                Dictionary<Type, TypeGenericArgMapping> duckGenericParamToMapping = [];
                Dictionary<Type, TypeGenericArgMapping> targetGenericParamToMapping = [];

                foreach (var mapping in mappings)
                {
                    duckGenericArgToMapping.Add(mapping.ArgOnDuck, mapping);
                    duckGenericParamToMapping.Add(mapping.ParamOnDuck, mapping);
                    targetGenericArgToMapping.Add(mapping.ArgOnTarget, mapping);
                    targetGenericParamToMapping.Add(mapping.ParamOnDuck, mapping);
                }
                DuckArgToMapping = duckGenericArgToMapping.AsReadOnly();
                TargetArgToMapping = targetGenericArgToMapping.AsReadOnly();
                DuckParamToMapping = duckGenericParamToMapping.AsReadOnly();
                TargetParamToMapping = targetGenericParamToMapping.AsReadOnly();

            }

            #region 遍历
            /// <summary>
            /// 获取遍历所有映射项的遍历器
            /// </summary>
            /// <returns></returns>
            public readonly IEnumerable<TypeGenericArgMapping> All() => DuckArgToMapping.Values;
            #endregion

            #region 实现接口
            public readonly IEnumerator<TypeGenericArgMapping> GetEnumerator()
            {
                return All().GetEnumerator();
            }

            readonly IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion

            #region 获取映射关系
            /// <summary>
            /// 根据来自鸭子标准的泛型参数类型 <paramref name="argType"/> 寻找映射关系, 未能找到时抛出异常
            /// </summary>
            /// <param name="argType"></param>
            /// <returns></returns>
            public readonly TypeGenericArgMapping FromDuckArg(Type argType)
            {
                return TryFromDuckArg(argType, out var mapping) ? mapping.Value : throw new InvalidOperationException($"未能根据来自鸭子标准的泛型参数类型 {argType} 找到映射关系");
            }
            /// <summary>
            /// 根据来自鸭子标准的泛型参数类型 <paramref name="argType"/> 寻找映射关系
            /// </summary>
            /// <param name="argType"></param>
            /// <param name="mapping"></param>
            /// <returns></returns>
            public readonly bool TryFromDuckArg(Type argType, [NotNullWhen(true)] out TypeGenericArgMapping? mapping)
            {
                if (DuckArgToMapping.TryGetValue(argType, out var _mapping))
                {
                    mapping = _mapping;
                    return true;
                }
                else
                {
                    mapping = null;
                    return false;
                }
            }
            /// <summary>
            /// 根据来自鸭子标准的泛型形参类型 <paramref name="paramType"/> 寻找映射关系, 未能找到时抛出异常
            /// </summary>
            /// <param name="paramType"></param>
            /// <returns></returns>
            public readonly TypeGenericArgMapping FromDuckParam(Type paramType)
            {
                return TryFromDuckParam(paramType, out var mapping) ? mapping.Value : throw new InvalidOperationException($"未能根据来自鸭子标准的泛型形参类型 {paramType} 找到映射关系");
            }
            /// <summary>
            /// 根据来自鸭子标准的泛型形参类型 <paramref name="paramType"/> 寻找映射关系
            /// </summary>
            /// <param name="paramType"></param>
            /// <param name="mapping"></param>
            /// <returns></returns>
            public readonly bool TryFromDuckParam(Type paramType, [NotNullWhen(true)] out TypeGenericArgMapping? mapping)
            {
                if (DuckParamToMapping.TryGetValue(paramType, out var _mapping))
                {
                    mapping = _mapping;
                    return true;
                }
                else
                {
                    mapping = null;
                    return false;
                }
            }

            /// <summary>
            /// 根据来自目标类型的泛型参数类型 <paramref name="argType"/> 寻找映射关系, 未能找到时抛出异常
            /// </summary>
            /// <param name="argType"></param>
            /// <returns></returns>
            public readonly TypeGenericArgMapping FromTargetArg(Type argType)
            {
                return TryFromTargetArg(argType, out var mapping) ? mapping.Value : throw new InvalidOperationException($"根据来自目标类型的泛型参数类型 {argType} 找到映射关系");
            }
            /// <summary>
            /// 根据来自目标类型的泛型参数类型 <paramref name="argType"/> 寻找映射关系
            /// </summary>
            /// <param name="argType"></param>
            /// <param name="mapping"></param>
            /// <returns></returns>
            public readonly bool TryFromTargetArg(Type argType, [NotNullWhen(true)] out TypeGenericArgMapping? mapping)
            {
                if (TargetArgToMapping.TryGetValue(argType, out var _mapping))
                {
                    mapping = _mapping;
                    return true;
                }
                else
                {
                    mapping = null;
                    return false;
                }
            }
            /// <summary>
            /// 根据来自目标类型的泛型形参类型 <paramref name="paramType"/> 寻找映射关系, 未能找到时抛出异常
            /// </summary>
            /// <param name="paramType"></param>
            /// <returns></returns>
            public readonly TypeGenericArgMapping FromTargetParam(Type paramType)
            {
                return TryFromTargetParam(paramType, out var mapping) ? mapping.Value : throw new InvalidOperationException($"根据来自目标类型的泛型参数类型 {paramType} 找到映射关系");
            }
            /// <summary>
            /// 根据来自目标类型的泛型形参类型 <paramref name="paramType"/> 寻找映射关系
            /// </summary>
            /// <param name="paramType"></param>
            /// <param name="mapping"></param>
            /// <returns></returns>
            public readonly bool TryFromTargetParam(Type paramType, [NotNullWhen(true)] out TypeGenericArgMapping? mapping)
            {
                if (TargetParamToMapping.TryGetValue(paramType, out var _mapping))
                {
                    mapping = _mapping;
                    return true;
                }
                else
                {
                    mapping = null;
                    return false;
                }
            }
            #endregion

        }

        #endregion

        #region 私有的一些判断方法
        /// <summary>
        /// 判断两个形参是否等价, 如果是泛型形参, 则约束条件相同即可 (不考虑形参是包含了泛型参数的某个泛型类型, 只考虑形参直接是泛型参数的情况)
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

        //private static bool isEquivalent(ParameterInfo a, ParameterInfo b)
        //{

        //}

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

        /// <summary>
        /// 懒加载资源: 泛型参数的等价比较器, 当两个传入值拥有相同的约束时, 则等价
        /// </summary>
        private readonly static Lazy<IEqualityComparer<Type>> lazyGenericArgEC = new(
            () =>
            {
                return EqualityComparer<Type>.Create(
                    (a, b) =>
                    {
                        if (a == null && b == null)
                        {
                            return true;
                        }
                        else if (a != null && b != null)
                        {
                            return ReflectionHelper.GenericParameterHasSameConstraints(a, b);
                        }
                        else
                        {
                            return false;
                        }
                    },
                    gArg => gArg.GetGenericParameterConstraints().GetHashCode()
                    );
            });

        #endregion

        #region 私有的一些映射关系获取方法

        /// <summary>
        /// 尝试整理分别来自目标类型与鸭子标准的泛型参数列表之间的映射关系. 可以是类型上的泛型参数, 也可以是方法上的泛型参数, 但是两者要统一
        /// </summary>
        /// <param name="gTypesFromTarget">来自目标类型的泛型参数与形参列表 (方法上的, 或者类型上的)</param>
        /// <param name="gTypesFromDuck">来自鸭子标准的泛型参数与形参列表 (方法上的, 或者类型上的)</param>
        /// <param name="customGenericArgMappings">自定义获取映射项的方法</param>
        /// <param name="mergeMappings">需要合并到一块的映射项, 当不为kong</param>
        /// <param name="mappings">整理出来的映射关系列表</param>
        /// <returns></returns>
        private static bool tryTidiedGenericArgsMappings(
            (Type argType, Type paramType)[] gTypesFromTarget, (Type argType, Type paramType)[] gTypesFromDuck, 
            GetGenericArgFromOtherDelegate customGenericArgMappings,
            IEnumerable<TypeGenericArgMapping>? mergeMappings,
            [NotNullWhen(true)] out List<TypeGenericArgMapping>? mappings)
        {
            mappings = null;

            if (gTypesFromTarget.Length > gTypesFromDuck.Length && customGenericArgMappings == GetGenericArgFromOtherDefaultImpl)
            {
                // 如果按索引顺序对应, 在目标类型的泛型参数数量比鸭子标准的多时, 必定会有无法对应上的
                return false;
            }

            List<TypeGenericArgMapping> mappingList = [];
            delayConstrainsCheckComparar constrainsCheckComparar = new()
            {
                WaitingCheckGenericParams = []
            };

            // 自定义约束条件比较方法: 比较并排除两组约束条件中, 含泛型形参的项, 以及除此之外的能一一对应的项
            List<(Type[], Type[])> delayConstrainsCheck_Waiting = [];
            Func<Type[], Type[], bool> customConstrainsCompaperFunc = (csTarget, csDuck) =>
            {
                var (eCsTarget, eCsDuck) = csTarget.ExcludeDisorderEquals(csDuck, constrainsCheckComparar);
                var tempObj = (eCsTarget.ToArray(), eCsDuck.ToArray());
                if (tempObj.Item1.Length != tempObj.Item2.Length)
                {
                    return false;   // 排除后的约束条件数量不一致, 必定不同
                }
                else
                {
                    // 剩余项保存起来, 延迟检查
                    delayConstrainsCheck_Waiting.Add(tempObj);
                    return true;
                }
            };

            foreach (var (index, (gATargetType, gPTargetType)) in gTypesFromTarget.WithIndex())
            {

                TypeGenericArgWithIndex gADuckInfos;
                // 按一定规则, 在鸭子标准中寻找当前遍历项对应的泛型参数
                var _gPReturn = customGenericArgMappings.Invoke(gTypesFromDuck, (index, gATargetType, gPTargetType));
                if (_gPReturn == null)
                {
                    // 目标类型的泛型参数存在任一未能找到对应鸭子标准泛型参数的项的
                    return false;
                }
                gADuckInfos = _gPReturn.Value;
                if (gATargetType.IsGenericParameter && gADuckInfos.ArgType.IsGenericParameter)
                {
                    // 两者均为泛型形参
                    if (!ReflectionHelper.GenericParameterHasSameConstraints(
                        gATargetType, gADuckInfos.ArgType, 
                        customConstrainsCompaperFunc))
                    {
                        // 双方不具有相同的约束条件
                        return false;
                    }
                }
                else if (!gATargetType.IsGenericParameter && !gADuckInfos.ArgType.IsGenericParameter)
                {
                    // 非泛型形参的情况下需要相等
                    if (!gATargetType.Equals(gADuckInfos.ArgType))
                    {
                        return false;
                    }
                }
                else
                {
                    // 其他情况下无法等价
                    return false;
                }

                TypeGenericArgMapping mapping = new(gADuckInfos, (index, gATargetType, gPTargetType));

                mappingList.Add(mapping);
            }

            if (mergeMappings != null)
            {
                foreach (var mapping in mergeMappings)
                {
                    mappingList.Add(mapping);
                }
            }

            // 延迟检查约束条件
            if (constrainsCheckComparar.WaitingCheckGenericParams.Count > 0)
            {
                // 泛型形参 => 泛型实参
                Dictionary<Type, TypeGenericArgMapping> duckParamToMapping = [];
                Dictionary<Type, TypeGenericArgMapping> targetParamToMapping = [];
                foreach (var mappingItem in mappingList)
                {
                    duckParamToMapping.Add(mappingItem.ParamOnDuck, mappingItem);
                    targetParamToMapping.Add(mappingItem.ParamOnTarget, mappingItem);
                }

                foreach (var checkItem in constrainsCheckComparar.WaitingCheckGenericParams)
                {
                    Type item1 = checkItem.Item1;
                    Type item2 = checkItem.Item2;

                    // 判断等待比较的泛型形参映射关系, 是否在映射关系列表中能够找到, 不能找到的话说明没有这样子的映射关系, 约束条件不相同
                    bool found = false;
                    // 尝试查找是否存在映射关系, 假设 item1 是鸭子标准或目标类型的一个泛型形参, 尝试获取对应的映射关系, 然后检查 item2 是否处于该映射关系中
                    TypeGenericArgMapping mapping;
                    if (duckParamToMapping.TryGetValue(item1, out mapping) && mapping.ParamOnTarget == item2
                        || targetParamToMapping.TryGetValue(item1, out mapping) && mapping.ParamOnDuck == item2)
                    {
                        found = true;
                    }

                    if (!found) return false;
                }
            }

            mappings = mappingList;
            return true;
        }
        /// <summary>
        /// 用于延迟约束检查的比较器, 不包含泛型形参的直接作比较, 包含的统一将泛型形参视作相等. 同时也比较结构是否一致, 不论是否泛型形参, 结构不一致的都会返回 <see langword="false"/>
        /// </summary>
        private struct delayConstrainsCheckComparar : IEqualityComparer<Type>
        {
            /// <summary>
            /// 等待后续延迟检查的泛型形参对
            /// </summary>
            public HashSet<DisorderPair<Type>> WaitingCheckGenericParams { get; set; }

            public readonly bool Equals(Type? x, Type? y)
            {
                if (x == null && y == null) return true;
                if (x == null ^ y == null) return false;

                ImpossibleForkException.ImpossibleNull(x);
                ImpossibleForkException.ImpossibleNull(y);

                // 本次检查新增的待延迟检查类型对
                HashSet<DisorderPair<Type>> waitChecks = new HashSet<DisorderPair<Type>>();

                // 均为泛型形参的情况下固定视为相等, 同时
                if (x.IsGenericParameter && y.IsGenericParameter)
                {
                    waitChecks.Add((x, y));
                    goto IS_EQUALS;
                }
                // 只有一方为泛型形参的话视为不等
                else if (x.IsGenericParameter ^ y.IsGenericParameter) return false;

                // 只有一方是泛型类型的情况下视为不等
                if (x.IsGenericType ^ y.IsGenericType) return true;
                // 均不是泛型类型的情况下直接用 Type.Equals 比较是否相等
                else if (!x.IsGenericType && y.IsGenericType) return x.Equals(y);
                // 均是泛型类型的情况下比较结构是否相同
                else
                {
                    var tree1 = ReflectionHelper.PreorderGenericParameterTree(x);
                    var tree2 = ReflectionHelper.PreorderGenericParameterTree(y);

                    foreach (var (t1, t2) in (tree1, tree2).UntilAllAway())
                    {
                        if (t1 == null || t2 == null) return false; // 长度不等, 结构不同

                        if (t1.IsGenericParameter && t2.IsGenericParameter)
                        {
                            WaitingCheckGenericParams.Add((t1, t2));
                        }
                        else if (t1.IsGenericParameter ^ y.IsGenericParameter)  // 只有一方是泛型形参
                        {
                            return false;
                        }

                        if (t1.IsGenericType && t2.IsGenericType)
                        {
                            if (t1.GetGenericArguments().Length != t2.GetGenericArguments().Length) // 泛型类型的参数数量不等
                            {
                                return false;
                            }
                        }
                        else if (t1.IsGenericType ^ y.IsGenericType)    // 只有一方是泛型类型
                        {
                            return false;
                        }
                        else if (t1.Equals(t2)) // 双方都是普通类型的情况下不等
                        {
                            return false;
                        }
                    }
                }


            IS_EQUALS:
                foreach (var item in waitChecks)
                {
                    WaitingCheckGenericParams.Add(item);
                }
                return true;
            }

            public readonly int GetHashCode([DisallowNull] Type obj)
            {
                return HashCode.Combine(
                    // 用于区分是否有可能包含泛型形参
                    obj.IsGenericParameter,
                    obj.IsGenericType,
                    // 不可能包含泛型形参的情况下固定追加 obj 本身的 HashCode
                    !obj.IsGenericType && !obj.IsGenericParameter ?
                        // 不可能包含泛型形参的情况
                        obj.GetHashCode()   
                        :
                        // 有可能包含泛型形参, 如果一个是泛型形参, 一个是泛型类型, 不可能对应上, 所以取这个 HashCode, 而不是 0
                        HashCode.Combine(obj.IsGenericType, obj.IsGenericParameter) 
                        );
            }
        }
        #endregion


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
