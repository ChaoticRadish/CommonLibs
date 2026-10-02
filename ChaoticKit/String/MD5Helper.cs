using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaoticKit.String
{
    /// <summary>
    /// MD5 的 helper 类, 全部使用 UTF8
    /// </summary>
    public static class MD5Helper
    {

        /// <summary>
        /// 计算 MD5 哈希值, 并将 16 字节的哈希值直接按 UTF-8 解码为字符串
        /// </summary>
        /// <param name="input">原始字符串</param>
        /// <returns>byte[16] 16位的Hash值直接使用Utf-8格式转换后的结果</returns>
        public static string Md5Utf8String(string input)
        {
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            return System.Text.Encoding.UTF8.GetString(md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input)));
        }


        /// <summary>
        /// 将 32 个十六进制字符组成的 MD5 码, 还原成 16 字节的哈希值, 并按 UTF-8 解码为字符串
        /// </summary>
        /// <param name="input">32 个十六进制字符组成的 MD5 码 (大小写不敏感)</param>
        /// <returns>16 字节哈希值按 Utf-8 格式转换后的结果</returns>
        public static string ConvertMd5Digit32ToUtf8String(string input)
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


        #region 计算 32 位十六进制 MD5 码

        /// <summary>
        /// 计算 32 位十六进制 MD5 码 (输入字符串以 UTF-8 编码参与计算)
        /// </summary>
        /// <param name="input">原始字符串</param>
        /// <param name="toUpper">哈希值格式, true 为使用大写字母</param>
        /// <returns>32 个十六进制字符组成的哈希值</returns>
        public static string Md5Digit32(string input, bool toUpper = true)
        {
            return Md5Digit32(System.Text.Encoding.UTF8.GetBytes(input), toUpper);
        }

        /// <summary>
        /// 计算 32 位十六进制 MD5 码
        /// </summary>
        /// <param name="input">参与计算的字节数据</param>
        /// <param name="toUpper">哈希值格式, true 为使用大写字母</param>
        /// <returns>32 个十六进制字符组成的哈希值</returns>
        public static string Md5Digit32(byte[] input, bool toUpper = true)
        {
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] hash = md5.ComputeHash(input);
            return ToMd5Digit32String(hash, toUpper);
        }

        /// <summary>
        /// 计算 32 位十六进制 MD5 码
        /// </summary>
        /// <param name="stream">参与计算的流</param>
        /// <param name="toUpper">哈希值格式, true 为使用大写字母</param>
        /// <returns>32 个十六进制字符组成的哈希值</returns>
        public static string Md5Digit32(Stream stream, bool toUpper = true)
        {
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] hash = md5.ComputeHash(stream);
            return ToMd5Digit32String(hash, toUpper);
        }

        #endregion

        #region 已弃用 (保留以兼容既有调用, 内部转发到改名后的新方法)

        /// <summary>
        /// 计算MD5码, byte[]直接转换为Utf-8字符串
        /// </summary>
        /// <param name="input"></param>
        /// <returns>byte[16] 16位的Hash值直接使用Utf-8格式转换后的结果</returns>
        [Obsolete("已弃用, 请使用 Md5Utf8String(string) 代替。")]
        public static string MD5(string input)
        {
            return Md5Utf8String(input);
        }
        /// <summary>
        /// 转换MD5码, 由32位的16进制Hash字符串, 转化成16位的Hash值, Utf-8格式的字符串
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Obsolete("已弃用, 请使用 ConvertMd5Digit32ToUtf8String(string) 代替。")]
        public static string Convert_Str32ToUTF8(string input)
        {
            return ConvertMd5Digit32ToUtf8String(input);
        }

        /// <summary>
        /// 计算32位MD5码
        /// </summary>
        /// <param name="input"></param>
        /// <param name="toUpper">哈希值格式, true为使用大写字母</param>
        /// <returns></returns>
        [Obsolete("已弃用, 请使用 Md5Digit32(string, bool) 代替; 此重载曾忽略 toUpper 参数, 现已一并修正。")]
        public static string MD5_32(string input, bool toUpper = true)
        {
            return Md5Digit32(input, toUpper);
        }

        /// <summary>
        /// 计算32位MD5码
        /// </summary>
        /// <param name="input"></param>
        /// <param name="toUpper">哈希值格式, true为使用大写字母</param>
        /// <returns></returns>
        [Obsolete("已弃用, 请使用 Md5Digit32(byte[], bool) 代替。")]
        public static string MD5_32(byte[] input, bool toUpper = true)
        {
            return Md5Digit32(input, toUpper);
        }

        /// <summary>
        /// 计算32位MD5码
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="toUpper">哈希值格式, true为使用大写字母</param>
        /// <returns></returns>
        [Obsolete("已弃用, 请使用 Md5Digit32(Stream, bool) 代替。")]
        public static string MD5_32(Stream stream, bool toUpper = true)
        {
            return Md5Digit32(stream, toUpper);
        }

        #endregion

        #region 输出转换
        private static string ToMd5Digit32String(byte[] hash, bool toUpper = true)
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
