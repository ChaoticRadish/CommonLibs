using Common_Util.Data.Constraint;
using Common_Util.Data.Structure.Tree.Extensions;
using Common_Util.Extensions.ObjectModel;
using Common_Util.Interfaces.Behavior;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Structure.Tree
{
    /// <summary>
    /// 可观测变化的多叉树 (树本身不实现 <see cref="INotifyCollectionChanged"/>, 因为不是一维结构, 节点中的子节点集合类型为 <see cref="ObservableCollection{T}"/>)
    /// <para>可枚举: 按先根次序遍历树节点值</para>
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class ObservableMultiTree<TValue> 
        : IMultiTree<TValue>, 
        IEnumerable<TValue>,
        IUnorderedCollectionChanged<ObservableMultiTreeNode<TValue>>,
        ISuspendUpdate,
        ISortable<TValue>
    {
        public ObservableMultiTreeNode<TValue>? Root { get; private set; }

        IMultiTreeNode<TValue>? IMultiTree<TValue>.Root => Root;

        #region 子项
        public void CreateRootNode(TValue nodeValue)
        {
            if (Root != null)
            {
                RemoveRootNode();
            }
            var node = new ObservableMultiTreeNode<TValue>(null, nodeValue)
            {
                SettingTree = this,
            };
            node.ForkChildrenCollectionChanged += Root_ForkChildrenCollectionChanged;
            Root = node;
        }

        /// <summary>
        /// 将根节点重设为传入节点, 将改变节点当前所属树 <see cref="ObservableMultiTreeNode{TValue}.SettingTree"/>
        /// </summary>
        /// <param name="node"></param>
        public void SetRootNode(ObservableMultiTreeNode<TValue> node)
        {
            if (Root != null)
            {
                RemoveRootNode();
            }
            node.SettingTree = this;
            node.ForkChildrenCollectionChanged += Root_ForkChildrenCollectionChanged;
            Root = node;
            if (UnorderedCollectionChanged != null)
            {
                var addItems = node.PreorderNode()
                                    .Select(n => (ObservableMultiTreeNode<TValue>)n)
                                    .ToArray();
                if (addItems.Length != 0)
                {
                    UnorderedCollectionChanged.Invoke(
                        this,
                        UnorderedCollectionChangedEventArgsHelper.AddItems(addItems));
                }
            }
        }

        public void RemoveRootNode()
        {
            if (Root == null) return;
            var node = Root;
            Root.ForkChildrenCollectionChanged -= Root_ForkChildrenCollectionChanged;
            Root = null;

            if (UnorderedCollectionChanged != null)
            {
                var removeItems = node.PreorderNode()
                                    .Select(n => (ObservableMultiTreeNode<TValue>)n)
                                    .ToArray();
                if (removeItems.Length != 0)
                {
                    UnorderedCollectionChanged.Invoke(
                        this,
                        UnorderedCollectionChangedEventArgsHelper.RemoveItems(removeItems));
                }
            }
        }


        private void Root_ForkChildrenCollectionChanged(object? sender, UnorderedCollectionChangedEventArgs<ObservableMultiTreeNode<TValue>> e)
        {
            lock (triggerUCCLocker)
            {
                if (IsSuspendUpdate)
                {
                    if (e.AddItems != null)
                    {
                        foreach (var item in e.AddItems)
                        {
                            suspendAdd.Add(item);
                            suspendRemove.Remove(item);
                        }
                    }
                    if (e.RemoveItems != null)
                    {
                        foreach (var item in e.RemoveItems)
                        {
                            suspendAdd.Remove(item);
                            suspendRemove.Add(item);
                        }
                    }
                }
                else
                {
                    UnorderedCollectionChanged?.Invoke(this, e);
                }
            }
        }

        #endregion

        #region IUnorderedCollectionChanged

        /// <summary>
        /// 触发 <see cref="UnorderedCollectionChanged"/> 事件的锁, 将在收到节点的变化事件与接触挂起时使用
        /// </summary>
        private readonly object triggerUCCLocker = new();
        public event EventHandler<UnorderedCollectionChangedEventArgs<ObservableMultiTreeNode<TValue>>>? UnorderedCollectionChanged;

        

        #endregion

        #region ISuspendUpdate
        public bool IsSuspendUpdate { get; private set; }
        private HashSet<ObservableMultiTreeNode<TValue>> suspendAdd = [];
        private HashSet<ObservableMultiTreeNode<TValue>> suspendRemove = [];


        public void SuspendUpdate()
        {
            IsSuspendUpdate = true;
        }

        public void ResumeUpdate()
        {
            IsSuspendUpdate = false;

            lock (triggerUCCLocker)
            {
                if (suspendAdd.Any() || suspendRemove.Any())
                {
                    UnorderedCollectionChanged?.Invoke(
                        this, 
                        UnorderedCollectionChangedEventArgsHelper.Changed<ObservableMultiTreeNode<TValue>>(
                            suspendAdd.ToArray(),
                            suspendRemove.ToArray()));

                    suspendAdd.Clear();
                    suspendRemove.Clear();
                }

            }

        }



        #endregion


        #region IEnumerator
        public IEnumerator<TValue> GetEnumerator()
        {
            return this.Preorder().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region ISortable

        /// <summary>
        /// 使用传入的比较器, 对<b>所有节点</b>的子节点集合执行排序
        /// </summary>
        /// <param name="comparer"></param>
        /// <param name="desc"></param>
        public void Sort(IComparer<TValue> comparer, bool desc)
        {
            if (Root == null) return;
            foreach (var nodeData in Root.PostorderNode())
            {
                if (nodeData is ObservableMultiTreeNode<TValue> node)
                {
                    node.Sort(comparer, desc);
                }
            }
        }
        #endregion
    }

    public class ObservableMultiTreeNode<TValue> : IMultiTreeNode<TValue>, IUnorderedCollectionChanged<ObservableMultiTreeNode<TValue>>, ISortable<TValue>
    {

        public ObservableMultiTreeNode(ObservableMultiTreeNode<TValue>? parent, TValue nodeValue)
        {
            Parent = parent;
            NodeValue = nodeValue;

            Childrens.CollectionChanged += Childrens_CollectionChanged;
        }



        #region 数据
        public TValue NodeValue { get; set; }

        /// <summary>
        /// 子节点集合
        /// </summary>
        public ObservableCollection<ObservableMultiTreeNode<TValue>> Childrens { get; private set; } = [];

        IEnumerable<IMultiTreeNode<TValue>> IMultiTreeNode<TValue>.Childrens => Childrens;

        #endregion

        #region 关系
        /// <summary>
        /// 所属树, 未设置 <see cref="SettingTree"/> 时, 将一路寻找父节点的 <see cref="Tree"/> 值, 直到找到非 null 值或根节点
        /// </summary>
        public ObservableMultiTree<TValue>? Tree
        {
            get
            {
                return SettingTree ?? Parent?.Tree;
            }
        }
        /// <summary>
        /// 所属树的设定值
        /// </summary>
        public ObservableMultiTree<TValue>? SettingTree { get; set; }

        /// <summary>
        /// 父节点
        /// </summary>
        public ObservableMultiTreeNode<TValue>? Parent { get; set; }

        /// <summary>
        /// 是否拥有父节点
        /// </summary>
        public bool HasParent { get => Parent != null; }
        
        #endregion



        #region 节点变更

        private void Childrens_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            var tree = Tree;
            if (tree != null)
            {
                if (e.Action == NotifyCollectionChangedAction.Move) return; // 移动子项的情况无需触发集合变化

                // 能够找到对应树的情况下, 触发无序集合变化事件
                UnorderedCollectionChangedEventArgs<ObservableMultiTreeNode<TValue>> args;
                args = UnorderedCollectionChangedEventArgsHelper.Changed<ObservableMultiTreeNode<TValue>>(
                    findNodeAndTidyAllNodeOnFork(e.NewItems).ToArray(),
                    findNodeAndTidyAllNodeOnFork(e.OldItems).ToArray());
                if (args.Count > 0)
                {
                    UnorderedCollectionChanged?.Invoke(this, args);

                    _childrenCollectionChange(this, args);
                }
            }
        }
        private IEnumerable<ObservableMultiTreeNode<TValue>> findNodeAndTidyAllNodeOnFork(IList? list)
        {
            if (list == null) yield break;

            foreach (var obj in list)
            {
                if (obj is ObservableMultiTreeNode<TValue> node)
                {
                    foreach (var n in node.PreorderNode())
                    {
                        if (n is ObservableMultiTreeNode<TValue> _n)
                        {
                            yield return _n;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 需要由子节点调用, 逐级传递无序集合的变更信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void _childrenCollectionChange(ObservableMultiTreeNode<TValue> sender, UnorderedCollectionChangedEventArgs<ObservableMultiTreeNode<TValue>> args)
        {
            if (Parent == null)
            {
                ForkChildrenCollectionChanged?.Invoke(sender, args);
            }
            else
            {
                Parent._childrenCollectionChange(sender, args);
            }
        }
        #endregion

        #region 无序集合变更事件

        /// <summary>
        /// 当前节点的子节点 (相邻的下一级节点) 集合变更事件
        /// </summary>
        public event EventHandler<UnorderedCollectionChangedEventArgs<ObservableMultiTreeNode<TValue>>>? UnorderedCollectionChanged;

        /// <summary>
        /// 当前分支下, 任意节点的子节点集合变更事件
        /// <para>仅在当前节点没有父节点时 (<see cref="Parent"/> == null) 触发, 否则会将变更内容继续向父节点传递</para>
        /// </summary>
        public event EventHandler<UnorderedCollectionChangedEventArgs<ObservableMultiTreeNode<TValue>>>? ForkChildrenCollectionChanged;

        #endregion


        #region 操作


        /// <summary>
        /// 使用传入的比较器, 对<b>当前节点</b>的子节点集合执行排序
        /// </summary>
        /// <param name="comparer"></param>
        /// <param name="desc">是否执行降序排序</param>
        public void Sort(IComparer<TValue> comparer, bool desc)
        {
            Childrens.Sort(n => n.NodeValue, comparer, desc);
        }
        #endregion

    }
}