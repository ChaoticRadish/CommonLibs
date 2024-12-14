using Common_Util.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Struct
{
    /// <summary>
    /// 索引值遮罩, 固定长度, 相当于一个 <see cref="bool"/>[]
    /// </summary>
    public struct IndexMask : IEnumerable<bool>
    {
        #region 常量/静态值
        private const byte DEFAULT_ALL_TRUE_BYTE = 0b_1111_1111;
        private const byte DEFAULT_ALL_FALSE_BYTE = 0b_0;
        private readonly static byte[] GetSetValueWithByteMasks =
        [
            0b_0000_0001,
            0b_0000_0010,
            0b_0000_0100,
            0b_0000_1000,
            0b_0001_0000,
            0b_0010_0000,
            0b_0100_0000,
            0b_1000_0000,
        ];
        #endregion

        #region 构造函数
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
                for (int i = 0; i < arrSize; i++)
                {
                    bytes[i] = defaultState ? DEFAULT_ALL_TRUE_BYTE : DEFAULT_ALL_FALSE_BYTE;
                }

            }
        }
        /// <summary>
        /// 通过一个 <paramref name="collection"/> 实例化一个遮罩, 其长度与集合相等
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
                    temp |= GetSetValueWithByteMasks[tempIndex];
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
        /// 通过一个 <paramref name="collection"/> 实例化一个遮罩, 其长度与集合相等
        /// </summary>
        /// <param name="collection"></param>
        public IndexMask(IEnumerable<bool> collection)
        {
            int length = 0;
            List<byte> bytes = [];

            byte temp = 0;
            int tempIndex = 0;
            foreach (var (index, b) in collection.WithIndex())
            {
                if (b)
                {
                    temp |= GetSetValueWithByteMasks[tempIndex];
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
        /// <param name="index"></param>
        /// <returns></returns>
        public readonly bool this[int index]
        {
            get
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(index, 0);
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, length);
                return (bytes[index >> 3] & GetSetValueWithByteMasks[index & 0b_111]) > 0;
            }
            set
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(index, 0);
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, length);
                int subIndex = index >> 3;
                byte b = bytes[subIndex];
                byte mask = GetSetValueWithByteMasks[index & 0b_111];
                bytes[subIndex] = (byte)((b & (~mask)) | (value ? mask : 0));
            }
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
                    yield return (bytes[i >> 3] & GetSetValueWithByteMasks[i & 0b_111]) > 0;
                }
            }
            else
            {
                for (int i = length - 1; i >= 0; i--)
                {
                    yield return (bytes[i >> 3] & GetSetValueWithByteMasks[i & 0b_111]) > 0;
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
        /// 以 <see langword="false"/> 为参数, 调用 <see cref="ToString(bool)"/> 取得字符串
        /// </summary>
        /// <returns></returns>
        public readonly override string ToString()
        {
            return ToString(false);
        }
        /// <summary>
        /// 取得简单描述该遮罩的字符串
        /// </summary>
        /// <param name="desc"><see langword="true"/> 时, 遮罩值的高位放在前面</param>
        /// <returns></returns>
        public readonly string ToString(bool desc = true)
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
                        cLeft[index] = b ? '1' : '0';
                    }
                    if (index >= rightStart)
                    {
                        cRight[index - rightStart] = b ? '1' : '0';
                    }
                }
                body = $"{new string(cLeft)}{省略号}{省略量}{省略号}{new string(cRight)}";
            }
            else
            {
                char[] cs = new char[Length];
                foreach (var (index, b) in All(desc).WithIndex())
                {
                    cs[index] = b ? '1' : '0';
                }
                body = new string(cs);
            }
            return $"{nameof(IndexMask)}_{Length}_{body}";
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
    }
}
