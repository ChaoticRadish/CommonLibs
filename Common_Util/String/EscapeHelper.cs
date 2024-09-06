using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.String
{
    /// <summary>
    /// 转义帮助类
    /// </summary>
    public static class EscapeHelper
    {
        /// <summary>
        /// 未转义的原始字符串 => 插入转义字符后的字符串
        /// <para>寻找输入字符串中的需转义字符, 在其前方插入转义字符</para>
        /// </summary>
        /// <param name="input"></param>
        /// <param name="escapeChar"></param>
        /// <param name="needEscapes"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string AddEscape(string input, char escapeChar, params char[] needEscapes)
        {
            if (input.IsEmpty()) return input;

            Span<char> needEscapeSpan = _tidyNeedEscapes(escapeChar, needEscapes);

            StringBuilder sb = new StringBuilder();
            foreach (char c in input)
            {
                if (needEscapeSpan.Contains(c))
                {
                    sb.Append(escapeChar);
                }
                sb.Append(c);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 插入转义字符后的字符串 => 未转义的原始字符串
        /// <para>寻找输入字符串中的发挥转义作用的转义字符并移除</para>
        /// <para>如果转义字符是最后一个字符, 也会被移除</para>
        /// </summary>
        /// <param name="input"></param>
        /// <param name="escapeChar"></param>
        /// <param name="needEscapes"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string RemoveEscape(string input, char escapeChar)
        {
            if (input.IsEmpty()) return input;

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (c == escapeChar)
                {
                    if (i == input.Length - 1)
                    {
                        // 最后一个字符是转义字符
                        break;
                    }
                    else
                    {
                        // 添加下一个字符, 然后跳过
                        sb.Append(input[i + 1]);
                        i++;
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }


        /// <summary>
        /// 遍历字符方法的委托
        /// </summary>
        /// <param name="c">当前字符</param>
        /// <param name="beEscape">此字符是否被转义</param>
        public delegate void ErgodicHandler(char c, bool beEscape);
        /// <summary>
        /// 遍历输入的字符串, 对字符逐一执行传入的方法
        /// </summary>
        /// <param name="input"></param>
        /// <param name="escapeChar"></param>
        public static void Ergodic(string input, char escapeChar, ErgodicHandler handler)
        {
            if (input.IsEmpty()) return;
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (c == escapeChar)
                {
                    if (i == input.Length - 1)
                    {
                        // 最后一个字符是转义字符
                        throw new ArgumentException($"无效的转义: 转义字符后无其他字符");
                    }
                    else
                    {
                        c = input[i + 1];
                        handler(c, true);
                        i++;
                    }
                }
                else
                {
                    handler(c, false);
                }
            }
        }

        /// <summary>
        /// 整理需转义字符
        /// </summary>
        /// <param name="escapeChar"></param>
        /// <param name="needEscapes"></param>
        /// <returns></returns>
        private static Span<char> _tidyNeedEscapes(char escapeChar, params char[] needEscapes)
        {
            // 整理需转义字符的数组, 确保不重复, 且包含转义字符
            char[] cArr = new char[needEscapes.Length + 1];
            cArr[0] = escapeChar;
            int totalChar = 1;
            bool existSame;
            for (int i = 0; i < needEscapes.Length; i++)
            {
                char inputNeed = needEscapes[i];
                existSame = false;
                for (int j = 0; j < totalChar; j++)
                {
                    if (inputNeed == cArr[j])
                    {
                        existSame = true;
                        break;
                    }
                }
                if (!existSame)
                {
                    cArr[totalChar] = inputNeed;
                    totalChar++;
                }
            }
            return new Span<char>(cArr, 0, totalChar); ;
        }
    }
}
