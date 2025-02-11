using Common_Util.Data.Structure.Tree.Comparer;
using Common_Util.Interfaces.Owner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Structure.Tree.Extensions
{
    public static partial class IMultiTreeExtensions
    {
        /* 扩展: 将各种各样的数据整合为一棵树
         * 
         */

        /// <summary>
        /// 将 <paramref name="values"/> 组织为一个分支, 然后返回这个分支的根节点
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="values">
        /// 需要组织的节点值集合, 这些节点值必须符合以下条件: <br/>
        /// 1. 所有节点值都在同一个分支内, 即最浅层只有一个节点 <br/>
        /// 2. 最终所有节点值都能直接连接上 <br/> 
        /// </param>
        /// <param name="createBaseNodeFunc">
        /// 组织过程中, 根节点无法通过 <see cref="IAddableMultiTreeNode{TValue}.TryAdd(TValue, out IAddableMultiTreeNode{TValue}?)"/> 来获取, 需要手动创建
        /// </param>
        /// <param name="includeComparer">包含关系的比较器</param>
        /// <returns></returns>
        public static TNode TidyToTreeFork<TNode, TValue>(this IEnumerable<TValue> values, 
            Func<TValue, TNode> createBaseNodeFunc,
            ITreeNodeIncludeComparer<TValue> includeComparer)
            where TNode : IAddableMultiTreeNode<TValue, TNode>, ISameTypeParentOwner<TNode>
        {
            var arr = values.OrderBy(i => i, includeComparer.DepthComparer()).ToArray(); // 按深度由浅到深排序
            
            if (arr.Length == 0)
            {
                throw new InvalidOperationException($"传入集合 {nameof(values)} 为空");
            }
            // 最浅的首个节点值, 也就是最终的根节点
            var firstValue = arr[0];
            var firstNode = createBaseNodeFunc(firstValue);

            // 当前节点
            var currentNode = firstNode;

            for (int i = 1; i < arr.Length; i++) 
            {
                var value = arr[i];
                var includeResult = includeComparer.IncludeComparer(value, firstValue);
                // 检查是否在分支内, 不在分支内则提前结束
                if ((includeResult & TreeNodeIncludeCompareResult.ChildFork) != TreeNodeIncludeCompareResult.ChildFork
                    &&
                    (includeResult & TreeNodeIncludeCompareResult.SameOne) != TreeNodeIncludeCompareResult.SameOne)
                {
                    // 不是最浅的节点值, 也不在最浅的节点值内
                    throw new InvalidOperationException($"节点值 {value} 不在分支 {firstValue} 内! ");
                }

                // 移动当前节点, 找到 value 的邻接父节点
                IEnumerator<TNode>? currentChildEnumerator = null;
                while (true)
                {
                    if (currentChildEnumerator != null) // 只有 "非邻接父节点" 和 "不同分支" 两种情况需要继续遍历遍历器, 在 "邻接父节点" 的时候结束遍历 (此时也就会跳出循环了)
                    {
                        if (!currentChildEnumerator.MoveNext()) // 如果遍历结束, 则说明有节点值无法直接连接到 (正常是: 节点 -- 子节点 -- 孙子节点, 出现了: 节点 -- -- 孙子节点, 这样子的情况)
                        {
                            throw new InvalidOperationException($"未能找到节点值 {value} 的邻接父节点");
                        }
                        currentNode = currentChildEnumerator.Current;
                    }
                    var currentIncludeResult = includeComparer.IncludeComparer(currentNode.NodeValue, value);
                    if (currentIncludeResult.HasFlag(TreeNodeIncludeCompareResult.SameOne))
                    {
                        // 当前节点和 value 在同一位置, 跳过
                        goto GotoNext;
                    }
                    else if (currentIncludeResult.HasFlag(TreeNodeIncludeCompareResult.AdjoinParent))
                    {
                        // 邻接父节点
                        goto Found;
                    } 
                    else if (currentIncludeResult.HasFlag(TreeNodeIncludeCompareResult.DistantParent))
                    {
                        // 当前节点是 value 的非邻接父节点, 当前节点下移一层
                        currentChildEnumerator = currentNode.Childrens.GetEnumerator();
                    }
                    else if (currentIncludeResult.HasFlag(TreeNodeIncludeCompareResult.DiffFork))
                    {
                        // 当前节点与 value 不在一个分支内
                        if (currentChildEnumerator == null)
                        {
                            // 不处于遍历状态, 需要上移当前节点, 寻找 value 的非邻接父节点 (如果是邻接的话就不会到这一行代码了)
                            if (currentNode.Parent == null)
                            {
                                // 已经是最顶级的节点了, 说明不在同一个分支内
                                throw new Common_Util.Exceptions.General.ImpossibleForkException($"未能找到节点值 {value} 的非邻接父节点, 此节点值在分支 {firstValue} 之外");
                            }
                            else
                            {
                                currentNode = currentNode.Parent;
                            }
                        }
                        else
                        {
                            // 处于遍历状态, 继续遍历即可
                        }
                    }
                    else if (currentIncludeResult.HasFlag(TreeNodeIncludeCompareResult.AdjoinChild))
                    {
                        throw new Common_Util.Exceptions.General.ImpossibleForkException($"遍历过程是由浅层节点到深层节点遍历的, 不应该出现当前节点是节点值 {value} 的邻接子节点的情况! ");
                    }
                    else if (currentIncludeResult.HasFlag(TreeNodeIncludeCompareResult.DistantChild))
                    {
                        throw new Common_Util.Exceptions.General.ImpossibleForkException($"遍历过程是由浅层节点到深层节点遍历的, 不应该出现当前节点是节点值 {value} 的非邻接子节点的情况! ");
                    }
                    else
                    {
                        // 逻辑上没有其他的情况了
                        throw Common_Util.Exceptions.General.ImpossibleForkException.Create(currentIncludeResult);
                    }
                }
            Found:
                if (!currentNode.TryAdd(value, out var newNode))
                {
                    throw new InvalidOperationException($"添加节点值 {value} 到节点 {currentNode} 失败! ");
                }
            GotoNext:
                continue;

            }

            return firstNode;
        }
    }
}
