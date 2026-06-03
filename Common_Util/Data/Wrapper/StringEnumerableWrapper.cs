using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Wrapper
{
    /// <summary>
    /// 字符串集合的包装器, 将一个字符串数组包装成一个只读列表
    /// </summary>
    /// <remarks>
    /// 包装会忽略掉其中的空字符串, 例如: 
    /// <code>new string[] { "hello", null, " world ", "", "~"} </code>
    /// 会被包装为: hellow world ~
    /// </remarks>
    public readonly struct StringEnumerableWrapper : IReadOnlyList<char>
    {
        public static StringEnumerableWrapper Create(IEnumerable<string> strs)
        {
            if (strs == null || !strs.Any()) return Empty;
            var arr = strs.Where(str => !string.IsNullOrEmpty(str)).ToArray();
            return Create(arr);
        }
        public static StringEnumerableWrapper Create(params string[] strs)
        {
            if (strs == null || strs.Length == 0) return Empty;
            var arr = strs.Where(str => !string.IsNullOrEmpty(str)).ToArray();
            return new StringEnumerableWrapper()
            {
                IsEmpty = false,
                WrapperStrings = arr,
                Count = arr.Length == 0 ? 0 : arr.Sum(str => str.Length)
            };
        }
        public static StringEnumerableWrapper Empty { get; } = new StringEnumerableWrapper()
        {
            IsEmpty = true,
            WrapperStrings = null!,
            Count = 0,
        };

        public bool IsEmpty { get; private init; }
        public string[] WrapperStrings { get; private init; }
        public int Count { get; private init; }

        /// <summary>
        /// 取得包装后特定位置的字符
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public char this[int index]
        {
            get
            {
                if (IsEmpty) throw new InvalidOperationException($"包装器为空! ");
                ArgumentOutOfRangeException.ThrowIfLessThan(index, 0);
                ArgumentOutOfRangeException.ThrowIfGreaterThan(index, Count);
                var (strIndex, charIndex) = GetIndex(index);
                return WrapperStrings[strIndex][charIndex];
            }
        }
        /// <summary>
        /// 取得包装后特定位置的字符
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public char this[Index index]
        {
            get
            {
                return this[index.GetOffset(Count)];
            }
        }
        /// <summary>
        /// 取得包装后指定范围内的字符序列
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        public IEnumerable<char> this[Range range]
        {
            get
            {
                if (IsEmpty) throw new InvalidOperationException($"包装器为空! ");
                (int offset, int length) = range.GetOffsetAndLength(Count);
                return Sub(offset, length);
            }
        }
        /// <summary>
        /// 切分取得偏移量 <paramref name="offset"/> 处开始, <paramref name="length"/> 长度的字符序列
        /// </summary>
        /// <param name="offset">偏移量, 允许是负数, 但是此时只会截取到有效的部分 (正数索引的部分)</param>
        /// <param name="length">截取长度, 非正数时永远会得到空序列</param>
        /// <returns></returns>
        public IEnumerable<char> Sub(int offset, int length)
        {
            if (IsEmpty) throw new InvalidOperationException($"包装器为空! ");
            if (offset < 0)
            {
                length += offset;
                offset = 0;
            }
            if (offset > Count) yield break;
            if (length <= 0) yield break;
            if (offset + length > Count)
            {
                length = Count - offset;
            }

            var (strIndex, charIndex) = GetIndex(offset);

            string currentStr = WrapperStrings[strIndex];
            for (int readIndex = 0; readIndex < length; readIndex++)
            {
                yield return WrapperStrings[strIndex][charIndex];
                charIndex++;
                if (charIndex == currentStr.Length)
                {
                    charIndex = 0;
                    strIndex++;
                    if (strIndex >= WrapperStrings.Length) yield break;
                    currentStr = WrapperStrings[strIndex];
                }
            }
        }

        public IEnumerator<char> GetEnumerator()
        {
            return Sub(0, Count).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #region 私有方法

        /// <summary>
        /// 取得获取对于包装器的索引位置 <paramref name="wrapperIndex"/> 处字符所需要的索引信息
        /// </summary>
        /// <param name="wrapperIndex">输入值需提前验证在有效范围 [0, <see cref="Count"/>) 内</param>
        /// <returns></returns>
        private (int strIndex, int charIndex) GetIndex(int wrapperIndex)
        {
            int currentStart = 0;
            for (int strIndex = 0; strIndex < WrapperStrings.Length; strIndex++)
            {
                int charIndex = wrapperIndex - currentStart;
                if (charIndex < WrapperStrings[strIndex].Length)
                {
                    return (strIndex, charIndex);
                }
                currentStart += WrapperStrings[strIndex].Length;
            }
            return (-1, -1);    // 输入值做过验证的话不可能出现这种情况
        }

        #endregion

    }
}
