using Common_Util.Data.Constraint;
using Common_Util.Data.Converter;
using Common_Util.Enums;
using Common_Util.Extensions;
using Common_Util.Maths;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Common_Util.Data.Structure.Value
{
    /// <summary>
    /// 简易的十进制的定点数
    /// <para>直接保存各个位对应的数字, 会有较大的空间浪费</para>
    /// <para>可用于性能要求不高, 但是要求数值在十进制运算时不允许丢失精度的场景</para>
    /// </summary>
    [TypeConverter(typeof(DecFixedPointNumberTypeConverter))]
    public partial struct DecFixedPointNumber : IStringConveying, ICloneable
    {
        #region 构造函数
        public DecFixedPointNumber()
        {
            _setZero();
        }
        /// <summary>
        /// 使用传入参数创建结构体
        /// </summary>
        /// <param name="isPositive"></param>
        /// <param name="integerPart">整数部分的数据</param>
        /// <param name="decimalPart">小数部分的数据</param>
        public DecFixedPointNumber(
            bool isPositive, byte[] integerPart, byte[] decimalPart)
        {
            IsZero = !(
                (integerPart.Length > 0 && integerPart.Any(i => i > 0)) ||
                (decimalPart.Length > 0 && decimalPart.Any(i => i > 0))
                );
            if (IsZero)
            {
                IntegerPart = [];
                DecimalPart = [];
                IsPositive = false;
            }
            else
            {
                IntegerPart = integerPart.TrimStart();
                DecimalPart = decimalPart.TrimEnd();
                IsPositive = isPositive;
            }
        }
        #endregion

        #region 数据
        /// <summary>
        /// 是零值
        /// </summary>
        public bool IsZero { get; private set; }
        /// <summary>
        /// 是否正数
        /// </summary>
        public bool IsPositive { get; private set; }
        /// <summary>
        /// 整数部分, 从高位到低位排列
        /// <para>例如: 数值 1234 对应 byte[] { 1, 2, 3, 4, }</para>
        /// </summary>
        public byte[] IntegerPart { get; private set; } = [];
        public readonly byte[] IntegerPartProxy { get =>  IntegerPart ?? []; }
        /// <summary>
        /// 整数部分的长度, 最小值为1 (即个位数, 或者0)
        /// </summary>
        public readonly int IntegerPartLength { get => IsZero ? 1 : (IntegerPart?.Length ?? 0); }
        /// <summary>
        /// 小数部分, 从高位到低位排列
        /// <para>例如: 数值 0.1234 的小数部分, 对应 byte[] { 1, 2, 3, 4, }</para>
        /// </summary>
        public byte[] DecimalPart { get; private set; } = [];
        public readonly byte[] DecimalPartProxy { get => DecimalPart ?? []; }
        /// <summary>
        /// 小数部分的长度, 最小值为0 (即没有小数)
        /// </summary>
        public readonly int DecimalPartLength { get => DecimalPart?.Length ?? 0; }
        #endregion

        #region 静态 (转换相关)

        /// <summary>
        /// 取得代表零值的结构体
        /// </summary>
        public static DecFixedPointNumber Zero
        {
            get
            {
                var output = new DecFixedPointNumber();
                output._setZero();
                return output;
            }
        }

        /// <summary>
        /// 判断输入文本能否被转换
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool CanConvert(string str)
        {
            string value = str.Trim();
            return CanConvertRegex().IsMatch(value);
        }
        /// <summary>
        /// 尝试转换输入的字符串为 <see cref="DecFixedPointNumber"/>
        /// </summary>
        /// <param name="str"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool TryPasue(string str, out DecFixedPointNumber result)
        {
            if (CanConvert(str))
            {
                result = new DecFixedPointNumber();
                result.ChangeValue(str);
                return true;
            }
            else
            {
                result = new DecFixedPointNumber();
                return false;
            }
        }
        /// <summary>
        /// 将 int 值转换为 <see cref="DecFixedPointNumber"/>
        /// </summary>
        /// <param name="i"></param>
        /// <param name="pointPosition">小数点所在位置, 即int值中有几位是小数</param>
        /// <returns></returns>
        public static DecFixedPointNumber Convert(int i, int pointPosition = 0)
        {
            // 判断是否为0
            if (i == 0)
            {
                return Zero;
            }

            // 取得方向
            bool isPosition = i > 0;

            // 取得各位上的数值
            byte[] buffer = new byte[10];
            int mod;
            int calc = i;
            bool position = i > 0;
            for (int _i = 1, _b = buffer.Length - 1;    // _i 跳过 0, 因为 10^0 == 1, 需跳过.
                _i <= IntCalcUtil.Pow10MaxIndex && calc != 0; _i++, _b--)
            {
                mod = calc % 10;
                calc = calc / 10;

                buffer[_b] = position ? (byte)mod : (byte)-mod;
            }
            buffer[0] = (byte)calc;     // 高位到低位排列

            DecFixedPointNumber output;
            if (pointPosition >= 0 && pointPosition <= 10)
            {
                output = new()
                {
                    IsZero = false,
                    IsPositive = isPosition,
                    IntegerPart = buffer[..^pointPosition].TrimStart(0),
                    DecimalPart = buffer[^pointPosition..].TrimEnd(0)
                };
            }
            else if (pointPosition < 0)
            {
                var temp = new byte[10 + (-pointPosition)];
                Array.Copy(buffer, temp, buffer.Length);
                output = new()
                {
                    IsZero = false,
                    IsPositive = isPosition,
                    IntegerPart = temp.TrimStart(0),
                    DecimalPart = [],
                };
            }
            else
            {
                var temp = new byte[pointPosition];
                Array.Copy(buffer, 0, temp, pointPosition - buffer.Length, buffer.Length);
                output = new()
                {
                    IsZero = false,
                    IsPositive = isPosition,
                    IntegerPart = [],
                    DecimalPart = temp.TrimEnd(0),
                };
            }

            return output;
        }

        #endregion

        /// <summary>
        /// 用来检查输入值是不是数字的正则表达式\
        /// <para>可匹配: 带正负号的小数或整数, 允许前后包含任意数量的0</para>
        /// </summary>
        // [GeneratedRegex(@"^[-\+]?([1-9]\d*\.\d*|0\.\d*[1-9]\d*|0?\.0+|0|[1-9]\d*)$")]
        [GeneratedRegex(@"^[-\+]?(\d*\.\d*|\d*)$")]
        private static partial Regex CanConvertRegex();


        private void _setZero()
        {
            IsZero = true;
            IsPositive = false;
            IntegerPart = [];
            DecimalPart = [];
        }

        #region 转换
        public void ChangeValue(string value)
        {
            value = value.Trim();
            if (string.IsNullOrEmpty(value) || !CanConvertRegex().IsMatch(value))
            {
                _setZero();
            }
            else
            {
                // 读取符号
                IsPositive = value[0] switch
                {
                    '-' => false,
                    _ => true,
                };
                int findPoint = value.IndexOf('.');


                // 找到第一个非 0 数字
                int firstNumIndex_noZero = -1;
                for (int i = 0; i < value.Length; i++)
                {
                    if (value[i] >= '1' && value[i] <= '9')
                    {
                        firstNumIndex_noZero = i;
                        break;
                    }
                }
                if (firstNumIndex_noZero < 0)
                {
                    // 没有非0数值, 说明当前值是0
                    _setZero();
                    return;
                }

                byte[] integerBuffer;
                byte[] decimalBuffer;
                // 读取数值
                if (findPoint < 0)
                {
                    // 无小数点
                    byte[] temp = new byte[value.Length - firstNumIndex_noZero];
                    for (int i = firstNumIndex_noZero, j = 0; i < value.Length; i++, j++)
                    {
                        temp[j] = (byte)(value[i] - '0');
                    }
                    integerBuffer = temp;
                    decimalBuffer = [];
                }
                else
                {
                    // 有小数点
                    // 整数
                    if (findPoint > firstNumIndex_noZero)
                    {
                        // 小数点的位置在第一个非0数字之后
                        byte[] temp1 = new byte[findPoint - firstNumIndex_noZero];
                        for (int i = firstNumIndex_noZero, j = 0; i < findPoint; i++, j++)
                        {
                            temp1[j] = (byte)(value[i] - '0');
                        }
                        integerBuffer = temp1;
                    }
                    else
                    {
                        integerBuffer = [0];
                    }
                    // 小数
                    int firstDecimal = findPoint + 1;
                    byte[] temp2 = new byte[value.Length - firstDecimal];
                    for (int i = firstDecimal, j = 0; i < value.Length; i++, j++)
                    {
                        temp2[j] = (byte)(value[i] - '0');
                    }
                    decimalBuffer = temp2;

                }

                // 排除多余的数据
                integerBuffer = integerBuffer.TrimStart(0);
                decimalBuffer = decimalBuffer.TrimEnd(0);

                // 正 负 零 判断
                if (integerBuffer.Length == 0 && decimalBuffer.Length == 0)
                {
                    _setZero();
                    return;
                }
                else
                {
                    IsZero = false;
                }

                // 复制到结构体
                IntegerPart = integerBuffer;
                DecimalPart = decimalBuffer;

            }
        }

        public readonly string ConvertToString()
        {
            if (IsZero)
            {
                return "0";
            }
            int iPLength = IntegerPart?.Length ?? 0;
            int dPLength = DecimalPart?.Length ?? 0;
            StringBuilder sb = new(1 + iPLength + 1 + dPLength);
            if (!IsPositive)
            {
                sb.Append('-');
            }
            if (iPLength == 0)
            {
                sb.Append('0');
            }
            else
            {
                for (int i = 0; i < iPLength; i++)
                {
                    sb.Append((char)(IntegerPart![i] + '0'));
                }
            }
            if (dPLength == 0 || !DecimalPartProxy.Any(i => i > 0))
            {
            }
            else
            {
                sb.Append('.');
                for (int i = 0; i < dPLength; i++)
                {
                    sb.Append((char)(DecimalPartProxy[i] + '0'));
                }
            }
            return sb.ToString();
        }

        public override readonly string ToString()
        {
            return ConvertToString();
        }

        #endregion

        #region 拷贝
        /// <summary>
        /// 拷贝为一个新的数据 (会分配内存)
        /// </summary>
        /// <returns></returns>
        public readonly DecFixedPointNumber Clone()
        {
            return new DecFixedPointNumber()
            {
                IsZero = IsZero,
                IsPositive = IsPositive,
                DecimalPart = (byte[])DecimalPartProxy.Clone(),
                IntegerPart = (byte[])IntegerPartProxy.Clone(),
            };
        }

        readonly object ICloneable.Clone()
        {
            return Clone();
        }

        #endregion

        #region 运算
        /// <summary>
        /// 判断当前长度是否小于输入值. 输入值为null时, 对应部分不做检查. 当前值为 0 时, 总是返回 true
        /// </summary>
        /// <param name="integerPaet"></param>
        /// <param name="decimalPart"></param>
        /// <returns></returns>
        public bool LengthSmallThen(int? integerPaet, int? decimalPart)
        {
            if (IsZero) return true;
            if (integerPaet != null)
            {
                if (integerPaet.Value < 1)
                {
                    integerPaet = 1;
                }
                if (integerPaet.Value < IntegerPartProxy.Length)
                {
                    return false;
                }
            }
            if (decimalPart != null)
            {
                if (decimalPart.Value < 0)
                {
                    decimalPart = 0;
                }
                if (decimalPart.Value < DecimalPartProxy.Length)
                {
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// 根据传入参数裁剪整数部分或小数部分的长度, 将裁剪掉距离小数点较远一端的数据, 使其长度不超过传入值
        /// </summary>
        /// <param name="integerPart">整数部分最高位距离小数点的距离, 允许的最小值: 0; 传入null表示不限制</param>
        /// <param name="decimalPart">小数部分最低位距离小数点的距离, 允许的最小值: 0; 传入null表示不限制</param>
        /// <param name="integerPartSubMode">整数部分的切割模式 (保留哪一部分) (低位一端: 尾部)</param>
        /// <param name="decimalPartSubMode">小数部分的切割模式 (保留哪一部分) (高位一端: 首部)</param>
        /// <returns>会分配内存</returns>
        public DecFixedPointNumber LimitMaxLength(int? integerPart, int? decimalPart,
            HeadTailEnum integerPartSubMode = HeadTailEnum.Tail, HeadTailEnum decimalPartSubMode = HeadTailEnum.Head)
        {
            if (integerPart == null && decimalPart == null)
            {
                return Clone();
            }
            else if (integerPart != null && integerPart.Value == 0 && decimalPart != null && decimalPart.Value == 0)
            {
                return Zero;
            }
            else
            {
                byte[] integerBuffer;
                byte[] decimalBuffer;

                if (integerPart != null && integerPart.Value < IntegerPartProxy.Length)
                {
                    integerBuffer = IntegerPartProxy.Sub(integerPart.Value, integerPartSubMode);
                }
                else
                {
                    integerBuffer = IntegerPartProxy;
                }

                if (decimalPart != null && decimalPart.Value < DecimalPartProxy.Length)
                {
                    decimalBuffer = DecimalPartProxy.Sub(decimalPart.Value, decimalPartSubMode);
                }
                else
                {
                    decimalBuffer = DecimalPartProxy;
                }

                integerBuffer = integerBuffer.TrimStart();
                decimalBuffer = decimalBuffer.TrimEnd();
                if (integerBuffer.Length == 0 && decimalBuffer.Length == 0)
                {
                    return Zero;
                }
                else
                {
                    return new()
                    {
                        IsZero = false,
                        IsPositive = IsPositive,
                        DecimalPart = decimalBuffer,
                        IntegerPart = integerBuffer,
                    };
                }
            }
        }
        #endregion

        #region 比较
        public override readonly bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is DecFixedPointNumber number)
            {
                if (number.IsZero && IsZero) return true;
                if (number.IsPositive != IsPositive) return false;
                int ipLength = IntegerPart?.Length ?? 0;
                int dpLength = DecimalPart?.Length ?? 0;
                int nipLength = number.IntegerPart?.Length ?? 0;
                int ndpLength = number.DecimalPart?.Length ?? 0;
                if (ipLength != nipLength) return false;
                if (dpLength != ndpLength) return false;

                for (int i = 0; i < ipLength; i++)
                {
                    if (number.IntegerPartProxy[i] != IntegerPartProxy[i]) return false;
                }
                for (int i = 0; i < dpLength; i++)
                {
                    if (number.DecimalPartProxy[i] != DecimalPartProxy[i]) return false;
                }
                return true;
            }
            return base.Equals(obj);
        }


        /// <summary>
        /// GetHashCode计算参数: 长度取余时使用的参数
        /// </summary>
        const int _LENGTH_MOD_ARG = 16;
        /// <summary>
        /// GetHashCode计算参数: 对应位的计算比例
        /// </summary>
        static int[] _scale = [1, 10, 100];
        public override readonly int GetHashCode()
        {
            if (IsZero) { return 0; }
            int output = 0b_0000_0000_0000_0000_0000_0000_0000_0001;    // 是否0, 32位
            output &= IsPositive ? (1 << 31) : (0);  // 是否正, 1位

            int ipLength = IntegerPart?.Length ?? 0;
            int dpLength = DecimalPart?.Length ?? 0;

            int mod_integer = ipLength % _LENGTH_MOD_ARG; // 长度取余
            int mod_decimal = dpLength % _LENGTH_MOD_ARG;
            output &= (mod_integer << 27);  // 第2~5位
            output &= (mod_decimal << 23);  // 第6~9位

            int integerTop3Sum = 0; // 最大值2进制长度: 10位
            int decimalTop3Sum = 0;
            for (int i = 0; i < 3; i++)
            {
                integerTop3Sum += _scale[i] * (ipLength > i ? IntegerPartProxy[i] : 0);
                decimalTop3Sum += _scale[i] * (dpLength > i ? DecimalPartProxy[i] : 0);
            }

            output &= (mod_integer << 12);  // 第10~20位
            output &= (mod_decimal << 1);  // 第21~31位

            return output;
        }


        #endregion

        #region 隐式转换 (不会损失精度)
        public static implicit operator DecFixedPointNumber(int i)
        {
            return Convert(i, 0);
        }
        public static implicit operator DecFixedPointNumber(float f)
        {
            return StringConveyingHelper.FromString<DecFixedPointNumber>(f.NoScientificNotationString());
        }
        public static implicit operator DecFixedPointNumber(double d)
        {
            return StringConveyingHelper.FromString<DecFixedPointNumber>(d.NoScientificNotationString());
        }
        public static implicit operator string(DecFixedPointNumber number)
        {
            return number.ToString();
        }
        #endregion
        #region 显式转换 (可能会失败, 或者精度可能会损失)
        public static explicit operator DecFixedPointNumber(string number)
        {
            var output = new DecFixedPointNumber();
            output.ChangeValue(number);
            return output;
        }
        public static explicit operator int(DecFixedPointNumber number)
        {
            if (number.IsZero || number.IntegerPartProxy.Length == 0) return 0;
            if (number.IntegerPartLength > IntCalcUtil.Pow10MaxIndex + 1)
            {
                // 必定超过最大值/最小值
                return number.IsPositive ? int.MaxValue : int.MinValue;
            }
            else
            {
                if (number.IntegerPartLength < IntCalcUtil.Pow10MaxIndex + 1)
                {
                    // 不可能超过最大值/最小值的情况
                    int output = 0;
                    int a;
                    int power;
                    for (int i = 0; i < number.IntegerPartProxy.Length; i++) // 从高位到低位遍历
                    {
                        a = number.IntegerPartProxy[i];  // 对应位的值
                        power = IntCalcUtil.Pow10(number.IntegerPartProxy.Length - i - 1);   // 当前对应位的倍数
                        output += power * a;
                    }
                    return number.IsPositive ? output : -output;
                }
                else
                {
                    // 可能超过最大值/最小值的情况
                    int output = 0;
                    unchecked
                    {
                        int a;
                        int power;
                        int temp;
                        if (number.IntegerPartProxy[0] > 2)
                        {
                            // 这种情况下, temp 就会超过最大值了
                            return number.IsPositive ? int.MaxValue : int.MinValue;
                        }
                        for (int i = 0; i < number.IntegerPartProxy.Length; i++) // 从高位到低位遍历
                        {
                            a = number.IntegerPartProxy[i];  // 对应位的值
                            power = IntCalcUtil.Pow10(number.IntegerPartProxy.Length - i - 1);   // 当前对应位的倍数

                            temp = power * a;
                            if (number.IsPositive)
                            {
                                if (output + temp - temp != output)
                                {
                                    return int.MaxValue;
                                }
                                else
                                {
                                    output += temp;
                                }
                            }
                            else
                            {
                                if (output - temp + temp != output)
                                {
                                    return int.MinValue;
                                }
                                else
                                {
                                    output -= temp;
                                }
                            }
                        }
                    }

                    return output;
                }
            }
        }
        public static explicit operator long(DecFixedPointNumber number)
        {
            throw new NotImplementedException();
        }
        public static explicit operator float(DecFixedPointNumber number)
        {
            return float.Parse(number.ToString());
        }
        public static explicit operator double(DecFixedPointNumber number)
        {
            return double.Parse(number.ToString());
        }

        #endregion


        #region 运算符重载
        public static bool operator ==(DecFixedPointNumber left, DecFixedPointNumber right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DecFixedPointNumber left, DecFixedPointNumber right)
        {
            return !(left == right);
        }
        #endregion
    }
}
