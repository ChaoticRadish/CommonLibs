using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.String
{
    /// <summary>
    /// MD5的helper类, 全部使用UTF8
    /// </summary>
    public static class MD5Helper
    {

        /// <summary>
        /// 计算MD5码, byte[]直接转换为Utf-8字符串
        /// </summary>
        /// <param name="input"></param>
        /// <returns>byte[16] 16位的Hash值直接使用Utf-8格式转换后的结果</returns>
        public static string MD5(string input)
        {
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            return System.Text.Encoding.UTF8.GetString(md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input)));
        }

        /// <summary>
        /// 转换MD5码, 由32位的16进制Hash字符串, 转化成16位的Hash值, Utf-8格式的字符串
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Convert_Str32ToUTF8(string input)
        {
            string lower = input.ToLower();
            byte[] hash = new byte[16];

            for (int i = 0; i < 16; i++)
            {
                byte b;

                char c = lower[i * 2];
                if (c >= '0' && c <= '9')
                {
                    b = (byte)((c - '0') * 16);
                }
                else
                {
                    b = (byte)((c - 'a' + 10) * 16);
                }
                c = lower[i * 2 + 1];
                if (c >= '0' && c <= '9')
                {
                    b += (byte)(c - '0');
                }
                else
                {
                    b += (byte)(c - 'a' + 10);
                }

                hash[i] = b;
            }

            return System.Text.Encoding.UTF8.GetString(hash);
        }

        /// <summary>
        /// 计算32位MD5码
        /// </summary>
        /// <param name="input"></param>
        /// <param name="toUpper">哈希值格式, true为使用大写字母</param>
        /// <returns></returns>
        public static string MD5_32(string input, bool toUpper = true)
        {
            return MD5_32(System.Text.Encoding.UTF8.GetBytes(input));
        }

        /// <summary>
        /// 计算32位MD5码
        /// </summary>
        /// <param name="input"></param>
        /// <param name="toUpper">哈希值格式, true为使用大写字母</param>
        /// <returns></returns>
        public static string MD5_32(byte[] input, bool toUpper = true)
        {
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] hash = md5.ComputeHash(input);
            return toMD5String(hash, toUpper);
        }
        /// <summary>
        /// 计算32位MD5码
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="toUpper">哈希值格式, true为使用大写字母</param>
        /// <returns></returns>
        public static string MD5_32(Stream stream, bool toUpper = true)
        {
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] hash = md5.ComputeHash(stream);
            return toMD5String(hash, toUpper);
        }

        #region 输出转换
        private static string toMD5String(byte[] hash, bool toUpper = true)
        {
            const int startOf_number = 48;    // 48: 0x30 数字0
            int startOf_letter = toUpper ? 65 : 97; // 65: 0x41 大写字母A
                                                    // 97: 0x61 小写字母a 

            StringBuilder output = new StringBuilder();
            for (int counter = 0; counter < hash.Length; counter++)
            {
                char temp;
                long i = hash[counter] / 16;
                if (i > 9)
                {
                    temp = (char)(i - 10 + startOf_letter);
                }
                else
                {
                    temp = (char)(i + startOf_number);
                }
                output.Append(temp);

                i = hash[counter] % 16;
                if (i > 9)
                {
                    temp = (char)(i - 10 + startOf_letter);
                }
                else
                {
                    temp = (char)(i + startOf_number);
                }
                output.Append(temp);
            }


            return output.ToString();
        }


        #endregion
    }
}
