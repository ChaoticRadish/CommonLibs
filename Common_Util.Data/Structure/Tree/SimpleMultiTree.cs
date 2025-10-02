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
    public class SimpleMultiTree<TValue> : SimpleMultiTree<TValue, SimpleMultiTreeNode<TValue>>
    {
        public SimpleMultiTree() : base() { }
        /// <summary>
        /// 实例化树对象, 并实例化节点值为 <paramref name="rootValue"/> 的节点作为根节点
        /// </summary>
        /// <param name="rootValue"></param>
        public SimpleMultiTree(TValue rootValue) : base()
        {
            Root = new(rootValue);
        }
    }
    /// <summary>
    /// 自定义节点类型的简易多叉树
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TNode"></typeparam>
    public class SimpleMultiTree<TValue, TNode>
        : IAddableMultiTree<TValue>,
        IHasRootNodeAs<TNode>
        where TNode : SimpleMultiTreeNode<TValue, TNode>
    {
        public SimpleMultiTree() { }

        public TNode? Root { get; set; }

        IMultiTreeNode<TValue>? IMultiTree<TValue>.Root => Root;
        IAddableMultiTreeNode<TValue>? IAddableMultiTree<TValue>.Root => Root;
    }
    /// <summary>
    /// 简易多叉树的节点
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class SimpleMultiTreeNode<TValue> : SimpleMultiTreeNode<TValue, SimpleMultiTreeNode<TValue>>
    {
        public SimpleMultiTreeNode(TValue nodeValue, IList<SimpleMultiTreeNode<TValue>>? initChildrens = null) : base(nodeValue, initChildrens)
        {
        }

        protected override SimpleMultiTreeNode<TValue> CreateChildren(TValue value)
        {
            return new(value);
        }
    }
    /// <summary>
    /// 自定义子节点类型的简易多叉树的节点
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public abstract class SimpleMultiTreeNode<TValue, TChildren> : IAddableMultiTreeNode<TValue, TChildren>
        where TChildren : SimpleMultiTreeNode<TValue, TChildren>
    {
        public SimpleMultiTreeNode(TValue nodeValue, IList<TChildren>? initChildrens = null) 
        {
            NodeValue = nodeValue;
            Childrens = initChildrens;
        }
        public TValue NodeValue { get; set; }

        /// <summary>
        /// 所有子节点
        /// </summary>
        public IList<TChildren>? Childrens { get; protected set; }
        /// <summary>
        /// 检查 <see cref="Childrens"/> 是否已经初始化, 如果未初始化则初始化它
        /// </summary>
        [MemberNotNull(nameof(Childrens))]
        public void CheckInitChildrenList()
        {
            Childrens ??= InitCreateChildrenList();
        }
        protected virtual IList<TChildren> InitCreateChildrenList()
        {
            return [];
        }
        protected abstract TChildren CreateChildren(TValue value);

        IEnumerable<IMultiTreeNode<TValue>> IMultiTreeNode<TValue>.Childrens => Childrens ?? [];
        IEnumerable<IAddableMultiTreeNode<TValue>> IAddableMultiTreeNode<TValue>.Childrens => Childrens ?? [];
        IEnumerable<TChildren> IAddableMultiTreeNode<TValue, TChildren>.Childrens => Childrens ?? [];
        IEnumerable<TChildren> IMultiTreeNode<TValue, TChildren>.Childrens => Childrens ?? [];

        #region 操作
        public virtual void AddNode(TChildren node)
        {
            CheckInitChildrenList();
            Childrens.Add(node);
        }

        /// <summary>
        /// 使用传入值实例化新节点后, 添加到当前节点的子节点集合中
        /// </summary>
        /// <param name="item"></param>
        public void Add(TValue item)
        {
            ((IAddableMultiTreeNode<TValue, TChildren>)this).TryAdd(item, out _);
        }

        public bool TryAdd(TValue item, [NotNullWhen(true)] out IAddableMultiTreeNode<TValue>? node)
        {
            return ((IAddableMultiTreeNode<TValue, TChildren>)this).TryAdd(item, out node);
        }

        public bool TryAdd(TValue item, [NotNullWhen(true)] out TChildren? node)
        {
            var _node = CreateChildren(item);

            AddNode(_node);

            node = _node;
            return true;
        }
        #endregion
    }
}
