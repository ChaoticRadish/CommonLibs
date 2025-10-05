using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.String
{
    /// <summary>
    /// 字符串解析工具
    /// </summary>
    public static class StringAnalysis
    {
        /// <summary>
        /// 根据字符对解析字符串
        /// </summary>
        /// <param name="str">要解析的字符串</param>
        /// <param name="normalCharAction">一般字符的操作</param>
        /// <param name="inPairNormalAction">在字符对内的字符串的操作</param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        public static void AnalysisForPair(
            IEnumerable<char> str,
            Action<char> normalCharAction,
            Action<string> inPairNormalAction,
            char left = '{', char right = '}')
        {
            if (str == null || !str.Any() || left == right)
            {
                return;
            }
            // 容器
            StringBuilder builder = new StringBuilder();
            // 是否正在字符对中
            bool innerPair = false;
            foreach (char temp in str)
            {
                if (innerPair)
                {// 在字符对之内
                    if (temp == right)
                    {// 是字符对之右
                        inPairNormalAction?.Invoke(builder.ToString());
                        innerPair = false;
                    }
                    else
                    {
                        builder.Append(temp);
                    }
                }
                else
                {// 在字符对之外
                    if (temp == left)
                    {// 是字符对之左
                        builder.Clear();
                        innerPair = true;
                    }
                    else
                    {
                        normalCharAction?.Invoke(temp);
                    }
                }
            }
            // 读取结束
            if (innerPair)
            {
                inPairNormalAction?.Invoke(builder.ToString());
                innerPair = false;
            }
        }

        /// <summary>
        /// 根据字符对解析字符串并输出为字符串
        /// </summary>
        /// <param name="str">要解析的字符串</param>
        /// <param name="pairConvert">字符对内的字符串的解析方法</param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static string AnalysisToStringForPair(
            IEnumerable<char> str, Func<string, string> pairConvert,
            char left = '{', char right = '}')
        {
            StringBuilder builder = new StringBuilder();
            AnalysisForPair(
                str,
                (c) =>
                {
                    builder.Append(c);
                },
                (s) =>
                {
                    builder.Append(pairConvert?.Invoke(s));
                }, left, right);
            return builder.ToString();
        }

        #region 字符串中的数字获取
        /// <summary>
        /// 截取输入字符串中的第一个数字
        /// </summary>
        /// <param name="input"></param>
        /// <param name="defaultValue"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static float SubFirstFloat(string input, float defaultValue = 0, string? filter = null)
        {
            if (string.IsNullOrEmpty(input))
            {
                return defaultValue;
            }
            input = input.Trim();
            bool readEnd = false;
            bool existNumber = false;
            bool existMinus = false;
            bool existPoint = false;
            StringBuilder builder = new StringBuilder();
            foreach (char c in input)
            {
                if (!string.IsNullOrEmpty(filter) && filter.Contains(c))
                {
                    continue;
                }
                switch (c)
                {
                    case '-':
                        if (!existMinus)
                        {
                            existMinus = true;
                            builder.Append(c);
                        }
                        else
                        {
                            if (builder[builder.Length - 1] != c)
                            {
                                readEnd = true;
                            }
                        }
                        break;
                    case '.':
                        if (!existNumber)
                        {
                            existNumber = true;
                            builder.Append('0');
                        }
                        if (!existPoint)
                        {
                            existPoint = true;
                            builder.Append(c);
                        }
                        else
                        {
                            if (builder[builder.Length - 1] != c)
                            {
                                readEnd = true;
                            }
                        }
                        break;
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        existNumber = true;
                        builder.Append(c);
                        break;
                    default:
                        if (existNumber || existMinus || existPoint)
                        {
                            readEnd = true;
                        }
                        break;
                }
                if (readEnd)
                {
                    break;
                }
            }
            if (float.TryParse(builder.ToString(), out float output))
            {
                return output;
            }
            else
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// 截取输入字符串中的数字
        /// </summary>
        /// <param name="input"></param>
        /// <param name="filter"></param>
        /// <param name="originals"><see langword="float"/> 值与其对应的原文详情</param>
        /// <returns></returns>
        public static List<float> SubFloats(
            string input,
            string filter,
            out List<FloatSubOriginal> originals)
        {
            List<float> output = new List<float>();
            originals = new List<FloatSubOriginal>();
            if (string.IsNullOrEmpty(input))
            {
                return output;
            }
            input = input.Trim();

            bool readEnd = false;
            bool existNumber = false;
            bool existMinus = false;
            bool existPoint = false;
            StringBuilder builder = new StringBuilder();
            StringBuilder originalBuilder = new StringBuilder();
            int index = 0;
            foreach (char c in input)
            {
                if (!string.IsNullOrEmpty(filter) && filter.Contains(c))
                {
                    readEnd = true;
                }
                else
                {
                    switch (c)
                    {
                        case '-':
                            if (!existMinus && !existNumber)
                            {
                                existMinus = true;
                                builder.Append(c);
                            }
                            else
                            {
                                if (builder[builder.Length - 1] != c)
                                {
                                    readEnd = true;
                                }
                            }
                            break;
                        case '.':
                            if (!existNumber)
                            {
                                existNumber = true;
                                builder.Append('0');
                            }
                            if (!existPoint)
                            {
                                existPoint = true;
                                builder.Append(c);
                            }
                            else
                            {
                                if (builder[builder.Length - 1] != c)
                                {
                                    readEnd = true;
                                }
                            }
                            break;
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                            existNumber = true;
                            builder.Append(c);
                            break;
                        default:
                            if (existNumber || existMinus || existPoint)
                            {
                                readEnd = true;
                            }
                            break;
                    }
                }
                originalBuilder.Append(c);  // 所有字符均会被读取
                if (readEnd || index == input.Length - 1)
                {
                    if (float.TryParse(builder.ToString(), out float v))
                    {
                        // 当前字符串转换成功
                        output.Add(v);
                        originals.Add(new FloatSubOriginal()
                        {
                            Value = v,
                            LastCharIndex = index,
                            Original = originalBuilder.ToString(),
                        });
                    }
                    readEnd = false;
                    existNumber = false;
                    existMinus = false;
                    existPoint = false;
                    builder.Clear();
                    originalBuilder.Clear();
                }
                index++;
            }
            return output;
        }
        /// <summary>
        /// <see langword="float"/> 的截取原始信息
        /// </summary>
        public struct FloatSubOriginal
        {
            public float Value { get; set; }
            public string Original { get; set; }
            public int LastCharIndex { get; set; }
        }
        #endregion

        /// <summary>
        /// 尝试从 <paramref name="input"/> 中, 从 <paramref name="startIndex"/> 位置开始, 寻找 <paramref name="findStr"/> 字符串, 寻找总字符数不超过 <paramref name="maxReadLength"/>
        /// </summary>
        /// <param name="input"></param>
        /// <param name="findStr">如果是空字符串, 且 <paramref name="input"/> 不是 <see langword="null"/>, 均会返回空字符串</param>
        /// <param name="startIndex">如果小于 0, 将从 0 开始查找, 同时会缩减 <paramref name="maxReadLength"/></param>
        /// <param name="maxReadLength">如果为 <see langword="null"/>, 则不作限制</param>
        /// <param name="output"></param>
        /// <returns></returns>
        public static bool TryReadUntil(string input, string findStr, int startIndex, int? maxReadLength, [NotNullWhen(true)] out string? output)
        {
            output = null;

            if (input == null) return false;
            findStr ??= string.Empty;
            if (findStr == string.Empty)
            {
                output = string.Empty;
                return true;
            }

            int useStartIndex = startIndex < 0 ? 0 : startIndex;
            if ((input.Length - useStartIndex) < findStr.Length) return false; // 查找范围不可能找到指定字符串
            int _maxReadLength;
            if (maxReadLength != null)
            {
                if (startIndex < 0)
                {
                    maxReadLength += startIndex;    // 最终值会缩小
                }
                if (useStartIndex + maxReadLength > input.Length)
                {
                    _maxReadLength = input.Length - useStartIndex;
                }
                else
                {
                    _maxReadLength = maxReadLength.Value;
                }
                if (findStr.Length > _maxReadLength)
                {
                    return false; // 查找范围不可能找到指定字符串
                }
            }
            else
            {
                _maxReadLength = input.Length - useStartIndex;
            }

            // 一边读取一边判断
            char[] buffer = new char[findStr.Length];
            int writeIndex = 0; // 需要写入时, 当前需要写入的位置
            int readIndex = useStartIndex;
            for (int readCount = 0; readCount < _maxReadLength;)
            {
                buffer[writeIndex] = input[readIndex];

                writeIndex++;
                if (writeIndex >= buffer.Length)
                {
                    writeIndex = 0;
                }

                readIndex++;
                readCount++;

                // 判断
                if (readCount >= findStr.Length)
                {
                    bool found = true;
                    for (int checkIndex = 0; checkIndex < findStr.Length; checkIndex++)
                    {
                        if (findStr[checkIndex] != buffer[(writeIndex + checkIndex) % findStr.Length])
                        {
                            found = false;
                            break;
                        }
                    }

                    if (found)
                    {
                        output = input.Substring(useStartIndex, readCount - findStr.Length);
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 尝试从 <paramref name="input"/> 中, 从 <paramref name="startIndex"/> 位置开始, 寻找 <paramref name="findStr"/> 字符串, 如果未寻找到, 则返回直到中止处的字符串, 寻找的总字符数不超过 <paramref name="maxReadLength"/>
        /// </summary>
        /// <remarks>
        /// 与 <see cref="TryReadUntil(string, string, int, int?, out string?)"/> 的主要差异在于, 如果 <paramref name="input"/> 不包含 <paramref name="findStr"/>, 则将返回范围内的所有字符
        /// </remarks>
        /// <param name="input"></param>
        /// <param name="findStr">如果是空字符串, 且 <paramref name="input"/> 不是 <see langword="null"/>, 均会返回空字符串</param>
        /// <param name="startIndex">如果小于 0, 将从 0 开始查找, 同时会缩减 <paramref name="maxReadLength"/></param>
        /// <param name="maxReadLength">如果为 <see langword="null"/>, 则不作限制</param>
        /// <param name="output"></param>
        /// <returns></returns>
        public static bool TryReadUntilOrEnd(string input, string findStr, int startIndex, int? maxReadLength, [NotNullWhen(true)] out string? output)
        {
            output = null;
            if (input == null) return false;
            if (input == string.Empty)
            {
                output = string.Empty;
                return true;
            }
            findStr ??= string.Empty;
            if (findStr == string.Empty)
            {
                output = string.Empty;
                return true;
            }

            bool needReturnToEnd = false;

            int useStartIndex = startIndex < 0 ? 0 : startIndex;
            if ((input.Length - useStartIndex) < findStr.Length) needReturnToEnd = true; 
            int _maxReadLength;
            if (maxReadLength != null)
            {
                if (startIndex < 0)
                {
                    maxReadLength += startIndex;
                }
                if (useStartIndex + maxReadLength > input.Length)
                {
                    _maxReadLength = input.Length - useStartIndex;
                }
                else
                {
                    _maxReadLength = maxReadLength.Value;
                }
                if (findStr.Length > _maxReadLength)
                {
                    needReturnToEnd = true;
                }
            }
            else
            {
                _maxReadLength = input.Length - useStartIndex;
            }

            if (needReturnToEnd)
            {
                // 查找范围不可能找到指定字符串
                if (_maxReadLength <= 0)
                {
                    output = string.Empty;
                }
                else
                {
                    output = input.Substring(useStartIndex, _maxReadLength);
                }
                return true;
            }

            // 一边读取一边判断
            char[] buffer = new char[findStr.Length];
            int writeIndex = 0; // 需要写入时, 当前需要写入的位置
            int readIndex = useStartIndex;
            for (int readCount = 0; readCount < _maxReadLength;)
            {
                buffer[writeIndex] = input[readIndex];

                writeIndex++;
                if (writeIndex >= buffer.Length)
                {
                    writeIndex = 0;
                }

                readIndex++;
                readCount++;

                // 判断
                if (readCount >= findStr.Length)
                {
                    bool found = true;
                    for (int checkIndex = 0; checkIndex < findStr.Length; checkIndex++)
                    {
                        if (findStr[checkIndex] != buffer[(writeIndex + checkIndex) % findStr.Length])
                        {
                            found = false;
                            break;
                        }
                    }

                    if (found)
                    {
                        output = input.Substring(useStartIndex, readCount - findStr.Length);
                        return true;
                    }
                }
            }

            // 未找到匹配的字符串时
            output = input.Substring(useStartIndex, _maxReadLength);
            return true;
        }


        /// <summary>
        /// 尝试从 <paramref name="input"/> 中, 从 <paramref name="startIndex"/> 位置开始, 寻找 <paramref name="findStrs"/> 中任一字符串, 如果未寻找到, 则返回直到中止处的字符串, 寻找的总字符数不超过 <paramref name="maxReadLength"/>
        /// </summary>
        /// <param name="input"></param>
        /// <param name="findStrs">需要寻找的字符串, 寻找到其中任意一个值时即停止, 没有优先顺序的差异 (因为只影响截取的位置). 如果是空的或其中任一值为空, 且 <paramref name="input"/> 不是 <see langword="null"/>, 均会返回空字符串</param>
        /// <param name="startIndex">如果小于 0, 将从 0 开始查找, 同时会缩减 <paramref name="maxReadLength"/></param>
        /// <param name="maxReadLength">如果为 <see langword="null"/>, 则不作限制</param>
        /// <param name="foundStr">如果为 <see langword="null"/>, 说明没有找到匹配项, 反之则是找到的在 <paramref name="findStrs"/> 内的匹配项</param>
        /// <param name="output"></param>
        /// <returns></returns>
        public static bool TryReadUntilOrEnd(string input, string[] findStrs, int startIndex, int? maxReadLength, out string? foundStr, [NotNullWhen(true)] out string? output)
        {
            output = null;
            foundStr = null;
            if (input == null) return false;
            if (input == string.Empty)
            {
                output = string.Empty;
                foundStr = null;
                return true;
            }
            if (findStrs == null || findStrs.Length == 0 || findStrs.Any(string.IsNullOrEmpty))
            {
                output = string.Empty;
                foundStr = string.Empty;
                return true;
            }

            // 查询字符串数组非空, 且均为非空字符串的情况: 

            int minFindLength = findStrs.Min(str => str.Length);
            int maxFindLength = findStrs.Max(str => str.Length);

            bool needReturnToEnd = false;

            int useStartIndex = startIndex < 0 ? 0 : startIndex;
            if ((input.Length - useStartIndex) < minFindLength) needReturnToEnd = true;
            int _maxReadLength;
            if (maxReadLength != null)
            {
                if (startIndex < 0)
                {
                    maxReadLength += startIndex;
                }
                if (useStartIndex + maxReadLength > input.Length)
                {
                    _maxReadLength = input.Length - useStartIndex;
                }
                else
                {
                    _maxReadLength = maxReadLength.Value;
                }
                if (minFindLength > _maxReadLength)
                {
                    needReturnToEnd = true;
                }
            }
            else
            {
                _maxReadLength = input.Length - useStartIndex;
            }

            if (needReturnToEnd)
            {
                // 查找范围不可能找到指定字符串
                if (_maxReadLength <= 0)
                {
                    output = string.Empty;
                }
                else
                {
                    output = input.Substring(useStartIndex, _maxReadLength);
                }
                foundStr = null;
                return true;
            }

            // 一边读取一边判断
            char[] buffer = new char[maxFindLength];
            int writeIndex = 0; // 需要写入时, 当前需要写入的位置
            int readIndex = useStartIndex;
            for (int readCount = 0; readCount < _maxReadLength;)
            {
                buffer[writeIndex] = input[readIndex];

                writeIndex++;
                if (writeIndex >= buffer.Length)
                {
                    writeIndex = 0;
                }

                readIndex++;
                readCount++;

                // 判断
                if (readCount >= minFindLength)
                {
                    bool found = false;
                    string findStr = string.Empty;
                    for (int findIndex = 0; findIndex < findStrs.Length; findIndex++)
                    {
                        findStr = findStrs[findIndex];
                        if (readCount < findStr.Length) continue;
                        int checkOffset = maxFindLength - findStr.Length;
                        bool subFound = true;
                        for (int checkIndex = 0; checkIndex < findStr.Length; checkIndex++)
                        {
                            if (findStr[checkIndex] != buffer[(writeIndex + checkOffset + checkIndex) % maxFindLength])
                            {
                                subFound = false;
                                break;
                            }
                        }
                        if (subFound)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (found)
                    {
                        output = input.Substring(useStartIndex, readCount - findStr.Length);
                        foundStr = findStr;
                        return true;
                    }
                }
            }

            // 未找到匹配的字符串时
            output = input.Substring(useStartIndex, _maxReadLength);
            foundStr = null;
            return true;
        }


        /* 
         * 解析过程中会将字符串值作为一个整体略过的解析方式
         * 例如: "mytest=\"plan=123\"" 用这里的分割方法分割后会得到: ["mytest", "\"plan=123\""], 而不是 ["mytest", "\"plan", "123\""]
         */
        #region 包含 C# 字符串值的解析

        /// <summary>
        /// 将字符串分割成子字符串，忽略 C# 字符串值。
        /// </summary>
        /// <remarks>
        /// 支持使用反斜杠 \ 来转义引号字符。
        /// </remarks>
        /// <param name="input">要分割的字符串。</param>
        /// <param name="delimiter">用作分隔符的字符。</param>
        public static IEnumerable<string> SplitIgnoreStringValue(string input, char delimiter, StringSplitOptions splitOptions = StringSplitOptions.None)
        {
            if (string.IsNullOrEmpty(input))
            {
                yield break;
            }
            static bool handleReturnValue(StringBuilder sb, StringSplitOptions splitOptions, out string value)
            {
                if (sb.Length == 0)
                {
                    value = string.Empty;
                    if ((splitOptions & StringSplitOptions.RemoveEmptyEntries) > 0)
                    {
                        return false;
                    }
                }
                else
                {
                    value = sb.ToString();
                    if ((splitOptions & StringSplitOptions.TrimEntries) > 0)
                    {
                        value = value.Trim();
                        if (value.Length == 0)
                        {
                            if ((splitOptions & StringSplitOptions.RemoveEmptyEntries) > 0)
                            {
                                return false;
                            }
                        }
                    }
                }
                return true;
            }

            var currentSegment = new StringBuilder();

            var flagMap = IgnoreStringValueFlagMap(input);

            string returnValue;
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (!flagMap[i])
                {
                    currentSegment.Append(c);
                }
                else
                {
                    if (c == delimiter)
                    {
                        if (handleReturnValue(currentSegment, splitOptions, out returnValue))
                        {
                            yield return returnValue;
                        }
                        currentSegment.Clear();
                        continue;
                    }
                    else
                    {
                        currentSegment.Append(c);
                    }
                }

            }

            // 返回最后一个段
            if (handleReturnValue(currentSegment, splitOptions, out returnValue))
            {
                yield return returnValue;
            }
        }

        /// <summary>
        /// 报告指定字符在此字符串中的第一个匹配项的索引，忽略 C# 字符串值。
        /// </summary>
        /// <param name="input">要搜索的字符串。</param>
        /// <param name="value">要查找的字符。</param>
        /// <returns>如果找到该字符，则为 value 的从零开始的索引位置；如果未找到，则为 -1。</returns>
        public static int IndexOfIgnoreStringValue(string input, char value)
        {
            if (string.IsNullOrEmpty(input))
            {
                return -1;
            }

            // 判定范围的标记表, true 为需判断的字符
            var flagMap = IgnoreStringValueFlagMap(input);

            // 第二次遍历查找字符
            for (int i = 0; i < input.Length; i++)
            {
                if (!flagMap[i]) continue;
                char c = input[i];

                if (c == value)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// 报告指定字符在此字符串中的最后一个匹配项的索引，忽略 C# 字符串值。
        /// </summary>
        /// <param name="input">要搜索的字符串。</param>
        /// <param name="value">要查找的字符。</param>
        /// <returns>如果找到该字符，则为 value 的从零开始的索引位置；如果未找到，则为 -1。</returns>
        public static int LastIndexOfIgnoreStringValue(string input, char value)
        {
            if (string.IsNullOrEmpty(input))
            {
                return -1;
            }

            // 判定范围的标记表, true 为需判断的字符
            var flagMap = IgnoreStringValueFlagMap(input);

            // 第二次遍历查找字符
            // 从后往前遍历
            for (int i = input.Length - 1; i >= 0; i--)
            {
                if (!flagMap[i]) continue;
                char c = input[i];

                if (c == value)
                {
                    return i;
                }
            }

            return -1;
        }
        private static bool[] IgnoreStringValueFlagMap(string input)
        {
            bool[] flagMap = new bool[input.Length];

            const char quoteChar = '"';
            const char escapeChar = '\\';
            bool inQuotes = false;
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (inQuotes)
                {
                    switch (c)
                    {
                        case quoteChar:
                            inQuotes = false;
                            break;
                        case escapeChar:
                            i++;
                            break;
                    }
                }
                else
                {
                    switch (c)
                    {
                        case quoteChar:
                            inQuotes = true;
                            break;
                        default:
                            flagMap[i] = true;
                            break;
                    }
                }
            }
            if (inQuotes)
                throw new InvalidOperationException("字符串内的双引号未正确闭合! ");
            return flagMap;
        }

        #endregion
    }
}
