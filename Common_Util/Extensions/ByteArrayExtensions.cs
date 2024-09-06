using Common_Util.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Extensions
{
    /// <summary>
    /// Byte[]的扩展方法
    /// </summary>
    public static class ByteArrayExtensions
    {
        #region 对象转换
        /// <summary>
        /// 将byte[]转换为T类型的数组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static T[] ToObject<T>(this byte[] data)
        {
            int size = Marshal.SizeOf<T>();
            int count = data.Length / size;
            T[] output = new T[count];
            IntPtr buffer = Marshal.AllocHGlobal(size);
            try
            {
                for (int i = 0; i < count; i++)
                {
                    Marshal.Copy(data, i * size, buffer, size);
                    var obj = Marshal.PtrToStructure<T>(buffer);
                    if (obj == null)
                    {
                        throw new NullReferenceException($"尝试将数据段 (start:{i*size}, length:{size}) 转换为 {typeof(T).Name} 失败! 取得 null 结果");
                    }
                    output[i] = obj;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
            return output;
        }

        #endregion

        #region 转换字符串

        /// <summary>
        /// 将字节数组转换为16进制格式(保留2个字符)的字符串, 使用输入字符分割各个值
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string ToHexString(this byte[] data, string split = " ")
        {
            if (data == null || data.Length == 0)
            {
                return string.Empty;
            }
            split ??= "";
            
            StringBuilder stringBuilder= new(data.Length * 2 + split.Length * (data.Length - 1));
            for (int i = 0; i < data.Length; i++)
            {
                byte b = data[i];
                stringBuilder.Append(Convert.ToString(b, 16).ToUpper().PadLeft(2, '0'));
                if (i != data.Length - 1 && split != "")
                {
                    stringBuilder.Append(split);
                }
            }
            return stringBuilder.ToString() ?? string.Empty;
        }
        public static string ToHexString(this IEnumerable<byte> data, string split = " ")
        {
            return ToHexString(data.ToArray(), split);
        }

        #endregion

        #region 裁切
        /// <summary>
        /// 按位置裁切一段指定长度的数据
        /// </summary>
        /// <param name="data"></param>
        /// <param name="targetLength"></param>
        /// <param name="subMode">首: 索引较小的一端; 尾: 索引较大的一端</param>
        /// <returns></returns>
        public static byte[] Sub(this byte[] data, int targetLength, HeadTailEnum subMode)
        {
            if (data.Length == 0)
            {
                return [];
            }
            if (data.Length <= targetLength)
            {
                return data[..];
            }
            switch (subMode)
            {
                default:
                case HeadTailEnum.Head:
                    return data[..targetLength];
                case HeadTailEnum.Center:
                    int a = data.Length - targetLength;
                    int h = a / 2;
                    int t = a - h;
                    return data[h..^t];
                case HeadTailEnum.Tail:
                    return data[^targetLength..];
            }
        }

        /// <summary>
        /// 分别修剪首尾两端的数据, 直到遇到不需要修剪的值, 返回一个新的修剪过的数组
        /// </summary>
        /// <param name="data"></param>
        /// <param name="trimValue"></param>
        /// <returns></returns>
        public static byte[] Trim(this byte[] data, params byte[] trimValue)
        {
            if (data == null || data.Length == 0)
            {
                return [];
            }
            if (trimValue == null || trimValue.Length == 0)
            {
                return data[..];
            }

            int subStart = 0;
            for (int i = 0; i < data.Length; ++i)
            {
                if (!trimValue.Contains(data[i]))
                {
                    break;
                }
                subStart++;
            }
            if (subStart == data.Length) return [];
            int subEnd = 0;
            for (int i = data.Length - 1; i >= 0; i--)
            {
                if (i == subStart)
                {
                    return [];
                }
                if (!trimValue.Contains(data[i]))
                {
                    break;
                }
                subEnd++;
            }
            if (subStart == data.Length) return [];
            return data[subStart..^subEnd];
        }
        /// <summary>
        /// 从前往后修剪数组, 修剪掉传入的数值, 直到遇到不需要修剪的值, 返回一个新的修剪过的数组
        /// </summary>
        /// <param name="data"></param>
        /// <param name="trimValue">需要修剪掉的数值</param>
        /// <returns></returns>
        public static byte[] TrimStart(this byte[] data, params byte[] trimValue)
        {
            if (data == null || data.Length == 0)
            {
                return [];
            }
            if (trimValue == null || trimValue.Length == 0)
            {
                return data[..];
            }

            int subStart = 0;
            for (int i = 0; i < data.Length; ++i)
            {
                if (!trimValue.Contains(data[i]))
                {
                    break;
                }
                subStart++;
            }
            return data[subStart..];
        }

        /// <summary>
        /// 从后往前修剪数组, 修剪掉传入的数值, 直到遇到不需要修剪的值, 返回一个新的修剪过的数组
        /// </summary>
        /// <param name="data"></param>
        /// <param name="trimValue">需要修剪掉的数值</param>
        /// <returns></returns>
        public static byte[] TrimEnd(this byte[] data, params byte[] trimValue)
        {
            if (data == null || data.Length == 0)
            {
                return [];
            }
            if (trimValue == null || trimValue.Length == 0)
            {
                return data[..];
            }


            int subEnd = 0;
            for (int i = data.Length - 1; i >= 0; i--)
            {
                if (!trimValue.Contains(data[i]))
                {
                    break;
                }
                subEnd++;
            }
            return data[..^subEnd];
        }
        #endregion

    }
}
