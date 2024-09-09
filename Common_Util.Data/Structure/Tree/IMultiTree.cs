using Common_Util.Interfaces.Behavior;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Structure.Tree
{
    /// <summary>
    /// 多叉树
    /// </summary>
    public interface IMultiTree<TValue> : ITree<TValue>
    {
        /// <summary>
        /// 树的根节点, 此值可以为空
        /// </summary>
        public IMultiTreeNode<TValue>? Root { get; }
    }


    /// <summary>
    /// 多叉树节点
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public interface IMultiTreeNode<TValue> 
    {
        /// <summary>
        /// 节点数据
        /// </summary>
        public TValue NodeValue { get; }

        /// <summary>
        /// 所有直接的子节点 (不包含孙子节点或更下层的节点)
        /// </summary>
        public IEnumerable<IMultiTreeNode<TValue>> Childrens { get; } 
    }

    /// <summary>
    /// 树节点可添加子项的多叉树
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public interface IAddableMultiTree<TValue> : IMultiTree<TValue>
    {
        /// <summary>
        /// 树的根节点, 此值可以为空
        /// </summary>
        public new IAddableMultiTreeNode<TValue>? Root { get; }
    }

    /// <summary>
    /// 可添加子项的树节点
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public interface IAddableMultiTreeNode<TValue> : IMultiTreeNode<TValue>, IAddable<TValue>
    {
        /// <summary>
        /// 尝试创建一个节点值为 <paramref name="item"/> 的新子节点并添加到当前节点的子节点集合中
        /// </summary>
        /// <param name="item"></param>
        /// <param name="node">如果添加成功, 将 <see langword="out"/> 新创建的子节点</param>
        /// <returns></returns>
        bool TryAdd(TValue item, [NotNullWhen(true)] out IAddableMultiTreeNode<TValue>? node);
        /// <summary>
        /// 所有直接的子节点 (不包含孙子节点或更下层的节点)
        /// </summary>
        public new IEnumerable<IAddableMultiTreeNode<TValue>> Childrens { get; }
    }



}
