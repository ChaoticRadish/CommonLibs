using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Input
{
    /// <summary>
    /// 输入数值修正
    /// </summary>
    public class ValueRevision
    {
        /// <summary>
        /// 将数值修正到指定区间内[start, end]
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static int ToRange(ref int input, int start = 0, int end = int.MaxValue)
        {
            if (input < start)
            {
                input = start;
            }
            else if (input > end)
            {
                input = end;
            }
            return input;
        }
        /// <summary>
        /// 取得将数值修正到指定区间内[start, end]后的值
        /// </summary>
        /// <param name="input"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static int ToRange(int input, int start = 0, int end = int.MaxValue)
        {
            if (input < start)
            {
                input = start;
            }
            else if (input > end)
            {
                input = end;
            }
            return input;
        }

        #region 极值
        /// <summary>
        /// 取得输入值中数值最大的值
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="v3"></param>
        /// <returns></returns>
        public static float Max(float v1, float v2, float v3)
        {
            if (v1 > v2)
            {
                if (v2 > v3)
                {
                    return v1;
                }
                else
                {
                    if (v1 > v3)
                    {
                        return v1;
                    }
                    else
                    {
                        return v3;
                    }
                }
            }
            else
            {
                if (v1 > v3)
                {
                    return v2;
                }
                else
                {
                    if (v2 > v3)
                    {
                        return v2;
                    }
                    else
                    {
                        return v3;
                    }
                }
            }
        }
        /// <summary>
        /// 取得输入值中数值最小的值
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="v3"></param>
        /// <returns></returns>
        public static float Min(float v1, float v2, float v3)
        {
            if (v1 < v2)
            {
                if (v2 < v3)
                {
                    return v1;
                }
                else
                {
                    if (v1 < v3)
                    {
                        return v1;
                    }
                    else
                    {
                        return v3;
                    }
                }
            }
            else
            {
                if (v1 < v3)
                {
                    return v2;
                }
                else
                {
                    if (v2 < v3)
                    {
                        return v2;
                    }
                    else
                    {
                        return v3;
                    }
                }
            }
        }

        #endregion


        #region 取整
        /// <summary>
        /// 根据步长向上取整
        /// </summary>
        /// <param name="input">输入值</param>
        /// <param name="step">步长</param>
        /// <returns></returns>
        public static double CeilByStep(double input, double step)
        {
            if (input == 0)
            {
                return 0;
            }
            if (input % step == 0)
            {
                return input;
            }
            else
            {
                int count = (int)(input / step);
                if (input > 0)
                {
                    return (count + 1) * step;
                }
                else
                {
                    return (count - 1) * step;
                }
            }
        }
        #endregion


        #region 文件名
        private const string WINDOW_FILEPATH_ILLEGAL_CHARS = "/\\:*?\"<>|";

        /// <summary>
        /// 将输入字符串中的window系统文件名不可用字符替换为输入的字符
        /// </summary>
        /// <param name="input"></param>
        /// <param name="toChar"></param>
        /// <returns></returns>
        public static string ReplaceWindowIllegalChar(string input, char toChar = ' ')
        {
            if (string.IsNullOrEmpty(input)) return input;
            StringBuilder builder = new StringBuilder(input);
            foreach (char c in WINDOW_FILEPATH_ILLEGAL_CHARS)
            {
                builder.Replace(c, toChar);
            }
            return builder.ToString();
        }
        /// <summary>
        /// 将输入字符串中的window系统文件名不可用字符替换为输入的字符串
        /// </summary>
        /// <param name="input"></param>
        /// <param name="toString"></param>
        /// <returns></returns>
        public static string ReplaceWindowIllegalChar(string input, string toString = " ")
        {
            if (string.IsNullOrEmpty(input)) return input;
            StringBuilder builder = new StringBuilder(input);
            foreach (char c in WINDOW_FILEPATH_ILLEGAL_CHARS)
            {
                builder.Replace(c.ToString(), toString);
            }
            return builder.ToString();
        }
        #endregion
    }
}
