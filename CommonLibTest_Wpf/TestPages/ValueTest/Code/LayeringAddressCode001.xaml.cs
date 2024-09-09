using Common_Util.Data.Constraint;
using Common_Util.Data.Struct;
using Common_Util.Data.Structure.Tree;
using Common_Util.Data.Structure.Tree.Extensions;
using Common_Util.Data.Structure.Value;
using Common_Util.Data.Structure.Value.Extensions;
using Common_Util.Data.Wrapper;
using Common_Util.Extensions;
using Common_Util.Log;
using Common_Util.Module.Command;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CommonLibTest_Wpf.TestPages.ValueTest.Code
{
    /// <summary>
    /// LayeringAddressCode001.xaml 的交互逻辑
    /// </summary>
    public partial class LayeringAddressCode001 : Page
    {
        public LayeringAddressCode001()
        {
            InitializeComponent();

            ViewModel = new();
            DataContext = ViewModel;
        }

        private LayeringAddressCodeViewModel ViewModel { get; set; }
    }

    public class LayeringAddressCodeViewModel : Models.NotifyTestModelBase
    {

        private string inputText = string.Empty;
        public string InputText 
        {
            get => inputText;
            set
            {
                inputText = value;
                OnPropertyChanged();

                try
                {
                    CurrentInput = StringConveyingHelper.FromString<LayeringAddressCode>(inputText);
                }
                catch
                {
                    CurrentInput = LayeringAddressCode.All;
                }
            }
        }

        private LayeringAddressCode currentInput;
        public LayeringAddressCode CurrentInput
        {
            get => currentInput;
            set
            {
                currentInput = value;
                OnPropertyChanged();
                CheckSameAndUpdate();
            }
        }
        private void CheckSameAndUpdate()
        {
            foreach (var item in Items)
            {
                item.Check(CurrentInput);   
            }
        }



        public ObservableCollection<ListItem> Items { get; } = [
            new(){ Code = "aa.bb.cc:1" },
            new(){ Code = "aa.bb.cc:2" },
            new(){ Code = "0" },
            new(){ Code = "aa.bb.cc:3" },
            new(){ Code = "aa.bb.cc.4" },
            new(){ Code = "aa.bb.4" },
            new(){ Code = "aa.bb.cc:4" },
            new(){ Code = "aa.bb" },
            ];

        public ICommand AddToListCommand => new SampleCommand(addToList);
        private void addToList()
        {
            if (!Items.Any(i => i.Code == CurrentInput))
            {
                Items.Add(new ListItem()
                {
                    Code = CurrentInput,
                });
            }
        }

        public ICommand SortCommand => new SampleCommand(sort);
        private void sort() 
        {
            var newList = Items.ToList();
            newList.Sort();
            Items.Clear();
            newList.ForEach(Items.Add);
        }


        #region 树

        #region 增改删操作
        public ICommand AddToTreeCommand => new SampleCommand(addToTree);
        public ICommand AddToTreeAscCommand => new SampleCommand(addToTreeAsc);
        public ICommand AddToTreeDescCommand => new SampleCommand(addToTreeDesc);

        private void addToTree(object? obj)
        {
            if (obj is LayeringAddressCode code)
            {
                var result = Tree.Add<string, ListItem>(
                    new ListItem() { Code = code },
                    ListItem.ItemFromPath,
                    ListItem.RangeFromPath);
                Logger.Operation.Info(
                    result.Match(
                    () => "添加节点到树: 成功",
                    () => "添加节点到树: 失败"));
            }
            else
            {
                Logger.Def.Warning($"无法添加到树的对象: {obj ?? "<null>"}");
            }
        }

        private void addToTreeAsc(object? obj)
        {
            if (obj is LayeringAddressCode code)
            {
                var result = Tree.OrderlyAdd<string, ListItem>(
                    new ListItem() { Code = code },
                    ListItem.ItemFromPath,
                    ListItem.RangeFromPath,
                    desc: false,
                    new LayeringAddressCodeComparer<string, ListItem>());
                Logger.Operation.Info(
                    result.Match(
                    () => "添加节点到树(升序): 成功",
                    () => "添加节点到树(升序): 失败"));
            }
            else
            {
                Logger.Def.Warning($"无法添加到树的对象(升序): {obj ?? "<null>"}");
            }
        }

        private void addToTreeDesc(object? obj)
        {
            if (obj is LayeringAddressCode code)
            {
                var result = Tree.OrderlyAdd<string, ListItem>(
                    new ListItem() { Code = code },
                    ListItem.ItemFromPath,
                    ListItem.RangeFromPath,
                    desc: true,
                    new LayeringAddressCodeComparer<string, ListItem>());
                Logger.Operation.Info(
                    result.Match(
                    () => "添加节点到树(降序): 成功",
                    () => "添加节点到树(降序): 失败"));
            }
            else
            {
                Logger.Def.Warning($"无法添加到树的对象(降序): {obj ?? "<null>"}");
            }
        }


        public ICommand AddMiniForkCommand => new SampleCommand(addMiniFork);
        private void addMiniFork(object? obj)
        {
            LayeringAddressCode? code = null;

            if (obj is ListItem item)
            {
                code = item.Code;
            }
            else if (obj is LayeringAddressCode _code)
            {
                code = _code;
            }
            if (code != null)
            {
                if (!code.Value.IsRange)
                {
                    Logger.Operation.Warning($"无法添加迷你分支到节点: {code}, 因为该节点节点值不是范围节点");
                    return;
                }

                Logger.Operation.Info($"尝试添加迷你分支到节点: {code}");

                List<ILayeringAddressCode<string>> codes = LayeringAddressCodeHelper.Random(
                    ["aaa", "bbb", "ccc", "ddd"], ["1", "2", "3", "4", "5", "6", "7", "8", "9"],
                    1, 6, 2, 5, 0.99, 0.2).ConcatRange(code.Value).ToList();
                Logger.Operation.Info($"随机生成编码得: {codes.Select(i => ((LayeringAddressCode)(i.IsRange, i.LayerValues)).ConvertToString()).ToArray().FullInfoString()}");
                var fork = codes.CreateFork(null,
                    ListItem.ItemFromPath, ListItem.RangeFromPath);

                var addResult = Tree.AddTo<string, ListItem>(new ListItem() { Code = code.Value }, fork);
                Logger.Operation.Info(
                    addResult.Match(
                        () => $"添加随机迷你分支到 {code} : 成功",
                        () => $"添加随机迷你分支到 {code} : 失败, {addResult.FailureReason}"));
            }
            else
            {
                Logger.Def.Warning($"无法用作移除参数的对象: {obj ?? "<null>"}");
            }
        }


        public ICommand RemoveNodeCommand => new SampleCommand(removeNode);

        private void removeNode(object? obj)
        {
            LayeringAddressCode? code = null;

            if (obj is ListItem item)
            {
                code = item.Code;
            }
            else if (obj is LayeringAddressCode _code)
            {
                code = _code;
            }
            if (code != null)
            {
                Logger.Operation.Info($"尝试移除节点: {code}");

                var result1 = Tree.Find<string, ListItem>(code);
                Logger.Operation.Info(
                    result1.Match(
                        () => "在树中寻找节点: 成功",
                        () => "在树中寻找节点: 失败"));

                var result2 = Tree.Remove<string, ListItem>(code);
                Logger.Operation.Info(
                    result2.Match(
                        () => "移除节点: 成功",
                        () => "移除节点: 失败"));
            }
            else
            {
                Logger.Def.Warning($"无法用作移除参数的对象: {obj ?? "<null>"}");
            }
        }

        public ICommand RemoveRootNodeCommand => new SampleCommand(removeRootNode);

        private void removeRootNode()
        {
            Tree.RemoveRootNode();
            OnPropertyChanged(nameof(Tree));
            OnPropertyChanged(nameof(TreeRootLayer));
        }

        public ICommand SetRandomRootNodeCommand => new SampleCommand(setRandomRootNode);

        private void setRandomRootNode()
        {
            var rootNode = Tree.Root;
            List <ILayeringAddressCode<string>> codes = LayeringAddressCodeHelper.Random(
                ["aaa", "bbb", "ccc", "ddd", "ee", "ff"], ["1", "2", "3", "4", "5", "6", "7", "8", "9", "i", "ii", "iii"],
                3, 9, 3, 6, 0.99, 0.2).ToList();
            Logger.Operation.Info($"随机生成编码得: {codes.Select(i => ((LayeringAddressCode)(i.IsRange, i.LayerValues)).ConvertToString()).ToArray().FullInfoString()}");
            var fork = codes.CreateFork(null,
            ListItem.ItemFromPath, ListItem.RangeFromPath);

            Tree.SetRootNode(fork);
            
            OnPropertyChanged(nameof(Tree));
            OnPropertyChanged(nameof(TreeRootLayer));
    }
        #endregion

        #region 排序操作
        public ICommand TreeSordAsc1Command => new SampleCommand(treeSordAsc1);
        public ICommand TreeSordDesc1Command => new SampleCommand(treeSordDesc1);

        public ICommand TreeSordAsc2Command => new SampleCommand(treeSordAsc2);
        public ICommand TreeSordDesc2Command => new SampleCommand(treeSordDesc2);

        void treeSordAsc1()
        {
            Tree.Sort(new CompaperWrapper<ListItem, LayeringAddressCode>(item => item.Code), false);
        }
        void treeSordDesc1()
        {
            Tree.Sort(new CompaperWrapper<ListItem, LayeringAddressCode>(item => item.Code), true);
        }
        void treeSordAsc2()
        {
            Tree.Sort(new LayeringAddressCodeComparer<string, ListItem>(), false);
        }
        void treeSordDesc2()
        {
            Tree.Sort(new LayeringAddressCodeComparer<string, ListItem>(), true);
        }
        #endregion

        private ObservableMultiTree<ListItem>? tree;
        public ObservableMultiTree<ListItem> Tree 
        {
            get
            {
                if (tree == null)
                {
                    Tree = new();
                }
                return tree!;
            }
            set 
            {
                tree = value;
                tree.UnorderedCollectionChanged += Tree_UnorderedCollectionChanged;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TreeRootLayer));
            }
        }
        public ObservableCollection<ObservableMultiTreeNode<ListItem>> TreeRootLayer => Tree.Root == null ? [] : [Tree.Root];

        public ICommand BuildTreeCommand => new SampleCommand(buildTree);
        private void buildTree()
        {
            var tree = Items.AsMultiTree(
                ListItem.ItemFromPath, 
                ListItem.RangeFromPath);
            Tree = tree.AsObservableMultiTree();
        }

        private void Tree_UnorderedCollectionChanged(object? sender, Common_Util.Interfaces.Behavior.UnorderedCollectionChangedEventArgs<ObservableMultiTreeNode<ListItem>> e)
        {
            StringBuilder sb = new();
            sb.AppendLine($"Tree.无序集合变化事件: sender => ({sender?.GetType().Name ?? "<null>"}){sender}");
            sb.AppendLine($"Add: ({e.AddCount})");
            foreach (var item in e.GetAddItems())
            {
                sb.AppendLine("-" + item.NodeValue.ToString());
            }
            sb.AppendLine($"Remove: ({e.RemoveCount})");
            foreach (var item in e.GetRemoveItems())
            {
                sb.AppendLine("-" + item.NodeValue.ToString());
            }
            Logger.Def.Info(sb.ToString());
        }


        #endregion


        public class ListItem : Models.NotifyTestModelBase, IComparable<ListItem>, ILayeringAddressCode<string>, IEquatable<ListItem>
        {
            public static ListItem ItemFromPath(string[] path) => new ListItem() { Code = new() { LayerValues = path, IsRange = false } };
            public static ListItem RangeFromPath(string[] path) => new ListItem() { Code = new() { LayerValues = path, IsRange = true } };

            public LayeringAddressCode Code { get; set; }

            public string IsRangeString => IsAll ? "[ALL]" : (Code.IsRange ? "[范围]" : string.Empty);

            public string EndLayerValue => Code.LayerValues.Length > 0 ? Code.LayerValues[^1] : string.Empty;

            public string FullString => Code.ConvertToString();

            #region 接口 ILayeringAddressCode<string>

            public string[] LayerValues => Code.LayerValues;

            public int LayerCount => Code.LayerCount;

            public bool IsRange => Code.IsRange;

            #endregion


            public bool IsAll { get => Code.IsAll(); }

            public string CodeString { get => Code.ConvertToString(); }

            public bool SameWithCurrentInput { get; set; }

            public bool CurrentIsInThisRange { get; set; }

            public string HashString
            {
                get
                {
                    int hashCode = Code.GetHashCode();
                    StringBuilder sb = new StringBuilder();

                    sb.Append("Code Hash: ");
                    for (int i = 0; i < 32; i++)
                    {
                        int temp = (hashCode >> (32 - 1 - i)) & 0b1;

                        sb.Append(temp);
                    }
                    sb.AppendLine();

                    for (int l = 0; l < Code.LayerCount; l++)
                    {
                        int lHash = Code.LayerValues[l].GetHashCode();
                        sb.Append("l").Append(l).Append(" Hash: ");
                        for (int i = 0; i < 32; i++)
                        {
                            int temp = (lHash >> (32 - 1 - i)) & 0b1;

                            sb.Append(temp);
                        }
                        sb.AppendLine();
                    }
                    return sb.ToString();
                }
            }


            public void Check(LayeringAddressCode currentInput)
            {
                SameWithCurrentInput = Code == currentInput;
                CurrentIsInThisRange = currentInput.IsIn(Code);
                OnPropertyChanged(nameof(SameWithCurrentInput));
                OnPropertyChanged(nameof(CurrentIsInThisRange));
            }

            public int CompareTo(ListItem? other)
            {
                return Code.CompareTo(other?.Code);
            }

            public override string ToString()
            {
                return Code.ToString();
            }

            public bool Equals(ListItem? other)
            {
                return Code == other?.Code;
            }
        }
    }
}
