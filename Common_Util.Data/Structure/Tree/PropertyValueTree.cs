using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Structure.Tree
{
    /// <summary>
    /// <para>基于属性值分组的树结构</para>
    /// <para>默认使用反射取值</para>
    /// <typeparam name="TValue">节点的类型</typeparamref>
    /// </summary>
    public class PropertyValueTree<TValue>
    {
        #region 配置
        public class ConfigItem
        {
            /// <summary>
            /// 层编号, 0为根节点, 可配置值从1开始
            /// </summary>
            public int? LayerId { get; set; }

            /// <summary>
            /// 属性名
            /// </summary>
            public string PropertyName { get; init; } = string.Empty;

            /// <summary>
            /// 取得对应值的方法, 未配置时将默认使用反射找到同名的属性或字段的值
            /// </summary>
            public Func<TValue, object?>? GetValueFunc { get; set; }

            public delegate bool IsSameDelegate(object? a, object? b);
            /// <summary>
            /// 比较两个属性值是否相等的方法, 未配置时将会使用是否均为 null 或 <see cref="object.Equals(object?)"/> 来判断是否相等
            /// </summary>
            public IsSameDelegate? IsSameFunc { get; set; }

            public delegate bool IsEndPointDelegate(int layerId, string propertyName, object? value);
            /// <summary>
            /// 判断是否已经到达终结点节点的方法, 未配置时当到达叶子层数(未配置时默认为非叶子最大层数+1)后就会被视为叶子节点
            /// </summary>
            public IsEndPointDelegate? IsEndPointFunc { get; set; }
        }
        public class ConfigCollection
        {
            #region 暴露给外面的数据
            /// <summary>
            /// 根外的第一层ID
            /// </summary>
            public int FirstLayerId { get; private set; }
            /// <summary>
            /// 非叶子最大层ID
            /// </summary>
            public int MaxLayerId { get; private set; }
            /// <summary>
            /// 叶子层ID
            /// </summary>
            public int LeafLayerId { get; private set; }
            /// <summary>
            /// 每一层的配置数据, 根层和叶子层没有配置数据
            /// </summary>
            public IEnumerable<ConfigItem> Configs { get => _Configs; }
            /// <summary>
            /// 非叶子非根层的ID集合, 顺序从浅到深
            /// </summary>
            public IEnumerable<int> LayerIds { get => (IEnumerable<int>?)_LayerIds ?? Array.Empty<int>(); }

            /// <summary>
            /// 取得层ID对应的配置
            /// </summary>
            /// <param name="layerId"></param>
            /// <returns></returns>
            public ConfigItem? this[int layerId]
            {
                get
                {
                    return _Configs.FirstOrDefault(i => i.LayerId == layerId);
                }
            }
            #endregion

            internal ConfigCollection(ConfigItem[] configs, int? LeafLayerId = null) 
            {
                _Configs = configs;
                _settingLeafLayerId = LeafLayerId;
            }
            private int? _settingLeafLayerId;

            internal ConfigItem[] _Configs { get; }
            internal List<int>? _LayerIds { get; private set; }





            /// <summary>
            /// 整理数据
            /// </summary>
            internal void Tidy()
            {
                Type type = typeof(TValue);
                PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);    // 公共实例属性
                FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);   // 公共实例字段

                int autoIdOffset = 1;   // 层ID默认偏移量: 1 因为需要跳过0值
                int currentMaxId = 0;
                for (int i = 0; i < _Configs.Length; i++)
                {
                    // 整理配置项内的层ID值
                    if (_Configs[i].LayerId == null)
                    {
                        int temp;
                        int tryCount = -1;
                        do
                        {
                            tryCount++;
                            temp = autoIdOffset + tryCount + i;
                        } while (temp < currentMaxId || _Configs.Any(i => i.LayerId == temp) || (_settingLeafLayerId != null &&  _settingLeafLayerId == temp));
                        autoIdOffset += tryCount;
                        _Configs[i].LayerId = temp;

                    }
                    if (currentMaxId < _Configs[i].LayerId)
                    {
                        currentMaxId = _Configs[i].LayerId!.Value;
                    }

                    // 配置默认的相关方法
                    if (_Configs[i].IsSameFunc == null)
                    {
                        _Configs[i].IsSameFunc = (a, b) =>
                        {
                            return (a == null && b == null) || (a != null && b != null && a.Equals(b));
                        };
                    }
                    if (_Configs[i].GetValueFunc == null)
                    {
                        var property = properties.FirstOrDefault(p => p.Name == _Configs[i].PropertyName);
                        if (property != null)
                        {
                            _Configs[i].GetValueFunc = (v) =>
                            {
                                return property.GetValue(v);
                            };
                        }
                        else
                        {
                            var field = fields.FirstOrDefault(f => f.Name == _Configs[i].PropertyName);
                            if (field != null)
                            {
                                _Configs[i].GetValueFunc = (v) =>
                                {
                                    return field.GetValue(v);
                                };
                            }
                            else
                            {
                                throw new Exception($"层 {_Configs[i]} ({_Configs[i].PropertyName})未配置获取值的方法, 无法找到与配置的名字同名的公共属性或字段! ");
                            }
                        }

                    }
                }

                _LayerIds = _Configs
                    .Where(i => i.LayerId != null)
                    .Select(i => i.LayerId!.Value)
                    .OrderBy(i => i)
                    .ToList();
                FirstLayerId = _LayerIds.First();
                MaxLayerId = currentMaxId;
                LeafLayerId = _settingLeafLayerId ?? (MaxLayerId + 1);
            }
            /// <summary>
            /// 找到输入层ID的下一个层ID
            /// </summary>
            /// <param name="currentLayer"></param>
            /// <returns></returns>
            internal int NextLayer(int currentLayer)
            {
                if (currentLayer <= 0)
                {
                    return FirstLayerId;
                }
                if (currentLayer >= MaxLayerId)
                {
                    return LeafLayerId;
                }
                else
                {
                    return _LayerIds?.First(i => i > currentLayer) ?? LeafLayerId;
                }
            }
        }

        #endregion

        #region 构建

        /// <summary>
        /// 使用集合数据以及配置信息构建树
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="configs">层ID值为空时, 将按顺序提供一个值</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static PropertyValueTree<TValue> Build(
            IEnumerable<TValue> collection,
            ConfigItem[] configs, 
            int? leafLayerId = null)
        {
            if (collection.Any(i => i == null))
            {
                throw new ArgumentException("传入数据出现null值", nameof(collection));
            }
            if (configs == null || configs.Length == 0)
            {
                throw new ArgumentException("没有传入树结构的配置数据", nameof(configs));
            }
            if (configs.Any(i => i.LayerId != null && i.LayerId <= 0))
            {
                throw new ArgumentException("配置的层ID出现非正值! ", nameof(configs));
            }

            ConfigCollection configCollection = new ConfigCollection(configs, leafLayerId);
            configCollection.Tidy();

            PropertyValueTree<TValue> output = new()
            {
                Config = configCollection,
            };

            DataGroup rootGroup = new DataGroup(configCollection.FirstLayerId, output.Root, configCollection);
            rootGroup.Datas.AddRange(collection);
            var nodes = rootGroup.ToNodes();
            if (nodes != null)
            {
                output.Root.ChildrenNodes.AddRange(nodes);
            }

            return output;
        }
        /// <summary>
        /// 使用集合数据以及配置信息构建树
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="configs">层ID值为空时, 将按顺序提供一个值</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static PropertyValueTree<TValue> Build(IEnumerable<TValue> collection, 
            params ConfigItem[] configs)
        {
            return Build(collection, configs, null);
        }

        private class DataGroup
        {
            public DataGroup(int layerId, Node parent, ConfigCollection configs) 
            {
                LayerId = layerId;
                Parent = parent;
                Configs = configs;
            }
            /// <summary>
            /// 层ID
            /// </summary>
            public int LayerId { get; }

            /// <summary>
            /// 组所属值
            /// </summary>
            public object? GroupParentValue { get => Parent.NodeValue; }

            /// <summary>
            /// 配置数据
            /// </summary>
            public ConfigCollection Configs {  get; }

            public bool IsLeaf { get; }
            /// <summary>
            /// 此组所属的父节点
            /// </summary>
            public Node Parent { get; }
            /// <summary>
            /// 此组包含的数据
            /// </summary>
            public List<TValue> Datas { get; init; } = new List<TValue>();

            public List<Node>? ToNodes()
            {
                if (Datas.Count == 0)
                {
                    return null;
                }
                else if (Configs.LeafLayerId == LayerId)
                {
                    return Datas.Select(i => new Node()
                    {
                        LayerId = LayerId,
                        Config = null,
                        IsRoot = false,
                        IsEndPoint = false,
                        IsLeaf = true,
                        Value = i,
                        Parent = Parent,
                    }).ToList();
                }
                ConfigItem config = Configs[LayerId] ?? throw new Exception("没有找到层ID对应的配置");

                List<Node> output = new List<Node>();

                int nextId = Configs.NextLayer(LayerId);
                bool nextIsLeaf = nextId == Configs.LeafLayerId;

                List<DataGroup> childGroups = new List<DataGroup>();
                foreach (TValue data in Datas)
                {
                    object? value = config.GetValueFunc!(data);

                    bool isEndPoint = nextIsLeaf;
                    if (!isEndPoint && config.IsEndPointFunc != null)
                    {
                        isEndPoint = config.IsEndPointFunc.Invoke(LayerId, config.PropertyName, value);
                    }


                    bool foundGroup = false;
                    foreach (DataGroup group in childGroups)
                    {
                        if (config.IsSameFunc!(group.GroupParentValue, value))
                        {
                            group.Datas.Add(data);
                            foundGroup = true;
                        }
                    }
                    if (!foundGroup)
                    {
                        Node node = new()
                        {
                            LayerId = LayerId,
                            Config = config,
                            IsRoot = false,
                            IsEndPoint = isEndPoint,
                            IsLeaf = false,
                            NodeValue = value,
                            Parent = Parent,
                        };

                        DataGroup group = new DataGroup(
                            isEndPoint ? Configs.LeafLayerId : nextId, 
                            node, Configs);
                        group.Datas.Add(data);
                        childGroups.Add(group);
                        output.Add(node);
                    }

                }
                foreach (DataGroup group in childGroups)
                {
                    var nodes = group.ToNodes();
                    if (nodes != null)
                    {
                        group.Parent.ChildrenNodes.AddRange(nodes);
                    }
                }



                return output;
            }
        }


        #endregion

        #region 节点结构
        public class Node
        {

            /// <summary>
            /// 所属层
            /// </summary>
            public int LayerId { get; init; }

            /// <summary>
            /// 节点对应值
            /// </summary>
            public object? NodeValue { get; init; }

            /// <summary>
            /// 关联的配置
            /// </summary>
            public ConfigItem? Config { get; init; }

            /// <summary>
            /// 父节点
            /// </summary>
            public Node? Parent { get; set; }
            /// <summary>
            /// 子节点
            /// </summary>
            public IEnumerable<Node> Childrens { get => ChildrenNodes; }
            /// <summary>
            /// 子节点
            /// </summary>
            internal List<Node> ChildrenNodes { get; private set; } = new List<Node>();


            /// <summary>
            /// 是否根节点
            /// </summary>
            public bool IsRoot { get; init; }

            /// <summary>
            /// 是否终结点
            /// </summary>
            public bool IsEndPoint { get; init; }

            /// <summary>
            /// 当前是否叶子节点, 一个终结点可能会分出多个叶子
            /// </summary>
            public bool IsLeaf { get; init; }

            /// <summary>
            /// 节点值
            /// </summary>
            public TValue? Value { get; init; }


            #region 遍历
            /// <summary>
            /// 先序遍历此节点下的分支
            /// </summary>
            /// <param name="action"></param>
            public void Preorder(Action<Node> action)
            {
                action(this);
                foreach (Node children in ChildrenNodes)
                {
                    children.Preorder(action);
                }
            }

            /// <summary>
            /// 遍历此节点下的所有叶子
            /// </summary>
            /// <param name="action"></param>
            public void ErgodicLeaf(Action<Node> action)
            {
                if (IsLeaf)
                {
                    action(this);
                }
                else
                {
                    foreach (Node children in ChildrenNodes)
                    {
                        children.ErgodicLeaf(action);
                    }
                }
            }
            /// <summary>
            /// 遍历子节点 (不会遍历孙节点)
            /// </summary>
            /// <param name="action"></param>
            public void ErgodicChildren(Action<Node> action)
            {
                foreach (Node children in ChildrenNodes)
                {
                    action(children);
                }
            }
            /// <summary>
            /// 遍历子节点中的叶子节点 (不会遍历孙节点)
            /// </summary>
            /// <param name="action"></param>
            public void ErodicChildrenLeaf(Action<Node> action)
            {
                foreach (Node children in ChildrenNodes)
                {
                    if (children.IsLeaf)
                    {
                        action(children);
                    }
                }
            }
            #endregion
        }
        #endregion

        #region 数据
        /// <summary>
        /// 数的配置
        /// </summary>
        public ConfigCollection Config 
        {
            get => config ?? throw new Exception("未对树做配置");
            internal set => config = value;
        }
        private ConfigCollection? config;

        /// <summary>
        /// 根节点
        /// </summary>
        public Node Root { get; private set; } = new Node()
        {
            LayerId = 0,
            IsRoot = true,
        };


        #endregion

        #region 操作

        #region 查询
        public enum FindMode
        {
            /// <summary>
            /// 查找终结点
            /// </summary>
            FindEndPoints,
            /// <summary>
            /// 仅查找一个终结点
            /// </summary>
            FindOneEndPoint,
            /// <summary>
            /// 查找叶子
            /// </summary>
            FindLeaf,
        }
        public class FindHelper
        {
            #region 查找状态
            /// <summary>
            /// 查找结束
            /// </summary>
            public bool FoundDone { get; private set; }

            public Node? ResultNode { get; private set; }

            public Node[]? ResultNodes { get; private set; }

            private void _emptyResult()
            {
                FoundDone = true;
                ResultNode = null;
                ResultNodes = Array.Empty<Node>();
            }
            private void _result(IEnumerable<Node> nodes)
            {
                FoundDone = true;
                ResultNodes = nodes.ToArray();
                ResultNode = ResultNodes.FirstOrDefault();
            }
            #endregion

            public FindHelper(Node current, ConfigCollection configs)
            {
                CurrentLayerId = current.LayerId;
                CurrentNodes = new List<Node>() { current };
                Configs = configs;
            }

            public ConfigCollection Configs { get; set; }
            public FindMode Mode { get; set; }
            public Node? Parent { get; set; }
            public int CurrentLayerId { get; private set; }
            public List<Node> CurrentNodes { get; set; }


            #region 查找单个节点
            /// <summary>
            /// 查找当前节点下与输入值匹配的非叶子节点
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="arg"></param>
            /// <returns></returns>
            public FindHelper Find<T>(T arg)
            {
                if (FoundDone) return this;

                int nextId = Configs.NextLayer(CurrentLayerId);

                if (nextId == Configs.LeafLayerId)
                {
                    _emptyResult();
                }

                List<Node> nextLayer = new List<Node>();
                ConfigItem config = Configs[nextId] ?? throw new Exception($"意外错误, 没有找到层 {nextId} 对应的配置");

                foreach (Node node in CurrentNodes)
                {
                    if (node.IsLeaf)
                    {
                        continue;
                    }
                    if (node.IsEndPoint)
                    {
                        continue;
                    }
                    foreach (Node children in node.ChildrenNodes)
                    {
                        if (children.IsLeaf)
                        {
                            continue;
                        }
                        if (config.IsSameFunc!(children.NodeValue, arg))
                        {
                            nextLayer.Add(children);
                            if (Mode == FindMode.FindOneEndPoint)
                            {
                                goto QUERY_END;
                            }
                        }
                    }
                }
                QUERY_END:

                if (nextLayer.Count == 0)
                {
                    _emptyResult();
                    return this;
                }
                if (Mode == FindMode.FindEndPoints) 
                {
                    if (nextLayer.Any(i => i.IsEndPoint))
                    {
                        _result(nextLayer.Where(i => i.IsEndPoint));
                        return this;
                    }
                }

                CurrentNodes = nextLayer;

                return this;
            }

            /// <summary>
            /// 查找当前节点下与输入的任意值匹配的子节点, 按传入的顺序查找
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="arg"></param>
            /// <returns></returns>
            public FindHelper FindOr<T>(params T[] args)
            {
                if (FoundDone) return this;

                int nextId = Configs.NextLayer(CurrentLayerId);

                if (nextId == Configs.LeafLayerId)
                {
                    _emptyResult();
                }

                List<Node> nextLayer = new List<Node>();
                ConfigItem config = Configs[nextId] ?? throw new Exception($"意外错误, 没有找到层 {nextId} 对应的配置");

                foreach (Node node in CurrentNodes)
                {
                    if (node.IsLeaf)
                    {
                        continue;
                    }
                    if (node.IsEndPoint)
                    {
                        continue;
                    }
                    foreach (T arg in args)
                    {
                        foreach (Node children in node.ChildrenNodes)
                        {
                            if (children.IsLeaf)
                            {
                                continue;
                            }

                            if (config.IsSameFunc!(children.NodeValue, arg))
                            {
                                nextLayer.Add(children);
                                if (Mode == FindMode.FindOneEndPoint)
                                {
                                    goto QUERY_END;
                                }
                                break;
                            }
                        }
                    }
                }
                QUERY_END:

                if (nextLayer.Count == 0)
                {
                    _emptyResult();
                    return this;
                }
                if (Mode == FindMode.FindEndPoints)
                {
                    if (nextLayer.Any(i => i.IsEndPoint))
                    {
                        _result(nextLayer.Where(i => i.IsEndPoint));
                        return this;
                    }
                }

                CurrentNodes = nextLayer;

                return this;
            }

            /// <summary>
            /// 查找当前节点下, 节点值为null的子节点
            /// </summary>
            /// <returns></returns>
            public FindHelper FindNull()
            {
                if (FoundDone) return this;

                int nextId = Configs.NextLayer(CurrentLayerId);

                if (nextId == Configs.LeafLayerId)
                {
                    _emptyResult();
                }

                List<Node> nextLayer = new List<Node>();
                ConfigItem config = Configs[nextId] ?? throw new Exception($"意外错误, 没有找到层 {nextId} 对应的配置");

                foreach (Node node in CurrentNodes)
                {
                    if (node.IsLeaf)
                    {
                        continue;
                    }
                    if (node.IsEndPoint)
                    {
                        continue;
                    }
                    foreach (Node children in node.ChildrenNodes)
                    {
                        if (children.IsLeaf)
                        {
                            continue;
                        }
                        if (children.NodeValue == null)
                        {
                            nextLayer.Add(children);
                            if (Mode == FindMode.FindOneEndPoint)
                            {
                                goto QUERY_END;
                            }
                            break;
                        }
                    }
                }
                QUERY_END:

                if (nextLayer.Count == 0)
                {
                    _emptyResult();
                    return this;
                }
                if (Mode == FindMode.FindEndPoints)
                {
                    if (nextLayer.Any(i => i.IsEndPoint))
                    {
                        _result(nextLayer.Where(i => i.IsEndPoint));
                        return this;
                    }
                }

                CurrentNodes = nextLayer;

                return this;
            }

            #endregion

            #region 取得结果
            /// <summary>
            /// 将当前节点下所有叶子节点合并为查找结果
            /// </summary>
            /// <param name="depthFind">是否查找子节点下的节点</param>
            /// <returns></returns>
            public FindHelper AllLeaf(bool depthFind = true)
            {
                if (FoundDone) return this;

                List<Node> output = new List<Node>();

                if (depthFind)
                {
                    foreach (Node node in CurrentNodes)
                    {
                        node.ErgodicLeaf((n) => output.Add(n));
                    }
                }
                else
                {
                    foreach (Node node in CurrentNodes)
                    {
                        node.ErodicChildrenLeaf((n) => output.Add(n));
                    }
                }

                _result(output);

                return this;
            }

            /// <summary>
            /// 将当前节点下的所有节点, 包含叶子节点, 终结点等
            /// </summary>
            /// <returns></returns>
            public FindHelper AllNode()
            {
                if (FoundDone) return this;

                List<Node> output = new List<Node>();
                foreach (Node node in CurrentNodes)
                {
                    node.Preorder((n) => output.Add(n));
                }
                _result(output);

                return this;
            }
            #endregion
        }

        /// <summary>
        /// 使用输入方式从根节点开始查找
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public FindHelper StartFind(FindMode mode = FindMode.FindEndPoints)
        {
            return new FindHelper(Root, Config)
            {
                Parent = null,
                Mode = mode
            };
        }
        #endregion

        #region 插入 待实现

        #endregion

        #endregion

        #region 遍历
        /// <summary>
        /// 遍历树的委托
        /// </summary>
        /// <param name="layerId">所属终结点层编号</param>
        /// <param name="value">当前节点</param>
        /// <param name="index">当前节点在父节点下的遍历序号</param>
        public delegate void ErgodicDelegate(int layerId, TValue value, int index);

        /// <summary>
        /// 遍历叶子节点 (先序遍历)
        /// </summary>
        public void ErgodicLeaf(ErgodicDelegate action)
        {
            if (Root == null) return;
            void invokeNode(Node node, int indexInParent)
            {
                if (node.IsLeaf)
                {
                    if (node.Value != null)
                    {
                        if (!node.IsRoot)
                        {
                            action.Invoke(node.Parent!.LayerId, node.Value, indexInParent);
                        }
                    }
                }
                else
                {
                    int i = 0;
                    foreach (Node child in node.ChildrenNodes)
                    {
                        invokeNode(child, i);
                        i++;
                    }
                }
            }
            invokeNode(Root, 0);
        }
        #endregion

        #region 信息

        /// <summary>
        /// 取得简易的该树的字符串信息
        /// </summary>
        /// <param name="getNodeValueStringFunc">取得节点(分叉)值字符串的方法, 参数1: 节点深度; 参数2: 节点值;</param>
        /// <param name="getLeafStringFunc">取得叶子值字符串的方法</param>
        /// <param name="leftStr">字符串的左标符号</param>
        /// <param name="rightStr">字符串的右标符号</param>
        /// <param name="splitHoz">水平链接不同项的符号</param>
        /// <param name="splitVer">垂直链接不同项的符号</param>
        /// <param name="splitStartCross">水平与垂直链接符号交错的首次交错的符号</param>
        /// <param name="splitCross">水平与垂直链接符号交错的符号</param>
        /// <param name="splitEndCross">水平与垂直链接符号交错的最后一次符号</param>
        /// <param name="splitSingleCross">水平与垂直链接符号交错的指向单一子项的符号</param>
        /// <param name="nullValueStr">null值的表示字符串</param>
        /// <returns></returns>
        public string GetSimpleTreeString(
            Func<int, object?, string>? getNodeValueStringFunc = null,
            Func<TValue, string>? getLeafStringFunc = null,
            string leftStr = "[", string rightStr = "]",
            string splitHoz = "-", string splitVer = "|",
            string splitStartCross = "┬", string splitCross = "├", string splitEndCross = "└", string splitSingleCross = "-",
            string rootStr = "Root",
            string nullValueStr = "<null>")
        {
            // 层的最长非叶子字符串长度 (非层ID, 而是层深度)
            Dictionary<int, int> layerStringMaxLength = new Dictionary<int, int>();
            // 末端数量
            int leafCount = 0;

            TreeNodeInfoNode tempTreeRoot = new TreeNodeInfoNode()
            {
                Text = rootStr,
            };
            layerStringMaxLength.Add(0, tempTreeRoot.Text.Length);
            void tidyBaseData(TreeNodeInfoNode tempTreeParent, Node node, int depth/*节点所属层深度*/)
            {
                foreach (Node chilren in node.ChildrenNodes)
                {
                    if (chilren.IsLeaf)
                    {
                        leafCount++;
                        TreeNodeInfoNode nodeInfo = new TreeNodeInfoNode()
                        {
                            Text = chilren.Value != null ?
                                (
                                    getLeafStringFunc == null ?
                                        chilren.Value.ToString() ?? string.Empty
                                        :
                                        getLeafStringFunc.Invoke(chilren.Value)
                                )
                                :
                                nullValueStr,

                            ChildrenCount = 0,
                            ChildrenLeafCount = 0,
                            IsLeaf = true,
                            LayerIndex = depth + 1,

                            Parent = tempTreeParent,
                        };
                        tempTreeParent.Childrens.Add(nodeInfo);
                    }
                    else
                    {
                        TreeNodeInfoNode nodeInfo = new TreeNodeInfoNode()
                        {
                            Text = chilren.NodeValue != null ?
                                (
                                    getNodeValueStringFunc == null ?
                                        chilren.NodeValue.ToString() ?? string.Empty
                                        :
                                        getNodeValueStringFunc.Invoke(depth + 1, chilren.NodeValue)
                                )
                                :
                                nullValueStr,

                            Parent = tempTreeParent,
                        };
                        tidyBaseData(nodeInfo, chilren, depth + 1);


                        int textLength = nodeInfo.Text == null ? 0 : nodeInfo.Text.Length;
                        if (layerStringMaxLength.ContainsKey(nodeInfo.LayerIndex))
                        {
                            if (layerStringMaxLength[nodeInfo.LayerIndex] < textLength)
                            {
                                layerStringMaxLength[nodeInfo.LayerIndex] = textLength;
                            }
                        }
                        else
                        {
                            layerStringMaxLength.Add(nodeInfo.LayerIndex, textLength);
                        }

                        tempTreeParent.Childrens.Add(nodeInfo);
                    }
                }
                tempTreeParent.ChildrenCount = tempTreeParent.Childrens.Count;
                tempTreeParent.ChildrenLeafCount = tempTreeParent.Childrens.Count(i => i.IsLeaf) + tempTreeParent.Childrens.Sum(i => i.ChildrenLeafCount);
                tempTreeParent.IsLeaf = false;
                tempTreeParent.LayerIndex = depth;
            }
            tidyBaseData(tempTreeRoot, Root, 0);

            // 各行的字符串构建器
            List<StringBuilder> builders = new List<StringBuilder>(leafCount);
            for (int i = 0; i < leafCount; i++)
            {
                builders.Add(new StringBuilder());
            }

            tempTreeRoot.Preorder((node, index) =>
            {
                // 计算节点对应的行号
                int rowIndex = node.Parent == null ? 0 : node.Parent.RowIndex;
                for (int i = 0; i < index; i++)
                {
                    rowIndex +=
                        node.Parent!.Childrens[i].IsLeaf ?
                        1 : node.Parent.Childrens[i].ChildrenLeafCount;
                }
                node.RowIndex = rowIndex;   // 提供给子节点用

                // 当前行
                if (builders[rowIndex].Length >= 0)
                {
                    builders[rowIndex].Append(splitHoz);
                }
                // 写主要内容
                builders[rowIndex].Append(leftStr);
                if (node.IsLeaf)
                {
                    builders[rowIndex].Append(node.Text);   // 叶子节点无需填空格
                }
                else
                {
                    builders[rowIndex].Append(
                        Common_Util.String.StringHelper.FillBlankSpace(
                            node.Text, 
                            layerStringMaxLength[node.LayerIndex]));
                }
                builders[rowIndex].Append(rightStr);
                // 写其他内容
                if (!node.IsLeaf)
                {
                    // 填充空位
                    for (int i = 0; i < node.ChildrenLeafCount; i++)
                    {
                        if (i == 0)
                        {
                            builders[rowIndex + i].Append(splitHoz);
                        }
                        else
                        {
                            // 除第一个子节点外的其他行需要填充相应数量的空格以对齐
                            builders[rowIndex + i].Append(
                                Common_Util.String.StringHelper.GetBlankSpaceString(
                                    splitHoz.Length
                                    + leftStr.Length
                                    + layerStringMaxLength[node.LayerIndex]
                                    + rightStr.Length
                                    + splitHoz.Length));
                        }
                    }
                    // 分支符号
                    int offset = 0;
                    for (int n = 0; n < node.Childrens.Count; n++)
                    {
                        TreeNodeInfoNode childrenNode = node.Childrens[n];

                        if (node.Childrens.Count == 1)  // 只有一个子节点
                        {
                            builders[rowIndex + offset].Append(splitSingleCross);
                        }
                        else if (n == 0)    // 多子节点的起始符号
                        {
                            builders[rowIndex + offset].Append(splitStartCross);
                        }
                        else if (n == node.Childrens.Count - 1) // 多子节点的终止符号
                        {
                            builders[rowIndex + offset].Append(splitEndCross);
                        }
                        else // 中间节点的分支符号
                        {
                            builders[rowIndex + offset].Append(splitCross);
                        }
                        // 如果有多个节点, 需要在两个子分支之间填上垂直方向的连接符号
                        if (node.ChildrenLeafCount > 1)
                        {
                            for (int i = 1; i < childrenNode.ChildrenLeafCount; i++)
                            {
                                if (n == node.Childrens.Count - 1)  // 如果已经处理到最后一个节点, 不再填入连接符号, 而是填入空格
                                {
                                    builders[rowIndex + offset + i].Append(Common_Util.String.StringHelper.GetBlankSpaceString(splitVer));
                                }
                                else
                                {
                                    builders[rowIndex + offset + i].Append(splitVer);
                                }
                            }
                        }

                        offset += childrenNode.IsLeaf ? 1 : childrenNode.ChildrenLeafCount;
                    }
                }

            }, 0);


            // 拼接所有行
            StringBuilder output = new StringBuilder();
            for (int i = 0; i < builders.Count; i++)
            {
                output.Append(builders[i]);
                if (i < builders.Count - 1)
                {
                    output.AppendLine();
                }
            }
            return output.ToString();
        }


        /// <summary>
        /// 树节点信息后的构造体
        /// </summary>
        private class TreeNodeInfoNode
        {
            public TreeNodeInfoNode? Parent { get; set; }
            public List<TreeNodeInfoNode> Childrens { get; private set; } = new List<TreeNodeInfoNode>();

            public string Text { get; set; } = string.Empty;
            /// <summary>
            /// 子节点数量
            /// </summary>
            public int ChildrenCount { get; set; }
            /// <summary>
            /// 子终结点数量
            /// </summary>
            public int ChildrenLeafCount { get; set; }
            /// <summary>
            /// 是否叶子
            /// </summary>
            public bool IsLeaf { get; set; }
            public int RowIndex { get; set; }
            public int LayerIndex { get; set; }

            /// <summary>
            /// 先序遍历
            /// </summary>
            /// <param name="action"></param>
            public void Preorder(Action<TreeNodeInfoNode, int> action, int index)
            {
                action.Invoke(this, index);
                int i = 0;
                foreach (TreeNodeInfoNode children in Childrens)
                {
                    children.Preorder(action, i);
                    i++;
                }
            }
        }

        #endregion
    }
}
