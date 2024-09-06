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

    }
}
