using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common_Winform.Controls.Menus
{
    public class TopMenuItem
    {
        public ulong Index { get; set; }

        public Action? OnClick { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;


        public string ShowName { get; set; } = string.Empty;

        public bool IsButton { get => OnClick != null; }

        public Form? RelateForm { get; set; }

        public ToolStripItem? RelateToolStripItem { get; set; }

        public PropertyInfo? RelateProperty { get; set; }

        public MethodInfo? RelateMethod { get; set; }

        /// <summary>
        /// 实际所属层
        /// </summary>
        public int LayerIndex { get; set; }
        /// <summary>
        /// 分析所得的层级数值的字典
        /// </summary>
        public Dictionary<int, ulong> LayerValueDic { get; private set; } = new Dictionary<int, ulong>();
        /// <summary>
        /// 以精简的方式设置层级数值
        /// </summary>
        /// <param name="layerData"></param>
        public void SimplifySetLayerData(Dictionary<int, ulong> layerData)
        {
            LayerValueDic.Clear();
            var layerIndices = layerData.Where(i => i.Value > 0).OrderBy(i => i.Key);
            LayerIndex = layerIndices.Max(i => i.Key);
            int temp = 0;
            foreach (var kvPair in layerIndices)
            {
                if (kvPair.Key - temp > 1)
                {
                    break;
                }
                LayerValueDic.Add(kvPair.Key, kvPair.Value);
                temp++;
            }
        }


        /// <summary>
        /// 精简之后的索引, 即去掉多余信息后的索引值, 比如一个索引含有1层, 2层和4层的值, 4层的值是无意义的, 将被精简
        /// </summary>
        public ulong SimplifyIndex { get; set; }
    }
}
