using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Common_Util.String
{
    public static class StringHelper
    {
        #region 相识度比较


        /// <summary>
        /// 一致时的相似度
        /// </summary>
        public const float SAME_SIMILARITY_VALUE = 7;
        /// <summary>
        /// 是首部的相似度
        /// </summary>
        public const float START_SIMILARITY_VALUE = 4;
        /// <summary>
        /// 是尾部的相似度
        /// </summary>
        public const float END_SIMILARITY_VALUE = 3;
        /// <summary>
        /// 包含的相似度
        /// </summary>
        public const float CONATIN_SIMILARITY_VALUE = 1;
        /// <summary>
        /// 单位长度的相似度差值
        /// </summary>
        public const float DETAIL_LENGTH_SIMILARITY_VALUE = 0.1f;

        /// <summary>
        /// 默认的高相似度的值
        /// </summary>
        public const float DEFAULT_BIG_SIMILARITY_VALUE = 100;
        /// <summary>
        /// 默认的无相似度的值
        /// </summary>
        public const float DEFAULT_NOT_SIMILARITY_VALUE = 0;


        /// <summary>
        /// 比较输入值与参照值的相似度
        /// </summary>
        /// <param name="input"></param>
        /// <param name="reference">参照值</param>
        /// <param name="sameValueScale">相等值的补正系数</param>
        /// <param name="lengthCheck">参照值</param>
        /// <returns>越大越相似</returns>
        public static float Similarity(string input, string reference, float sameValueScale = 1f, bool lengthCheck = true)
        {
            if ((string.IsNullOrEmpty(reference) && !string.IsNullOrEmpty(input))
                || (!string.IsNullOrEmpty(reference) && string.IsNullOrEmpty(input)))
            {// 一个是空的, 另一个不是
                return DEFAULT_NOT_SIMILARITY_VALUE;
            }
            float output = DEFAULT_NOT_SIMILARITY_VALUE;
            if (string.IsNullOrEmpty(reference) && string.IsNullOrEmpty(input))
            {// 两者均为空
                return SAME_SIMILARITY_VALUE;
            }
            else if (reference.Equals(input))
            {
                output = SAME_SIMILARITY_VALUE * sameValueScale;
            }
            else if (reference.StartsWith(input) || input.StartsWith(reference))
            {
                output = START_SIMILARITY_VALUE;
            }
            else if (reference.EndsWith(input) || input.EndsWith(reference))
            {
                output = END_SIMILARITY_VALUE;
            }
            else if (reference.Contains(input) || input.Contains(reference))
            {
                output = CONATIN_SIMILARITY_VALUE;
            }
            // 长度偏差
            if (lengthCheck && output > CONATIN_SIMILARITY_VALUE)
            {
                output += (float)Math.Pow(1 - DETAIL_LENGTH_SIMILARITY_VALUE, Math.Abs(input.Length - reference.Length));
            }

            return output;
        }
        /// <summary>
        /// 比较输入值与参照值的相似度
        /// </summary>
        /// <param name="input"></param>
        /// <param name="references">参照值列表</param>
        /// <param name="sameValueScale">相等值的补正系数</param>
        /// <param name="lengthCheck">参照值</param>
        /// <returns>越大越相似</returns>
        public static float Similarity(string input, IEnumerable<string> references, float sameValueScale = 1f, bool lengthCheck = true)
        {
            float output = 0;
            references = references.Where(s => !string.IsNullOrEmpty(s)).Distinct();
            foreach (string reference in references)
            {
                output += Similarity(input, reference, sameValueScale, lengthCheck);
            }
            return output;
        }


        /// <summary>
        /// 比较输入值中, 哪一项与输入值的相识度最高
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inputs"></param>
        /// <param name="reference"></param>
        /// <returns>Value1: 文本值, Value2: 相似度, Value3: 绑定对象</returns>
        public static (string text, float similarityValue, T relateObj)? MostSimilarity<T>(
            List<KeyValuePair<string, T>> inputs, string reference)
        {
            if (inputs == null || inputs.Count == 0) return null;

            Dictionary<string, float> similarityValues = new Dictionary<string, float>();
            foreach (KeyValuePair<string, T> input in inputs)
            {
                similarityValues.Add(input.Key, Similarity(input.Key, reference));
            }
            var most = similarityValues.ToList()
                .OrderByDescending(item => item.Value).Take(1).First();
            string s = most.Key;
            float v = most.Value;
            T obj = inputs.First(i => i.Key == s).Value;
            return (text: s, similarityValue: v, relateObj: obj);
        }
        #endregion


        #region 长度控制
        /// <summary>
        /// 将文本转换为预览文本
        /// </summary>
        /// <param name="input"></param>
        /// <param name="length">长度显示</param>
        /// <param name="endString">超出长度之后将多余的部分替换为此字符串</param>
        /// <returns></returns>
        public static string Preview(string input, int length = 50, string endString = "...")
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }
            // 输入长度值检查
            length = length < 0 ? 0 : length;
            endString = endString ?? "";

            if (input.Length > length)
            {
                if (endString.Length > length)
                {
                    return endString.Substring(0, length);
                }
                else
                {
                    StringBuilder builder = new StringBuilder();
                    builder.Append(input, 0, length - endString.Length).Append(endString);
                    return builder.ToString();
                }
            }
            else
            {
                return input;
            }
        }
        #endregion


        #region 拼接
        /// <summary>
        /// 拼接字符串
        /// </summary>
        /// <param name="inputs"></param>
        /// <param name="split">分隔符</param>
        /// <param name="lastSplit">是否保留最后一个分隔符</param>
        /// <returns></returns>
        public static string Concat(IList<string> inputs, string split = "\n", bool lastSplit = false)
        {
            if (inputs == null) return string.Empty;

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < inputs.Count; i++)
            {
                sb.Append(inputs[i]);
                if (i < inputs.Count - 1)
                {
                    sb.Append(split);
                }
                else if (lastSplit)
                {
                    sb.Append(split);
                }
            }
            return sb.ToString();
        }
        /// <summary>
        /// 拼接字符串, 将各个子字符串使用输入的参数包裹 (如默认使用方括号包裹)
        /// </summary>
        /// <param name="inputs"></param>
        /// <param name="split">分隔符</param>
        /// <param name="lastSplit">是否保留最后一个分隔符</param>
        /// <returns></returns>
        public static string ConcatAndWrap(
            List<string?> inputs,
            string split = ", ", string left = "[", string right = "]",
            bool lastSplit = false)
        {
            if (inputs == null) return string.Empty;

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < inputs.Count; i++)
            {
                sb.Append(left).Append(inputs[i]).Append(right);
                if (i < inputs.Count - 1)
                {
                    sb.Append(split);
                }
                else if (lastSplit)
                {
                    sb.Append(split);
                }
            }
            return sb.ToString();
        }
        #endregion

        #region 切分
        /// <summary>
        /// 分割字符串, 然后过滤掉空字符串
        /// </summary>
        /// <param name="input"></param>
        /// <param name="split"></param>
        /// <returns></returns>
        public static List<string> Split(string input, char split = ' ')
        {
            return input.Split(split).Where(s => !string.IsNullOrEmpty(s)).ToList();
        }
        /// <summary>
        /// 使用输入的字符串切分, 然后过滤掉空字符串
        /// </summary>
        /// <param name="input"></param>
        /// <param name="splits">标志分割位的字符, 将会视为多个字符来使用, 而不是作为字符串使用</param>
        /// <param name="doSomething">分割完之后对切分得到的字符串做一些处理</param>
        /// <returns></returns>
        public static List<string> SplitMultiChar(
            string input,
            string splits = " ,;，。\n\t:",
            Func<string, string>? doSomething = null)
        {
            IEnumerable<string> strArr = input.Split(splits.ToArray());
            if (doSomething != null)
            {
                strArr = strArr.Select(s => doSomething.Invoke(s));
            }
            return strArr.Where(s => !string.IsNullOrEmpty(s)).ToList();
        }
        #endregion

        #region 换行

        /// <summary>
        /// 每隔一定间距, 插入一个换行符
        /// </summary>
        /// <param name="str"></param>
        /// <param name="lineWidth"></param>
        /// <returns></returns>
        public static string SplitLine(string str, int lineWidth = 60)
        {
            if (str.Length > lineWidth)
            {
                StringBuilder sb = new();
                for (int i = 0; i < str.Length; i++)
                {
                    sb.Append(str[i]);
                    if ((i + 1) % 50 == 0 && i != str.Length - 1)
                    {
                        sb.AppendLine();
                    }
                }
                str = sb.ToString();
            }
            return str;
        }

        #endregion

        #region 类型字符串
        /// <summary>
        /// 取得类型字符串, 会对一些特殊的类型做一点处理, 比如Nullable<>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetTypeString(Type type)
        {
            string output = type.FullName ?? type.Name;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                output = $"Nullable<{type.GetGenericArguments()[0].FullName}>";
            }
            return output;
        }
        #endregion

        #region 字节
        /// <summary>
        /// 将字符串转换为byte数组
        /// </summary>
        /// <param name="str"></param>
        /// <param name="encoding">字符集, 默认使用 <see cref="Encoding.ASCII"/></param>
        /// <returns></returns>
        public static byte[] ToByteArray(string str, Encoding? encoding = null)
        {
            encoding = encoding ?? Encoding.ASCII;
            return encoding.GetBytes(str);
        }
        #endregion

        #region 整理字符串
        /// <summary>
        /// 将输入的字符串整理成键值对列表
        /// <para>输入的第一个值作为键, 第二个值作为值, 第三个值作为键, 第四个值作为值...以此类推</para>
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static List<KeyValuePair<string?, string?>> CreatePairList(params string?[] str)
        {
            List<KeyValuePair<string?, string?>> pairList = new();
            if (str != null && str.Length > 0)
            {
                for (int i = 0; i < str.Length / 2; i++)
                {
                    pairList.Add(new KeyValuePair<string?, string?>(str[i * 2], str[i * 2 + 1]));
                }
                if (str.Length % 2 != 0)
                {// 奇数的字符串数量
                    pairList.Add(new KeyValuePair<string?, string?>(str[^1], null));
                }
            }
            return pairList;
        }
        #endregion

        #region 空格字符串
        /// <summary>
        /// 取得指定长度的空格字符串
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetBlankSpaceString(int length)
        {
            if (length <= 0)
            {
                return "";
            }
            else
            {
                char[] c = new char[length];
                for (int i = 0; i < length; i++)
                {
                    c[i] = ' ';
                }
                return new string(c);
            }
        }
        /// <summary>
        /// 取得长度与输入字符串长度一致的空格字符串
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetBlankSpaceString(string input)
        {
            return input == null ? "" : GetBlankSpaceString(input.Length);
        }
        /// <summary>
        /// 填充字符到字符串, 使其长度达到指定长度
        /// </summary>
        /// <param name="input"></param>
        /// <param name="length"></param>
        /// <param name="leftScale">向左填充字符串的比例</param>
        /// <returns></returns>
        public static string FillBlankSpace(string input, int length, double leftScale = 0.5)
        {
            if (string.IsNullOrEmpty(input))
            {
                return GetBlankSpaceString(length);
            }
            else
            {
                if (input.Length > length)
                {
                    return input;
                }
                else
                {
                    int deltaLength = length - input.Length;
                    int leftCount = (int)(leftScale * deltaLength);
                    int rightCount = deltaLength - leftCount;
                    return new StringBuilder()
                        .Append(GetBlankSpaceString(leftCount))
                        .Append(input)
                        .Append(GetBlankSpaceString(rightCount))
                        .ToString();
                }
            }
        }

        #endregion


        #region 数据量字符串

        /// <summary>
        /// 取得进位后的字符串, 返回结果: 输入数量使用输入的进制, 尽量进位后得到的值(保留指定小数位数), 加上当前位对应的单位字符串(中间用一个空格分割)
        /// </summary>
        /// <param name="total"></param>
        /// <param name="reserved">保留小数位数</param>
        /// <param name="scale">进制</param>
        /// <param name="units">单位, null时默认使用 K, M, G... 这一些单位</param>
        /// <returns></returns>
        public static string GetCarryString(long total, int reserved = 2, int scale = 1024, string[]? units = null)
        {
            units ??= UnitsOfMeasure;

            int unitIndex = 0;  // 单位索引
            double valueThis = total;    // 当前单位下的数值
            while (valueThis > scale && unitIndex < units.Length)
            {
                valueThis /= scale;
                unitIndex++;    // 单位索引增加
            }
            string output = $"{valueThis.ToString($"f{(reserved >= 0 ? reserved : 0)}")} {units[unitIndex]}";
            return output;
        }
        /// <summary>
        /// 存储单位, 从小到大
        /// </summary>
        public static readonly string[] UnitsOfMeasure =
        [
            "", "K", "M", "G", "T", "P", "E", "Z", "Y", "B", "N", "D"
        ];

        #endregion
    }
}
