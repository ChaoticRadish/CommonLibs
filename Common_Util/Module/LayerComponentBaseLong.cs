using System.Collections;
using System.Runtime.InteropServices;

namespace Common_Util.Module
{
    /// <summary>
    /// 基于ulong数值(16进制, 16位值)的层级组件
    /// <para>层编号: 层对应的编号</para>
    /// <para>层内编号: 层内某个具体的东西在某个层级上的编号</para>
    /// <para>ID值: 层内某个具体的东西对应的ulong数值</para>
    /// </summary>
    public class LayerComponentBaseLong
    {
        #region 配置结构体
        /// <summary>
        /// 规则配置, 或者说ID的解析方法
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Rule : IEnumerable<char>
        {
            /// <summary>
            /// 规则字符串的大小
            /// </summary>
            public const int SIZE = 16;
            /// <summary>
            /// 默认填充符号
            /// </summary>
            public const char DEFAULT_FILL_CHAR = '-';
            /// <summary>
            /// 允许的字符 (0~F, 以及填充字符 '.')
            /// </summary>
            public const string ALLOW_CHARS = "0123456789ABCDEF-";
            /// <summary>
            /// 层编号字符, 按顺序分层
            /// </summary>
            public const string LAYER_CHARS = "0123456789ABCDEF";
            #region 构造函数
            public Rule()
            {
                Clear();
            }
            /// <summary>
            /// 使用输入的规则字符串初始化规则, 如果输入的长度不够长, 将在左侧补到 <see cref="SIZE"/> 长度
            /// </summary>
            /// <param name="initRule"></param>
            public Rule(string initRule)
            {
                Clear();
                Write(initRule);
            }
            public Rule(params string[] initRules)
            {
                Clear();
                foreach (string rule in initRules)
                {
                    Write(rule);
                }
            }
            #endregion
            #region 控制
            /// <summary>
            /// 将所有数据清空为填充字符
            /// </summary>
            /// <param name="c"></param>
            public void Clear(char c = DEFAULT_FILL_CHAR)
            {
                if (!ALLOW_CHARS.Contains(c))
                {
                    c = DEFAULT_FILL_CHAR;
                }
                byte b = (byte)c;
                unsafe
                {
                    fixed (byte* ptr = Data)
                    {
                        for (int i = 0; i < SIZE; i++)
                        {
                            *(ptr + i) = b;
                        }
                    }
                }
            }
            /// <summary>
            /// 覆写规则到本规则中
            /// </summary>
            /// <param name="str"></param>
            public void Write(Rule other)
            {
                Write(other.DataString());
            }
            /// <summary>
            /// 写指定的字符串到规则中, 如果输入的长度不够长, 将在左侧补到 <see cref="SIZE"/> 长度
            /// </summary>
            /// <param name="str"></param>
            public void Write(string str)
            {
                if (!string.IsNullOrEmpty(str))
                {
                    str ??= "";
                    if (str.Length < SIZE)
                    {
                        str = str.PadLeft(SIZE, DEFAULT_FILL_CHAR);
                    }
                    unsafe
                    {
                        fixed (byte* ptr = Data)
                        {
                            for (int i = 0; i < SIZE && i < str.Length; i++)
                            {
                                char c = str[i];
                                if (c == DEFAULT_FILL_CHAR || !ALLOW_CHARS.Contains(c))
                                {
                                    continue;
                                }
                                byte b = (byte)c;

                                *(ptr + i) = b;
                            }
                        }
                    }
                }
            }

            public List<SubRule> AllSubRules()
            {
                List<SubRule> subRules = new List<SubRule>();
                for (int i = 0; i < LAYER_CHARS.Length; i++)
                {
                    byte b = (byte)LAYER_CHARS[i];
                    SubRule subRule = new SubRule()
                    {
                        Layer = i,
                    };
                    for (int n = 0; n < SIZE; n++)
                    {
                        if (Data[n] == b)
                        {
                            subRule[n] = true;
                        }
                    }
                    if (subRule.IndexPartValue > 0)
                    {
                        subRules.Add(subRule);
                    }
                }
                return subRules;
            }
            #endregion

            /// <summary>
            /// 规则字符串
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = SIZE, ArraySubType = UnmanagedType.U1)]
            public readonly byte[] Data = new byte[SIZE];

            /// <summary>
            /// 获取指定位置的具体字符
            /// </summary>
            /// <param name="index"></param>
            /// <returns></returns>
            /// <exception cref="IndexOutOfRangeException"></exception>
            public char this[int index]
            {
                get
                {
                    if (index < 0 || index >= SIZE)
                    {
                        throw new IndexOutOfRangeException($"输入值 {index} 不在范围 [0, {SIZE}) 内");
                    }
                    unsafe
                    {
                        fixed (byte* ptr = Data)
                        {
                            return (char)*(ptr + index);
                        }
                    }
                }
                set
                {
                    char c = value;
                    if (!ALLOW_CHARS.Contains(c))
                    {
                        c = DEFAULT_FILL_CHAR;
                    }
                    byte b = (byte)c;
                    if (index < 0 || index >= SIZE)
                    {
                        throw new IndexOutOfRangeException($"输入值 {index} 不在范围 [0, {SIZE}) 内");
                    }
                    unsafe
                    {
                        fixed (byte* ptr = Data)
                        {
                            *(ptr + index) = b;
                        }
                    }
                }
            }

            #region 转换
            public string DataString()
            {
                char[] chars = new char[SIZE + 1];
                unsafe
                {
                    fixed (byte* ptr = Data)
                    {
                        for (int i = 0; i < SIZE; i++)
                        {
                            chars[i] = (char)*(ptr + i);
                        }
                        chars[SIZE] = '\0';
                    }
                }
                return new string(chars);
            }

            public static implicit operator string(Rule rule)
            {
                return rule.DataString();
            }
            public static implicit operator Rule(string str) 
            {
                return new(str);
            }

            public override string ToString()
            {
                return DataString();
            }
            #endregion


            #region IEnumerable
            public IEnumerator<char> GetEnumerator()
            {
                return Data.Select(i => (char)i).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return Data.Select(i => (char)i).GetEnumerator();
            }
            #endregion
        }
        
        /// <summary>
        /// 子规则, 标记规则层级和实际对应的字符位置
        /// </summary>
        public struct SubRule
        {
            #region 常量
            /// <summary>
            /// 用于层编号的裁剪
            /// </summary>
            private const int LAYER_SUB_VALUE = 0b0000_1111;
            #endregion
            public SubRule()
            {
            }
            /// <summary>
            /// 数据 0~3: 层编号, 4~19: 生效位, 20~23: 暂时不用
            /// </summary>
            [MarshalAs(UnmanagedType.U4)]
            public int Data = 0;

            #region 属性
            /// <summary>
            /// 当前子规则的层编号
            /// </summary>
            public int Layer 
            { 
                get => Data & LAYER_SUB_VALUE;
                set
                {
                    byte subLayer = LAYER_SUB_VALUE;
                    Data = (Data & ~subLayer) | (value & subLayer);
                }
            }
            /// <summary>
            /// 当前子规则是否读取指定位的字符 (生效位)
            /// </summary>
            /// <param name="index"></param>
            /// <returns></returns>
            /// <exception cref="IndexOutOfRangeException"></exception>
            public bool this[int index]
            {
                get
                {
                    if (index < 0 || index >= Rule.SIZE)
                    {
                        throw new IndexOutOfRangeException($"输入值 {index} 不在范围 [0, {Rule.SIZE}) 内");
                    }
                    //int dataIndex = index < 4 ? 0 : 1;
                    //int bitIndex = index < 4 ? index + 4 : index - 4;
                    //return (Data[dataIndex] & (0b1 << bitIndex)) == 1;
                    int bitIndex = index + 4;
                    return (Data & (0b1 << bitIndex)) == 1;
                }
                set
                {
                    if (index < 0 || index >= Rule.SIZE)
                    {
                        throw new IndexOutOfRangeException($"输入值 {index} 不在范围 [0, {Rule.SIZE}) 内");
                    }
                    //int dataIndex = index < 4 ? 0 : 1;
                    //int bitIndex = index < 4 ? index + 4 : index - 4;
                    int bitIndex = index + 4;
                    int temp = 0b1 << bitIndex;
                    //Data[dataIndex] = (byte)((Data[dataIndex] & ~temp) + (value ? temp : 0));
                    Data = (Data & ~temp) + (value ? temp : 0);
                }
            }
            /// <summary>
            /// 生效位数据段的数值
            /// </summary>
            public int IndexPartValue
            {
                get
                {
                    return Data >> 4;
                }
            }
            /// <summary>
            /// 所有需要生效的索引
            /// </summary>
            public List<int> AllIndex
            {
                get
                {
                    List<int> output = new List<int>();
                    for (int i = 0; i < Rule.SIZE; i++)
                    {
                        //int dataIndex = i < 4 ? 0 : 1;
                        //int bitIndex = i < 4 ? i + 4 : i - 4;
                        int bitIndex = i + 4;
                        if ((Data & (0b1 << bitIndex)) > 0)
                        {
                            output.Add(i);
                        }
                    }
                    return output;
                }
            }

            #endregion

            public override string ToString()
            {
                char[] chars = new char[Rule.SIZE];
                for (int i = 0; i < Rule.SIZE; i++)
                {
                    //int dataIndex = i < 4 ? 0 : 1;
                    //int bitIndex = i < 4 ? i + 4 : i - 4;
                    int bitIndex = i + 4;
                    //chars[i] = (Data[dataIndex] & (0b1 << bitIndex)) > 0 ? 'O' : '-';
                    chars[i] = (Data & (0b1 << bitIndex)) > 0 ? 'O' : '-';
                }
                return $"[{Convert.ToString(Layer, 16).ToUpper().PadLeft(2, '0')}]{new string(chars)}";
            }

        }

        #endregion

        #region 解析结果结构体
        public struct Result
        {
            public bool IsSuccess { get; set; }

            public bool FailureInfo { get; set; }
            /// <summary>
            /// 由层编号, 对应层的层内编号, 组成的字典
            /// </summary>
            public Dictionary<int, ulong> LayerValueMap { get; set; }

            public static implicit operator bool(Result result)
            {
                return result.IsSuccess;
            }
        }
        #endregion

        #region 构造函数
        public LayerComponentBaseLong() { }
        /// <summary>
        /// 实例化层级组件, 然后使用输入的规则初始化
        /// </summary>
        /// <param name="rule"></param>
        public LayerComponentBaseLong(Rule rule) : this()
        {
            Init(rule);
        }
        #endregion

        #region 控制
        /// <summary>
        /// 使用输入的规则初始化
        /// </summary>
        /// <param name="rule"></param>
        public void Init(Rule rule) 
        {
            UseRule = rule;
            SubRules = rule.AllSubRules();
            SubIndexList = new Dictionary<int, List<int>>();
            foreach (var subRules in SubRules)
            {
                List<int> indexList = subRules.AllIndex;
                if (indexList.Count > 0)
                {
                    SubIndexList.Add(subRules.Layer, indexList);
                }
            }
            UsefulLayer = SubRules.Select(l => l.Layer).OrderBy(l => l).ToList();
            if (UsefulLayer == null || UsefulLayer.Count == 0)
            {
                IsSequence = false;
                IsStartWithZero = false;
                StartWithLayerId = null;
            }
            else
            {
                StartWithLayerId = UsefulLayer.Min();
                IsStartWithZero = StartWithLayerId == 0;
                IsSequence = true;
                for (int i = 0; i < UsefulLayer.Count - 1; i++)
                {
                    int a = UsefulLayer[i];
                    int b = UsefulLayer[i + 1];
                    if (b - a > 1)
                    {
                        IsSequence = false;
                        break;
                    }
                }
            }
        }
        #endregion

        #region ID值与层的转换

        /// <summary>
        /// 从输入ID值获取层级信息
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public Result Analysis(ulong value)
        {
            if (SubIndexList == null) throw new Exception("未初始化! ");
            Result result = new()
            {
                LayerValueMap = new Dictionary<int, ulong>(),
            };
            foreach (int layer in SubIndexList.Keys)
            {
                List<int> indexList = SubIndexList[layer];
                ulong temp = 0;
                for (int i = 0; i < indexList.Count; i++)
                {
                    int index = indexList[i];   // 从16位开始, 从左到右的第n位数, 当i==0, 为此层内的最高位
                    int offset = (Rule.SIZE - 1 - index) * 4;   // 这里需要传入索引, 所以-1
                    int scale = (indexList.Count - 1 - i) * 4;  // 取得需要乘多少倍: 若数据总位数3位, 则第一个数应该乘16^2, 即左移8位
                    temp += (value & ((ulong)0b1111 << offset)) // 找到数据位置
                            //>> offset                         // 移动到0的位置, 值范围: [0, 16)
                            //<< scale;                         // 乘到当前的倍率
                            >> (offset - scale);                // 合并两个运算
                }
                result.LayerValueMap.Add(layer, temp);
            }
            result.IsSuccess = true;
            return result;
        }
        /// <summary>
        /// 从层级信息生成ID值
        /// </summary>
        /// <param name="layerData"></param>
        /// <returns></returns>
        public ulong Create(Dictionary<int, ulong> layerData)
        {
            if (SubIndexList == null) throw new Exception("未初始化! ");
            ulong result = 0;
            if (layerData == null || layerData.Count == 0) { return result; }
            foreach (int layer in SubIndexList.Keys)
            { 
                if (!layerData.ContainsKey(layer))
                {
                    continue;
                }
                ulong value = layerData[layer];
                if (value == 0)
                {
                    continue;
                }
                List<int> indexList = SubIndexList[layer];
                for (int i = 0; i < indexList.Count; i++) 
                {
                    int index = indexList[i];   // 从16位开始, 从左到右的第n位数, 当i==0, 为此层内的最高位
                    int offset = (Rule.SIZE - 1 - index) * 4;   // 这里需要传入索引, 所以-1
                    int scale = (indexList.Count - 1 - i) * 4;  // 取得需要缩小多少倍: 若数据总位数3位, 则第一个数应该缩小16^2倍, 即右移8位

                    result += ((value >> scale) & 0b1111)     // 本层从左到右的第n位对应的数值
                                << offset;   // 移动到位需要放置的位置
                }
            }
            return result;
        }
        #endregion

        #region 数据
        public Rule UseRule { get; private set; }


        #region 缓存
        /// <summary>
        /// 所有子规则
        /// </summary>
        public List<SubRule>? SubRules { get; private set; } 
        /// <summary>
        /// 所有子规则
        /// </summary>
        private Dictionary<int, List<int>>? SubIndexList;
        /// <summary>
        /// 有效层
        /// </summary>
        private List<int>? UsefulLayer;

        /// <summary>
        /// 有效层是否从0开始
        /// </summary>
        public bool IsStartWithZero { get; private set; }

        /// <summary>
        /// 有效层的最小值
        /// </summary>
        public int? StartWithLayerId { get; private set; }
        /// <summary>
        /// 有效层的层编号是否连续 (差值均为1)
        /// </summary>
        public bool IsSequence { get; private set; }
        #endregion

        #endregion
    }
}
