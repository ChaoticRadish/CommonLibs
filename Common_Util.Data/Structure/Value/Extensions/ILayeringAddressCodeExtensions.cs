using Common_Util.Data.Constraint;
using Common_Util.Data.Structure.Tree;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Structure.Value.Extensions
{
    public static class ILayeringAddressCodeExtensions
    {
        #region 计算

        /// <summary>
        /// 取得两个编码路径中相交的部分, 即从头开始判断, 截取路径中层值等价的部分 (遇到不等的层时将停下)
        /// </summary>
        /// <typeparam name="TLayer"></typeparam>
        /// <param name="codeA"></param>
        /// <param name="codeB"></param>
        /// <param name="comparer">比较两个层值是否等价的比较器, 如果为null, 将使用 <see cref="EqualityComparer{T}.Default"/></param>
        /// <returns>返回一个新的数组, 包含从传入的 A 值路径中截取的等价部分</returns>
        public static TLayer[] Crossing<TLayer>(
            this ILayeringAddressCode<TLayer> codeA,
            ILayeringAddressCode<TLayer> codeB,
            IEqualityComparer<TLayer>? comparer = null)
        {
            int sameCount = 0;

            comparer = EqualityComparer<TLayer>.Default;

            int min = Math.Min(codeA.LayerCount, codeB.LayerCount);
            for (int i = 0; i < min; i++)
            {
                var valueA = codeA.LayerValues[i];
                var valueB = codeB.LayerValues[i];
                if (comparer.Equals(valueA, valueB))
                {
                    sameCount++;
                }
                else
                {
                    break;
                }
            }

            return codeA.LayerValues[..sameCount];
        }

        /// <summary>
        /// 取得输入编码路径与层值片段中相交的部分, 即从头开始判断, 截取路径中层值等价的部分 (遇到不等的层时将停下)
        /// </summary>
        /// <typeparam name="TLayer"></typeparam>
        /// <param name="code"></param>
        /// <param name="layers"></param>
        /// <param name="comparer">比较两个层值是否等价的比较器, 如果为null, 将使用 <see cref="EqualityComparer{T}.Default"/></param>
        /// <returns>返回一个新的数组, 包含从传入的编码值路径中截取的等价部分</returns>
        public static TLayer[] Crossing<TLayer>(
            this ILayeringAddressCode<TLayer> code,
            Span<TLayer> layers,
            IEqualityComparer<TLayer>? comparer = null)
        {
            int sameCount = 0;

            comparer = EqualityComparer<TLayer>.Default;

            int min = Math.Min(code.LayerCount, layers.Length);
            for (int i = 0; i < min; i++)
            {
                var valueA = code.LayerValues[i];
                var valueB = layers[i];
                if (comparer.Equals(valueA, valueB))
                {
                    sameCount++;
                }
                else
                {
                    break;
                }
            }

            return code.LayerValues[..sameCount];
        }

        /// <summary>
        /// 取得包含输入编码的最小范围, 如果输入的是范围, 则将返回其自身
        /// </summary>
        /// <typeparam name="TLayer"></typeparam>
        /// <param name="code"></param>
        /// <returns></returns>
        public static ILayeringAddressCode<TLayer> MinRange<TLayer>(
            this ILayeringAddressCode<TLayer> code)
        {
            if (code.IsRange)
            {
                return code;
            }
            else
            {
                return new LayeringAddressCodeBaseImpl<TLayer>(true, code.LayerCount == 1 ? [] : code.LayerValues[..(code.LayerCount - 1)]);
            }
        }

        /// <summary>
        /// 取得包含输入编码的最小范围的路径, 如果输入的是范围, 则将返回其自身的路径
        /// </summary>
        /// <typeparam name="TLayer"></typeparam>
        /// <param name="code"></param>
        /// <returns></returns>
        public static IEnumerable<TLayer> MinRangePath<TLayer>(
            this ILayeringAddressCode<TLayer> code)
        {
            if (code.IsRange)
            {
                return code.LayerValues;
            }
            else
            {
                return code.LayerValues[..(code.LayerCount - 1)];
            }
        }

        /// <summary>
        /// 取得编码的前一级范围编码
        /// </summary>
        /// <typeparam name="TLayer"></typeparam>
        /// <param name="code"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static ILayeringAddressCode<TLayer> PreRange<TLayer>(
            this ILayeringAddressCode<TLayer> code)
        {
            if (code.IsAll())
            {
                throw new Exception($"完全范围编码不具有前一级范围编码! ");
            }
            else
            {
                return new LayeringAddressCodeBaseImpl<TLayer>(true, code.LayerCount == 1 ? [] : code.LayerValues[..(code.LayerCount - 1)]);
            }
        }

        /// <summary>
        /// 取得编码的前一级范围编码的路径
        /// </summary>
        /// <typeparam name="TLayer"></typeparam>
        /// <param name="code"></param>
        /// <returns></returns>
        public static IEnumerable<TLayer> PreRangePath<TLayer>(
            this ILayeringAddressCode<TLayer> code)
        {
            if (code.IsAll())
            {
                throw new Exception($"完全范围编码不具有前一级范围编码! ");
            }
            else
            {
                return code.LayerValues[..(code.LayerCount - 1)];
            }
        }


        /// <summary>
        /// 取得编码的路径末端值
        /// <para>如果编码是参数为 0, 将会抛出异常</para>
        /// </summary>
        /// <typeparam name="TLayer"></typeparam>
        /// <param name="code"></param>
        /// <returns></returns>
        public static TLayer Endpoint<TLayer>(this ILayeringAddressCode<TLayer> code)
        {
            if (code.LayerCount == 0) throw new ArgumentException("传入编码的层数为 0 ");
            return code.LayerValues[^1];
        }
        #endregion

        #region 扩展判断方法
        /// <summary>
        /// 判断一个编码是否标识包含所有的一个范围, 此范围在此库中被称为 "完全范围"
        /// </summary>
        /// <typeparam name="TLayer"></typeparam>
        /// <param name="code"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAll<TLayer>(this ILayeringAddressCode<TLayer> code)
        {
            return code.IsRange && code.LayerCount == 0;
        }

        /// <summary>
        /// 判断一个编码是否在另一个编码表示的范围内, 前者是后者的次次级 (以及往后的所有层级) 亦视为在后者范围内
        /// <para>如果传入的范围编码值不是范围, 则返回 false</para>
        /// <para>如果传入的编码值与范围编码相等, 也返回 false</para>
        /// </summary>
        /// <typeparam name="TLayer"></typeparam>
        /// <param name="code"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool IsIn<TLayer>(this ILayeringAddressCode<TLayer> code, ILayeringAddressCode<TLayer> range)
        {
            if (!range.IsRange) return false;
            if (code.LayerCount <= range.LayerCount) return false;

            for (int i = 0; i < range.LayerCount; i++)
            {
                TLayer? t1 = code.LayerValues[i];
                TLayer? t2 = range.LayerValues[i];

                if (t1 == null ^ t2 == null) return false;
                else if (t1 == null && t2 == null) continue;
                else if (!t1!.Equals(t2)) return false;
                else continue;
            }
            return true;
        }

        /// <summary>
        /// 判断编码的路径与传入的路径是否等价
        /// </summary>
        /// <typeparam name="TLayer"></typeparam>
        /// <param name="code"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool PathEquals<TLayer>(this ILayeringAddressCode<TLayer> code, IEnumerable<TLayer> path, IEqualityComparer<TLayer>? comparer = null)
        {
            return LayeringAddressCodeHelper.PathEquals(code, path, comparer);
        }
        /// <summary>
        /// 判断编码的路径与传入的另一个编码是否等价
        /// <para>此比较仅比较路径, 不比较类别是否相同, 比如是否都是范围编码</para>
        /// </summary>
        /// <typeparam name="TLayer"></typeparam>
        /// <param name="code"></param>
        /// <param name="other"></param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        public static bool PathEquals<TLayer>(this ILayeringAddressCode<TLayer> code, ILayeringAddressCode<TLayer> other, IEqualityComparer<TLayer>? comparer = null)
        {
            return LayeringAddressCodeHelper.PathEquals(code, other.LayerValues, comparer);
        }

        #endregion

        #region 搜索
        /// <summary>
        /// 查询集合中, 在特定范围内的所有子项 (含次级, 次次级, 以及往后的所有层级)
        /// <para>如果传入编码值不是范围, 则什么都不会返回</para>
        /// </summary>
        /// <typeparam name="TLayer"></typeparam>
        /// <param name="collection"></param>
        /// <param name="range"></param>
        /// <param name="findItem"></param>
        /// <param name="findRange"></param>
        /// <returns></returns>
        public static IEnumerable<ILayeringAddressCode<TLayer>> AllChilds<TLayer>(
            this IEnumerable<ILayeringAddressCode<TLayer>> collection,
            ILayeringAddressCode<TLayer> range,
            bool findItem = true,
            bool findRange = true)
        {
            if (!range.IsRange) yield break;
            foreach (var code in collection)
            {
                if (!findItem && !code.IsRange) continue;
                else if (!findRange && code.IsRange) continue;

                if (IsAll(range)) yield return code;
                else if (IsIn(code, range))
                {
                    yield return code;
                }
            }
        }


        #endregion

        #region 转换
        /// <summary>
        /// 将集合中的各项转换为默认的实现类型 <see cref="LayeringAddressCode{TLayer}"/>
        /// </summary>
        /// <typeparam name="TLayer"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static IEnumerable<LayeringAddressCode<TLayer>> ToDefaultImpl<TLayer>(this IEnumerable<ILayeringAddressCode<TLayer>> collection)
            where TLayer : IStringConveying, new()
        {
            foreach (var code in collection)
            {
                if (code is LayeringAddressCode<TLayer> layer)
                {
                    yield return layer;
                }
                else
                {
                    yield return LayeringAddressCodeHelper.Convert(code);
                }
            }
        }
        /// <summary>
        /// 将集合中的各项转换为默认的实现类型 <see cref="LayeringAddressCode"/>
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static IEnumerable<LayeringAddressCode> ToDefaultImpl(this IEnumerable<ILayeringAddressCode<string>> collection)
        {
            foreach (var code in collection)
            {
                if (code is LayeringAddressCode layer)
                {
                    yield return layer;
                }
                else
                {
                    yield return LayeringAddressCodeHelper.Convert(code);
                }
            }
        }


        /// <summary>
        /// 通用的将编码转换成字符串的方法 (默认格式)
        /// <para>转换后的编码形如: ValueA.ValueB:ValueC </para>
        /// </summary>
        /// <typeparam name="TLayer"></typeparam>
        /// <param name="code"></param>
        /// <param name="convertFunc">层值到字符串的转换方法</param>
        /// <param name="escapeChar">转义字符, 如果层值字符串出现了方法传入的几个特殊符号, 会使用此字符来加以区分, 表示是普通的字符</param>
        /// <param name="splitChar">分隔不同层值的标识字符</param>
        /// <param name="itemMarkChar">项编码的标识字符, 如果是项编码, 将被标记在最后一个层值的字符串前</param>
        /// <returns></returns>
        public static string ToDefaultFormatString<TLayer>(this ILayeringAddressCode<TLayer> code,
            Func<TLayer, string>? convertFunc = null,
            char escapeChar = LayeringAddressCode.EscapeChar,
            char splitChar = LayeringAddressCode.SplitChar,
            char itemMarkChar = LayeringAddressCode.ItemMarkChar)
        {
            return LayeringAddressCodeHelper.ConvertToString(code, convertFunc, escapeChar, splitChar, itemMarkChar);
        }
        #endregion

        #region 拼接

        /// <summary>
        /// 批量处理传入的编码集合: 合并传入的范围编码和集合内的编码, 得到一系列新的编码 (范围编码在前)
        /// <para>使用默认的创建接口对象的方法</para>
        /// </summary>
        /// <typeparam name="TLayer"></typeparam>
        /// <typeparam name="TCode"></typeparam>
        /// <param name="collection"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static IEnumerable<ILayeringAddressCode<TLayer>> ConcatRange<TLayer, TCode>(this IEnumerable<TCode> collection,
            ILayeringAddressCode<TLayer> range)
            where TCode : ILayeringAddressCode<TLayer>
        {
            return ConcatRange(collection.Select(i => (ILayeringAddressCode<TLayer>)i), range);
        }
        /// <summary>
        /// 批量处理传入的编码集合: 合并传入的范围编码和集合内的编码, 得到一系列新的编码 (范围编码在前)
        /// <para>使用默认的创建接口对象的方法</para>
        /// </summary>
        /// <typeparam name="TLayer"></typeparam>
        /// <param name="collection"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static IEnumerable<ILayeringAddressCode<TLayer>> ConcatRange<TLayer>(this IEnumerable<ILayeringAddressCode<TLayer>> collection,
            ILayeringAddressCode<TLayer> range)
        {
            return ConcatRange(collection, range, _createItem, _createRange);
        }
        /// <summary>
        /// 批量处理传入的编码集合: 合并传入的范围编码和集合内的编码, 得到一系列新的编码 (范围编码在前)
        /// <para>使用自定创建新编码对象 (或不是编码对象) 的方法</para>
        /// </summary>
        /// <typeparam name="TLayer"></typeparam>
        /// <typeparam name="TCode"></typeparam>
        /// <param name="collection"></param>
        /// <param name="range"></param>
        /// <param name="createItemFunc"></param>
        /// <param name="createRangeFunc"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static IEnumerable<TCode> ConcatRange<TLayer, TCode>(this IEnumerable<ILayeringAddressCode<TLayer>> collection,
            ILayeringAddressCode<TLayer> range,
            Func<TLayer[], TCode> createItemFunc,
            Func<TLayer[], TCode> createRangeFunc)
        {
            if (!range.IsRange)
            {
                throw new ArgumentException("传入的编码不是范围编码! ", nameof(range));
            }
            foreach (var code in collection)
            {
                var newPath = LayeringAddressCodeHelper.CreatePath(range, code);
                if (code.IsRange)
                {
                    yield return createRangeFunc(newPath);
                }
                else
                {
                    yield return createItemFunc(newPath);
                }
            }

        }
        #endregion

        #region 整理

        /// <summary>
        /// 将编码集合转换为多叉树 (不定数量的分支), 每一层节点会尝试创建范围节点
        /// <para>树的节点值, 其类型实现 <see cref="ILayeringAddressCode{TLayer}"/>, 具体类型根据默认创建节点值的方法实现, 可能会有不同类型 (与传入类型无关)</para>
        /// <para>等价的节点将被忽略</para>
        /// </summary>
        /// <typeparam name="TLayer"></typeparam>
        /// <param name="collection"></param>
        /// <param name="rootValue">根节点值, 如果为 null, 则将被设置为完全范围. 在此范围之外的其他节点将被忽略</param>
        /// <param name="comparer"></param>
        /// <returns>将返回多叉树, 其根节点的值为完全范围</returns>
        public static IMultiTree<ILayeringAddressCode<TLayer>> AsMultiTree<TLayer>(
            this IEnumerable<ILayeringAddressCode<TLayer>> collection, ILayeringAddressCode<TLayer>? rootValue = null, IEqualityComparer<TLayer>? comparer = null)
        {
            return AsMultiTree(collection, _createItem, _createRange, rootValue, comparer);
        }


        /// <summary>
        /// 将编码集合转换为多叉树 (不定数量的分支), 每一层节点会尝试创建范围节点
        /// <para>树的节点值, 由传入的创建方法来创建 (会提供节点的路径)</para>
        /// <para>等价的节点将被忽略</para>
        /// </summary>
        /// <typeparam name="TLayer"></typeparam>
        /// <param name="collection"></param>
        /// <param name="createItemFunc">创建叶子节点节点值的方法, 传入参数: [0]: 节点路径;</param>
        /// <param name="createRangeFunc">创建分叉节点节点值的方法, 传入参数: [0]: 节点路径;</param>
        /// <param name="rootValue">根节点值, 如果为 null, 则将被设置为完全范围. 在此范围之外的其他节点将被忽略</param>
        /// <param name="comparer"></param>
        /// <returns>将返回多叉树, 其根节点的值为完全范围</returns>
        public static IMultiTree<TNodeValue> AsMultiTree<TLayer, TNodeValue>(
            this IEnumerable<ILayeringAddressCode<TLayer>> collection,
            Func<TLayer[], TNodeValue> createItemFunc,
            Func<TLayer[], TNodeValue> createRangeFunc,
            ILayeringAddressCode<TLayer>? rootValue = null,
            IEqualityComparer<TLayer>? comparer = null)
            where TNodeValue : ILayeringAddressCode<TLayer>
        {
            if (rootValue != null && !rootValue.IsRange)
            {
                throw new ArgumentException("传入根节点值不是范围", nameof(rootValue));
            }

            comparer ??= EqualityComparer<TLayer>.Default;

            SimpleMultiTree<TNodeValue> output = new();

            rootValue ??= _createRange<TLayer>();
            output.Root = new SimpleMultiTreeNode<TNodeValue>(createRangeFunc([]))
            {
                Childrens = [],
            };

            // 插入节点到树中

            #region 实现 2024/08/03 缓存当前范围值栈

            ConvertCollectionToMultiTreeNode_Impl01(
                collection, output.Root,
                createItemFunc, createRangeFunc,
                (arg) => new SimpleMultiTreeNode<TNodeValue>(arg.code),
                (arg) =>
                {
                    arg.parent.Childrens ??= [];
                    arg.parent.Childrens.Add(arg.node);
                },
                comparer);


            #endregion


            return output;
        }
        #endregion

        #region 库范围内的一些通用方法
        /// <summary>
        /// 将一系列的编码, 转换为多叉树节点, 添加到传入的节点上
        /// </summary>
        /// <typeparam name="TLayer"></typeparam>
        /// <typeparam name="TCode"></typeparam>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="collection">需要处理的一系列编码</param>
        /// <param name="rootNode">将被作为根节点的节点</param>
        /// <param name="createItemCodeFunc">将路径转换为项编码的方法</param>
        /// <param name="createRangeCodeFunc">将路径转换为范围编码的方法</param>
        /// <param name="createNodeFunc"></param>
        /// <param name="addNodeToParentFunc"></param>
        /// <param name="comparer"></param>
        internal static void ConvertCollectionToMultiTreeNode_Impl01<TLayer, TCode, TNode>(
            this IEnumerable<ILayeringAddressCode<TLayer>> collection,
            TNode rootNode,
            Func<TLayer[], TCode> createItemCodeFunc,
            Func<TLayer[], TCode> createRangeCodeFunc,
            Func<(TCode code, TNode parent), TNode> createNodeFunc,
            Action<(TNode parent, TNode node)> addNodeToParentFunc,
            IEqualityComparer<TLayer> comparer)
            where TCode : ILayeringAddressCode<TLayer>
            where TNode : IMultiTreeNode<TCode>
        {
            TCode rootValue = rootNode.NodeValue;

            Stack<TLayer> rangeStack = new();
            foreach (var layer in rootValue.LayerValues)
            {
                rangeStack.Push(layer);
            }
            int rootIndex = rangeStack.Count;   // 如果编码是项编码: 层数小于或等于此值时, 说明在根节点外
                                                //                相对根节点编码, 分叉于此值之前的层数, 同样说明处于根节点外
                                                // 如果编码是范围编码: 层数小于此值时, 说明在根节点外
                                                //                  层数等于此值时, 如果编码与根节点编码一样, 则在范围内, 如果不一样, 则在范围外. 因此无论如何都应该被忽略
                                                //                  相对根节点编码, 分叉于此值之前的层数, 同样说明处于根节点外
            Stack<TNode> rangeNodeStack = new();
            TNode currentRange = rootNode;
            rangeNodeStack.Push(currentRange);

            foreach (var code in collection)
            {
                if (code.LayerCount <= rootIndex) { continue; }

                if (rootIndex > 0)  // 如果等于 0, 那无论如何都会在范围内, 不用检查分叉位置
                {
                    var rootCrossing = Crossing(code, rootValue);
                    if (rootCrossing.Length < rootIndex) { continue; }
                }

                var minRange = code.MinRange();

                var crossing = Crossing(minRange, currentRange.NodeValue, comparer);
                int backLayer = currentRange.NodeValue.LayerCount - crossing.Length;    // 需回退层数

                for (int i = 0; i < backLayer; i++)
                {
                    var test = rangeStack.Pop();
                    currentRange = rangeNodeStack.Pop();
                }
                currentRange = rangeNodeStack.Peek();

                for (int i = crossing.Length; i < minRange.LayerCount; i++)
                {
                    TLayer target = minRange.LayerValues[i];
                    rangeStack.Push(target);

                    TNode? existNode = (TNode?)currentRange.Childrens?.FirstOrDefault(
                        child => child is TNode && child.NodeValue.IsRange && comparer.Equals(target, child.NodeValue.LayerValues[i]));
                    if (existNode != null)
                    {
                        rangeNodeStack.Push(existNode);
                        currentRange = existNode;
                    }
                    else
                    {
                        TLayer[] newPath = new TLayer[currentRange.NodeValue.LayerCount + 1];
                        for (int index = 0; index < currentRange.NodeValue.LayerCount; index++)
                        {
                            newPath[index] = currentRange.NodeValue.LayerValues[index];
                        }
                        newPath[^1] = target;
                        var newRange = createRangeCodeFunc(newPath);
                        var newNode = createNodeFunc((newRange, currentRange));
                        addNodeToParentFunc((currentRange, newNode));
                        rangeNodeStack.Push(newNode);
                        currentRange = newNode;
                    }

                }

                if (!code.IsRange)
                {
                    TLayer target = code.LayerValues[^1];

                    TNode? existNode = (TNode?)currentRange.Childrens?.FirstOrDefault(
                        child => child is TNode && !child.NodeValue.IsRange && comparer.Equals(target, child.NodeValue.LayerValues[^1]));
                    if (existNode == null)
                    {
                        TLayer[] newPath = new TLayer[currentRange.NodeValue.LayerCount + 1];
                        for (int index = 0; index < currentRange.NodeValue.LayerCount; index++)
                        {
                            newPath[index] = currentRange.NodeValue.LayerValues[index];
                        }
                        newPath[^1] = target;
                        var newItem = createItemCodeFunc(newPath);
                        var newNode = createNodeFunc((newItem, currentRange));
                        addNodeToParentFunc((currentRange, newNode));
                    }
                }

            }
        }
        #endregion

        #region 私有的一些通用方法

        /// <summary>
        /// 创建一个范围编码, 其实现后续可能会有所调整, 只能保证返回值是 <see cref="ILayeringAddressCode{TLayer}"/> 接口实例
        /// </summary>
        /// <typeparam name="TLayer"></typeparam>
        /// <param name="layers"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ILayeringAddressCode<TLayer> _createRange<TLayer>(params TLayer[] layers)
        {
            if (layers == null || layers.Length == 0) return LayeringAddressCodeBaseImpl<TLayer>.All;
            else
            {
                return new LayeringAddressCodeBaseImpl<TLayer>(true, layers);
            }
        }
        /// <summary>
        /// 创建一个项编码, 其实现后续可能会有所调整, 只能保证返回值是 <see cref="ILayeringAddressCode{TLayer}"/> 接口实例
        /// </summary>
        /// <typeparam name="TLayer"></typeparam>
        /// <param name="layers"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ILayeringAddressCode<TLayer> _createItem<TLayer>(params TLayer[] layers)
        {
            if (layers == null || layers.Length < 1) throw new ArgumentException($"项编码需要至少一个层值");
            else
            {
                return new LayeringAddressCodeBaseImpl<TLayer>(false, layers);
            }
        }
        #endregion



    }
}
