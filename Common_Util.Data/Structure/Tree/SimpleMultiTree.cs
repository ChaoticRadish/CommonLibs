using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Structure.Tree
{
    /// <summary>
    /// 简易多叉树
    /// </summary>
    public class SimpleMultiTree<TValue> : IMultiTree<TValue>
    {
        public SimpleMultiTreeNode<TValue>? Root { get; set; }

        IMultiTreeNode<TValue>? IMultiTree<TValue>.Root => Root;
    }
    /// <summary>
    /// 简易多叉树的节点
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class SimpleMultiTreeNode<TValue> : IMultiTreeNode<TValue>
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
    }
}
