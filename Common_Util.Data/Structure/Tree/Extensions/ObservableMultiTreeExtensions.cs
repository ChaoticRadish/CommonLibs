using Common_Util.Data.Struct;
using Common_Util.Data.Structure.Value;
using Common_Util.Data.Structure.Value.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Structure.Tree.Extensions
{
    public static class ObservableMultiTreeExtensions
    {
        #region ILayeringAddressCode 相关


        #region 值插入

        /// <summary>
        /// 向节点值类型为 <see cref="ILayeringAddressCode{TLayer}"/> 的 <see cref="ObservableMultiTree{TValue}"/> 中指定路径 (由传入的编码值决定) 上添加传入的值
        /// <para>此方法要求根节点必须是一个范围编码</para>
        /// </summary>
        /// <typeparam name="TLayer"></typeparam>
        /// <typeparam name="TCode"></typeparam>
        /// <param name="tree"></param>
        /// <param name="code"></param>
        /// <param name="createItemFunc"></param>
        /// <param name="createRangeFunc"></param>
        /// <param name="comparer"></param>
        /// <returns>
        /// <para>Failure => 因传入值不在树根节点的范围内, 已有等价值, 或其他原因, 未添加传入编码到树上</para>
        /// <para>Exception => 树本身的结构或数据等有问题, 例如: 根节点不是一个范围编码</para>
        /// </returns>
        public static IOperationResultEx Add<TLayer, TCode>(
            this ObservableMultiTree<TCode> tree, 
            TCode code,
            Func<TLayer[], TCode> createItemFunc,
            Func<TLayer[], TCode> createRangeFunc,
            IEqualityComparer<TLayer>? comparer = null)
            where TCode : ILayeringAddressCode<TLayer>
        {
            OperationResultEx result;

            comparer ??= EqualityComparer<TLayer>.Default;

            TCode rootCode;
            if (tree.Root == null)
            {
                rootCode = createRangeFunc([]);
                tree.CreateRootNode(rootCode);
            }
            else
            {
                rootCode = tree.Root.NodeValue;
            }
            if (!rootCode.IsRange)
            {
                return result = new Exception("树的节点值不是范围编码! ");
            }

            return Add(tree.Root!, code, createItemFunc, createRangeFunc, comparer);
        }

        public static IOperationResultEx Add<TLayer, TCode>(
            this ObservableMultiTreeNode<TCode> node,
            TCode code,
            Func<TLayer[], TCode> createItemFunc,
            Func<TLayer[], TCode> createRangeFunc,
            IEqualityComparer<TLayer>? comparer = null)
            where TCode : ILayeringAddressCode<TLayer>
        {
            return _addFromCode(
                node, code, 
                createItemFunc, createRangeFunc, 
                (arg) =>
                {
                    arg.targetCollection.Add(arg.newNode);
                    return (OperationResultEx)true;
                },
                comparer);
        }

        /// <summary>
        /// 向节点值类型为 <see cref="ILayeringAddressCode{TLayer}"/> 的 <see cref="ObservableMultiTree{TValue}"/> 中指定路径 (由传入的编码值决定) 上有序得添加传入的值
        /// </summary>
        /// <remarks>
        /// 此方法要求根节点必须是一个范围编码. <br/>
        /// 插入到子项集合中时, 会使用传入的比较器将其插入到合适的位置. <br/>(注: 插入时逐一比对, 有符合比较条件的位置就插入, 不会改变集合其他项的顺序, 所以如果原本就是无序的, 或者排序与参数相反, 可能得不到预期的结果)
        /// </remarks>
        /// <typeparam name="TLayer"></typeparam>
        /// <typeparam name="TCode"></typeparam>
        /// <param name="tree"></param>
        /// <param name="code"></param>
        /// <param name="createItemFunc"></param>
        /// <param name="createRangeFunc"></param>
        /// <param name="desc">是否降序排列</param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        public static IOperationResultEx OrderlyAdd<TLayer, TCode>(
            this ObservableMultiTree<TCode> tree,
            TCode code,
            Func<TLayer[], TCode> createItemFunc,
            Func<TLayer[], TCode> createRangeFunc,
            bool desc = false,
            IComparer<TCode>? comparer = null,
            IEqualityComparer<TLayer>? equalityComparer = null)
            where TCode : ILayeringAddressCode<TLayer>
        {
            OperationResultEx result;

            equalityComparer ??= EqualityComparer<TLayer>.Default;

            TCode rootCode;
            if (tree.Root == null)
            {
                rootCode = createRangeFunc([]);
                tree.CreateRootNode(rootCode);
            }
            else
            {
                rootCode = tree.Root.NodeValue;
            }
            if (!rootCode.IsRange)
            {
                return result = new Exception("树的节点值不是范围编码! ");
            }

            return OrderlyAdd(tree.Root!, code, createItemFunc, createRangeFunc, desc, comparer, equalityComparer);
        }
        /// <summary>
        /// 向节点值类型为 <see cref="ILayeringAddressCode{TLayer}"/> 的 <see cref="ObservableMultiTree{TValue}"/> 中指定路径 (由传入的编码值决定) 上有序得添加传入的值
        /// </summary>
        /// <remarks>
        /// 此方法要求根节点必须是一个范围编码. <br/>
        /// 插入到子项集合中时, 会使用传入的比较器将其插入到合适的位置. <br/>(注: 插入时逐一比对, 有符合比较条件的位置就插入, 不会改变集合其他项的顺序, 所以如果原本就是无序的, 或者排序与参数相反, 可能得不到预期的结果)
        /// </remarks>
        /// <typeparam name="TLayer"></typeparam>
        /// <typeparam name="TCode"></typeparam>
        /// <param name="node"></param>
        /// <param name="code"></param>
        /// <param name="createItemFunc"></param>
        /// <param name="createRangeFunc"></param>
        /// <param name="desc">是否降序排列</param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        public static IOperationResultEx OrderlyAdd<TLayer, TCode>(
            this ObservableMultiTreeNode<TCode> node, 
            TCode code,
            Func<TLayer[], TCode> createItemFunc,
            Func<TLayer[], TCode> createRangeFunc,
            bool desc = false,
            IComparer<TCode>? comparer = null,
            IEqualityComparer<TLayer>? equalityComparer = null)
            where TCode : ILayeringAddressCode<TLayer>
        {
            comparer ??= Comparer<TCode>.Default;
            return _addFromCode<TLayer, TCode>(
                node, code,
                createItemFunc, createRangeFunc,
                (arg) =>
                {
                    int index = 0;
                    bool endFlag = false;
                    foreach (var item in arg.targetCollection)
                    {
                        switch (comparer.Compare(arg.newCode, item.NodeValue))
                        {
                            case < 0:   // 新值 < 当前遍历项
                                if (!desc)
                                {
                                    endFlag = true;
                                }
                                break;

                            case 0:
                                break;

                            case > 0:   // 新值 > 当前遍历项
                                if (desc)
                                {
                                    endFlag = true;
                                }
                                break;
                        }
                        if (endFlag) break;
                        index++;
                    }
                    arg.targetCollection.Insert(index, arg.newNode);

                    return (OperationResultEx)true;
                },
                equalityComparer);
        }

        private static IOperationResultEx _addFromCode<TLayer, TCode>(
            this ObservableMultiTreeNode<TCode> node,
            TCode code,
            Func<TLayer[], TCode> createItemFunc,
            Func<TLayer[], TCode> createRangeFunc,
            Func<(TLayer targetLayerValue, TCode newCode, ObservableMultiTreeNode<TCode> newNode, ObservableCollection<ObservableMultiTreeNode<TCode>> targetCollection), IOperationResultEx> addToChildrenCollectionFunc, 
            IEqualityComparer<TLayer>? comparer)
            where TCode : ILayeringAddressCode<TLayer>
        {
            OperationResultEx result;

            comparer ??= EqualityComparer<TLayer>.Default;

            TCode rootCode = node.NodeValue;
            if (!rootCode.IsRange)
            {
                return result = new Exception("传入节点的节点值不是范围编码! ");
            }

            TLayer[] codeRange = code.IsRange ? code.LayerValues : code.LayerValues[..^1];

            TLayer[] rootCrossing;
            if (rootCode.LayerCount > 0)
            {
                rootCrossing = rootCode.Crossing(codeRange, comparer);
            }
            else
            {
                rootCrossing = [];
            }

            if (rootCrossing.Length < rootCode.LayerCount)
            {
                return result = "传入编码属于根节点之前的其他分支路径";
            }

            var currentNode = node;
            for (int i = rootCrossing.Length; i < codeRange.Length; i++)
            {
                TLayer target = codeRange[i];
                var existNode = currentNode.Childrens.FirstOrDefault(child => child.NodeValue.IsRange && comparer.Equals(target, child.NodeValue.LayerValues[i]));
                if (existNode != null)
                {
                    if (code.IsRange && i == code.LayerCount - 1)
                    {
                        return result = "已存在等价节点! ";
                    }
                    else
                    {
                        currentNode = existNode;
                    }
                }
                else
                {
                    TLayer[] newPath = LayeringAddressCodeHelper.CreatePath(currentNode.NodeValue.LayerValues, target);
                    TCode rangeCode = createRangeFunc(newPath);
                    ObservableMultiTreeNode<TCode> newNode = new(currentNode, rangeCode);
                    // currentNode.Childrens.Add(newNode);
                    var addResult = addToChildrenCollectionFunc((target, rangeCode, newNode, currentNode.Childrens));
                    if (addResult.IsFailure)
                    {
                        return result = OperationResultEx.Failure(addResult);
                    }
                    currentNode = newNode;
                }
            }

            if (!code.IsRange)
            {
                TLayer target = code.LayerValues[^1];
                var existNode = currentNode.Childrens.FirstOrDefault(child => !child.NodeValue.IsRange && comparer.Equals(target, child.NodeValue.LayerValues[^1]));
                if (existNode != null)
                {
                    return result = "已存在等价节点! ";
                }
                else
                {
                    TLayer[] newPath = LayeringAddressCodeHelper.CreatePath(currentNode.NodeValue.LayerValues, target);
                    TCode newCode = createItemFunc(newPath);
                    ObservableMultiTreeNode<TCode> newNode = new(currentNode, newCode);
                    var addResult = addToChildrenCollectionFunc((target, newCode, newNode, currentNode.Childrens));
                    if (addResult.IsFailure)
                    {
                        return result = OperationResultEx.Failure(addResult);
                    }
                    //currentNode.Childrens.Add(newNode);
                }

            }

            return result = true;
        }

        #endregion

        #region 节点插入

        /// <summary>
        /// 向节点值类型为 <see cref="ILayeringAddressCode{TLayer}"/> 的 <see cref="ObservableMultiTree{TValue}"/> 中指定路径 (由传入的编码值决定) 上添加传入的分支
        /// <para>此方法要求树的根节点必须是一个范围编码</para>
        /// </summary>
        /// <typeparam name="TLayer"></typeparam>
        /// <typeparam name="TCode"></typeparam>
        /// <param name="tree"></param>
        /// <param name="target">分支要放置的位置, 需确保对应节点存在, 且该编码是传入分支根节点节点值的前一级范围编码</param>
        /// <param name="fork">待添加的分支. 如果要放置分支的位置已有等价节点值的子节点, 将添加失败</param>
        /// <param name="comparer"></param>
        /// <returns>
        /// <para>Failure => 因传入值不在树根节点的范围内, 已有等价值, 或其他原因, 未添加传入分支到树上</para>
        /// <para>Exception => 树本身的结构或数据等有问题, 例如: 根节点不是一个范围编码</para>
        /// </returns>
        public static IOperationResultEx AddTo<TLayer, TCode>(
            this ObservableMultiTree<TCode> tree,
            TCode target,
            ObservableMultiTreeNode<TCode> fork,
            IEqualityComparer<TLayer>? comparer = null)
            where TCode : ILayeringAddressCode<TLayer>
        {
            OperationResultEx result;
            if (!target.IsRange)
            {
                return result = "放置位置编码需要是范围编码";
            }
            if (!target.PathEquals(fork.NodeValue.PreRangePath()))
            {
                return result = "目标位置不是传入分支的前一级范围编码";
            }

            comparer ??= EqualityComparer<TLayer>.Default;

            var findResult = Find(tree, target, comparer);
            return findResult.Match<
                    ObservableMultiTreeNode<TCode>,
                    OperationResultEx<ObservableMultiTreeNode<TCode>>
                    >(
                (node) =>
                {
                    var exist = node.Childrens.FirstOrDefault(
                        n => n.NodeValue.IsRange == fork.NodeValue.IsRange && comparer.Equals(n.NodeValue.Endpoint(), fork.NodeValue.Endpoint()));
                    if (exist == null)
                    {
                        node.Childrens.Add(fork);
                        fork.Parent = node;
                        return true;
                    }
                    else
                    {
                        return "目标节点下已有与传入分支根节点节点值等价的子节点! ";
                    }
                },
                () => "未找到目标放置位置对应的节点",
                () => OperationResultEx<ObservableMultiTreeNode<TCode>>.Failure(findResult));
        }



        #endregion

        /// <summary>
        /// 寻找节点值与传入编码等价的节点
        /// <para>此方法要求根节点必须是一个范围编码</para>
        /// </summary>
        /// <typeparam name="TLayer"></typeparam>
        /// <typeparam name="TCode"></typeparam>
        /// <param name="tree"></param>
        /// <param name="code">准备寻找的节点值</param>
        /// <param name="comparer"></param>
        /// <returns>
        /// <para>Success:</para>
        /// <para>- Data != null => 找到了对应的节点</para>
        /// <para>Failure:</para>
        /// <para>- 普通失败 => 未找到对应的节点</para>
        /// <para>- 出现异常</para>
        /// </returns>
        public static IOperationResultEx<ObservableMultiTreeNode<TCode>> Find<TLayer, TCode>(
            this ObservableMultiTree<TCode> tree, ILayeringAddressCode<TLayer> code,
            IEqualityComparer<TLayer>? comparer = null)
            where TCode : ILayeringAddressCode<TLayer>
        {
            OperationResultEx<ObservableMultiTreeNode<TCode>> result;

            comparer ??= EqualityComparer<TLayer>.Default;

            TCode rootCode;
            if (tree.Root == null)
            {
                return result = new Exception($"传入树没有根节点! ");
            }
            else
            {
                rootCode = tree.Root.NodeValue;
            }
            if (!rootCode.IsRange)
            {
                return result = new Exception("树的节点值不是范围编码! ");
            }

            TLayer[] codeRange = code.IsRange ? code.LayerValues : code.LayerValues[..^1];

            TLayer[] rootCrossing;
            if (rootCode.LayerCount > 0)
            {
                rootCrossing = rootCode.Crossing(codeRange, comparer);
            }
            else
            {
                rootCrossing = [];
            }

            if (rootCrossing.Length < rootCode.LayerCount)
            {
                return result = "传入编码属于根节点之前的其他分支路径";
            }

            var currentNode = tree.Root!;
            for (int i = rootCrossing.Length; i < codeRange.Length; i++)
            {
                TLayer target = codeRange[i];
                var existNode = currentNode.Childrens.FirstOrDefault(child => child.NodeValue.IsRange && comparer.Equals(target, child.NodeValue.LayerValues[i]));
                if (existNode != null)
                {
                    if (code.IsRange && i == code.LayerCount - 1)
                    {
                        return result = existNode;
                    }
                    else
                    {
                        currentNode = existNode;
                    }
                }
                else
                {
                    string rangeStr = Common_Util.String.StringHelper.Concat(
                        currentNode.NodeValue.LayerValues.Select(value => value?.ToString() ?? string.Empty).ToArray(),
                        ".");
                    return result = $"查找过程止步于 {rangeStr}";
                }
            }

            if (code.IsRange)
            {
                return result = currentNode;
            }
            else
            {
                TLayer target = code.LayerValues[^1];
                var existNode = currentNode.Childrens.FirstOrDefault(child => !child.NodeValue.IsRange && comparer.Equals(target, child.NodeValue.LayerValues[^1]));
                if (existNode != null)
                {
                    return result = existNode;
                }
                else
                {
                    string rangeStr = Common_Util.String.StringHelper.Concat(
                        currentNode.NodeValue.LayerValues.Select(value => value?.ToString() ?? string.Empty).ToArray(),
                        ".");
                    return result = $"范围 {rangeStr} 中未找到 {target?.ToString() ?? string.Empty}";
                }
            }
        }

        /// <summary>
        /// 寻找并移除节点值与传入编码等价的节点, 并将其返回. 
        /// <para>此方法要求根节点必须是一个范围编码</para>
        /// <para>如果传入编码为根节点值, 将返回失败</para>
        /// </summary>
        /// <typeparam name="TLayer"></typeparam>
        /// <typeparam name="TCode"></typeparam>
        /// <param name="tree"></param>
        /// <param name="code"></param>
        /// <param name="comparer"></param>
        /// <returns>
        /// <para>Success:</para>
        /// <para>- Data != null => 找到了对应的节点</para>
        /// <para>Failure:</para>
        /// <para>- 普通失败 => 未找到对应的节点或移除失败</para>
        /// <para>- 出现异常</para>
        /// </returns>
        public static IOperationResultEx<ObservableMultiTreeNode<TCode>> Remove<TLayer, TCode>(
            this ObservableMultiTree<TCode> tree, ILayeringAddressCode<TLayer> code,
            IEqualityComparer<TLayer>? comparer = null)
            where TCode : ILayeringAddressCode<TLayer>
        {
            var findResult = Find(tree, code, comparer);
            return findResult.Match<
                    ObservableMultiTreeNode<TCode>,
                    OperationResultEx<ObservableMultiTreeNode<TCode>>
                    >(
                (node) => 
                {
                    if (node.Parent == null)
                    {
                        // 准备移除的是根节点
                        tree.RemoveRootNode();
                        return true;
                    }
                    else 
                    {
                        node.Parent.Childrens.Remove(node);
                        node.Parent = null;
                        return true;
                    }
                },
                () => "未找到对应的节点",
                () => OperationResultEx<ObservableMultiTreeNode<TCode>>.Failure(findResult));
        }


        #region 整理
        /// <summary>
        /// 将一系列编码创建为一个 <see cref="ObservableMultiTree{TValue}"/> 分支
        /// </summary>
        /// <typeparam name="TLayer"></typeparam>
        /// <typeparam name="TCode"></typeparam>
        /// <param name="codes"></param>
        /// <param name="rootValue">此分支的根节点, 如果为 null, 将采用编码集合中深度最浅的一个范围编码作为根节点. 最好还是手动提供上</param>
        /// <param name="createItemFunc">将路径创建为项编码的方法</param>
        /// <param name="createRangeFunc">将路径创建为范围编码的方法</param>
        /// <param name="comparer"></param>
        /// <returns>创建得到的分支的根节点</returns>
        public static ObservableMultiTreeNode<TCode> CreateFork<TLayer, TCode>(
            this IEnumerable<ILayeringAddressCode<TLayer>> codes,
            ILayeringAddressCode<TLayer>? rootValue,
            Func<TLayer[], TCode> createItemFunc,
            Func<TLayer[], TCode> createRangeFunc,
            IEqualityComparer<TLayer>? comparer = null)
            where TCode : ILayeringAddressCode<TLayer>
        {
            var codeList = codes.ToList();
            if (!codes.Any())
            {
                throw new ArgumentException("传入了空集合", nameof(codes));
            }

            comparer ??= EqualityComparer<TLayer>.Default;

            // 确定传入的根节点值
            int i_count = codeList.Count;
            ILayeringAddressCode<TLayer>? i_rootValue;
            if (i_count == 1)
            {
                i_rootValue = codeList.First();
            }
            else
            {
                i_rootValue = rootValue ?? codeList.OrderBy(code => code.LayerCount).Where(code => code.IsRange).FirstOrDefault();
                if (i_rootValue == null)
                {
                    throw new ArgumentException("传入集合数量大于 1, 但是其中没有范围编码! ", nameof(codes));
                }
            }
            // 输出的根节点值
            var o_rootValue = i_rootValue.IsRange ? createRangeFunc(i_rootValue.LayerValues) : createItemFunc(i_rootValue.LayerValues);
            // 根节点
            var rootNode = new ObservableMultiTreeNode<TCode>(null, o_rootValue);

            if (i_count > 1)
            {
                // 多个编码, 创建子节点丢到根节点下
                codeList.ConvertCollectionToMultiTreeNode_Impl01(
                    rootNode,
                    createItemFunc, createRangeFunc,
                    (arg) => new ObservableMultiTreeNode<TCode>(arg.parent, arg.code),
                    (arg) => arg.parent.Childrens.Add(arg.node),
                    comparer);

                return rootNode;
            }
            else 
            {
                // 传入单个编码, 直接返回上面创建的根节点就行
                return rootNode;
            }
        }
        #endregion


        #endregion
    }
}
