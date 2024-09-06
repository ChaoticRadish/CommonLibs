using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Structure.Tree
{
    /// <summary>
    /// 多叉树
    /// </summary>
    public interface IMultiTree<TValue>
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
}
