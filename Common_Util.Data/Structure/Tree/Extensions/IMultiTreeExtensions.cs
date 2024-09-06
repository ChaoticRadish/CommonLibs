using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Structure.Tree.Extensions
{
    public static partial class IMultiTreeExtensions
    {
        #region 遍历过程


        /// <summary>
        /// 以先根次序遍历树, 取得 <see cref="IEnumerable{T}"/>
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="tree"></param>
        /// <returns></returns>
        public static IEnumerable<TValue> Preorder<TValue>(this IMultiTree<TValue> tree)
        {
            if (tree.Root == null)
            {
                return [];
            }
            else
            {
                return Preorder(tree.Root);
            }
        }
        /// <summary>
        /// 以先根次序遍历节点 (包含传入节点), 取得 <see cref="IEnumerable{T}"/>
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="node"></param>
        /// <returns></returns>
        public static IEnumerable<TValue> Preorder<TValue>(this IMultiTreeNode<TValue> node)
        {
            return PreorderNode(node).Select(n =>
            {
                return n.NodeValue;
            });
        }
        /// <summary>
        /// 以先根次序遍历节点 (包含传入节点)
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="node"></param>
        /// <returns></returns>
        public static IEnumerable<IMultiTreeNode<TValue>> PreorderNode<TValue>(this IMultiTreeNode<TValue> node)
        {
            Stack<(IMultiTreeNode<TValue>, IEnumerator<IMultiTreeNode<TValue>>)> nodeStack = new();
            nodeStack.Push((node, node.Childrens.GetEnumerator()));

            bool push = true;

            while (nodeStack.Count != 0)
            {
                (var currentNode, var childrenEnumerator) = nodeStack.Peek();

                if (push)
                {
                    yield return currentNode;
                }

                if (childrenEnumerator.MoveNext())
                {
                    var nextNode = childrenEnumerator.Current;
                    nodeStack.Push((nextNode, nextNode.Childrens.GetEnumerator()));
                    push = true;
                }
                else
                {
                    // 当前节点的子项枚举器已结束
                    nodeStack.Pop();
                    push = false;
                }


            }
        }



        /// <summary>
        /// 附带索引信息得, 以先根遍历的方式遍历树. 
        /// <para>索引信息为遍历过程中第 n-1 个被返回的节点. 因为是先根次序, 所以可以将节点的父节点索引一同返回</para>
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="tree"></param>
        /// <returns>
        /// 一个 <see cref="IEnumerable{T}"/> 其字段:
        /// <para>[0]: 当前节点的索引; [1]: 当前节点的父节点的索引, 如果没有父节点, 则为 -1; [2]: 当前节点的值</para></returns>
        public static IEnumerable<NodeIndexData<TValue>> IndexPreorder<TValue>(this IMultiTree<TValue> tree)
        {
            if (tree.Root == null)
            {
                return [];
            }
            else
            {
                return IndexPreorder(tree.Root);
            }
        }
        /// <summary>
        /// 附带索引信息得, 以先根遍历的方式遍历节点. 
        /// <para>索引信息为遍历过程中第 n-1 个被返回的节点. 因为是先根次序, 所以可以将节点的父节点索引一同返回</para>
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="node"></param>
        /// <returns>
        /// 一个 <see cref="IEnumerable{T}"/> 其字段:
        /// <para>[0]: 当前节点的索引; [1]: 当前节点的父节点的索引, 如果没有父节点, 则为 -1; [2]: 当前节点的值</para></returns>
        public static IEnumerable<NodeIndexData<TValue>> IndexPreorder<TValue>(this IMultiTreeNode<TValue> node) 
        {
            Stack<(IMultiTreeNode<TValue>, IEnumerator<IMultiTreeNode<TValue>>, int nodeIndex, int parentIndex)> nodeStack = new();

            int nextIndex = 0;
            const int nullNodeIndex = -1;   // 不存在的节点, 其索引用此值表示

            nodeStack.Push((node, node.Childrens.GetEnumerator(), nextIndex, nullNodeIndex));

            bool push = true;

            while (nodeStack.Count != 0)
            {
                (var currentNode, var childrenEnumerator, int nodeIndex, int parentIndex) = nodeStack.Peek();

                if (push)
                {
                    yield return new NodeIndexData<TValue>()
                    {
                        Node = currentNode,
                        ParentIndex = parentIndex,
                        NodeIndex = nodeIndex,
                        NodeValue = currentNode.NodeValue,
                    };
                    nextIndex++;
                }

                if (childrenEnumerator.MoveNext())
                {
                    var nextNode = childrenEnumerator.Current;
                    nodeStack.Push((nextNode, nextNode.Childrens.GetEnumerator(), nextIndex, nodeIndex));
                    push = true;
                }
                else
                {
                    // 当前节点的子项枚举器已结束
                    nodeStack.Pop();
                    push = false;
                }


            }
        }






        /// <summary>
        /// 以后根次序遍历树, 取得 <see cref="IEnumerable{T}"/>
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="tree"></param>
        /// <returns></returns>
        public static IEnumerable<TValue> Postorder<TValue>(this IMultiTree<TValue> tree)
        {
            if (tree.Root == null)
            {
                return [];
            }
            else
            {
                return Postorder(tree.Root);
            }
        }
        /// <summary>
        /// 以后根次序遍历节点, 取得 <see cref="IEnumerable{T}"/>
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="node"></param>
        /// <returns></returns>
        public static IEnumerable<TValue> Postorder<TValue>(this IMultiTreeNode<TValue> node)
        {
            return PostorderNode(node).Select(n => n.NodeValue);
        }
        /// <summary>
        /// 以后根次序遍历节点
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="node"></param>
        /// <returns></returns>
        public static IEnumerable<IMultiTreeNode<TValue>> PostorderNode<TValue>(this IMultiTreeNode<TValue> node)
        {
            Stack<(IMultiTreeNode<TValue>, IEnumerator<IMultiTreeNode<TValue>>)> nodeStack = new();
            nodeStack.Push((node, node.Childrens.GetEnumerator()));

            while (nodeStack.Count != 0)
            {
                (var currentNode, var childrenEnumerator) = nodeStack.Peek();


                if (childrenEnumerator.MoveNext())
                {
                    var nextNode = childrenEnumerator.Current;
                    nodeStack.Push((nextNode, nextNode.Childrens.GetEnumerator()));
                }
                else
                {
                    // 当前节点的子项枚举器已结束
                    nodeStack.Pop();

                    yield return currentNode;
                }
            }
        }




        /// <summary>
        /// 适用于方法 <see cref="IndexPreorder{TValue}(IMultiTree{TValue})"/> 返回枚举子项的结构体 
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        public struct NodeIndexData<TValue>
        {
            /// <summary>
            /// 当前节点的索引
            /// </summary>
            public int NodeIndex { get; set; }
            /// <summary>
            /// 当前节点的父节点的索引, 如果没有父节点, 则为 -1
            /// </summary>
            public int ParentIndex { get; set; }
            /// <summary>
            /// 当前节点
            /// </summary>
            public IMultiTreeNode<TValue> Node { get; set; }
            /// <summary>
            /// 当前节点的值
            /// </summary>
            public TValue NodeValue { get; set; }
        }
        #endregion

        #region 转换为特定类型的多叉树
        /// <summary>
        /// 将一个多叉树结构转换为通用多叉树
        /// <para>此转换可能导致可空性发生变化! </para>
        /// </summary>
        /// <typeparam name="TScope"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="tree"></param>
        /// <param name="convert2Scope">将一个节点的值转换为范围值</param>
        /// <returns></returns>
        public static GeneralTree<TScope, TValue> ToGeneralTree<TScope, TValue>(this IMultiTree<TValue> tree, Func<IMultiTreeNode<TValue>, TScope> convert2Scope)
        {
            GeneralTree<TScope, TValue> output = new();
            if (tree.Root == null) { return output; }
            output.BuildTree(tree.Root!,
                i => i.Childrens.ToList(),
                node => KeyValuePair.Create<TScope?, TValue?>(convert2Scope(node), node.NodeValue),
                node => true);
            return output;
        }

        /// <summary>
        /// 如果传入的树是 <see cref="SimpleMultiTree{TValue}"/> 则将其返回, 如果不是, 则实例化一个 <see cref="SimpleMultiTree{TValue}"/>
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="tree"></param>
        /// <returns></returns>
        public static SimpleMultiTree<TValue> AsSimpleMultiTree<TValue>(this IMultiTree<TValue> tree)
        {
            if (tree is SimpleMultiTree<TValue> simpleMultiTree)
            {
                return simpleMultiTree;
            }
            else
            {
                SimpleMultiTree<TValue> newTree = new SimpleMultiTree<TValue>();
                if (tree.Root == null) return newTree;

                List<SimpleMultiTreeNode<TValue>> newNodes = new List<SimpleMultiTreeNode<TValue>>();

                foreach (var item in IndexPreorder(tree))
                {
                    var newNode = new SimpleMultiTreeNode<TValue>(item.NodeValue);
                    if (item.ParentIndex >= 0)
                    {
                        var parentNode = newNodes[item.ParentIndex];
                        if (parentNode.Childrens == null)
                        {
                            parentNode.Childrens = [];
                        }
                        parentNode.Childrens.Add(newNode);
                    }
                    newNodes.Add(newNode);
                }
                newTree.Root = newNodes.FirstOrDefault();

                return newTree;
            }
        }

        /// <summary>
        /// 如果传入的树是 <see cref="ObservableMultiTree{TValue}"/> 则将其返回, 如果不是, 则实例化一个 <see cref="ObservableMultiTree{TValue}"/>
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="tree"></param>
        /// <returns></returns>
        public static ObservableMultiTree<TValue> AsObservableMultiTree<TValue>(this IMultiTree<TValue> tree)
        {
            if (tree is ObservableMultiTree<TValue> observableMultiTree)
            {
                return observableMultiTree;
            }
            else
            {
                ObservableMultiTree<TValue> newTree = new ObservableMultiTree<TValue>();
                if (tree.Root == null) return newTree;

                List<ObservableMultiTreeNode<TValue>> newNodes = [];

                foreach (var item in IndexPreorder(tree))
                {
                    ObservableMultiTreeNode<TValue>? parentNode = null;
                    if (item.ParentIndex >= 0)
                    {
                        parentNode = newNodes[item.ParentIndex];
                    }
                    var newNode = new ObservableMultiTreeNode<TValue>(parentNode, item.NodeValue);
                    if (parentNode != null)
                    {
                        parentNode.Childrens.Add(newNode);
                    }

                    newNodes.Add(newNode);
                }
                var root = newNodes.FirstOrDefault();
                if (root != null)
                {
                    newTree.SetRootNode(root);
                }

                return newTree;
            }
        }
        #endregion


        #region 转换树结构的内容

        /// <summary>
        /// 转换多叉树节点值的类型为另一种类型, 返回新的多叉树
        /// <para>将保留结构, 不会检查子分叉是否出现重复值! </para>
        /// </summary>
        /// <typeparam name="TNewValue"></typeparam>
        /// <typeparam name="TOldValue"></typeparam>
        /// <param name="tree"></param>
        /// <param name="valueConvertFunc"></param>
        /// <returns></returns>
        public static IMultiTree<TNewValue> Convert<TNewValue, TOldValue>(
            this IMultiTree<TOldValue> tree, 
            Func<IMultiTreeNode<TOldValue>, TNewValue> valueConvertFunc)
        {
            if (tree.Root == null)
            {
                return new SimpleMultiTree<TNewValue>();
            }
            else
            {
                return new SimpleMultiTree<TNewValue>()
                {
                    Root = _convert(tree.Root, valueConvertFunc)
                };
            }
        }

        /// <summary>
        /// 转化多叉树节点值的类型为另一种类型, 返回新的多叉树
        /// <para>将保留结构, 不会检查子分叉是否出现重复值! </para>
        /// </summary>
        /// <typeparam name="TNewValue"></typeparam>
        /// <typeparam name="TOldValue"></typeparam>
        /// <param name="node"></param>
        /// <param name="valueConvertFunc"></param>
        /// <returns></returns>
        public static IMultiTreeNode<TNewValue> Convert<TNewValue, TOldValue>(
            this IMultiTreeNode<TOldValue> node,
            Func<IMultiTreeNode<TOldValue>, TNewValue> valueConvertFunc)
        {
            return _convert(node, valueConvertFunc);
        }

        private static SimpleMultiTreeNode<TNewValue> _convert<TNewValue, TOldValue>(
            this IMultiTreeNode<TOldValue> node,
            Func<IMultiTreeNode<TOldValue>, TNewValue> valueConvertFunc)
        {
            List<SimpleMultiTreeNode<TNewValue>> newNodes = [];

            foreach (var item in IndexPreorder(node))
            {
                var newValue = valueConvertFunc.Invoke(item.Node);
                var newNode = new SimpleMultiTreeNode<TNewValue>(newValue);
                if (item.ParentIndex >= 0)
                {
                    var parentNode = newNodes[item.ParentIndex];
                    if (parentNode.Childrens == null)
                    {
                        parentNode.Childrens = [];
                    }
                    parentNode.Childrens.Add(newNode);
                }
                newNodes.Add(newNode);
            }

            return newNodes.Count > 0 ? new SimpleMultiTreeNode<TNewValue>(valueConvertFunc(node)) : newNodes[0];
        }

        #endregion



    }
}
