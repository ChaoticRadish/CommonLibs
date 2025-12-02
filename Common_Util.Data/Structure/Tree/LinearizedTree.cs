using Common_Util.Data.Helpers;
using Common_Util.Data.Structure.Tree.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Structure.Tree
{
    /// <summary>
    /// 结构固化的线性化多叉树
    /// </summary>
    /// <remarks>
    /// 预期使用场景: 数据持久化存储与加载, 不可变的多叉树重组为连续存储结构, 或其他
    /// </remarks>
    /// <typeparam name="TValue"></typeparam>
    public class LinearizedTree<TValue> : IMultiTree<TValue>
    {
        #region 数据
        /// <summary>
        /// 是否存在任意节点
        /// </summary>
        [MemberNotNullWhen(true, nameof(Root))]
        [MemberNotNullWhen(true, nameof(nodeValues))]
        [MemberNotNullWhen(true, nameof(nodeIndexInfos))]
        [MemberNotNullWhen(true, nameof(childIndices))]
        public bool AnyNode => NodeCount > 0;
        /// <summary>
        /// 节点数量
        /// </summary>
        public int NodeCount { get; init; }

        private readonly TValue[]? nodeValues;
        private readonly IndexInfo[]? nodeIndexInfos;
        private readonly int[]? childIndices;
        #endregion


        /// <summary>
        /// 实例化一个空的线性化多叉树
        /// </summary>
        public LinearizedTree() { }
        /// <summary>
        /// 使用已经线性化的数据实例化一个线性化多叉树
        /// </summary>
        /// <param name="values">包含两个数据: 1. 已经线性化之后数据对应节点的父节点索引; 2. 当前节点的节点值</param>
        public LinearizedTree(IEnumerable<(int parentIndex, TValue value)> values)
        {
            var sourceArr = values.ToArray();
            nodeValues = new TValue[sourceArr.Length];
            nodeIndexInfos = new IndexInfo[sourceArr.Length];
            int[] parentIndices = new int[sourceArr.Length];
            List<int>[] childGroupedIndices = ArrayHelper.CreateArray(sourceArr.Length, () => new List<int>());
            for (int i = 0; i < sourceArr.Length; i++)
            {
                var source = sourceArr[i];
                nodeValues[i] = source.value;
                parentIndices[i] = source.parentIndex;
                if (source.parentIndex >= 0)
                {
                    childGroupedIndices[source.parentIndex].Add(i);
                }
            }
            int childIndicesCount = childGroupedIndices.Sum(g => g.Count);
            childIndices = new int[childIndicesCount];
            int childIndicesWriteIndex = 0;
            for (int i = 0; i < sourceArr.Length; i++)
            {
                var group = childGroupedIndices[i];
                int start = childIndicesWriteIndex;
                for (int groupIndex = 0; groupIndex < group.Count; groupIndex++)
                {
                    childIndices[childIndicesWriteIndex] = group[groupIndex];
                    childIndicesWriteIndex++;
                }
                nodeIndexInfos[i] = new()
                {
                    ParentIndex = parentIndices[i],
                    ChildIndexStart = start,
                    ChildCount = group.Count,
                };
            }
            NodeCount = sourceArr.Length;
        }
        /// <summary>
        /// 根据一个多叉树的结构与数据, 实例化一个线性化多叉树
        /// </summary>
        /// <param name="multiTree"></param>
        public LinearizedTree(IMultiTree<TValue> multiTree) 
            : this(multiTree.IndexPreorder().Select(node => (node.ParentIndex, node.NodeValue)))
        {
        }


        /// <summary>
        /// 取得指定索引的节点的所有子节点
        /// </summary>
        /// <param name="nodeIndex"></param>
        /// <returns></returns>
        public IEnumerable<LinearizedTreeNode<TValue>> GetChildrens(int nodeIndex)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(nodeIndex, 0);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(nodeIndex, NodeCount);

            if (!AnyNode) yield break;

            var indexInfo = nodeIndexInfos[nodeIndex];
            if (indexInfo.ChildCount == 0) yield break;
            int end = indexInfo.ChildIndexStart + indexInfo.ChildCount;
            for (int indexOfChildIndices = indexInfo.ChildIndexStart; indexOfChildIndices < end; indexOfChildIndices++)
            {
                yield return getNode(indexOfChildIndices);
            }
        }
        public ReadOnlySpan<int> GetChildrenIndices(int nodeIndex)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(nodeIndex, 0);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(nodeIndex, NodeCount);

            if (!AnyNode) return [];

            var indexInfo = nodeIndexInfos[nodeIndex];
            if (indexInfo.ChildCount == 0) return [];

            return new ReadOnlySpan<int>(childIndices, indexInfo.ChildIndexStart, indexInfo.ChildCount);
        }

        private readonly struct IndexInfo
        {
            /// <summary>
            /// 父节点的索引值
            /// </summary>
            /// <remarks>
            /// 负值表示没有父节点
            /// </remarks>
            public int ParentIndex { get; init; }
            /// <summary>
            /// 子节点索引存储在 <see cref="LinearizedTree{TValue}.childIndices"/> 的范围起点
            /// </summary>
            public int ChildIndexStart { get; init; }
            /// <summary>
            /// 子节点数量
            /// </summary>
            public int ChildCount { get; init; }
        }
        #region 节点访问
        public IMultiTreeNode<TValue>? Root => AnyNode ? getNode(0) : null;
        public IEnumerable<LinearizedTreeNode<TValue>> Nodes
        {
            get
            {
                if (!AnyNode) yield break;
                for (int i = 0; i < NodeCount; i++)
                {
                    yield return getNode(i);
                }
            }
        }
        public LinearizedTreeNode<TValue> this[int nodeIndex]
        {
            get => GetNode(nodeIndex);
        }
        public LinearizedTreeNode<TValue> GetNode(int nodeIndex)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(nodeIndex, 0);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(nodeIndex, NodeCount);
            return getNode(nodeIndex);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private LinearizedTreeNode<TValue> getNode(int nodeIndex)
        {
            return new(this)
            {
                NodeIndex = nodeIndex,
                ParentIndex = nodeIndexInfos![nodeIndex].ParentIndex,
                NodeValue = nodeValues![nodeIndex],
            };
        }


        #endregion

    }
    /// <summary>
    /// 线性化多叉树的节点
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="parent"></param>
    public struct LinearizedTreeNode<TValue>(LinearizedTree<TValue> parent) : IMultiTreeNode<TValue>
    {
        private readonly LinearizedTree<TValue> parent = parent;

        /// <summary>
        /// 线性化多叉树节点的父节点在列表中的索引值
        /// </summary>
        public required int ParentIndex { get; init; }

        /// <summary>
        /// 线性化多叉树节点在列表中的索引值
        /// </summary>
        public required int NodeIndex { get; init; }
        /// <summary>
        /// 节点值
        /// </summary>
        public required TValue NodeValue { get; init; }
        /// <summary>
        /// 线性化多叉树节点的子节点在列表中的索引值
        /// </summary>
        public ReadOnlySpan<int> ChildrenIndices 
            => parent.GetChildrenIndices(NodeIndex);

        /// <summary>
        /// 所有直接的子节点
        /// </summary>
        public readonly IEnumerable<LinearizedTreeNode<TValue>> Childrens
            => parent.GetChildrens(NodeIndex);

        IEnumerable<IMultiTreeNode<TValue>> IMultiTreeNode<TValue>.Childrens => Childrens.Cast<IMultiTreeNode<TValue>>();
    }
}
