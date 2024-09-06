using Common_Util.Enums;
using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Common_Util.Data.Structure.Tree
{
    /// <summary>
    /// 通用树结构 (不定叉数的多叉树)
    /// </summary>
    /// <typeparam name="Scope">作用域</typeparam>
    /// <typeparam name="Value"></typeparam>
    public class GeneralTree<Scope, Value> : IMultiTree<Value?>
    {
        #region 比较方法
        /// <summary>
        /// 比较两个作用域是否相等
        /// </summary>
        /// <param name="scope1"></param>
        /// <param name="scope2"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsEqual(Scope? scope1, Scope? scope2)
        {
            if (scope1 == null || scope2 == null) return true;
            else if (scope1 != null && scope2 != null && scope1.Equals(scope2)) return true;
            return false;
        }
        /// <summary>
        /// 获取输入的作用域列表构成的字符串, 从头到尾依次排列
        /// </summary>
        /// <param name="scopes"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string GetScopeListString(IEnumerable<Scope?> scopes)
        {
            return Common_Util.String.StringHelper.ConcatAndWrap(scopes.Select(i => i == null ? "<null>" : i.ToString()).ToList(), "->", "[", "]");
        }
        #endregion

        /// <summary>
        /// 根节点
        /// </summary>
        public GeneralTreeNode<Scope?, Value?>? Root { get; set; }

        IMultiTreeNode<Value?>? IMultiTree<Value?>.Root => Root;



        #region 基于层信息建树
        /// <summary>
        /// 基于层信息建树, 需传入原始对象对应的层信息, 其中包含层级(0为根级, 正数为依次往下推的层级)和对应的作用域
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="layerInfos"></param>
        /// <param name="convertFunc">转换方法: 节点对象 => 节点值</param>
        /// <param name="nodeAvailableFunc">节点可用判断函数</param>
        public void BuildTreeFromLayer<T>(
            IEnumerable<KeyValuePair<T, Dictionary<int, Scope>>> layerInfos,
            Func<T, Value?> convertFunc,
            Func<GeneralTreeNode<Scope?, Value?>, bool> nodeAvailableFunc)
        {
            Root = null;
            List<int> layers = layerInfos.SelectMany(i => i.Value.Keys).Distinct().OrderBy(i => i).ToList();
            // 输入层级信息检查
            if (layers == null || layers.Count == 0)
            {
                return;
            }
            if (layers.Any(i => i < 0))
            {
                throw new ArgumentException("传入层信息包含负数层级", nameof(layerInfos));
            }
            int firstLayer = layers[0];
            if (firstLayer > 1)
            {
                throw new ArgumentException("传入层信息包含没有从0或1开始", nameof(layerInfos));
            }
            int temp = firstLayer;
            for (int i = 1; i < layers.Count; i++)
            {
                if (layers[i] - temp > 1)
                {
                    throw new ArgumentException($"传入层信息的层级出现断层: {temp} => {layers[i]}", nameof(layerInfos));
                }
                temp = layers[i];
            }
            // 整理对应对象与对应的路径, 按层分字典
            Dictionary<int, List<(T obj, Dictionary<int, Scope?> path)>> collectLayers = new Dictionary<int, List<(T obj, Dictionary<int, Scope?> path)>>();

            T? rootObj = default;
            Scope? rootScope = default;
            foreach (var item in layerInfos)
            {
                T itemObj = item.Key;   // 关联对象
                Dictionary<int, Scope?> path = item.Value.ToDictionary(i => i.Key, i => (Scope?)i.Value);  // 找到这一个节点的路径
                if (path.Count == 0 || (path.Count == 1 && path.First().Key == 0))
                {
                    if (rootObj == null)
                    {
                        rootObj = itemObj;
                        if (path.Count != 0)
                        {
                            rootScope = path.First().Value;
                        }
                    }
                    else
                    {
                        throw new ArgumentException($"传入层信息出现了两个根节点", nameof(layerInfos));
                    }
                }
                int layer = path.Keys.Max();
                if (!collectLayers.ContainsKey(layer))
                {
                    collectLayers.Add(layer, new List<(T obj, Dictionary<int, Scope?> path)>());
                }
                collectLayers[layer].Add((obj: itemObj, path: path));
            }

            // 根节点
            Value? rootValue = default;
            if (rootObj != null)
            {
                rootValue = convertFunc(rootObj);
            }
            var rootNode = new GeneralTreeNode<Scope?, Value?>(rootScope, rootValue);
            if (!nodeAvailableFunc.Invoke(rootNode))
            {
                return; // throw new Exception("根节点可用判断未通过! ");
            }
            Root = rootNode;

            // 剩余节点: 由浅到深创建节点
            int maxLayer = layers.Max();
            for (int i = 1; i <= maxLayer; i++)
            {
                if (!collectLayers.ContainsKey(i))
                {
                    continue;
                }

                // 遍历这一层中的对象
                foreach (var layerItem in collectLayers[i])
                {
                    var path = layerItem.path;
                    GeneralTreeNode<Scope?, Value?>? parent = Root;
                    int prevIndex = 0;

                    // 遍历对象对应的路径
                    foreach (int layerIndex in path.Keys.OrderBy(i => i))
                    {
                        if (layerIndex == 0)
                        {
                            continue;
                        }
                        Scope? scope = path[layerIndex];
                        if (layerIndex - prevIndex != 1)
                        {
                            throw new Exception("整理出来的节点路径出现间断: " + GetScopeListString(path.OrderBy(i => i.Key).Select(i => i.Value)));
                        }
                        var current = parent.Childrens.FirstOrDefault(i => IsEqual(i.NodeScope, scope));
                        if (current == null)
                        {
                            Value? value = default;
                            if (layerIndex == i)
                            {
                                // 路径当前位置已经是对象的位置了
                                value = convertFunc(layerItem.obj);
                            }
                            current = new GeneralTreeNode<Scope?, Value?>(scope, value);
                            if (!nodeAvailableFunc.Invoke(rootNode))
                            {
                                break; // throw new Exception("节点可用判断未通过! " + GetScopeListString(path.OrderBy(i => i.Key).Select(i => i.Value)));
                            }
                            parent.AddChildren(current);
                        }
                        parent = current;
                        prevIndex = layerIndex;
                    }
                }
            }

        }
        #endregion

        #region 基于获取子项的方法建树
        /// <summary>
        /// 建树
        /// </summary>
        /// <typeparam name="T">将要用于构建节点的对象</typeparam>
        /// <param name="rootObj">用于构建树的根节点的对象</param>
        /// <param name="childrenGetFunc">取得子对象的方法</param>
        /// <param name="convertFunc">将对象转换为节点的方法</param>
        /// <param name="nodeAvailableFunc">节点可用判断函数</param>
        public void BuildTree<T>(
            T rootObj,
            Func<T, List<T>> childrenGetFunc,
            Func<T, KeyValuePair<Scope?, Value?>> convertFunc,
            Func<GeneralTreeNode<Scope?, Value?>, bool> nodeAvailableFunc)
        {
            _buildTree(rootObj, childrenGetFunc(rootObj), childrenGetFunc, convertFunc, nodeAvailableFunc);
        }
        /// <summary>
        /// 建树
        /// </summary>
        /// <typeparam name="T">将要用于构建节点的对象</typeparam>
        /// <param name="firstLayer">输入的第一层对象</param>
        /// <param name="childrenGetFunc">取得子对象的方法</param>
        /// <param name="convertFunc">将对象转换为节点的方法</param>
        /// <param name="nodeAvailableFunc">节点可用判断函数</param>
        public void BuildTree<T>(
            List<T> firstLayer,
            Func<T, List<T>> childrenGetFunc,
            Func<T, KeyValuePair<Scope?, Value?>> convertFunc,
            Func<GeneralTreeNode<Scope?, Value?>, bool> nodeAvailableFunc)
        {
            _buildTree(default, firstLayer, childrenGetFunc, convertFunc, nodeAvailableFunc);
        }
        /// <summary>
        /// 建树
        /// </summary>
        /// <typeparam name="T">将要用于构建节点的对象</typeparam>
        /// <param name="rootObj">用于构建树的根节点的对象</param>
        /// <param name="firstLayer">输入的第一层对象</param>
        /// <param name="childrenGetFunc">取得子对象的方法</param>
        /// <param name="convertFunc">将对象转换为节点的方法</param>
        /// <param name="nodeAvailableFunc">节点可用判断函数</param>
        private void _buildTree<T>(
            T? rootObj,
            List<T> firstLayer, 
            Func<T, List<T>> childrenGetFunc,
            Func<T, KeyValuePair<Scope?, Value?>> convertFunc,
            Func<GeneralTreeNode<Scope?, Value?>, bool> nodeAvailableFunc)
        {
            Root = null;

            Dictionary<int, List<KeyValuePair<T?, GeneralTreeNode<Scope?, Value?>>>> layers
                = new Dictionary<int, List<KeyValuePair<T?, GeneralTreeNode<Scope?, Value?>>>>();
            // 根
            GeneralTreeNode<Scope?, Value?> root = rootObj == null ? new GeneralTreeNode<Scope?, Value?>() : new(convertFunc(rootObj));
            layers.Add(0, new List<KeyValuePair<T?, GeneralTreeNode<Scope?, Value?>>>()
                            { 
                                new KeyValuePair<T?, GeneralTreeNode<Scope?, Value?>>(rootObj, root)
                            });
            // 第一层
            List<KeyValuePair<T?, GeneralTreeNode<Scope?, Value?>>> _firstLayer 
                = new List<KeyValuePair<T?, GeneralTreeNode<Scope?, Value?>>>();
            layers.Add(1, _firstLayer);
            foreach (T t in firstLayer)
            {
                // 转换为节点对象
                GeneralTreeNode<Scope?, Value?> node
                    = new GeneralTreeNode<Scope?, Value?>(root, convertFunc(t));
                // 检查节点可不可用
                if (!nodeAvailableFunc(node))
                {
                    continue;
                }
                // 设置为根的子节点
                if (!root.AddChildren(node))
                {
                    continue;
                }
                // 将节点存入层集合中
                _firstLayer.Add(new KeyValuePair<T?, GeneralTreeNode<Scope?, Value?>>(t, node));
            }
            // 剩下的层
            int layerNow = 1;
            while (true)
            {
                // 取得当前层
                List<KeyValuePair<T?, GeneralTreeNode<Scope?, Value?>>> layer = layers[layerNow];
                List<KeyValuePair<T?, GeneralTreeNode<Scope?, Value?>>> nextLayer
                    = new List<KeyValuePair<T?, GeneralTreeNode<Scope?, Value?>>>();
                layers.Add(layerNow + 1, nextLayer);

                if (layer.Count == 0)
                {
                    break;
                }
                // 分析并写入子节点
                foreach (KeyValuePair<T?, GeneralTreeNode<Scope?, Value?>> pair in layer)
                {// 遍历当前层
                    if (pair.Key == null)
                    {
                        continue;
                    }
                    // 获取当前项的子项
                    List<T> ts = childrenGetFunc(pair.Key);
                    foreach (T t in ts)
                    {// 遍历子项
                        // 将子项转换为节点对象
                        GeneralTreeNode<Scope?, Value?> node
                            = new GeneralTreeNode<Scope?, Value?>(pair.Value, convertFunc(t));
                        // 检查节点可不可用
                        if (nodeAvailableFunc(node) && pair.Value.AddChildren(node))
                        {// 可用
                            // 放入下一层
                            nextLayer.Add(new KeyValuePair<T?, GeneralTreeNode<Scope?, Value?>>(t, node));
                        }
                    }
                }
                layerNow++;
            }
            Root = root;
        }


        #endregion

        #region 控制
        /// <summary>
        /// 使用输入的比较方法, 对各个节点的子节点排序
        /// </summary>
        /// <param name="compareFunc"></param>
        /// <param name="asc">是否正序, 即比较结果较大的子节点排在后面</param>
        public void SortChildrenUse(Func<GeneralTreeNode<Scope?, Value?>, GeneralTreeNode<Scope?, Value?>, CompareResultEnum> compareFunc, bool asc = true)
        {
            Preorder((node, index) =>
            {
                if (node.Childrens.Count > 0)
                {
                    node.Childrens.BubbleSort(
                        (node1, node2) =>
                        {
                            return compareFunc(node1, node2) == CompareResultEnum.Bigger;
                        },
                        asc: asc);
                }
            });
        }
        #endregion

        /// <summary>
        /// 取得节点层次数据
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, GeneralTreeLayer<Scope?, Value?>> GetNodeLayer()
        {
            Dictionary<int, GeneralTreeLayer<Scope?, Value?>> output
                = new Dictionary<int, GeneralTreeLayer<Scope?, Value?>>
                {
                    // 根
                    { 0, new GeneralTreeLayer<Scope?, Value?>(0, Root) }
                };
            // 剩余层
            int layerNow = 0;
            while (output[layerNow].NodeCount > 0)
            {
                List<GeneralTreeNode<Scope?, Value?>> nextLayerNodes 
                    = new List<GeneralTreeNode<Scope?, Value?>>();
                foreach (GeneralTreeNode<Scope?, Value?> node in output[layerNow].Nodes)
                {
                    if (node.Childrens != null)
                    {
                        nextLayerNodes.AddRange(node.Childrens);
                    }
                }
                output.Add(layerNow + 1, new GeneralTreeLayer<Scope?, Value?>()
                {
                    Layer = layerNow + 1,
                    Nodes = nextLayerNodes
                });
            }

            
            return output;
        }

        /// <summary>
        /// 取得简易的该树的字符串信息
        /// </summary>
        /// <param name="getValueStringFunc">取得值对应字符串的方法</param>
        /// <param name="leftStr">字符串的左标符号</param>
        /// <param name="rightStr">字符串的右标符号</param>
        /// <param name="splitHoz">水平链接不同项的符号</param>
        /// <param name="splitVer">垂直链接不同项的符号</param>
        /// <param name="splitStartCross">水平与垂直链接符号交错的首次交错的符号</param>
        /// <param name="splitCross">水平与垂直链接符号交错的符号</param>
        /// <param name="splitEndCross">水平与垂直链接符号交错的最后一次符号</param>
        /// <param name="splitSingleCross">水平与垂直链接符号交错的指向单一子项的符号</param>
        /// <param name="showScopeWhenNullValue">如果作用域是null值, 是否显示</param>
        /// <param name="nullValueStr">null值的表示字符串</param>
        /// <returns></returns>
        public string GetSimpleTreeString(
            Func<Value, string>? getValueStringFunc = null,
            Func<Scope, string>? getScopeStringFunc = null,
            string leftStr = "[", string rightStr = "]", 
            string splitHoz = "-", string splitVer = "|", 
            string splitStartCross = "┬", string splitCross = "├", string splitEndCross = "└", string splitSingleCross = "-",
            bool showScopeWhenNullValue = false, string nullValueStr = "<null>")
        {
            // 层的最长字符串长度
            Dictionary<int, int> layerStringMaxLength = new Dictionary<int, int>();
            // 末端数量
            int endPointCount = 0;
            // 构建临时的信息树
            GeneralTree<Scope?, TreeNodeInfoStruct> tempTree
                = Convert<Scope, Value, Scope, TreeNodeInfoStruct>(this, (input, childrens) =>
            {
                TreeNodeInfoStruct info = new TreeNodeInfoStruct()
                {
                    Text
                    = input.NodeValue == null ?
                        (
                            showScopeWhenNullValue && input.NodeScope != null ?
                                (
                                    getScopeStringFunc == null ?
                                        input.NodeScope.ToString() ?? string.Empty
                                        :
                                        getScopeStringFunc.Invoke(input.NodeScope)
                                )
                                :
                                nullValueStr
                        )
                        :
                        (
                            getValueStringFunc == null ?
                                input.NodeValue.ToString() ?? string.Empty 
                                :
                                getValueStringFunc.Invoke(input.NodeValue)
                        ),

                    ChildrenCount
                    = childrens.Count + childrens.Sum(children => children.ChildrenCount),
                    
                    ChildrenEndPointCount
                    = input.Childrens.Count(children => children.Childrens.Count == 0) + 
                        childrens.Sum(children => children.ChildrenEndPointCount),
                    
                    IsEndPoint
                    = input.Childrens.Count == 0,
                };
                if (info.IsEndPoint)
                {
                    endPointCount++;
                }
                info.LayerIndex = input.GetRootChain().Count;
                int textLength = info.Text == null ? 0 : info.Text.Length;
                if (layerStringMaxLength.ContainsKey(info.LayerIndex))
                {
                    if (layerStringMaxLength[info.LayerIndex] < textLength)
                    {
                        layerStringMaxLength[info.LayerIndex] = textLength;
                    }
                }
                else
                {
                    layerStringMaxLength.Add(info.LayerIndex, textLength);
                }
                return new KeyValuePair<Scope?, TreeNodeInfoStruct>(input.NodeScope, info);
            });

            // 各行的字符串构建器
            List<StringBuilder> builders = new List<StringBuilder>(endPointCount);
            for (int i = 0; i < endPointCount; i++)
            {
                builders.Add(new StringBuilder());
            }

            tempTree.Preorder((node, index) =>
            {
                int rowIndex = node.Parent == null ? 0 : node.Parent.NodeValue.RowIndex;
                for (int i = 0; i < index; i++)
                {
                    rowIndex += 
                        node.Parent!.Childrens[i].NodeValue.IsEndPoint ? 
                        1 : node.Parent.Childrens[i].NodeValue.ChildrenEndPointCount;
                }
                node.NodeValue = new TreeNodeInfoStruct()
                {
                    Text = node.NodeValue.Text,
                    ChildrenCount = node.NodeValue.ChildrenCount,
                    RowIndex = rowIndex,
                    ChildrenEndPointCount = node.NodeValue.ChildrenEndPointCount,
                    IsEndPoint = node.NodeValue.IsEndPoint,
                    LayerIndex = node.NodeValue.LayerIndex,
                };

                // 当前行
                if (builders[rowIndex].Length >= 0)
                {
                    builders[rowIndex].Append(splitHoz);
                }
                builders[rowIndex]
                    .Append(leftStr)
                    .Append(Common_Util.String.StringHelper.FillBlankSpace(node.NodeValue.Text, layerStringMaxLength[node.NodeValue.LayerIndex]))
                    .Append(rightStr);
                // 其余行
                if (!node.NodeValue.IsEndPoint)
                {
                    for (int i = 0; i < node.NodeValue.ChildrenEndPointCount; i++)
                    {
                        if (i == 0)
                        {
                            builders[rowIndex + i].Append(splitHoz);
                        }
                        else
                        {
                            builders[rowIndex + i].Append(
                                Common_Util.String.StringHelper.GetBlankSpaceString(
                                    splitHoz.Length
                                    + leftStr.Length
                                    + layerStringMaxLength[node.NodeValue.LayerIndex]
                                    + rightStr.Length
                                    + splitHoz.Length));
                        }
                    }
                    int offset = 0;
                    for (int n = 0; n < node.Childrens.Count; n++)
                    {
                        GeneralTreeNode<Scope?, TreeNodeInfoStruct> childrenNode = node.Childrens[n];

                        if (node.Childrens.Count == 1)
                        {
                            builders[rowIndex + offset].Append(splitSingleCross);
                        }
                        else if (n == 0)
                        {
                            builders[rowIndex + offset].Append(splitStartCross);
                        }
                        else if (n == node.Childrens.Count - 1)
                        {
                            builders[rowIndex + offset].Append(splitEndCross);
                        }
                        else
                        {
                            builders[rowIndex + offset].Append(splitCross);
                        }
                        if (!node.NodeValue.IsEndPoint && node.NodeValue.ChildrenEndPointCount > 1)
                        {
                            for (int i = 1; i < childrenNode.NodeValue.ChildrenEndPointCount; i++)
                            {
                                if (n == node.Childrens.Count - 1)
                                {
                                    builders[rowIndex + offset + i].Append(Common_Util.String.StringHelper.GetBlankSpaceString(splitVer));
                                }
                                else
                                {
                                    builders[rowIndex + offset + i].Append(splitVer);
                                }
                            }
                        }

                        offset += childrenNode.NodeValue.IsEndPoint ? 1 : childrenNode.NodeValue.ChildrenEndPointCount;
                    }
                }


                // 测试用代码
                /*StringBuilder test = new StringBuilder();
                for (int i = 0; i < builders.Count; i++)
                {
                    test.AppendLine(builders[i].ToString());
                }
                Debug.WriteLine(test);*/
            });


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
        private struct TreeNodeInfoStruct
        {
            public string Text { get; set; }
            /// <summary>
            /// 子节点数量
            /// </summary>
            public int ChildrenCount { get; set; }
            /// <summary>
            /// 子终结点数量
            /// </summary>
            public int ChildrenEndPointCount { get; set; }
            /// <summary>
            /// 是否终结点
            /// </summary>
            public bool IsEndPoint { get; set; }
            public int RowIndex { get; set; }
            public int LayerIndex { get; set; }
        }

        #region 遍历
        /// <summary>
        /// 先序遍历
        /// </summary>
        /// <param name="action">输入参数: 1. 当前节点; 2. 当前节点在父节点下的遍历序号</param>
        public void Preorder(Action<GeneralTreeNode<Scope?, Value?>, int> action)
        {
            if (Root == null) return;
            void invokeNode(GeneralTreeNode<Scope?, Value?> node, int index)
            {
                action.Invoke(node, index);
                int i = 0;
                foreach (GeneralTreeNode<Scope?, Value?> children in node.Childrens)
                {
                    invokeNode(children, i);
                    i++;
                }
            }
            invokeNode(Root, 0);
        }
        /// <summary>
        /// 后序遍历
        /// </summary>
        /// <param name="action">输入参数: 1. 当前节点; 2. 当前节点在父节点下的遍历序号</param>
        public void Postorder(Action<GeneralTreeNode<Scope?, Value?>, int> action)
        {
            if (Root == null) return;
            void invokeNode(GeneralTreeNode<Scope?, Value?> node, int index)
            {
                int i = 0;
                foreach (GeneralTreeNode<Scope?, Value?> children in node.Childrens)
                {
                    invokeNode(children, i);
                    i++;
                }
                action.Invoke(node, index);
            }
            invokeNode(Root, 0);
        }
        #endregion



        #region 静态方法
        /// <summary>
        /// 将树的节点转换为另外的类型, 保持树的结构, 先序遍历
        /// </summary>
        /// <typeparam name="InScope"></typeparam>
        /// <typeparam name="InValue"></typeparam>
        /// <typeparam name="OutScope"></typeparam>
        /// <typeparam name="OutValue"></typeparam>
        /// <param name="nodeConvertFunc">输入参数: 1. 源数据</param>
        /// <returns></returns>
        public static GeneralTree<OutScope?, OutValue?> Convert<InScope, InValue, OutScope, OutValue>(
            GeneralTree<InScope?, InValue?> source, 
            Func<GeneralTreeNode<InScope?, InValue?>, KeyValuePair<OutScope?, OutValue?>> nodeConvertFunc)
        {
            if (source.Root == null)
            {
                return new GeneralTree<OutScope?, OutValue?>() { Root = null };
            }
            GeneralTreeNode<OutScope?, OutValue?> convertNode(GeneralTreeNode<InScope?, InValue?> inputNode)
            {
                GeneralTreeNode<OutScope?, OutValue?> outputNode
                    = new GeneralTreeNode<OutScope?, OutValue?>(
                        nodeConvertFunc(inputNode));
                foreach (GeneralTreeNode<InScope?, InValue?> node in inputNode.Childrens)
                {
                    outputNode.AddChildren(convertNode(node));
                }
                return outputNode;
            }
            GeneralTree<OutScope?, OutValue?> output = new GeneralTree<OutScope?, OutValue?>
            {
                Root = convertNode(source.Root)
            };

            return output;
        }
        /// <summary>
        /// 将树的节点转换为另外的类型, 保持树的结构, 后序遍历
        /// </summary>
        /// <typeparam name="InScope"></typeparam>
        /// <typeparam name="InValue"></typeparam>
        /// <typeparam name="OutScope"></typeparam>
        /// <typeparam name="OutValue"></typeparam>
        /// <param name="nodeConvertFunc">输入参数: 1. 源节点; 2. 子数据</param>
        /// <returns></returns>
        public static GeneralTree<OutScope?, OutValue?> Convert<InScope, InValue, OutScope, OutValue>(
            GeneralTree<InScope?, InValue?> source,
            Func<GeneralTreeNode<InScope?, InValue?>, List<OutValue?>, KeyValuePair<OutScope?, OutValue?>> nodeConvertFunc)
        {
            if (source.Root == null)
            {
                return new GeneralTree<OutScope?, OutValue?>() { Root = null };
            }
            GeneralTreeNode<OutScope?, OutValue?> convertNode(GeneralTreeNode<InScope?, InValue?> inputNode)
            {
                GeneralTreeNode<OutScope?, OutValue?> outputNode
                    = new GeneralTreeNode<OutScope?, OutValue?>();
                foreach (GeneralTreeNode<InScope?, InValue?> node in inputNode.Childrens)
                {
                    outputNode.AddChildren(convertNode(node));
                }
                KeyValuePair<OutScope?, OutValue?> pair
                    = nodeConvertFunc(
                        inputNode, 
                        outputNode.Childrens.Select(children => children.NodeValue).ToList());
                outputNode.NodeScope = pair.Key;
                outputNode.NodeValue = pair.Value;
                return outputNode;
            }
            GeneralTree<OutScope?, OutValue?> output = new GeneralTree<OutScope?, OutValue?>
            {
                Root = convertNode(source.Root)
            };

            return output;
        }
        #endregion
    }
    /// <summary>
    /// 通用树结构的节点
    /// </summary>
    /// <typeparam name="Scope"></typeparam>
    /// <typeparam name="Value"></typeparam>
    public class GeneralTreeNode<Scope, Value> : IMultiTreeNode<Value?>
    {
        /// <summary>
        /// 作用域实现 <see cref="Data.Property.IScope"/> 接口
        /// </summary>
        public readonly bool ScopeImplIScope;
        public GeneralTreeNode()
        {
            if (typeof(Data.Property.IScope).IsAssignableFrom(typeof(Scope)))
            {
                ScopeImplIScope = true;
            }
        }
        public GeneralTreeNode(
            Scope? scope, Value? value) : this()
        {
            NodeScope = scope;
            NodeValue = value;
        }
        public GeneralTreeNode(
            KeyValuePair<Scope?, Value?> pair) : this(pair.Key, pair.Value) { }

        public GeneralTreeNode(
            GeneralTreeNode<Scope?, Value?> parent,
            Scope? scope, Value? value) : this()
        {
            Parent = parent;
            NodeScope = scope;
            NodeValue = value;
        }
        public GeneralTreeNode(
            GeneralTreeNode<Scope?, Value?> parent,
            KeyValuePair<Scope?, Value?> pair) : this(parent, pair.Key, pair.Value) { }


        public GeneralTreeNode<Scope?, Value?>? this[Scope scope]
        {
            get
            {
                if (Childrens.Count == 0)
                {
                    return null;
                }
                return Childrens.FirstOrDefault(item => item.NodeScope == null ? scope == null : item.NodeScope.Equals(scope));
            }
        }

        /// <summary>
        /// 节点作用域
        /// </summary>
        public Scope? NodeScope { get; set; }
        /// <summary>
        /// 节点数据
        /// </summary>
        public Value? NodeValue { get; set; }
        /// <summary>
        /// 节点信息对
        /// </summary>
        public KeyValuePair<Scope?, Value?> Pair { get => new KeyValuePair<Scope?, Value?>(NodeScope, NodeValue); }
        
        /// <summary>
        /// 父节点
        /// </summary>
        public GeneralTreeNode<Scope?, Value?>? Parent { get; set; }
        /// <summary>
        /// 子节点
        /// </summary>
        public List<GeneralTreeNode<Scope?, Value?>> Childrens { get => _childrens; private set => _childrens = value; }
        private List<GeneralTreeNode<Scope?, Value?>> _childrens = [];

        IEnumerable<IMultiTreeNode<Value?>> IMultiTreeNode<Value?>.Childrens => _childrens;



        /// <summary>
        /// 检查输入的范围是否与子节点的范围重复
        /// </summary>
        /// <param name="scope"></param>
        /// <returns></returns>
        public bool CheckScope(Scope? scope)
        {
            if (Childrens.Count == 0)
            {
                return false;
            }
            else
            {
                bool result = false;
                foreach (Scope? s in Childrens.Select(c => c.NodeScope))
                {
                    if (s == null || scope == null)
                    {
                        if (s == null && scope == null)
                        {
                            result = true;
                            break;
                        }
                    }
                    else if (ScopeImplIScope)
                    {
                        if (((Property.IScope)s).CheckInterlace((Property.IScope)scope))
                        {
                            result = true;
                            break;
                        }
                    }
                    else
                    {
                        if (s.Equals(scope))
                        {
                            result = true;
                            break;
                        }
                    }
                }
                return result;
            }
        }


        /// <summary>
        /// 添加节点
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool AddChildren(GeneralTreeNode<Scope?, Value?> node)
        {
            // 检查范围是否重复
            if (CheckScope(node.NodeScope))
            {
                return false;
            }
            else
            {
                node.Parent = this;
                Childrens.Add(node);
                return true;
            }
        }
        /// <summary>
        /// 添加节点
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="value"></param>
        public bool AddChildren(Scope? scope, Value? value)
        {
            // 检查范围是否重复
            if (CheckScope(scope))
            {
                return false;
            }
            else
            {
                Childrens.Add(new GeneralTreeNode<Scope?, Value?>()
                {
                    Parent = this,
                    NodeScope = scope,
                    NodeValue = value,
                });
                return true;
            }
        }
        /// <summary>
        /// 取得根源链
        /// </summary>
        /// <returns></returns>
        public List<GeneralTreeNode<Scope?, Value?>> GetRootChain()
        {
            List<GeneralTreeNode<Scope?, Value?>> output = new List<GeneralTreeNode<Scope?, Value?>>();
            GeneralTreeNode<Scope?, Value?> temp = this;
            while(temp.Parent != null)
            {
                output.Add(temp.Parent);
                temp = temp.Parent;
            }
            // 反转
            output.Reverse();

            return output;
        }


        /// <summary>
        /// 取得建议的树节点字符串
        /// </summary>
        /// <returns></returns>
        public string GetSimpleTreeNodeString()
        {
            return $"[{NodeScope} : {NodeValue}]";
        }

    }

    /// <summary>
    /// 通用树的某层
    /// </summary>
    /// <typeparam name="Scope"></typeparam>
    /// <typeparam name="Value"></typeparam>
    public class GeneralTreeLayer<Scope, Value>
    {
        public GeneralTreeLayer()
        {

        }
        public GeneralTreeLayer(int layer, params GeneralTreeNode<Scope, Value>?[] nodes)
        {
            Layer = layer;
            Nodes = nodes.Where(i => i != null).Select(i => i!).ToList();
        }

        /// <summary>
        /// 层次数
        /// </summary>
        public int Layer { get; set; }

        /// <summary>
        /// 节点数
        /// </summary>
        public int NodeCount { get => Nodes == null ? 0 : Nodes.Count; }
        /// <summary>
        /// 当前层的节点集
        /// </summary>
        public List<GeneralTreeNode<Scope, Value>> Nodes { get; set; }
            = new List<GeneralTreeNode<Scope, Value>>();


    }


}
