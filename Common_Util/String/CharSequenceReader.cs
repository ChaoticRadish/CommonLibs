using Common_Util.Data.Structure.Linear;
using Common_Util.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.String
{
    /// <summary>
    /// 字符序列读取器, 提供一些单向的读取操作
    /// </summary>
    public class CharSequenceReader
    {
        private IEnumerator<char> enumerator;

        #region 构造函数
        public CharSequenceReader()
        {
            resetAs(EmptyEnumerator<char>.Shared);
        }
        public CharSequenceReader(IEnumerable<char> chars)
        {
            ResetAs(chars);
        }

        #endregion

        #region 读取缓存状态
        /// <summary>
        /// 当前逻辑读取位置 (指向的位置还未被读取)
        /// </summary>
        public int LogicLocation { get; private set; }
        /// <summary>
        /// 枚举器的当前位置 (指向的位置还未被读取)
        /// </summary>
        public int EnumeratorIndex { get; private set; }
        /// <summary>
        /// 读取缓存
        /// </summary>
        private ICharSequenceReadBuffer ReadBuffer { get; set; }

        /// <summary>
        /// 当前读取缓存的起始位置 (指向的位置还未被读取)
        /// </summary>
        private int ReadBufferStart { get; set; }
        /// <summary>
        /// 当前读取缓存的长度
        /// </summary>
        private int ReadBufferCount { get => ReadBuffer.Count; }

        /// <summary>
        /// 当前数据源是否已经读取到尽头
        /// </summary>
        /// <remarks>
        /// 注意这个属性不是逻辑位置是否到了尽头
        /// </remarks>
        public bool IsExhausted { get; private set; } = false;
        #endregion

        #region 缓存实例
        /// <summary>
        /// 创建 <see cref="ReadBuffer"/> 的实例
        /// </summary>
        /// <returns></returns>
        protected virtual ICharSequenceReadBuffer CreateReadBuffer() => new DefaultCharSequenceReadBuffer();
        #endregion

        #region 查看操作
        /// <summary>
        /// 从当前的逻辑位置后 <paramref name="offset"/> 位置处读取一个字符, 但是不推进逻辑位置
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public bool TryPeekValue(int offset, out char output)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(offset);

            output = default;
            if (offset >= ReadBufferCount)
            {
                int needRead = offset + 1 - ReadBufferCount;
                int readed = ReadToBuffer(needRead);
                if (needRead > readed) return false;    // 实际读取到的数据量少于需要读取的
            }
            output = ReadBuffer[offset];
            return true;
        }

        /// <summary>
        /// 从当前的逻辑位置开始读取 <paramref name="count"/> 个字符, 拼接为字符串. 
        /// </summary>
        /// <remarks>
        /// 能够读取到的长度不足 <paramref name="count"/> 时不会视为失败, 而是尽量返回范围内的字符
        /// </remarks>
        /// <param name="count"></param>
        public string PeekStringPartial(int offset, int count)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(offset);

            int needCount = offset + count;
            if (needCount > ReadBufferCount)
            {
                int needRead = needCount - ReadBufferCount;
                ReadToBuffer(needRead);
            }
            if (offset >= ReadBufferCount)
            {
                return string.Empty;
            }
            int returnCount = Math.Min(ReadBufferCount, needCount) - offset;
            char[] chars = new char[returnCount];
            for (int i = 0; i < returnCount; i++)
            {
                chars[i] = ReadBuffer[offset + i];
            }
            return new(chars);
        }
        /// <summary>
        /// 从当前的逻辑位置开始读取 <paramref name="count"/> 个字符, 拼接为字符串. 严格读取 <paramref name="count"/> 个字符, 不足会视为失败
        /// </summary>
        /// <param name="count"></param>
        /// <param name="str"></param>
        public bool TryPeekStringExact(int offset, int count, [NotNullWhen(true)] out string? str)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(offset);

            int needCount = offset + count;
            str = default;
            if (needCount > ReadBufferCount)
            {
                int needRead = needCount - ReadBufferCount;
                int readed = ReadToBuffer(needRead);
                if (needRead > readed) return false;    // 实际读取到的数据量少于需要读取的
            }
            char[] chars = new char[count];
            for (int i = 0; i < count; i++)
            {
                chars[i] = ReadBuffer[offset + i];
            }
            str = new(chars);
            return true;
        }
        #endregion

        #region 检查
        /// <summary>
        /// 检查当前逻辑读取位置是否已经读取到尽头
        /// </summary>
        /// <returns></returns>
        public bool CheckLogicExhausted()
        {
            return !TryReadSaveWhereLocation(LogicLocation, out _);
        }
        #endregion

        #region 读取操作

        /// <summary>
        /// 移动 <paramref name="count"/> 个字符, 并忽略他们
        /// </summary>
        /// <param name="count"></param>
        public void Skip(int count)
        {
            int needClearBuffer = count >= ReadBufferCount ? ReadBufferCount : count;
            ReadBuffer.Clear(needClearBuffer);
            LogicLocation += needClearBuffer;
            ReadBufferStart += needClearBuffer;

            int needReadDiscard = count - needClearBuffer;
            for (int i = 0; i < needReadDiscard; i++)
            {
                if (!TryReadDiscardFromSource())
                {
                    return;
                }
            }
        }

        /// <summary>
        /// 从当前位置开始读取, 直到读取到 <paramref name="count"/> 数量, 或读取到读取器源序列的终点
        /// </summary>
        /// <param name="count"></param>
        /// <param name="stringBuilder"></param>
        public void ReadCountOrEnd(int count, StringBuilder stringBuilder)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(count, 0);
            if (count == 0) return;

            int startIndex = LogicLocation;
            int index = 0;
            while (TryReadSaveWhereLocation(startIndex + index, out char readed))
            {
                stringBuilder.Append(readed);
                if (index == count - 1)
                {
                    SubmitReaded(startIndex + index + 1 - ReadBufferStart);
                    return;
                }
                index++;
            }
            SubmitAllBuffer();
        }

        /// <summary>
        /// 从当前位置开始读取, 直到读取器源序列的终点
        /// </summary>
        /// <param name="stringBuilder"></param>
        public void ReadUntilEnd(StringBuilder stringBuilder)
        {
            int startIndex = LogicLocation;
            int index = 0;
            while (TryReadSaveWhereLocation(startIndex + index, out char readed))
            {
                stringBuilder.Append(readed);
                index++;
            }
            SubmitAllBuffer();
        }

        /// <summary>
        /// 从当前位置开始读取, 直到读取到不在 <paramref name="chars"/> 中的字符, 或读取到读取器源序列的终点
        /// </summary>
        /// <param name="chars"></param>
        /// <param name="stringBuilder"></param>
        public void ReadUntilNotIn(IEnumerable<char> chars, StringBuilder stringBuilder)
        {
            var charSet = chars.ToHashSet();
            if (!chars.Any()) return;

            int startIndex = LogicLocation;
            int index = 0;
            while (TryReadSaveWhereLocation(startIndex + index, out char readed))
            {
                if (charSet.Contains(readed))
                {
                    stringBuilder.Append(readed);
                    SubmitReaded(startIndex + index + 1 - ReadBufferStart);
                }
                else
                {
                    return;
                }

                index++;
            }
            SubmitAllBuffer();
        }
        /// <summary>
        /// 从当前位置开始读取, 直到读取到不在 <paramref name="chars"/> 中的字符, 或读取到读取器源序列的终点
        /// </summary>
        /// <param name="chars"></param>
        public void ReadUntilNotIn(IEnumerable<char> chars)
        {
            var charSet = chars.ToHashSet();
            if (!chars.Any()) return;

            int startIndex = LogicLocation;
            int index = 0;
            while (TryReadSaveWhereLocation(startIndex + index, out char readed))
            {
                if (charSet.Contains(readed))
                {
                    SubmitReaded(startIndex + index + 1 - ReadBufferStart);
                }
                else
                {
                    return;
                }

                index++;
            }
            SubmitAllBuffer();
        }


        /// <summary>
        /// 从当前位置开始读取, 直到遇到指定的字符串
        /// </summary>
        /// <remarks>
        /// 移动量为 <paramref name="output"/> 的长度, 如果没有找到, 会一直移动到当前读取器源序列的终点
        /// </remarks>
        /// <param name="str"></param>
        /// <param name="output">
        /// 从当前位置开始到 <paramref name="str"/> 前的字符串 (不包含 <paramref name="str"/> 本身) <br/>
        /// 当未查找到时, 返回从开始查找的位置, 到源序列结束处的所有字符构成的字符串
        /// </param>
        /// <returns>如果读取到末尾都没有找到匹配的字符串, 将返回 <see langword="false"/></returns>
        public bool TryReadUntil(string str, [NotNullWhen(true)] out string? output)
        {
            ArgumentNullException.ThrowIfNull(str, nameof(str));
            output = null;
            if (string.IsNullOrEmpty(str)) return false;

            StringBuilder sb = new();
            int startIndex = LogicLocation;
            int index = 0;
            int readCount = 0;
            while (TryReadSaveWhereLocation(startIndex + index, out char readed))
            {
                readCount++;
                sb.Append(readed);


                if (readCount >= str.Length)
                {
                    bool foundFlag = true;
                    for (int checkIndex = 0; checkIndex < str.Length; checkIndex++)
                    {
                        if (str[checkIndex] != sb[sb.Length - str.Length + checkIndex])
                        {
                            foundFlag = false;
                            break;
                        }
                    }

                    // 提交时保留待查找字符串长度的缓存, 在此之前的则不保留
                    SubmitReaded(startIndex + index + 1 - str.Length - ReadBufferStart);

                    if (foundFlag)
                    {
                        sb.Remove(sb.Length - str.Length, str.Length);
                        output = sb.ToString();
                        return true;
                    }

                }

                index++;
            }

            // 未找到时, 提交当前所有内容
            SubmitAllBuffer();
            output = sb.ToString();
            return false;
        }

        /// <summary>
        /// 从当前位置开始读取, 直到遇到 <paramref name="chars"/> 中出现过的字符
        /// </summary>
        /// <remarks>
        /// 移动量为 <paramref name="output"/> 的长度, 如果没有遇到在 <paramref name="chars"/> 中的字符, 会一直移动到当前读取器源序列的终点
        /// </remarks>
        /// <param name="chars"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public bool TryReadUntilAnyChar(IEnumerable<char> chars, [NotNullWhen(true)] out string? output, out char foundChar)
        {
            ArgumentNullException.ThrowIfNull(chars, nameof(chars));
            output = null;
            foundChar = default;
            if (!chars.Any()) return false;

            StringBuilder sb = new StringBuilder();
            bool b = TryReadUntilAnyChar(chars, sb, out foundChar);
            output = sb.ToString();
            return b;
        }
        /// <summary>
        /// 从当前位置开始读取, 直到遇到 <paramref name="stopChars"/> 中出现过的字符
        /// </summary>
        /// <remarks>
        /// 如果没有遇到在 <paramref name="stopChars"/> 中的字符, 会一直移动到当前读取器源序列的终点
        /// </remarks>
        /// <param name="stopChars"></param>
        /// <param name="stringBuilder">寻找过程中读取出来的字符追加到此对象, 不会包含 <paramref name="foundChar"/></param>
        /// <returns></returns>
        public bool TryReadUntilAnyChar(IEnumerable<char> stopChars, StringBuilder stringBuilder, out char foundChar)
        {
            ArgumentNullException.ThrowIfNull(stopChars, nameof(stopChars));
            foundChar = default;
            if (!stopChars.Any())
                return false;

            var stopCharSet = stopChars.ToHashSet();

            int startIndex = LogicLocation;
            int index = 0;
            while (TryReadSaveWhereLocation(startIndex + index, out char readed))
            {
                if (stopCharSet.Contains(readed))
                {
                    foundChar = readed;
                    return true;
                }
                else
                {
                    stringBuilder.Append(readed);
                    // 提交读取到的内容
                    SubmitReaded(startIndex + index + 1 - ReadBufferStart);
                }

                index++;
            }

            // 未找到时, 提交当前所有内容
            SubmitAllBuffer();
            return false;
        }
        /// <summary>
        /// 从当前位置开始读取, 直到遇到指定的字符串 (忽略字符串值的部分)
        /// </summary>
        /// <param name="str"></param>
        /// <param name="output">从当前位置开始到 <paramref name="str"/> 前的字符串 (不包含 <paramref name="str"/> 本身)</param>
        /// <returns></returns>
        public bool TryReadUntilIgnoreStringText(string str, [NotNullWhen(true)] out string? output)
        {
            ArgumentNullException.ThrowIfNull(str, nameof(str));
            output = null;
            if (string.IsNullOrEmpty(str)) return false;

            StringBuilder sb = new();
            int startIndex = LogicLocation;
            int index = 0;
            int readCount = 0;

            bool inQuotes = false;
            const char quoteChar = '"';
            const char escapeChar = '\\';
            bool escape = false;

            while (TryReadSaveWhereLocation(startIndex + index, out char readed))
            {
                readCount++;
                sb.Append(readed);

                if (inQuotes)
                {
                    switch (readed)
                    {
                        case quoteChar:
                            if (escape)
                            {
                                escape = false;
                            }
                            else
                            {
                                inQuotes = false;
                            }
                            break;
                        case escapeChar:
                            if (escape)
                            {
                                escape = false;
                            }
                            else
                            {
                                escape = true;
                            }
                            break;
                        default:
                            if (escape)
                            {
                                escape = false;
                            }
                            break;
                    }
                }
                else
                {
                    switch (readed)
                    {
                        case quoteChar:
                            inQuotes = true;
                            break;
                        default:
                            break;
                    }
                }

                if (readCount >= str.Length)
                {
                    bool foundFlag;
                    if (!inQuotes)
                    {
                        foundFlag = true;
                        for (int checkIndex = 0; checkIndex < str.Length; checkIndex++)
                        {
                            if (str[checkIndex] != sb[sb.Length - str.Length + checkIndex])
                            {
                                foundFlag = false;
                                break;
                            }
                        }
                    }
                    else
                    {
                        foundFlag = false;
                    }

                    // 提交时保留待查找字符串长度的缓存, 在此之前的则不保留
                    SubmitReaded(startIndex + index + 1 - str.Length - ReadBufferStart);

                    if (foundFlag)
                    {
                        sb.Remove(sb.Length - str.Length, str.Length);
                        output = sb.ToString();
                        return true;
                    }

                }

                index++;
            }

            // 未找到时, 提交当前所有内容
            SubmitAllBuffer();
            output = sb.ToString();
            return false;
        }

        /// <summary>
        /// 读取从当前位置开始之后的所有字符作为一个字符串
        /// </summary>
        /// <returns></returns>
        public string Drain()
        {
            TrimBufferToCurrent();
            if (CheckLogicExhausted()) return string.Empty;
            while (TryReadSaveFromSource(out _)) ;
            return ReadBuffer.GetBufferContent();
        }

        #endregion

        #region 缓存操作
        /// <summary>
        /// 清理逻辑位置前的历史缓存中 <paramref name="length"/> 个字符
        /// </summary>
        /// <param name="length"></param>
        public void TrimBuffer(int length)
        {
            if (length > LogicLocation - ReadBufferStart)
                throw new ArgumentException($"清理字符数 {length} 超出当前历史缓存部分长度 {LogicLocation - ReadBufferStart}", nameof(length));
            ClearBuffer(length);
        }
        /// <summary>
        /// 清理所有历史缓存, 以使当前缓存位置与逻辑位置重合
        /// </summary>
        public void TrimBufferToCurrent() => ClearBuffer(LogicLocation - ReadBufferStart);

        private void ClearBuffer(int length)
        {
            if (length <= 0) return;
            ReadBuffer.Clear(length);
            ReadBufferStart += length;
        }
        #endregion

        #region 重置操作
        /// <summary>
        /// 重置当前读取器
        /// </summary>
        /// <param name="chars"></param>
        [MemberNotNull(nameof(enumerator), nameof(ReadBuffer))]
        public void ResetAs(IEnumerable<char> chars)
        {
            resetAs(chars.GetEnumerator());
        }
        [MemberNotNull(nameof(enumerator), nameof(ReadBuffer))]
        private void resetAs(IEnumerator<char> enumerator)
        {
            this.enumerator = enumerator;
            LogicLocation = 0;
            ReadBuffer = CreateReadBuffer();
            IsExhausted = false;
        }
        #endregion

        #region 内部操作
        /// <summary>
        /// 尝试从数据源读取一个字符, 读取成功会存入 <see cref="ReadBuffer"/>, 不会推进 <see cref="LogicLocation"/>
        /// </summary>
        /// <param name="readed"></param>
        /// <returns></returns>
        private bool TryReadSaveFromSource(out char readed)
        {
            if (enumerator.MoveNext())
            {
                readed = enumerator.Current;
                ReadBuffer.Add(readed);
                EnumeratorIndex++;
                return true;
            }
            else
            {
                IsExhausted = true;
                readed = default;
                return false;
            }
        }

        /// <summary>
        /// 读取序列位置 <paramref name="location"/> 处的字符
        /// </summary>
        /// <remarks>
        /// 只能读取 <see cref="ReadBufferStart"/> 之后的字符, 如果还不在缓存中, 则将加载到缓存
        /// </remarks>
        /// <param name="location"></param>
        /// <returns></returns>
        private bool TryReadSaveWhereLocation(int location, out char readed)
        {
            readed = default;
            if (location < ReadBufferStart) return false;
            if (location >= ReadBufferStart && location < ReadBufferStart + ReadBufferCount)
            {
                readed = ReadBuffer[location - ReadBufferStart];
                return true;
            }
            else
            {
                int needRead = location + 1 - EnumeratorIndex;
                int readCount = ReadToBuffer(needRead);
                if (readCount < needRead)
                    return false;

                readed = ReadBuffer[location - ReadBufferStart];
                return true;
            }
        }
        /// <summary>
        /// 尝试从数据源读取一个字符, 但是不存入 <see cref="ReadBuffer"/>, 会推进 <see cref="LogicLocation"/> (因为做了逻辑操作: 丢弃)
        /// </summary>
        /// <remarks>
        /// 只能在 <see cref="ReadBufferCount"/> = 0 时可用
        /// </remarks>
        /// <returns></returns>
        private bool TryReadDiscardFromSource()
        {
            if (ReadBufferCount != 0) return false;
            if (enumerator.MoveNext())
            {
                EnumeratorIndex++;
                LogicLocation++;
                ReadBufferStart++;
                return true;
            }
            else
            {
                IsExhausted = true;
                return false;
            }
        }

        /// <summary>
        /// 从数据源读取 <paramref name="length"/> 个字符, 存储到 <see cref="ReadBuffer"/>
        /// </summary>
        /// <param name="length"></param>
        private int ReadToBuffer(int length)
        {
            int counter = 0;
            for (int i = 0; i < length; i++)
            {
                if (TryReadSaveFromSource(out _))
                {
                    counter++;
                }
                else break;
            }
            return counter;
        }
        /// <summary>
        /// 将已读取的内容提交到逻辑状态
        /// </summary>
        /// <param name="length"></param>
        private void SubmitReaded(int length)
        {
            if (length <= 0) return;
            if (length > ReadBufferCount) throw new InvalidOperationException($"提交读取完成状态失败, 当前缓存字符量 {ReadBufferCount} 小于提交量 {length}");
            ReadBuffer.Clear(length);
            LogicLocation += length;
            ReadBufferStart += length;
        }
        /// <summary>
        /// 将已读取到缓存的内容提交到逻辑状态
        /// </summary>
        private void SubmitAllBuffer()
        {
            if (LogicLocation == ReadBufferStart)
            {
                int length = ReadBufferCount;
                if (length == 0) return;
                ReadBuffer.Clear(length);
                LogicLocation += length;
                ReadBufferStart += length;
            }
            else if (LogicLocation > ReadBufferStart)
            {
                int readBufferEnd = ReadBufferStart + ReadBufferCount;
                ReadBuffer.Clear();
                LogicLocation = readBufferEnd;
                ReadBufferStart = readBufferEnd;
            }
            else
            {
                throw new Exceptions.General.ImpossibleForkException($"逻辑位置不应小于缓存起点");
            }
        }

        #endregion


        #region 调试
        /// <summary>
        /// 取得描述当前状态的字符串
        /// </summary>
        /// <returns></returns>
        public string GetStatusString(string splitChar = "; ")
        {
            return string.Join(splitChar,
                $"{nameof(LogicLocation)}: {LogicLocation}",
                $"{nameof(EnumeratorIndex)}: {EnumeratorIndex}",
                $"{nameof(ReadBufferStart)}: {ReadBufferStart}",
                $"{nameof(ReadBufferCount)}: {ReadBufferCount}",
                $"{nameof(ReadBuffer)}: {ReadBuffer.GetBufferContent().WhenEmptyDefault("<empty>")}");
        }
        #endregion
    }
    /// <summary>
    /// 字符序列读取器的读取缓存接口
    /// </summary>
    public interface ICharSequenceReadBuffer
    {
        /// <summary>
        /// 取得特定序号处的索引值
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        char this[int index] { get; }

        /// <summary>
        /// 缓存数量
        /// </summary>
        int Count { get; }

        /// <summary>
        /// 添加字符到缓存的末尾
        /// </summary>
        /// <param name="c"></param>
        void Add(char c);
        /// <summary>
        /// 添加一系列字符到缓存的末尾
        /// </summary>
        /// <param name="chars"></param>
        void AddRange(IEnumerable<char> chars);
        /// <summary>
        /// 从缓存的首部清理 <paramref name="count"/> 个字符
        /// </summary>
        /// <param name="count"></param>
        void Clear(int count);
        /// <summary>
        /// 移除所有缓存着的字符
        /// </summary>
        void Clear();

        /// <summary>
        /// 取得缓存内容字符串
        /// </summary>
        /// <returns></returns>
        string GetBufferContent();
    }
    public readonly struct DefaultCharSequenceReadBuffer() : ICharSequenceReadBuffer
    {
        private readonly Queue<char> buffer = [];

        public char this[int index]
        {
            get
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, buffer.Count, nameof(index));
                return buffer.Skip(index).Take(1).First();
            }
        }

        public int Count => buffer.Count;

        public void Add(char c) => buffer.Enqueue(c);

        public void AddRange(IEnumerable<char> chars)
        {
            foreach (var c in chars)
            {
                buffer.Enqueue(c);
            }
        }

        public void Clear(int count)
        {
            for (int i = 0; i < count; i++)
            {
                buffer.Dequeue();
            }
        }
        public void Clear()
        {
            buffer.Clear();
        }

        public string GetBufferContent()
        {
            return new string(buffer.ToArray());
        }
    }
}
