using Common_Util.Data.Struct;
using Common_Util.Data.Structure.Value;
using Common_Util.Data.Structure.Value.Extensions;
using Common_Util.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Common_Util.Module.LayerComponentBaseLong;

namespace Common_Util.Data.Structure.Tree.Extensions
{
    public static partial class IMultiTreeExtensions
    {
        /* IMultiTree<ILayeringAddressCode<TValue>> 扩展
         * 
         */

        /// <summary>
        /// 检查结果枚举: 对节点值类型为 <see cref="ILayeringAddressCode{TValue}"/> 的多叉树分支 (可以是整个树, 也可以是某个节点下的所有节点) 的检查结果
        /// <para>其值为 0 (<see cref="Full"/>) 时表示分支是完整的没有问题的, 非 0 时有一定的缺陷, 按具体指判断具体是什么情况</para>
        /// </summary>
        [Flags]
        public enum CodeTreeForkCheckResultEnum : int
        {
            /// <summary>
            /// 完整的分支
            /// </summary>
            Full = 0,
            /// <summary>
            /// 缺失层, 出现某个节点, 其节点值不是任意子节点节点值的最小范围编码
            /// </summary>
            MissingLayer = 0b0001,
            /// <summary>
            /// 树结构出现错误, 如某个节点的节点值, 不属于其父节点节点值的范围, 或者一个项编码的节点缺拥有子节点, 等情况
            /// </summary>
            ErrorStruct = 0b0010,
            /// <summary>
            /// 同一层级出现节点值等价的子节点
            /// </summary>
            RepeatNodeValue = 0b0100,
        }

        /// <summary>
        /// 检查节点及其所有子项 (也包含次级子项) 所构成的分支是否完整 
        /// </summary>
        /// <typeparam name="TLayer"></typeparam>
        /// <typeparam name="TCode"></typeparam>
        /// <param name="node"></param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        public static CodeTreeForkCheckResultEnum CheckCodeTreeFork<TLayer>(
            this IMultiTreeNode<ILayeringAddressCode<TLayer>> node,
            IEqualityComparer<TLayer>? comparer = null)
        {
            return CheckCodeTreeFork<TLayer, ILayeringAddressCode<TLayer>>(node, comparer);
        }

        /// <summary>
        /// 检查节点及其所有子项 (也包含次级子项) 所构成的分支是否完整 
        /// </summary>
        /// <typeparam name="TLayer"></typeparam>
        /// <param name="node"></param>
        /// <returns></returns>
        public static CodeTreeForkCheckResultEnum CheckCodeTreeFork<TLayer, TCode>(
            this IMultiTreeNode<TCode> node,
            IEqualityComparer<TLayer>? comparer = null)
            where TCode : ILayeringAddressCode<TLayer>
        {
            comparer ??= EqualityComparer<TLayer>.Default;

            CodeTreeForkCheckResultEnum output = CodeTreeForkCheckResultEnum.Full;

            bool existMissingLayer = false;
            bool existErrorStrcut = false;
            bool existRepeatNodeValue = false;

            List<TCode> tempCodes = new(); 
            foreach (var item in node.IndexPreorder())
            {
                tempCodes.Add(item.NodeValue);

                if (item.ParentIndex >= 0)
                {
                    var parent = tempCodes[item.ParentIndex];

                    if (!existMissingLayer)
                    {
                        if (!parent.PathEquals(item.NodeValue.PreRangePath()))
                        {
                            existMissingLayer = true;
                        }
                    }
                    if (!existErrorStrcut)
                    {
                        if (!parent.IsRange)
                        {
                            existErrorStrcut = true;
                        }
                        else if (!item.NodeValue.IsIn(parent))
                        {
                            existErrorStrcut = true;
                        }
                    }
                }
                else
                {
                    // 只有根节点 (起始节点) 没有父节点, 这种情况下忽略
                }

                if (!existRepeatNodeValue && item.Node.Childrens.Any())
                {
                    int childCount = item.Node.Childrens.Count();
                    int distinctCount = item.Node.Childrens.Select(child => child.NodeValue.LayerValues[^1]).Distinct(comparer).Count();
                    if (childCount != distinctCount)
                    {
                        existRepeatNodeValue = true;
                    }
                }

                // 出现了所有异常情况
                if (existMissingLayer 
                    & existMissingLayer
                    & existRepeatNodeValue)
                {
                    break;
                }
            }


            output = output.AddFlagWhen(
                (existMissingLayer, CodeTreeForkCheckResultEnum.MissingLayer),
                (existErrorStrcut, CodeTreeForkCheckResultEnum.ErrorStruct),
                (existRepeatNodeValue, CodeTreeForkCheckResultEnum.RepeatNodeValue));

            return output;
        }

        #region 插入

        /// <summary>
        /// 向节点值类型为 <see cref="ILayeringAddressCode{TLayer}"/> 的 <see cref="IAddableMultiTree{TValue}"/> 中指定路径 (由传入的编码值决定) 上添加传入的值
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
            this IAddableMultiTree<TCode> tree,
            TCode code,
            Func<TLayer[], TCode> createItemFunc,
            Func<TLayer[], TCode> createRangeFunc,
            IEqualityComparer<TLayer>? comparer)
            where TCode : ILayeringAddressCode<TLayer>
        {
            comparer ??= EqualityComparer<TLayer>.Default;

            var checkResult = checkOrCreateRootCode(tree, createRangeFunc);
            if (checkResult.IsFailure) return checkResult;

            return Add(tree.Root!, code, createItemFunc, createRangeFunc, comparer);
        }


        /// <summary>
        /// 向节点值类型为 <see cref="ILayeringAddressCode{TLayer}"/> 的 <see cref="IAddableMultiTree{TValue}"/> 中指定路径 (由传入的编码值决定) 上添加传入的值
        /// <para>此方法要求传入节点必须是一个范围编码</para>
        /// </summary>
        /// <typeparam name="TLayer"></typeparam>
        /// <typeparam name="TCode"></typeparam>
        /// <param name="node">将要插入新节点的节点 (或者其某代父节点)</param>
        /// <param name="code"></param>
        /// <param name="createItemFunc"></param>
        /// <param name="createRangeFunc"></param>
        /// <param name="comparer"></param>
        /// <returns>
        /// <para>Failure => 因传入值不在树根节点的范围内, 已有等价值, 或其他原因, 未添加传入编码到树上</para>
        /// <para>Exception => 树本身的结构或数据等有问题, 例如: 根节点不是一个范围编码</para>
        /// </returns>
        public static IOperationResultEx Add<TLayer, TCode>(
            this IAddableMultiTreeNode<TCode> node,
            TCode code,
            Func<TLayer[], TCode> createItemFunc,
            Func<TLayer[], TCode> createRangeFunc,
            IEqualityComparer<TLayer>? comparer)
            where TCode : ILayeringAddressCode<TLayer>
        {
            return _insertFromCode(
                node, code,
                createItemFunc, createRangeFunc, 
                (args) =>
                {
                    OperationResult<IAddableMultiTreeNode<TCode>> result;
                    if (args.currentNode.TryAdd(args.newCode, out var newNode))
                    {
                        return result = OperationResult<IAddableMultiTreeNode<TCode>>.Success(newNode);
                    }
                    else
                    {
                        return result = "尝试添加编码到节点失败";
                    }
                }, 
                comparer);
        }

        /// <summary>
        /// 向节点值类型为 <see cref="ILayeringAddressCode{TLayer}"/> <see cref="IHasRootNodeAs{TNode}"/> 的 <see cref="IMultiTree{TValue}"/> 中指定路径 (由传入的编码值决定) 上有序得添加传入的值
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
        /// <param name="createNodeFunc">由父节点与编码值创建子节点的方法</param>
        /// <param name="insertFunc">将节点插入到指定节点的子节点集合中特定索引位置的方法</param>
        /// <param name="desc">是否降序排列</param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        public static IOperationResultEx OrderlyAdd<TLayer, TCode, TNode, TTree>(
            this TTree tree,
            TCode code,
            Func<TLayer[], TCode> createItemFunc,
            Func<TLayer[], TCode> createRangeFunc,
            Func<(TNode parent, TCode code), IOperationResult<TNode>> createNodeFunc,
            Func<(TNode parent, TNode newNode, int index), IOperationResult> insertFunc,
            bool desc = false,
            IComparer<TCode>? comparer = null,
            IEqualityComparer<TLayer>? equalityComparer = null)
            where TCode : ILayeringAddressCode<TLayer>
            where TNode : IMultiTreeNode<TCode>
            where TTree : IMultiTree<TCode>, IHasRootNodeAs<TNode>
        {
            equalityComparer ??= EqualityComparer<TLayer>.Default;

            var checkResult = checkOrCreateRootCode(tree, createRangeFunc);
            if (checkResult.IsFailure) return checkResult;

            return OrderlyAdd(((IHasRootNodeAs<TNode>)tree).Root!, code, createItemFunc, createRangeFunc, createNodeFunc, insertFunc, desc, comparer, equalityComparer);
        }

        /// <summary>
        /// 向节点值类型为 <see cref="ILayeringAddressCode{TLayer}"/> <see cref="IHasRootNodeAs{TNode}"/> 的 <see cref="IMultiTreeNode{TValue}"/> 中指定路径 (由传入的编码值决定) 上有序得添加传入的值
        /// </summary>
        /// <remarks>
        /// 此方法要求传入节点必须是一个范围编码. <br/>
        /// 插入到子项集合中时, 会使用传入的比较器将其插入到合适的位置. <br/>(注: 插入时逐一比对, 有符合比较条件的位置就插入, 不会改变集合其他项的顺序, 所以如果原本就是无序的, 或者排序与参数相反, 可能得不到预期的结果)
        /// </remarks>
        /// <typeparam name="TLayer"></typeparam>
        /// <typeparam name="TCode"></typeparam>
        /// <param name="node"></param>
        /// <param name="code"></param>
        /// <param name="createItemFunc"></param>
        /// <param name="createRangeFunc"></param>
        /// <param name="createNodeFunc">由父节点与编码值创建子节点的方法</param>
        /// <param name="insertFunc">将节点插入到指定节点的子节点集合中特定索引位置的方法</param>
        /// <param name="desc">是否降序排列</param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        public static IOperationResultEx OrderlyAdd<TLayer, TCode, TNode>(
            this TNode node,
            TCode code,
            Func<TLayer[], TCode> createItemFunc,
            Func<TLayer[], TCode> createRangeFunc,
            Func<(TNode parent, TCode code), IOperationResult<TNode>> createNodeFunc,
            Func<(TNode parent, TNode newNode, int index), IOperationResult> insertFunc,
            bool desc = false,
            IComparer<TCode>? comparer = null,
            IEqualityComparer<TLayer>? equalityComparer = null)
            where TCode : ILayeringAddressCode<TLayer>
            where TNode : IMultiTreeNode<TCode>
        {
            comparer ??= Comparer<TCode>.Default;
            return _insertFromCode<TLayer, TCode, TNode>(
                node, code,
                createItemFunc, createRangeFunc,
                (arg) =>
                {
                    int index = 0;
                    bool endFlag = false;
                    foreach (var item in arg.currentNode.Childrens)
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
                    var createResult = createNodeFunc((arg.currentNode, arg.newCode));
                    if (createResult.IsFailure)
                    {
                        return createResult;
                    }
                    var insertResult = insertFunc((arg.currentNode, createResult.Data!, index));
                    if (insertResult.IsFailure)
                    {
                        return OperationResult<TNode>.Failure(insertResult.FailureReason ?? $"插入子节点到索引为 {index} 的位置失败");
                    }
                    return (OperationResult<TNode>)createResult.Data!;
                },
                equalityComparer);
        }


        private static IOperationResultEx _insertFromCode<TLayer, TCode, TNode>(
            this TNode node,
            TCode code,
            Func<TLayer[], TCode> createItemFunc,
            Func<TLayer[], TCode> createRangeFunc,
            Func<(TCode newCode, TNode currentNode), IOperationResult<TNode>> insertImpl,
            IEqualityComparer<TLayer>? comparer)
            where TCode : ILayeringAddressCode<TLayer>
            where TNode : IMultiTreeNode<TCode>
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

            TNode currentNode = node;
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
                        currentNode = (TNode)existNode;
                    }
                }
                else
                {
                    TLayer[] newPath = LayeringAddressCodeHelper.CreatePath(currentNode.NodeValue.LayerValues, target);
                    TCode rangeCode = createRangeFunc(newPath);
                    var insertResult = insertImpl((rangeCode, currentNode));
                    if (insertResult.IsFailure)
                    {
                        return result = insertResult.FailureReason ?? "尝试由编码创建节点并插入到节点失败";
                    }
                    else
                    {
                        currentNode = insertResult.Data!;
                    }
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
                    var insertResult = insertImpl((newCode, currentNode));
                    if (insertResult.IsFailure)
                    {
                        return result = insertResult.FailureReason ?? "尝试由编码创建节点并插入到节点失败";
                    }
                }

            }

            return result = true;
        }

        #endregion

        #region 常用私有代码段
        /// <summary>
        /// 检查是否存在根节点, 以及根节点值是否有效值 (即是否范围编码), 如果没有根节点, 则根据是否可以使用编码值创建根节点, 来创建拥有完全范围编码的根节点或返回失败
        /// </summary>
        /// <typeparam name="TLayer"></typeparam>
        /// <typeparam name="TCode"></typeparam>
        /// <param name="tree"></param>
        /// <param name="createRangeFunc"></param>
        /// <returns></returns>
        private static IOperationResultEx checkOrCreateRootCode<TLayer, TCode>(
            IMultiTree<TCode> tree,
            Func<TLayer[], TCode> createRangeFunc)
            where TCode : ILayeringAddressCode<TLayer>
        {
            OperationResultEx result;
            TCode rootCode;
            if (tree.Root == null)
            {
                if (tree is ICanCreateRootNodeFrom<TCode> canCreateRootNodeTree)
                {
                    rootCode = createRangeFunc([]);
                    canCreateRootNodeTree.CreateRootNode(rootCode);
                }
                else
                {
                    return result = "传入多叉树没有根节点";
                }
            }
            else
            {
                rootCode = tree.Root.NodeValue;
            }
            if (!rootCode.IsRange)
            {
                return result = new Exception("树的节点值不是范围编码! ");
            }
            return result = true;
        }
        #endregion
    }
}
