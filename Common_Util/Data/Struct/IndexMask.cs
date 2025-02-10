using Common_Util.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Struct
{
    /// <summary>
    /// 索引值遮罩, 固定长度, 相当于一个 <see cref="bool"/>[]
    /// </summary>
    /// <remarks>
    /// 此结构体允许修改值, 在用于字典, 或其他需要其 HashCode 的场景时, 需注意不要修改到它的值! 
    /// </remarks>
    public struct IndexMask : IEnumerable<bool>, ICloneable, IEquatable<IndexMask>  
    {
        #region 常量/静态值
        /// <summary>
        /// 表示所有位都是 <see langword="true"/> 的字节
        /// </summary>
        private const byte ALL_TRUE_BYTE = 0b_1111_1111;
        /// <summary>
        /// 表示所有位都是 <see langword="false"/> 的字节
        /// </summary>
        private const byte ALL_FALSE_BYTE = 0b_0;
        #endregion

        #region 构造函数
        /// <summary>
        /// 实例化为特定长度, 特定数据容器大小的实例, 用于内部静态方法创建实例
        /// </summary>
        /// <param name="length"></param>
        /// <param name="byteArrSize"></param>
        private IndexMask(int length, int byteArrSize)
        {
            this.length = length;
            this.bytes = new byte[byteArrSize];
        }

        /// <summary>
        /// 实例化一个遮罩
        /// </summary>
        /// <param name="length">期望覆盖大小</param>
        /// <param name="defaultState">默认状态</param>
        public IndexMask(int length, bool defaultState = false)
        {
            this.length = length;
            if (length == 0)
            {
                bytes = [];
            }
            else
            {
                int arrSize = (length - 1) / 8 + 1;
                bytes = new byte[arrSize];
                if (defaultState)
                {
                    for (int i = 0; i < arrSize; i++)
                    {
                        bytes[i] = ALL_TRUE_BYTE;
                    }
                    int endByteBitCount = (length & 0b_111);
                    if (endByteBitCount > 0)
                    {
                        bytes[arrSize - 1] = BitHelper.BitLittleRange[endByteBitCount];
                    }
                }
                else
                {
                    for (int i = 0; i < arrSize; i++)
                    {
                        bytes[i] = ALL_FALSE_BYTE;
                    }
                }

            }
        }
        /// <summary>
        /// 通过 <paramref name="collection"/> 实例化一个遮罩, 其长度与集合相等
        /// </summary>
        /// <param name="collection"></param>
        public IndexMask(ICollection<bool> collection)
        {
            this.length = collection.Count;

            int arrSize = (length - 1) / 8 + 1;
            bytes = new byte[arrSize];

            byte temp = 0;
            int tempIndex = 0;
            foreach (var (index, b) in collection.WithIndex())
            {
                if (b)
                {
                    temp |= BitHelper.BitLocation[tempIndex];
                }
                tempIndex = (tempIndex + 1) & 0b111;
                if (tempIndex == 0) // 7 => 0 时
                {
                    bytes[index >> 3] = temp;
                    temp = 0;
                }
            }
            if (tempIndex != 0)
            {
                bytes[arrSize - 1] = temp;
            }
        }

        /// <summary>
        /// 通过 <paramref name="bools"/> 实例化一个遮罩, 其长度与集合相等
        /// </summary>
        /// <param name="bools"></param>
        public IndexMask(IEnumerable<bool> bools)
        {
            int length = 0;
            List<byte> bytes = [];

            byte temp = 0;
            int tempIndex = 0;
            foreach (var (index, b) in bools.WithIndex())
            {
                if (b)
                {
                    temp |= BitHelper.BitLocation[tempIndex];
                }
                tempIndex = (tempIndex + 1) & 0b111;
                if (tempIndex == 0) // 7 => 0 时
                {
                    bytes.Add(temp);
                    temp = 0;
                }
                length++;
            }
            if (tempIndex != 0)
            {
                bytes.Add(temp);
            }

            this.length = length;
            this.bytes = [.. bytes];
        }

        /// <summary>
        /// 通过 <paramref name="bools"/> 实例化一个遮罩, 其长度为 <see cref="length"/>
        /// </summary>
        /// <param name="bools"></param>
        /// <param name="length"></param>
        /// <param name="overMask">如果 <paramref name="length"/> 大于 <paramref name="bools"/> 的数量, 则取此值</param>
        public IndexMask(IEnumerable<bool> bools, int length, bool overMask = false)
        {
            this.length = length;

            int arrSize = (length - 1) / 8 + 1;
            bytes = new byte[arrSize];

            byte temp = 0;
            int tempIndex = 0;
            int index = 0;
            foreach (var b in bools)
            {
                if (index >= length) break;

                if (b)
                {
                    temp |= BitHelper.BitLocation[tempIndex];
                }
                tempIndex = (tempIndex + 1) & 0b111;
                if (tempIndex == 0) // 7 => 0 时
                {
                    bytes[index >> 3] = temp;
                    temp = 0;
                }

                index++;
            }
            int prevArrIndex = (index - 1) >> 3;    // 遍历结束时最后一项在 byte[] 中的索引
            if (tempIndex != 0)
            {
                if (overMask)   // 只有需要设置为 true 的情况下需要作这些操作, 因为默认情况下就会是 false
                {
                    while (index < length && tempIndex != 0)
                    {
                        temp |= BitHelper.BitLocation[tempIndex];
                        tempIndex = (tempIndex + 1) & 0b111;
                        index++;
                    }
                }
                bytes[prevArrIndex] = temp;
            }
            for (int i = prevArrIndex + 1; i < arrSize; i++)
            {
                bytes[i] = overMask ? ALL_TRUE_BYTE : ALL_FALSE_BYTE;
            }

        }
        #endregion

        #region 静态的创建方法
        /// <summary>
        /// 通过一组字符创建 <see cref="IndexMask"/>, 识别其中的字符是否在 <paramref name="flagChar"/> 中, 如果在, 则 <see langword="true"/>
        /// </summary>
        /// <param name="chars"></param>
        /// <param name="flagChar"></param>
        /// <returns></returns>
        public static IndexMask FromTrueChar(IEnumerable<char> chars, params char[] flagChar)
            => new(chars.Select(c => flagChar.Contains(c)));
        /// <summary>
        /// 通过一组字符创建 <see cref="IndexMask"/>, 识别其中的字符是否在 <paramref name="flagChar"/> 中, 如果在, 则 <see langword="false"/>
        /// </summary>
        /// <param name="chars"></param>
        /// <param name="flagChar"></param>
        /// <returns></returns>
        public static IndexMask FromFalseChar(IEnumerable<char> chars, params char[] flagChar)
            => new(chars.Select(c => flagChar.Contains(c)));


        #endregion

        #region 数据

        /// <summary>
        /// 遮罩各项值的存储容器
        /// </summary>
        /// <remarks>
        /// 在 <see langword="byte"/>[] 中, 数组索引值小的部分对应遮罩索引值小的值, <br/>
        /// <see langword="byte"/> 低位的部分对应遮罩索引较小的值
        /// </remarks>
        private readonly byte[] bytes;
        /// <summary>
        /// 原始数据
        /// </summary>
        public readonly byte[] Datas => bytes ?? [];

        private readonly int length;
        /// <summary>
        /// 维护长度
        /// </summary>
        public readonly int Length => length;

        #endregion

        #region 取值赋值
        /// <summary>
        /// 获取或设置遮罩在索引 <paramref name="index"/> 位置的值
        /// </summary>
        /// <remarks>注: <see langword="set"/> 操作时, 数组引用不会变更, 但是数组元素会被更改, HashCode 也会产生变更! </remarks>
        /// <param name="index"></param>
        /// <returns></returns>
        public readonly bool this[int index]
        {
            get
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(index, 0);
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, length);
                return (bytes[index >> 3] & BitHelper.BitLocation[index & 0b_111]) > 0;
            }
            set
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(index, 0);
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, length);
                int subIndex = index >> 3;
                byte b = bytes[subIndex];
                byte mask = BitHelper.BitLocation[index & 0b_111];
                bytes[subIndex] = (byte)((b & (~mask)) | (value ? mask : 0));
            }
        }

        /// <summary>
        /// 将当前数据所有位上的值取反
        /// </summary>
        /// <remarks>注: 数组引用不会变更, 但是数组元素会被更改, HashCode 也会产生变更! </remarks>
        public readonly void Reverse()
        {
            for (int i = 0; i < bytes.Length - 1; i++)
            {
                bytes[i] = (byte)(~bytes[i]);
            }
            int lastByteBitCount = length & 0b_111;
            if (lastByteBitCount > 0)
            {
                bytes[^1] = (byte)((~bytes[^1]) & BitHelper.BitLittleRange[lastByteBitCount]);
            }
            else
            {
                bytes[^1] = (byte)(~bytes[^1]); // 这种情况说明取了 8 位
            }
        }

        /// <summary>
        /// 将遮罩的所有值都设置为 <paramref name="value"/>
        /// </summary>
        /// <param name="value"></param>
        public readonly void Clear(bool value)
        {
            int bCount = Length / 8;

            byte setValue = value ? ALL_TRUE_BYTE : ALL_FALSE_BYTE;
            for (int i = 0; i < bCount; i++)
            {
                Datas[i] = setValue;
            }
            int bOver = Length % 8;
            if (bOver > 0)
            {
                byte lastSetValue = value ? BitHelper.BitLittleRange[bOver] : ALL_FALSE_BYTE;
                Datas[bCount] = lastSetValue;
            }
        }

        #endregion

        #region 判断
        /// <summary>
        /// 判断全部值是不是都是 <paramref name="value"/>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public readonly bool IsAll(bool value)
        {
            int bCount = Length / 8;
            
            byte check = value ? ALL_TRUE_BYTE : ALL_FALSE_BYTE;
            for (int i = 0; i < bCount; i++)
            {
                byte b = Datas[i];
                if (b != check)
                {
                    return false;
                }
            }
            int bOver = Length % 8;
            if (bOver > 0)
            {
                byte b = Datas[bCount];
                byte lastCheck = value ? BitHelper.BitLittleRange[bOver] : ALL_FALSE_BYTE;
                int compareValue = b & BitHelper.BitLittleRange[bOver];
                if (lastCheck != compareValue)
                {
                    return false;
                }
            }
            return true;
        }
        #endregion

        #region 遍历
        /// <summary>
        /// 从根据参数, 从低位或高位开始遍历遮罩
        /// </summary>
        /// <param name="desc"><see langword="true"/> 时, 从高位开始遍历</param>
        /// <returns></returns>
        public readonly IEnumerable<bool> All(bool desc = false)
        {
            if (!desc)
            {
                for (int i = 0; i < Length; i++)
                {
                    yield return (bytes[i >> 3] & BitHelper.BitLocation[i & 0b_111]) > 0;
                }
            }
            else
            {
                for (int i = length - 1; i >= 0; i--)
                {
                    yield return (bytes[i >> 3] & BitHelper.BitLocation[i & 0b_111]) > 0;
                }
            }
        }
        #endregion

        #region 实现接口: IEnumerable<bool>
        /// <summary>
        /// 取得遍历器, 其遍历方式为 <see cref="All(bool)"/> 方法, 使用参数 <see langword="false"/>
        /// </summary>
        /// <returns></returns>
        public readonly IEnumerator<bool> GetEnumerator()
        {
            return All(false).GetEnumerator();
        }

        readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion

        #region 转换为字符串
        /// <summary>
        /// 以 <see langword="false"/> 为参数, '1' 为 <see langword="true"/> 位字符, '0' 为 <see langword="false"/> 位字符, 调用 <see cref="ToString(bool)"/> 取得字符串
        /// </summary>
        /// <returns></returns>
        public readonly override string ToString()
        {
            return ToString(false);
        }
        /// <summary>
        /// 取得简单描述该遮罩的字符串 (过长的情况下省略中间的一些位)
        /// </summary>
        /// <param name="desc"><see langword="true"/> 时, 遮罩值的高位放在前面</param>
        /// <returns></returns>
        public readonly string ToString(bool desc = false, char trueChar = '1', char falseChar = '0')
        {
            const int 阈值 = 30;
            const string 省略号 = "...";

            string body;
            if (Length > 阈值)
            {
                int 省略量 = Length - (阈值 - 省略号.Length * 2);
                int left = (阈值 - 省略号.Length * 2) / 2;
                int right = (Length - 省略量) - left;
                int rightStart = length - right;
                char[] cLeft = new char[left];
                char[] cRight = new char[right];
                foreach (var (index, b) in All(desc).WithIndex())
                {
                    if (index < left)
                    {
                        cLeft[index] = b ? trueChar : falseChar;
                    }
                    if (index >= rightStart)
                    {
                        cRight[index - rightStart] = b ? trueChar : falseChar;
                    }
                }
                body = $"{new string(cLeft)}{省略号}{省略量}{省略号}{new string(cRight)}";
            }
            else
            {
                char[] cs = new char[Length];
                foreach (var (index, b) in All(desc).WithIndex())
                {
                    cs[index] = b ? trueChar : falseChar;
                }
                body = new string(cs);
            }
            return $"{nameof(IndexMask)}_{Length}_{body}";
        }

        /// <summary>
        /// 取得完整描述该遮罩的字符串, 类似于 <see cref="ToString(bool)"/>, 但是不再省略
        /// </summary>
        /// <param name="desc"><see langword="true"/> 时, 遮罩值的高位放在前面</param>
        /// <returns></returns>
        public readonly string ToFullString(bool desc = false, char trueChar = '1', char falseChar = '0')
        {
            char[] cs = new char[Length];
            foreach (var (index, b) in All(desc).WithIndex())
            {
                cs[index] = b ? trueChar : falseChar;
            }
            return new string(cs);
        }
        #endregion

        #region 隐式转换
        /// <summary>
        /// 将 <see langword="ulong"/> 值按位转换为长度为 64 的 <see cref="IndexMask"/>
        /// </summary>
        /// <remarks>
        /// 将 <paramref name="value"/> 作为二进制值, 从小端起, 第 n 位对应遮罩中的第 n 位, 即数值中低位值对应遮罩索引较小的值
        /// </remarks>
        /// <param name="value"></param>
        public static implicit operator IndexMask(ulong value)
        {
            IndexMask mask = new IndexMask(64);
            byte[] bs = BitConverter.GetBytes(value);
            for (int i = 0; i < bs.Length; i++)
            {
                mask.bytes[i] = bs[i];
            }
            return mask;
        }
        /// <summary>
        /// 将 <see langword="uint"/> 值按位转换为长度为 32 的 <see cref="IndexMask"/>
        /// </summary>
        /// <remarks>
        /// 将 <paramref name="value"/> 作为二进制值, 从小端起, 第 n 位对应遮罩中的第 n 位, 即数值中低位值对应遮罩索引较小的值
        /// </remarks>
        /// <param name="value"></param>
        public static implicit operator IndexMask(uint value)
        {
            IndexMask mask = new IndexMask(32);
            byte[] bs = BitConverter.GetBytes(value);
            for (int i = 0; i < bs.Length; i++)
            {
                mask.bytes[i] = bs[i];
            }
            return mask;
        }
        /// <summary>
        /// 将 <see langword="ushort"/> 值按位转换为长度为 16 的 <see cref="IndexMask"/>
        /// </summary>
        /// <remarks>
        /// 将 <paramref name="value"/> 作为二进制值, 从小端起, 第 n 位对应遮罩中的第 n 位, 即数值中低位值对应遮罩索引较小的值
        /// </remarks>
        /// <param name="value"></param>
        public static implicit operator IndexMask(ushort value)
        {
            IndexMask mask = new IndexMask(16);
            byte[] bs = BitConverter.GetBytes(value);
            for (int i = 0; i < bs.Length; i++)
            {
                mask.bytes[i] = bs[i];
            }
            return mask;
        }
        /// <summary>
        /// 将 <see langword="byte"/> 值按位转换为长度为 8 的 <see cref="IndexMask"/>
        /// </summary>
        /// <remarks>
        /// 将 <paramref name="value"/> 作为二进制值, 从小端起, 第 n 位对应遮罩中的第 n 位, 即数值中低位值对应遮罩索引较小的值
        /// </remarks>
        /// <param name="value"></param>
        public static implicit operator IndexMask(byte value)
        {
            IndexMask mask = new IndexMask(8);
            mask.bytes[0] = value;
            return mask;
        }

        /// <summary>
        /// 将 <paramref name="bs"/> 值按位转换为长度为与该数组长度的 <see cref="IndexMask"/>
        /// </summary>
        /// <param name="bs"></param>
        public static implicit operator IndexMask(bool[] bs)
        {
            return new(bs);
        }
        #endregion

        #region 克隆
        /// <summary>
        /// 克隆一个相同的实例
        /// </summary>
        /// <returns></returns>
        public readonly IndexMask Clone()
        {
            IndexMask mask = new IndexMask(length, this.bytes.Length);
            for (int i = 0; i < this.bytes.Length; i++)
            {
                mask.bytes[i] = this.bytes[i];
            }
            return mask;
        }
        /// <summary>
        /// 调用 <see cref="Clone"/> 克隆一个相同的实例
        /// </summary>
        /// <returns></returns>
        readonly object ICloneable.Clone()
        {
            return Clone();
        }

        #endregion


        #region 位运算
        /// <summary>
        /// 逐位作或运算, 返回一个新的遮罩, 长度为两者较长的一方, 较长者比较短者长出来的部分, 会直接取较长者的值
        /// </summary>
        /// <param name="m1"></param>
        /// <param name="m2"></param>
        /// <returns></returns>
        public static IndexMask operator |(IndexMask m1, IndexMask m2)
        {
            bool m1Longer = m1.length > m2.length;
            byte[] longerBytes = m1Longer ? m1.bytes : m2.bytes;
            byte[] shorterBytes = m1Longer ? m2.bytes : m1.bytes;
            int longer = m1Longer ? m1.length : m2.length;
            int shorter = m1Longer ? m2.length : m1.length;
            int longerEndByteBitCount = longer & 0b_111;
            int shorterEndByteBitCount = shorter & 0b_111;
            int longerFullByteCount = longer >> 3;
            int shorterFullByteCount = shorter >> 3;

            IndexMask output = new(longer, longerBytes.Length);

            int byteIndex = 0;
            while (byteIndex < shorterFullByteCount)
            {
                output.bytes[byteIndex] = (byte)(m1.bytes[byteIndex] | m2.bytes[byteIndex]);
                byteIndex++;
            }
            if (shorterFullByteCount == longerFullByteCount)
            {
                // 占据完整字节的部分长度相等
                if (longerEndByteBitCount > 0)
                {
                    int longerValue = longerBytes[byteIndex] & BitHelper.BitLittleRange[longerEndByteBitCount];
                    int shortValue = 
                        shorterEndByteBitCount == 0 ?
                        0
                        : 
                        (shorterBytes[byteIndex] & BitHelper.BitLittleRange[shorterEndByteBitCount]);
                    output.bytes[byteIndex] = (byte)(longerValue | shortValue);
                }
                else
                {
                    // 较长者最后一个 byte 也占满了, 所以总长度相等, 已经没有数据了
                }
            }
            else
            {
                // 较短者占据完整 byte 的最后一个 byte 的下一个 byte
                {
                    int longerValue = longerBytes[byteIndex];
                    int shortValue =
                        shorterEndByteBitCount == 0 ?
                        0
                        :
                        (shorterBytes[byteIndex] & BitHelper.BitLittleRange[shorterEndByteBitCount]);
                    output.bytes[byteIndex] = (byte)(longerValue | shortValue);
                    byteIndex++;
                }
                // 剩余较长者的 byte
                while (byteIndex < longerFullByteCount)
                {
                    output.bytes[byteIndex] = longerBytes[byteIndex];
                    byteIndex++;
                }
                if (longerEndByteBitCount > 0)
                {
                    output.bytes[byteIndex] = (byte)(longerBytes[byteIndex] & BitHelper.BitLittleRange[longerEndByteBitCount]);
                }

            }

            return output;
        }

        /// <summary>
        /// 逐位作与运算, 返回一个新的遮罩, 长度为两者较长的一方, 较长者比较短者长出来的部分, 会直接取 <see langword="false"/>
        /// </summary>
        /// <param name="m1"></param>
        /// <param name="m2"></param>
        /// <returns></returns>
        public static IndexMask operator &(IndexMask m1, IndexMask m2)
        {
            bool m1Longer = m1.length > m2.length;
            byte[] longerBytes = m1Longer ? m1.bytes : m2.bytes;
            byte[] shorterBytes = m1Longer ? m2.bytes : m1.bytes;
            int longer = m1Longer ? m1.length : m2.length;
            int shorter = m1Longer ? m2.length : m1.length;
            int longerEndByteBitCount = longer & 0b_111;
            int shorterEndByteBitCount = shorter & 0b_111;
            int longerFullByteCount = longer >> 3;
            int shorterFullByteCount = shorter >> 3;

            IndexMask output = new(longer, longerBytes.Length);

            int byteIndex = 0;
            while (byteIndex < shorterFullByteCount)
            {
                output.bytes[byteIndex] = (byte)(m1.bytes[byteIndex] & m2.bytes[byteIndex]);
                byteIndex++;
            }
            if (shorterFullByteCount == longerFullByteCount)
            {
                // 占据完整字节的部分长度相等
                if (longerEndByteBitCount > 0)
                {
                    int longerValue = longerBytes[byteIndex] & BitHelper.BitLittleRange[shorterEndByteBitCount];
                    int shortValue =
                        shorterEndByteBitCount == 0 ?
                        0
                        :
                        (shorterBytes[byteIndex] & BitHelper.BitLittleRange[shorterEndByteBitCount]);
                    output.bytes[byteIndex] = (byte)((longerValue & shortValue));   // 较长者长出较短者的部分, 都取为 0, 即 false 
                }
                else
                {
                    // 较长者最后一个 byte 也占满了, 所以总长度相等, 已经没有数据了
                }
            }
            else
            {
                // 较短者占据完整 byte 的最后一个 byte 的下一个 byte
                {
                    int longerValue = longerBytes[byteIndex] & BitHelper.BitLittleRange[shorterEndByteBitCount];
                    int shortValue =
                        shorterEndByteBitCount == 0 ?
                        0
                        :
                        (shorterBytes[byteIndex] & BitHelper.BitLittleRange[shorterEndByteBitCount]);
                    output.bytes[byteIndex] = (byte)((longerValue & shortValue));   // 较短者占据字节的剩余部分, 都取为 0, 即 false
                    byteIndex++;
                }
                // 剩余较长者的 byte, 全部取 0, 即 false
                while (byteIndex < longerFullByteCount)
                {
                    output.bytes[byteIndex] = ALL_FALSE_BYTE;
                    byteIndex++;
                }
                if (longerEndByteBitCount > 0)
                {
                    output.bytes[byteIndex] = ALL_FALSE_BYTE;
                }

            }

            return output;
        }

        /// <summary>
        /// 逐位作异或运算, 返回一个新的遮罩, 长度为两者较长的一方, 较长者比较短者长出来的部分, 会直接取 <see langword="true"/>
        /// </summary>
        /// <param name="m1"></param>
        /// <param name="m2"></param>
        /// <returns></returns>
        public static IndexMask operator ^(IndexMask m1, IndexMask m2)
        {
            bool m1Longer = m1.length > m2.length;
            byte[] longerBytes = m1Longer ? m1.bytes : m2.bytes;
            byte[] shorterBytes = m1Longer ? m2.bytes : m1.bytes;
            int longer = m1Longer ? m1.length : m2.length;
            int shorter = m1Longer ? m2.length : m1.length;
            int longerEndByteBitCount = longer & 0b_111;
            int shorterEndByteBitCount = shorter & 0b_111;
            int longerFullByteCount = longer >> 3;
            int shorterFullByteCount = shorter >> 3;

            IndexMask output = new(longer, longerBytes.Length);

            int byteIndex = 0;
            while (byteIndex < shorterFullByteCount)
            {
                output.bytes[byteIndex] = (byte)(m1.bytes[byteIndex] ^ m2.bytes[byteIndex]);
                byteIndex++;
            }
            if (shorterFullByteCount == longerFullByteCount)
            {
                // 占据完整字节的部分长度相等
                if (longerEndByteBitCount > 0)
                {
                    int longerValue = longerBytes[byteIndex] & BitHelper.BitLittleRange[shorterEndByteBitCount];
                    int shortValue =
                        shorterEndByteBitCount == 0 ?
                        0
                        :
                        (shorterBytes[byteIndex] & BitHelper.BitLittleRange[shorterEndByteBitCount]);
                    output.bytes[byteIndex] = (byte)(
                        (longerValue ^ shortValue) 
                        | (BitHelper.BitLittleRange[longerEndByteBitCount] ^ BitHelper.BitLittleRange[shorterEndByteBitCount])  // 较长者长出较短者的部分, 都取为 1, 即 true
                        );
                }
                else
                {
                    // 较长者最后一个 byte 也占满了, 所以总长度相等, 已经没有数据了
                }
            }
            else
            {
                // 较短者占据完整 byte 的最后一个 byte 的下一个 byte
                {
                    int longerValue = longerBytes[byteIndex] & BitHelper.BitLittleRange[shorterEndByteBitCount];
                    int shortValue =
                        shorterEndByteBitCount == 0 ?
                        0
                        :
                        (shorterBytes[byteIndex] & BitHelper.BitLittleRange[shorterEndByteBitCount]);
                    output.bytes[byteIndex] = (byte)(
                        (longerValue ^ shortValue)
                        | (~BitHelper.BitLittleRange[shorterEndByteBitCount])   // 较短者占据字节的剩余部分, 都取为 1, 即 true
                        );
                    byteIndex++;
                }
                // 剩余较长者的 byte, 全部取 1, 即 true
                while (byteIndex < longerFullByteCount)
                {
                    output.bytes[byteIndex] = ALL_TRUE_BYTE;
                    byteIndex++;
                }
                if (longerEndByteBitCount > 0)
                {
                    output.bytes[byteIndex] = BitHelper.BitLittleRange[longerEndByteBitCount];
                }

            }

            return output;
        }

        /// <summary>
        /// 按位求补运算, 返回一个新的遮罩, 长度与 <paramref name="mask"/> 相等, 其中各位与 <paramref name="mask"/> 均相反
        /// </summary>
        /// <param name="mask"></param>
        /// <returns></returns>
        public static IndexMask operator ~(IndexMask mask)
        {
            IndexMask output = new(mask.length, mask.bytes.Length);

            for (int i = 0; i < mask.bytes.Length - 1; i++)
            {
                output.bytes[i] = (byte)(~mask.bytes[i]);
            }
            int lastByteBitCount = mask.length & 0b_111;
            if (lastByteBitCount > 0)
            {
                output.bytes[^1] = (byte)((~mask.bytes[^1]) & BitHelper.BitLittleRange[lastByteBitCount]);
            }
            else
            {
                output.bytes[^1] = (byte)(~mask.bytes[^1]); // 这种情况说明取了 8 位
            }

            return output;
        }

        /// <summary>
        /// 左移运算, 返回一个新的遮罩, 长度与 <paramref name="mask"/> 相等, 使用 <see langword="false"/> 值补移动造成的空位
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="count">移动位数</param>
        /// <returns></returns>
        public static IndexMask operator >>(IndexMask mask, int count)
        {
            if (count == 0)
            {
                return mask.Clone();
            }
            else if (count < 0)
            {
                return mask << -count;  // 负数移动, 转为左移
            }
            if (count > mask.length)
            {
                // 移出了输入遮罩的范围
                return new IndexMask(mask.length, false);
            }

            IndexMask output = new(mask.length, mask.bytes.Length);

            if ((count & 0b_111) == 0)
            {
                // 移动恰好对齐字节
                int offsetByteCount = count >> 3;
                int endBitCount = mask.length & 0b_111;
                for (int i = 0; i < mask.bytes.Length; i++)
                {
                    int target = i - offsetByteCount;
                    if (target < 0)
                    {
                        output.bytes[i] = ALL_FALSE_BYTE;
                    }
                    else if (endBitCount > 0 && i == mask.bytes.Length - 1)
                    {
                        // 不整字节, 且当前正在写最后一个字节
                        output.bytes[i] = (byte)(mask.bytes[target] & BitHelper.BitLittleRange[endBitCount]);
                    }
                    else
                    {
                        output.bytes[i] = mask.bytes[target];
                    }
                }
            }
            else
            {
                int emptyByteCount = count >> 3;
                int endEmptyBitCount = count & 0b_111;
                // 对应移动前处于有效范围外的 byte, 全部设置为 false 即可
                for (int i = 0; i < emptyByteCount; i++)
                {
                    output.bytes[i] = ALL_FALSE_BYTE;
                }
                byte temp = 0;  // 首个 byte 的前 endEmptyBitCount 都应该是 false,
                                // 剩余部分暂时还是初始值 false,
                                // 所以目前就是 0  
                int bitIndex = endEmptyBitCount;
                int byteIndex = emptyByteCount;
                int index = count;
                while (index < mask.length)
                {
                    int target = index - count;
                    if ((mask.bytes[target >> 3] & BitHelper.BitLocation[target & 0b_111]) > 0)
                    {
                        temp |= BitHelper.BitLocation[bitIndex];
                    }
                    bitIndex = (bitIndex + 1) & 0b_111;
                    if (bitIndex == 0) 
                    {
                        output.bytes[byteIndex] = temp;
                        temp = 0;
                        byteIndex++;
                    }
                    index++;
                }
                if (bitIndex != 0)
                {
                    output.bytes[byteIndex] = temp;
                }

            }

            return output;
        }
        public static IndexMask operator <<(IndexMask mask, int count)
        {
            if (count == 0)
            {
                return mask.Clone();
            }
            else if (count < 0)
            {
                return mask >> -count;  // 负数移动, 转为右移
            }
            if (count > mask.length)
            {
                // 移出了输入遮罩的范围
                return new IndexMask(mask.length, false);
            }

            IndexMask output = new(mask.length, mask.bytes.Length);

            if ((count & 0b_111) == 0)
            {
                // 移动恰好对齐字节
                int offsetByteCount = count >> 3;
                int endBitCount = mask.length & 0b_111;
                for (int i = 0; i < mask.bytes.Length; i++)
                {
                    int target = i + offsetByteCount;
                    if (target >= mask.bytes.Length)
                    {
                        output.bytes[i] = ALL_FALSE_BYTE;
                    }
                    else if (endBitCount > 0 && i == mask.bytes.Length - 1)
                    {
                        // 不整字节, 且当前正在写最后一个字节
                        output.bytes[i] = (byte)(mask.bytes[target] & BitHelper.BitLittleRange[endBitCount]);
                    }
                    else
                    {
                        output.bytes[i] = mask.bytes[target];
                    }
                }
            }
            else
            {
                int haveValueRange = mask.length - count;   // 可能会有值的范围
                byte temp = 0;
                int bitIndex = 0;
                int byteIndex = 0;
                int index = 0;
                while (index < haveValueRange)
                {
                    int target = index + count;
                    if ((mask.bytes[target >> 3] & BitHelper.BitLocation[target & 0b_111]) > 0)
                    {
                        temp |= BitHelper.BitLocation[bitIndex];
                    }
                    bitIndex = (bitIndex + 1) & 0b_111;
                    if (bitIndex == 0)
                    {
                        output.bytes[byteIndex] = temp;
                        temp = 0;
                        byteIndex++;
                    }
                    index++;
                }
                if (bitIndex != 0)
                {
                    output.bytes[byteIndex] = temp; // 会有值的范围最后一个 byte 的剩余部分会是初始值 false
                                                    // 所以此处直接使用 temp 值即可
                    byteIndex++;
                }
                // 对应移动前处于有效范围外的 byte, 全部设置为 false 即可
                for (int i = byteIndex; i < output.bytes.Length; i++)
                {
                    output.bytes[i] = ALL_FALSE_BYTE;
                }
            }

            return output;
        }

        #endregion

        #region 等值比较
        /// <summary>
        /// 比较此遮罩是否与另一个遮罩相等
        /// </summary>
        /// <remarks>
        /// 需要符合: 1. 长度相等, 2. 各个位上的值相等
        /// </remarks>
        /// <param name="other"></param>
        /// <returns></returns>
        public readonly bool Equals(IndexMask other)
        {
            if (length != other.length) return false;
            for (int i = 0; i < bytes.Length - 1; i++)
            {
                if (bytes[i] != other.bytes[i]) return false;
            }
            int endByteBitCount = length & 0b_111;
            if (endByteBitCount > 0)
            {
                return ((bytes[^1] ^ other.bytes[^1]) & endByteBitCount) == 0;
            }
            else
            {
                return bytes[^1] == other.bytes[^1];
            }
        }
        public readonly override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is IndexMask other)
            {
                return Equals(other);
            }
            return base.Equals(obj);
        }
        public readonly override int GetHashCode()
        {
            return HashCode.Combine(length, bytes.Length,
                bytes.Length >= 1 ? bytes[0] : 0,
                bytes.Length >= 2 ? bytes[1] : 0,
                bytes.Length >= 3 ? bytes[2] : 0,
                bytes.Length >= 4 ? bytes[3] : 0);
        }

        public static bool operator ==(IndexMask left, IndexMask right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(IndexMask left, IndexMask right)
        {
            return !(left == right);
        }
        #endregion
    }
}
