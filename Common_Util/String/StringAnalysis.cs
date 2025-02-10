using System;
using System.Collections.Generic;
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
    }
}
