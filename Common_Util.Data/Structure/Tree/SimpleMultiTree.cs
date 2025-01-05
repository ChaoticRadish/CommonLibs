using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Structure.Tree
{
    /// <summary>
    /// 简易多叉树
    /// </summary>
    public class SimpleMultiTree<TValue> 
        : IAddableMultiTree<TValue>,
        IHasRootNodeAs<SimpleMultiTreeNode<TValue>>
    {
        public SimpleMultiTree() { }    
        /// <summary>
        /// 实例化树对象, 并实例化节点值为 <paramref name="rootValue"/> 的节点作为根节点
        /// </summary>
        /// <param name="rootValue"></param>
        public SimpleMultiTree(TValue rootValue)
        {
            Root = new(rootValue);
        }

        public SimpleMultiTreeNode<TValue>? Root { get; set; }

        IMultiTreeNode<TValue>? IMultiTree<TValue>.Root => Root;
        IAddableMultiTreeNode<TValue>? IAddableMultiTree<TValue>.Root => Root;
    }
    /// <summary>
    /// 简易多叉树的节点
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class SimpleMultiTreeNode<TValue> : IAddableMultiTreeNode<TValue, SimpleMultiTreeNode<TValue>>
    {
        public SimpleMultiTreeNode(TValue nodeValue) 
        {
            NodeValue = nodeValue;
        }
        public TValue NodeValue { get; set; }

        /// <summary>
        /// 所有子节点
        /// </summary>
        public List<SimpleMultiTreeNode<TValue>>? Childrens { get; set; }

        IEnumerable<IMultiTreeNode<TValue>> IMultiTreeNode<TValue>.Childrens => Childrens ?? [];
        IEnumerable<IAddableMultiTreeNode<TValue>> IAddableMultiTreeNode<TValue>.Childrens => Childrens ?? [];
        IEnumerable<SimpleMultiTreeNode<TValue>> IAddableMultiTreeNode<TValue, SimpleMultiTreeNode<TValue>>.Childrens => Childrens ?? [];
        IEnumerable<SimpleMultiTreeNode<TValue>> IMultiTreeNode<TValue, SimpleMultiTreeNode<TValue>>.Childrens => Childrens ?? [];

        #region 操作
        /// <summary>
        /// 使用传入值实例化新节点后, 添加到当前节点的子节点集合中
        /// </summary>
        /// <param name="item"></param>
        public void Add(TValue item)
        {
            ((IAddableMultiTreeNode<TValue, SimpleMultiTreeNode<TValue>>)this).TryAdd(item, out _);
        }

        public bool TryAdd(TValue item, [NotNullWhen(true)] out IAddableMultiTreeNode<TValue>? node)
        {
            return ((IAddableMultiTreeNode<TValue, SimpleMultiTreeNode<TValue>>)this).TryAdd(item, out node);
        }

        public bool TryAdd(TValue item, [NotNullWhen(true)] out SimpleMultiTreeNode<TValue>? node)
        {
            if (Childrens == null)
            {
                Childrens = [];
            }
            var _node = new SimpleMultiTreeNode<TValue>(item);
            Childrens.Add(_node);

            node = _node;
            return true;
        }
        #endregion
    }
}
