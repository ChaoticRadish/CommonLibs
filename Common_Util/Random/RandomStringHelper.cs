using System;
using System.Collections.Generic;
using System.Text;

namespace Common_Util.Random
{
    /// <summary>
    /// 随机字符串获取类
    /// </summary>
    public static class RandomStringHelper
    {
        #region 静态
        static RandomStringHelper()
        {
            EnglishLowercases = new List<char>();
            EnglishUppercases = new List<char>();
            EnglishLetters = new List<char>();
            // 遍历小写字母
            for (char c = 'a'; c <= 'z'; c++)
            {
                EnglishLowercases.Add(c);
                EnglishLetters.Add(c);
            }
            // 遍历大写字母
            for (char c = 'A'; c <= 'Z'; c++)
            {
                EnglishUppercases.Add(c);
                EnglishLetters.Add(c);
            }
        }

        /// <summary>
        /// 英文小写字母列表
        /// </summary>
        public static List<char> EnglishLowercases { get; private set; }
        /// <summary>
        /// 英文大写字母列表
        /// </summary>
        public static List<char> EnglishUppercases { get; private set; }
        /// <summary>
        /// 英文字母列表
        /// </summary>
        public static List<char> EnglishLetters { get; private set; }

        #endregion

        /// <summary>
        /// 获取随机的英文小写字母字符串
        /// </summary>
        /// <param name="length"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static string GetRandomLowerEnglishString(int length, System.Random? random = null)
        {
            return new RandomCharSplicer(EnglishLowercases, random).Get(length);
        }
        /// <summary>
        /// 获取随机的英文大写字母字符串
        /// </summary>
        /// <param name="length"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static string GetRandomUpperEnglishString(int length, System.Random? random = null)
        {
            return new RandomCharSplicer(EnglishUppercases, random).Get(length);
        }
        /// <summary>
        /// 获取随机的英文字母字符串
        /// </summary>
        /// <param name="length"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static string GetRandomEnglishString(int length, System.Random? random = null)
        {
            return new RandomCharSplicer(EnglishLetters, random).Get(length);
        }
    }
}
