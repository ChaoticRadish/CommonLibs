using Common_Util.Data.Constraint;
using Common_Util.Extensions;
using Common_Util.String;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Structure.Value
{
    /// <summary>
    /// 分层次的类似地址的编码
    /// <para>由一组的 <see cref="TLayer"/> 类型的值组成(至少一个), 当且仅当每一层级的值都相同时, 两个编码才相等 </para>
    /// </summary>
    /// <typeparam name="TLayer">层级标识的类型</typeparam>
    public interface ILayeringAddressCode<TLayer>
    {
        /* 涉及的一些名词: 
         * 完全范围 - 一个包含所有可能的编码的范围, 即 All! 其路径长度为 0
         * 路径 - 组成编码的一系列有序的值, 与编码的差别在于编码含是否范围, 相当于属性 LayerValues, 强调逐层寻找时的搜索内容
         * 路径末端 - 指最后一层
         * 路径末端值 - 值最后一层对应的值
         * 层数/深度 - 均指路径长度
         * 范围编码 - 表示某个范围的一个编码
         * 项编码 - 不表示范围, 而是表示具体某个东西的编码
         * 前一级范围编码 - 某个非完全范围的编码 (路径大于 0 ) 除路径末端外的其他值所构成的一个范围编码
         * 最小范围编码 - 指包含某个编码的所有可能范围编码中, 表示范围最小的一个编码. 
         *            ---- 范围编码: 其自身
         *            ---- 项编码: 编码的前一级范围编码
         * 
         */

        /// <summary>
        /// 每一层级对应的值
        /// </summary>
        TLayer[] LayerValues { get; }

        /// <summary>
        /// 层级数量
        /// </summary>
        int LayerCount { get; }

        /// <summary>
        /// 地址是否表示一个范围
        /// <para>true => 包含此地址之下的所有分支与项的范围</para>
        /// <para>false => 特定某一具体项</para>
        /// </summary>
        bool IsRange { get; }
    }

    /// <summary>
    /// 分层次的类似地址的编码, 其中的层级标识的类型是 <see cref="IStringConveying"/>
    /// </summary>
    /// <typeparam name="TLayer"></typeparam>
    public interface IStringConveyingLayeringAddressCode<TLayer> : ILayeringAddressCode<TLayer>
        where TLayer : IStringConveying
    {

    }

    internal struct LayeringAddressCodeBaseImpl<TLayer> : ILayeringAddressCode<TLayer>, IComparable<ILayeringAddressCode<TLayer>>, IComparable
    {
        public LayeringAddressCodeBaseImpl()
        {
            this.isRange = true;
            this.layerValues = [];
        }

        public LayeringAddressCodeBaseImpl(bool isRange, TLayer[] layerValues)
        {
            this.isRange = isRange;
            this.layerValues = layerValues;
        }


        public TLayer[] LayerValues
        {
            get => layerValues ?? [];   // 确保返回空集合
            set
            {
                layerValues = value;
                if (value == null || value.Length == 0)
                {
                    isRange = true;
                }
            }
        }
        private TLayer[] layerValues;

        public readonly int LayerCount { get => layerValues?.Length ?? 0; }

        public bool IsRange
        {
            get => layerValues == null || layerValues.Length == 0 || isRange;
            set
            {
                if (layerValues == null || layerValues.Length == 0)
                {
                    isRange = true;
                }
                else
                {
                    isRange = value;
                }
            }
        }
        private bool isRange;

        /// <summary>
        /// 表示一个包含一切的范围
        /// </summary>
        public static LayeringAddressCodeBaseImpl<TLayer> All { get; } = new LayeringAddressCodeBaseImpl<TLayer>();


        #region 相等比较
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return LayeringAddressCodeHelper.Equals(this, obj);
        }

        public static bool operator ==(LayeringAddressCodeBaseImpl<TLayer> left, LayeringAddressCodeBaseImpl<TLayer> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LayeringAddressCodeBaseImpl<TLayer> left, LayeringAddressCodeBaseImpl<TLayer> right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return LayeringAddressCodeHelper.GetHashCode(this);
        }


        #endregion

        #region 顺序比较
        public readonly int CompareTo(ILayeringAddressCode<TLayer>? other)
        {
            return LayeringAddressCodeHelper.CompareTo<TLayer>(this, other);
        }

        readonly int IComparable.CompareTo(object? obj)
        {
            return LayeringAddressCodeHelper.CompareTo(this, obj);
        }

        #endregion

    }


    /// <summary>
    /// 可与字符串互相转换的分层次的类似地址的编码, 层级标识的类型也是可与字符串互相转换的
    /// <para>层级标识转换为字符串时, 需要对部分字符作转义处理</para>
    /// </summary>
    /// <typeparam name="TLayer"></typeparam>
    public struct LayeringAddressCode<TLayer> : IStringConveyingLayeringAddressCode<TLayer>, IStringConveying, IComparable<ILayeringAddressCode<TLayer>>, IComparable
        where TLayer : IStringConveying, new()
    {
        public LayeringAddressCode() : this(true, []) { }
        public LayeringAddressCode(bool isRange, TLayer[] layerValues)
        {
            this.isRange = isRange;
            this.layerValues = layerValues;
        }


        #region 转义相关常量
        /// <summary>
        /// 转义字符
        /// </summary>
        public const char EscapeChar = '\\';
        /// <summary>
        /// 分隔不同层级标识的字符
        /// </summary>
        public const char SplitChar = '.';
        /// <summary>
        /// 具体项标识字符, 用于最后一个层级前, 如果最后一个层级前的字符是 '.', 则表示一个范围, 如果是 ':', 则表示具体某个东西
        /// <para>例如: </para>
        /// <para>AAA.BBB.CCC 这一字符串表示地址 AAA.BBB.CCC.* 这一范围</para>
        /// <para>AAA.BBB:CCC 这一字符串表示 AAA.BBB 中的 CCC</para>
        /// </summary>
        public const char ItemMarkChar = ':';

        #endregion

        public TLayer[] LayerValues 
        { 
            get => layerValues ?? [];   // 确保返回空集合
            set 
            { 
                layerValues = value; 
                if (value == null || value.Length == 0)
                {
                    isRange = true;
                }
            } 
        }
        private TLayer[] layerValues;

        public readonly int LayerCount { get => layerValues?.Length ?? 0; }

        public bool IsRange
        {
            get => layerValues == null || layerValues.Length == 0 || isRange;
            set
            {
                if (layerValues == null || layerValues.Length == 0) 
                {
                    isRange = true;
                }
                else
                {
                    isRange = value;
                }
            }
        }
        private bool isRange;



        public void ChangeValue(string value)
        {
            if (value.IsEmpty()) 
            {
                LayerValues = [];
                IsRange = true;
                return;
            }
            var (splitStrs, isRange) = LayeringAddressCodeHelper.AnalysisString(value, EscapeChar, SplitChar, ItemMarkChar);
            LayerValues = splitStrs.Select(StringConveyingHelper.FromString<TLayer>).ToArray();
            IsRange = isRange;

        }

        #region 静态值与转换方法


        /// <summary>
        /// 表示一个包含一切的范围
        /// </summary>
        public static LayeringAddressCode<TLayer> All { get; } = new LayeringAddressCode<TLayer>()
        {
            LayerValues = [],
            IsRange = true,
        };


        #endregion

        public string ConvertToString()
        {
            return LayeringAddressCodeHelper.ConvertToString(
                LayerValues, IsRange, 
                i => i.ConvertToString(), 
                EscapeChar, SplitChar, ItemMarkChar);
        }

        #region 隐式转换
        public static implicit operator LayeringAddressCode<TLayer>(string value)
        {
            return StringConveyingHelper.FromString<LayeringAddressCode<TLayer>>(value);
        }
        public static implicit operator LayeringAddressCode<TLayer>(string[] layerAddress)
        {
            return (layerAddress, false);
        }
        public static implicit operator LayeringAddressCode<TLayer>((bool isRange, string[] layerAddress) obj)
        {
            return (obj.layerAddress, obj.isRange);
        }
        public static implicit operator LayeringAddressCode<TLayer>((string[] layerAddress, bool isRange) obj)
        {
            return new LayeringAddressCode<TLayer>()
            {
                LayerValues = obj.layerAddress.Select(StringConveyingHelper.FromString<TLayer>).ToArray(),
                IsRange = obj.isRange,
            };
        }
        public static implicit operator LayeringAddressCode<TLayer>((bool isRange, TLayer[] layerAddress) obj)
        {
            return (obj.layerAddress, obj.isRange);
        }
        public static implicit operator LayeringAddressCode<TLayer>((TLayer[] layerAddress, bool isRange) obj)
        {
            return new LayeringAddressCode<TLayer>()
            {
                LayerValues = obj.layerAddress,
                IsRange = obj.isRange,
            };
        }
        public static implicit operator string(LayeringAddressCode<TLayer> value)
        {
            return value.ConvertToString();
        }
        #endregion


        public override string ToString()
        {
            return ConvertToString();
        }

        #region 相等比较
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return LayeringAddressCodeHelper.Equals(this, obj);
        }

        public static bool operator ==(LayeringAddressCode<TLayer> left, LayeringAddressCode<TLayer> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LayeringAddressCode<TLayer> left, LayeringAddressCode<TLayer> right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return LayeringAddressCodeHelper.GetHashCode(this);
        }


        #endregion

        #region 顺序比较
        public readonly int CompareTo(ILayeringAddressCode<TLayer>? other)
        {
            return LayeringAddressCodeHelper.CompareTo<TLayer>(this, other);
        }

        readonly int IComparable.CompareTo(object? obj)
        {
            return LayeringAddressCodeHelper.CompareTo(this, obj);
        }

        #endregion
    }

    /// <summary>
    /// 可与字符串互相转换的分层次的类似地址的编码
    /// </summary>
    public struct LayeringAddressCode : ILayeringAddressCode<string>, IStringConveying, IComparable<ILayeringAddressCode<string>>, IComparable
    {
        public LayeringAddressCode() : this(true, []) { }
        public LayeringAddressCode(bool isRange, string[] layerValues)
        {
            this.isRange = isRange;
            this.layerValues = layerValues;
        }
        /// <summary>
        /// 实例化一个项编码
        /// </summary>
        /// <param name="layers">编码中除了末端值之外, 其前方的所有层值</param>
        /// <param name="endPoint">末端值</param>
        public LayeringAddressCode(string[] layers, string endPoint)
        {
            this.isRange = false;

            string[] finalValue = new string[layers.Length + 1];
            for (int i = 0; i < layers.Length; i++)
            {
                finalValue[i] = layers[i];
            }
            finalValue[^1] = endPoint;
            this.layerValues = finalValue;
        }

        #region 转义相关常量
        /// <summary>
        /// 转义字符
        /// </summary>
        public const char EscapeChar = '\\';
        /// <summary>
        /// 分隔不同层级标识的字符
        /// </summary>
        public const char SplitChar = '.';
        /// <summary>
        /// 具体项标识字符, 用于最后一个层级前, 如果最后一个层级前的字符是 '.', 则表示一个范围, 如果是 ':', 则表示具体某个东西
        /// <para>例如: </para>
        /// <para>AAA.BBB.CCC 这一字符串表示地址 AAA.BBB.CCC.* 这一范围</para>
        /// <para>AAA.BBB:CCC 这一字符串表示 AAA.BBB 中的 CCC</para>
        /// </summary>
        public const char ItemMarkChar = ':';

        #endregion

        public string[] LayerValues
        {
            get => layerValues ?? [];   // 确保返回空集合
            set
            {
                layerValues = value;
                if (value == null || value.Length == 0)
                {
                    isRange = true;
                }
            }
        }
        private string[] layerValues;

        public readonly int LayerCount { get => layerValues?.Length ?? 0; }

        public bool IsRange
        {
            get => layerValues == null || layerValues.Length == 0 || isRange;
            set
            {
                if (layerValues == null || layerValues.Length == 0)
                {
                    isRange = true;
                }
                else
                {
                    isRange = value; 
                }
            }
        }
        private bool isRange;

        /// <summary>
        /// 表示一个包含一切的范围
        /// </summary>
        public static LayeringAddressCode All { get; } = new LayeringAddressCode()
        {
            LayerValues = [],
            IsRange = true,
        };

        public void ChangeValue(string value)
        {
            if (value.IsEmpty())
            {
                LayerValues = [];
                IsRange = true;
                return;
            }

           var (splitStrs, isRange) = LayeringAddressCodeHelper.AnalysisString(value, EscapeChar, SplitChar, ItemMarkChar);
            LayerValues = [.. splitStrs];
            IsRange = isRange;
        }

        public string ConvertToString()
        {
            return LayeringAddressCodeHelper.ConvertToString(
                LayerValues, IsRange,
                i => i,
                EscapeChar, SplitChar, ItemMarkChar);
        }

        #region 隐式转换
        public static implicit operator LayeringAddressCode(string value)
        {
            return StringConveyingHelper.FromString<LayeringAddressCode>(value);
        }
        public static implicit operator LayeringAddressCode(string[] layerAddress)
        {
            return (layerAddress, false);
        }
        public static implicit operator LayeringAddressCode((bool isRange, string[] layerAddress) obj)
        {
            return (obj.layerAddress, obj.isRange);
        }
        public static implicit operator LayeringAddressCode((string[] layerAddress, bool isRange) obj)
        {
            if (obj.layerAddress.Any(i => i.IsEmpty()))
            {
                throw new ArgumentException("地址出现了空值层级! ");
            }
            return new LayeringAddressCode()
            {
                LayerValues = obj.layerAddress,
                IsRange = obj.isRange,
            };
        }
        public static implicit operator string(LayeringAddressCode value)
        {
            return value.ConvertToString();
        }
        #endregion

        public override string ToString()
        {
            return ConvertToString();
        }


        #region 相等比较
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is ILayeringAddressCode<string> otherCode)
            {
                if (IsRange != otherCode.IsRange)
                {
                    return false;
                }
                else if (LayerCount != otherCode.LayerCount)
                {
                    return false;
                }
                else
                {
                    for (int i = 0; i < LayerCount; i++)
                    {
                        if (!LayerValues[i].Equals(otherCode.LayerValues[i]))
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        public static bool operator ==(LayeringAddressCode left, LayeringAddressCode right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LayeringAddressCode left, LayeringAddressCode right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return LayeringAddressCodeHelper.GetHashCode(this);
        }

        #endregion

        #region 顺序比较
        public int CompareTo(ILayeringAddressCode<string>? other)
        {
            return LayeringAddressCodeHelper.CompareTo<string>(this, other);
        }

        readonly int IComparable.CompareTo(object? obj)
        {
            if (obj is string str) 
            {
                LayeringAddressCode other = str;
                return LayeringAddressCodeHelper.CompareTo<string>(this, other);
            }
            return LayeringAddressCodeHelper.CompareTo(this, obj);
        }


        #endregion

        #region 静态方法
        /// <summary>
        /// 创建项编码
        /// </summary>
        /// <param name="layerValues"></param>
        /// <returns></returns>
        public static LayeringAddressCode CreateItem(string[] layerValues) => new(false, layerValues);
        /// <summary>
        /// 创建范围编码
        /// </summary>
        /// <param name="layerValues"></param>
        /// <returns></returns>
        public static LayeringAddressCode CreateRange(string[] layerValues) => new(true, layerValues);
        #endregion
    }



    public static class LayeringAddressCodeHelper
    {
        #region 路径
        /// <summary>
        /// 创建一个表示路径的数组
        /// </summary>
        /// <typeparam name="TLayer"></typeparam>
        /// <param name="layers">一系列层值, 按遍历顺序放在数组的前侧</param>
        /// <param name="layer">最后一个层值, 放在数组的最后一个元素</param>
        /// <returns></returns>
        public static TLayer[] CreatePath<TLayer>(IEnumerable<TLayer> layers, TLayer layer)
        {
            TLayer[] output = new TLayer[layers.Count() + 1];
            int index = 0;
            foreach (var l in layers)
            {
                output[index] = l;
                index++;
            }
            output[index] = layer;
            return output;
        }

        /// <summary>
        /// 拼接多个编码, 构成一个新的路径
        /// <para>传入的编码依次排列连接, 这要求只有最后一个编码可以是项编码, 其余编码都只能是范围编码</para>
        /// </summary>
        /// <typeparam name="TLayer"></typeparam>
        /// <param name="codes"></param>
        /// <returns></returns>
        public static TLayer[] CreatePath<TLayer>(params ILayeringAddressCode<TLayer>[] codes)
        {
            if (codes == null || codes.Length == 0) return [];
            else if (codes.Length == 1) return codes[0].LayerValues;
            else
            {
                List<TLayer> output = [];
                for (int i = 0; i < codes.Length; i++)
                {
                    if (i != codes.Length - 1)
                    {
                        if (!codes[i].IsRange)
                        {
                            throw new ArgumentException("传入编码集合异常, 集合中非末位的编码出现了项编码! ", nameof(codes));
                        }
                    }
                    output.AddRange(codes[i].LayerValues);
                }
                return [.. output];
            }
        }


        /// <summary>
        /// 判断编码的路径与传入的路径是否等价
        /// </summary>
        /// <typeparam name="TLayer"></typeparam>
        /// <param name="code"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool PathEquals<TLayer>(ILayeringAddressCode<TLayer> code, IEnumerable<TLayer> path, IEqualityComparer<TLayer>? comparer = null)
        {
            return PathEquals(code.LayerValues, path, comparer);
        }

        /// <summary>
        /// 判断两个路径是否等价
        /// </summary>
        /// <typeparam name="TLayer"></typeparam>
        /// <param name="path1"></param>
        /// <param name="path2"></param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        public static bool PathEquals<TLayer>(IEnumerable<TLayer> path1, IEnumerable<TLayer> path2, IEqualityComparer<TLayer>? comparer = null)
        {
            int count1 = path1.Count();
            int count2 = path2.Count();
            if (count1 != count2) return false;

            var e1 = path1.GetEnumerator();
            var e2 = path2.GetEnumerator();

            // 开始比较各层的值
            comparer ??= EqualityComparer<TLayer>.Default;  
            for (int i = 0; i < count1; i++)
            {
                if (e1.MoveNext() && e2.MoveNext())
                {
                    // 正常情况下, 双方应该都可以移动成功
                    var v1 = e1.Current;
                    var v2 = e2.Current;
                    if (!comparer.Equals(v1, v2))
                    {
                        // 任意一层不等价
                        return false;
                    }
                }
                else 
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region 转换

        /// <summary>
        /// 将实现接口的对象转换为 <see cref="LayeringAddressCode"/>
        /// </summary>
        /// <param name="inputValue"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public static LayeringAddressCode Convert(ILayeringAddressCode<string> inputValue)
        {
            string[] newLayers = new string[inputValue.LayerCount];
            Array.Copy(inputValue.LayerValues, newLayers, newLayers.Length);
            return new(inputValue.IsRange, newLayers);
        }

        /// <summary>
        /// 将实现接口的对象转换为 <see cref="LayeringAddressCode{TLayer}"/>
        /// </summary>
        /// <param name="inputValue"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public static LayeringAddressCode<TLayer> Convert<TLayer>(ILayeringAddressCode<TLayer> inputValue)
            where TLayer : IStringConveying, new()
        {
            TLayer[] newLayers = new TLayer[inputValue.LayerCount];
            Array.Copy(inputValue.LayerValues, newLayers, newLayers.Length);
            return new(inputValue.IsRange, newLayers);
        }
        #endregion

        #region 字符串相关
        /// <summary>
        /// 使用传入的分隔符, 以通用层级编码的形式, 拆分传入的字符串, 取得各层级标识对应的字符串, 以及传入字符串是否表示一个范围
        /// </summary>
        /// <param name="value"></param>
        /// <param name="escapeChar">转义字符</param>
        /// <param name="splitChar">范围层级分隔符</param>
        /// <param name="itemMarkChar">项编码标识字符</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (List<string> splitStrs, bool isRange) AnalysisString(string value, char escapeChar, char splitChar, char itemMarkChar)
        {
            List<string> strs = new();
            StringBuilder sb = new();
            bool hasItemMark = false;
            EscapeHelper.Ergodic(value, escapeChar,
                (c, b) =>
                {
                    if (!b)
                    {
                        if (c == splitChar || c == itemMarkChar)
                        {
                            if (sb.Length == 0 && (c == splitChar || strs.Count > 0))
                            {
                                throw new ArgumentException("出现空的层级标识! ");
                            }
                            else
                            {
                                strs.Add(sb.ToString());
                                sb.Clear();
                            }
                            if (c == itemMarkChar)
                            {
                                if (hasItemMark)
                                {
                                    throw new ArgumentException("出现了两个或以上的类型标识字符! ");
                                }
                                hasItemMark = true;
                            }
                        }
                        else
                        {
                            sb.Append(c);
                        }
                    }
                    else
                    {
                        sb.Append(c);
                    }
                });
            if (sb.Length == 0)
            {
                throw new ArgumentException("最后一个层级标识是空的! ");
            }
            else
            {
                strs.Add(sb.ToString());
            }
            return (strs, !hasItemMark);
        }

        /// <summary>
        /// 通用的将编码转换成字符串的方法
        /// <para>转换后的编码形如: ValueA.ValueB:ValueC </para>
        /// </summary>
        /// <typeparam name="TLayer"></typeparam>
        /// <param name="code"></param>
        /// <param name="convertFunc">层值到字符串的转换方法</param>
        /// <param name="escapeChar">转义字符, 如果层值字符串出现了方法传入的几个特殊符号, 会使用此字符来加以区分, 表示是普通的字符</param>
        /// <param name="splitChar">分隔不同层值的标识字符</param>
        /// <param name="itemMarkChar">项编码的标识字符, 如果是项编码, 将被标记在最后一个层值的字符串前</param>
        /// <returns></returns>
        public static string ConvertToString<TLayer>(ILayeringAddressCode<TLayer> code,
            Func<TLayer, string>? convertFunc = null, 
            char escapeChar = LayeringAddressCode.EscapeChar, 
            char splitChar = LayeringAddressCode.SplitChar, 
            char itemMarkChar = LayeringAddressCode.ItemMarkChar)
        {
            return ConvertToString(code.LayerValues, code.IsRange, 
                convertFunc ?? _defaultLayerValueToString, 
                escapeChar, splitChar, itemMarkChar);
        }
        private static string _defaultLayerValueToString<TLayer>(TLayer layerValue) => layerValue == null ? "<null>" : (layerValue.ToString() ?? string.Empty);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string ConvertToString<T>(T[] items, bool isRange, Func<T, string> convertFunc, char escapeChar, char splitChar, char itemMarkChar)
        {
            StringBuilder sb = new();
            if (items == null) return string.Empty;
            for (int i = 0; i < items.Length; i++)
            {
                if (!isRange && i == items.Length - 1)
                {
                    sb.Append(itemMarkChar);
                }
                else if (i > 0)
                {
                    sb.Append(splitChar);
                }
                sb.Append(EscapeHelper.AddEscape(convertFunc(items[i]), escapeChar, splitChar, itemMarkChar));
            }
            return sb.ToString();
        }
        #endregion

        #region 哈希值
        /// <summary>
        /// 通用的取得哈希值的方法
        /// </summary>
        /// <remarks>
        /// 返回结果由以下几项组成: <br/>
        /// 是否是范围编码? (占 1 bit) <br/>
        /// 层级数量 (占 4 bit) <br/>
        /// 第一个层级的哈希值的后 10 bit
        /// 第二个层级的哈希值的后 10 bit
        /// 第三个层级的哈希值的后 7 bit
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="code"></param>
        /// <returns></returns>
        public static int GetHashCode<T>(ILayeringAddressCode<T> code)
        {
            int rangeMark = code.IsRange ? (1 << 31) : 0;
            int layerCountMark = (code.LayerCount > 0b1111 ? 0b1111 : (code.LayerCount & 0b1111)) << 27;

            int l0Mark = 0;
            int l1Mark = 0;
            int l2Mark = 0;

            if (code.LayerCount > 0)
            {
                var v0 = code.LayerValues[0];
                if (v0 != null)
                {
                    l0Mark = (v0.GetHashCode() & 0b_0011_1111_1111) << 17;
                }
            }
            if (code.LayerCount > 1)
            {
                var v1 = code.LayerValues[1];
                if (v1 != null)
                {
                    l1Mark = (v1.GetHashCode() & 0b_0011_1111_1111) << 7;
                }
            }
            if (code.LayerCount > 2)
            {
                var v2 = code.LayerValues[2];
                if (v2 != null)
                {
                    l2Mark = (v2.GetHashCode() & 0b_0000_0111_1111) << 0;
                }
            }

            return rangeMark | layerCountMark | l0Mark | l1Mark | l2Mark;
        }
        #endregion

        #region 比较
        internal static bool Equals<T>(ILayeringAddressCode<T> code, object? other)
        {
            if (other is ILayeringAddressCode<T> otherCode)
            {
                if (code.IsRange != otherCode.IsRange)
                {
                    return false;
                }
                else if (code.LayerCount != otherCode.LayerCount)
                {
                    return false;
                }
                else
                {
                    // 任意层值不相等时, 返回 false
                    for (int i = 0; i < code.LayerCount; i++)
                    {
                        T layerValue = code.LayerValues[i];
                        T otherLayerValue = otherCode.LayerValues[i];
                        if (layerValue != null)
                        {
                            if (!layerValue.Equals(otherLayerValue))
                            {
                                return false;
                            }
                        }
                        else
                        {
                            if (otherLayerValue != null)
                            {   
                                return false;
                            }
                        }
                    }
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 比较一个编码与另一个对象, 根据另一个对象是否实现接口 <see cref="ILayeringAddressCode{TLayer}"/> 决定采取什么比较方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static int CompareTo<T>(ILayeringAddressCode<T> self, object? other)
        {
            if (other == null) return 1;
            else
            {
                Type otherType = other.GetType();
                var interfaces = otherType.GetInterfaces();

                bool isLayeringAddressCode = false;
                Type? layerValueType = null;
                foreach (var interfaceType in interfaces)
                {
                    if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(ILayeringAddressCode<>))
                    {
                        var gArgs = interfaceType.GetGenericArguments();
                        if (gArgs.Length == 1)
                        {
                            layerValueType = gArgs[0];
                            isLayeringAddressCode = true;
                            break;
                        }
                    }
                }

                if (isLayeringAddressCode) 
                {
                    var method = methodInfoCompareTo_T1_T2.MakeGenericMethod(typeof(T), layerValueType!);
                    object? result = method.Invoke(null, [self, other]);
                    if (result is int intValue)
                    {
                        return intValue;
                    }
                    else
                    {
                        throw new Exception($"调用比较方法 {nameof(CompareTo)}<{typeof(T)}, {layerValueType}>(...) 返回了意料之外的结果: {result?.ToString() ?? "<null>"}");
                    }
                }
                else
                {
                    return Comparer.Default.Compare(self, other);
                }
            }

        }

        private static MethodInfo methodInfoCompareTo_T1_T2
        {
            get
            {
                if (_methodInfoCompareTo_T1_T2 == null)
                {
                    var methods = typeof(LayeringAddressCodeHelper).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    foreach (var method in methods.Where(i => i.Name == nameof(CompareTo) && i.GetParameters().Length == 2))
                    {
                        if (method.IsGenericMethod && method.GetGenericArguments().Length == 2)
                        {
                            _methodInfoCompareTo_T1_T2 = method;
                        }
                    }
                }
                return _methodInfoCompareTo_T1_T2!;
            }
        }
        private static MethodInfo? _methodInfoCompareTo_T1_T2;
        /// <summary>
        /// 比较两个不同层值类型的编码
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="self"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        internal static int CompareTo<T1, T2>(ILayeringAddressCode<T1> self, ILayeringAddressCode<T2> other)
        {
            if (other == null) return 1;
            else if (typeof(T1) == typeof(T2))
            {
                return CompareTo<T1>(self, (ILayeringAddressCode<T1>)other);
            }
            else
            {
                bool t1IsIComparable = typeof(T1).IsAssignableTo(typeof(IComparable));
                bool t2IsIComparable = typeof(T1).IsAssignableTo(typeof(IComparable));

                Comparer comparer = Comparer.Default;

                int max = Math.Max(self.LayerCount, other.LayerCount);
                int min = Math.Min(self.LayerCount, other.LayerCount);
                for (int i = 0; i < max; i++)
                {
                    if (i >= min)
                    {
                        return self.LayerCount > other.LayerCount ? 1 : -1;
                    }

                    T1? tSelf = self.LayerValues[i];
                    T2? tOther = other.LayerValues[i];

                    if (tSelf == null && tOther == null)
                    {
                        continue;
                    }
                    else if (tSelf == null && tOther != null)
                    {
                        return -1;
                    }
                    else if (tSelf != null && tOther == null)
                    {
                        return 1;
                    }
                    else if (tSelf != null && tOther != null)
                    {
                        int compareResult;
                        if (t1IsIComparable)
                        {
                            compareResult = ((IComparable)tSelf).CompareTo(tOther);
                        }
                        else if (t2IsIComparable)
                        {
                            compareResult = -((IComparable)tOther).CompareTo(tSelf);
                        }
                        else
                        {
                            compareResult = comparer!.Compare(tSelf, tOther);

                        }
                        if (compareResult != 0) return compareResult;
                        else
                        {
                            if (i == max - 1)
                            {
                                // 两者均在最后一级, 且层值相等
                                if (self.IsRange == other.IsRange)
                                {
                                    return 0;
                                }
                                else
                                {
                                    return self.IsRange ? 1 : -1;
                                }
                            }

                        }
                    }
                }
                return 0;
            }
        }
        /// <summary>
        /// 比较两个相同层值类型的编码
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        internal static int CompareTo<T>(ILayeringAddressCode<T> self, ILayeringAddressCode<T>? other)
        {
            if (other == null) return 1;
            else
            {
                bool tIsIComparable_T = typeof(T).IsAssignableTo(typeof(IComparable<T>));
                bool tIsIComparable = !tIsIComparable_T && typeof(T).IsAssignableTo(typeof(IComparable));
                Comparer<T>? comparer = null;
                if (!tIsIComparable_T && !tIsIComparable) 
                {
                    comparer = Comparer<T>.Default;
                }

                int max = Math.Max(self.LayerCount, other.LayerCount);
                int min = Math.Min(self.LayerCount, other.LayerCount);
                for (int i = 0; i < max; i++)
                {
                    if (i >= min)
                    {
                        return self.LayerCount > other.LayerCount ? 1 : -1;
                    }

                    T? tSelf = self.LayerValues[i];
                    T? tOther = other.LayerValues[i];

                    if (tSelf == null && tOther == null)
                    {
                        continue;
                    }
                    else if (tSelf == null && tOther != null)
                    {
                        return -1;
                    }
                    else if (tSelf != null && tOther == null) 
                    {
                        return 1;
                    }
                    else if (tSelf != null && tOther != null)
                    {
                        int compareResult;
                        if (tIsIComparable_T)
                        {
                            compareResult = ((IComparable<T>)tSelf).CompareTo(tOther);
                        }
                        else if (tIsIComparable)
                        {
                            compareResult = ((IComparable)tSelf).CompareTo(tOther);
                        }
                        else
                        {
                            compareResult = comparer!.Compare(tSelf, tOther);

                        }
                        if (compareResult != 0) return compareResult;
                        else
                        {
                            if (i == max - 1)
                            {
                                // 两者均在最后一级, 且层值相等
                                if (self.IsRange == other.IsRange)
                                {
                                    return 0;
                                }
                                else
                                {
                                    return self.IsRange ? 1 : -1;
                                }
                            }

                        }
                    }
                }
                return 0;
            }
        }
        #endregion



        #region 用于测试的内容
        /// <summary>
        /// 随机生成一组编码, 生成的这一组编码将符合多叉树的完整结构, 可转换为多叉树而无需补充范围编码. 不会出现重复编码
        /// <para>遍历时的首个编码, 如果是范围编码的话, 将可包含这组编码中的剩余所有编码</para>
        /// <para>!!! 注意: 返回的是 IEnumerable! 如果没有用 ToArray 或 ToList 转换为固定的集合, 使用时可能会返回不同的结果 !!!</para>
        /// </summary>
        /// <typeparam name="TLayer"></typeparam>
        /// <param name="pathValues">路径可用值. 非路径末端值或范围编码的路径末端值, 都将会从这里面随机取值</param>
        /// <param name="endPointValues">项编码的路径末端值, 将会从这里面随机取值</param>
        /// <param name="minDepth">生成编码的最小深度</param>
        /// <param name="maxDepth">生成编码时, 允许最大深度, 深度等于此值的编码都会是项编码. 生成的最大深度的编码, 有可能会小于此值! </param>
        /// <param name="minForkingCount">最少分支数量 (包含范围编码和项编码), 尝试生成范围编码的子项时, 会尽可能满足此条件, 除非已经没有可用的层值了</param>
        /// <param name="maxForkingCount">最大分支数量 (包含范围编码和项编码), 生成范围编码的子项时, 会以此为生成上限, 即无论如何都会不会有超过这个数量的子项</param>
        /// <param name="entryRangeProbability">首层范围编码的生成概率, 一般情况下需要一个稍微高一点的概率, 避免大概率只生成一个编码的情况</param>
        /// <param name="rangeProbability">范围编码的生成概率. 同时可以是范围编码或者项编码的位置, 生成时以此概率决定是否作为一个范围编码来生成</param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static IEnumerable<ILayeringAddressCode<TLayer>> Random<TLayer>(
            IList<TLayer> pathValues, IList<TLayer> endPointValues,
            int minDepth, int maxDepth, int minForkingCount, int maxForkingCount, double entryRangeProbability = 0.95, double rangeProbability = 0.2, System.Random? random = null)
        {
            return Random(pathValues, endPointValues,
                minDepth, maxDepth, minForkingCount, maxForkingCount, entryRangeProbability, rangeProbability,
                (path) => new LayeringAddressCodeBaseImpl<TLayer>(false, path),
                (path) => new LayeringAddressCodeBaseImpl<TLayer>(true, path),
                random)
                .Select(i => (ILayeringAddressCode<TLayer>)i);
        }

        /// <summary>
        /// 随机生成一组编码, 生成的这一组编码将符合多叉树的完整结构, 可转换为多叉树而无需补充范围编码. 不会出现重复编码
        /// <para>遍历时的首个编码, 如果是范围编码的话, 将可包含这组编码中的剩余所有编码</para>
        /// <para>!!! 注意: 返回的是 IEnumerable! 如果没有用 ToArray 或 ToList 转换为固定的集合, 使用时可能会返回不同的结果 !!!</para>
        /// </summary>
        /// <typeparam name="TLayer"></typeparam>
        /// <typeparam name="TCode"></typeparam>
        /// <param name="pathValues">路径可用值. 非路径末端值或范围编码的路径末端值, 都将会从这里面随机取值</param>
        /// <param name="endPointValues">项编码的路径末端值, 将会从这里面随机取值</param>
        /// <param name="minDepth">生成编码的最小深度</param>
        /// <param name="maxDepth">生成编码时, 允许最大深度, 深度等于此值的编码都会是项编码. 生成的最大深度的编码, 有可能会小于此值! </param>
        /// <param name="minForkingCount">最少分支数量 (包含范围编码和项编码), 尝试生成范围编码的子项时, 会尽可能满足此条件, 除非已经没有可用的层值了</param>
        /// <param name="maxForkingCount">最大分支数量 (包含范围编码和项编码), 生成范围编码的子项时, 会以此为生成上限, 即无论如何都会不会有超过这个数量的子项</param>
        /// <param name="entryRangeProbability">首层范围编码的生成概率, 一般情况下需要一个稍微高一点的概率, 避免大概率只生成一个编码的情况</param>
        /// <param name="rangeProbability">除了首层外, 其他层范围编码的生成概率. 同时可以是范围编码或者项编码的位置, 生成时以此概率决定是否作为一个范围编码来生成</param>
        /// <param name="createItemFunc">创建项编码的方法</param>
        /// <param name="createRangeFunc">创建范围编码的方法</param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static IEnumerable<TCode> Random<TLayer, TCode>(
            IList<TLayer> pathValues, IList<TLayer> endPointValues,
            int minDepth, int maxDepth, int minForkingCount, int maxForkingCount, double entryRangeProbability, double rangeProbability,
            Func<TLayer[], TCode> createItemFunc,
            Func<TLayer[], TCode> createRangeFunc,
            System.Random? random = null)
            where TCode : ILayeringAddressCode<TLayer>
        {
            random ??= new System.Random();

            maxDepth = Math.Max(maxDepth, 1);
            minDepth = Math.Max(minDepth, 1);
            Common_Util.Maths.CompareHelper.JudgeBigger(ref minDepth, ref maxDepth);

            minForkingCount = Math.Max(minForkingCount, 0);
            maxForkingCount = Math.Max(maxForkingCount, 1);
            Common_Util.Maths.CompareHelper.JudgeBigger(ref minForkingCount, ref maxForkingCount);

            List<TCode> currentDepthRanges = new List<TCode>(); // 当前所在深度的所有范围编码 
            List<TLayer> currentRange_RangeValues = new List<TLayer>();   // 当前处理范围编码下一级所有范围编码的层值 (仅相邻的下一级)
            List<TLayer> currentRange_ItemValues = new List<TLayer>();    // 当前处理范围编码下一级所有项编码的层值 (仅相邻的下一级)

            // 创建根值
            TLayer[] rootPath = new TLayer[minDepth];
            for (int currentDepthIndex = 0; currentDepthIndex < minDepth; currentDepthIndex++)
            {
                if (currentDepthIndex < minDepth - 1)
                {
                    rootPath[currentDepthIndex] = pathValues.Random(random);
                }
                else
                {
                    bool isRange = minDepth > 1 // 最小层数要求大于 1 时, 这一个编码必须是范围编码
                                   || Common_Util.Random.RandomValueTypeHelper.RandomTrue(entryRangeProbability, random);
                    if (isRange)
                    {
                        rootPath[currentDepthIndex] = pathValues.Random(random);

                        var code = createRangeFunc(rootPath);
                        yield return code;

                        currentDepthRanges.Add(code);
                    }
                    else
                    {
                        rootPath[currentDepthIndex] = endPointValues.Random(random);

                        yield return createItemFunc(rootPath);
                        yield break;    // 随机生成的入口是项编码, 无法拥有下级编码
                    }
                }
            }
            // 创建后续层值
            for (int currentDepthIndex = minDepth; currentDepthIndex < maxDepth; currentDepthIndex++)
            {
                var preDepthRanges = currentDepthRanges;
                currentDepthRanges = new();

                if (preDepthRanges.Count == 0)
                {
                    yield break;
                }
                foreach (var range in preDepthRanges)
                {
                    currentRange_RangeValues.Clear();
                    currentRange_ItemValues.Clear();

                    int createCount = random.Next(minForkingCount, maxForkingCount);
                    for (int index = 0; index < createCount; index++)
                    {
                        bool mustItem = currentDepthIndex == maxDepth - 1;   // 当前处理中的深度已经是最大深度
                        bool isRange = !mustItem && Common_Util.Random.RandomValueTypeHelper.RandomTrue(rangeProbability, random);
                        if (isRange)
                        {
                            if (pathValues.TryGetRandom(out TLayer lTemp, currentRange_RangeValues, random: random))
                            {
                                var code = createRangeFunc(CreatePath(range.LayerValues, lTemp));
                                yield return code;

                                currentDepthRanges.Add(code);

                                currentRange_RangeValues.Add(lTemp);
                            }
                            else
                            {
                                // 没有可用的用于范围编码的层值了, 尝试创建项编码
                                if (endPointValues.TryGetRandom(out TLayer lTemp2, currentRange_ItemValues, random: random))
                                {
                                    var code = createItemFunc(CreatePath(range.LayerValues, lTemp2));
                                    yield return code;

                                    currentRange_ItemValues.Add(lTemp2);
                                }
                            }
                        }
                        else
                        {
                            if (endPointValues.TryGetRandom(out TLayer lTemp, currentRange_ItemValues, random: random))
                            {
                                var code = createItemFunc(CreatePath(range.LayerValues, lTemp));
                                yield return code;

                                currentRange_ItemValues.Add(lTemp);
                            }
                            else
                            {
                                // 没有可用的用于项编码的层值了, 且当前不必须生成项编码, 则尝试创建范围编码
                                if (!mustItem && pathValues.TryGetRandom(out TLayer lTemp2, currentRange_RangeValues, random: random))
                                {
                                    var code = createRangeFunc(CreatePath(range.LayerValues, lTemp2));
                                    yield return code;

                                    currentDepthRanges.Add(code);

                                    currentRange_RangeValues.Add(lTemp2);
                                }
                            }
                        }
                    }

                }
            }

        }

        #endregion
    }

}
