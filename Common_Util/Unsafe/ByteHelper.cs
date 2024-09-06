using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Unsafe
{
    /// <summary>
    /// 字节数据帮助类
    /// </summary>
    public static class ByteHelper
    {
        #region 枚举
        /// <summary>
        /// 字节序 端序, 表示字节的存储顺序
        /// </summary>
        public enum EndianEnum
        {
            /// <summary>
            /// 大端, 数据的低字节保存在内存的高地址中
            /// </summary>
            Big,
            /// <summary>
            /// 小端, 数据的低字节保存在内存的低地址中
            /// </summary>
            Little,
        }
        #endregion

        #region 指针操作
        /// <summary>
        /// 将指定位置的数据读取为指定类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <param name="endian"></param>
        /// <returns></returns>
        public unsafe static T ReadAs<T>(
            byte* data, int start, int length, 
            EndianEnum endian = EndianEnum.Big) where T : struct
        {
            IntPtr ptr = Marshal.AllocHGlobal(length);
            try
            {
                bool big = endian == EndianEnum.Big;
                for (int i = start; i < start + length; i++)
                {
                    // 遍历方向: 低地址 => 高地址
                    byte b = *(data + i);
                    if (big)
                    {
                        Marshal.WriteByte(ptr + (length - 1 - i), b);
                    }
                    else
                    {
                        Marshal.WriteByte(ptr + i, b);
                    }
                }
                return Marshal.PtrToStructure<T>(ptr);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }
        /// <summary>
        /// 将数据写入指定的位置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <param name="value"></param>
        /// <param name="endian"></param>
        /// <param name="writeValueOverSize">预备写入区域如果超过数据大小, 需要填充什么值, 参数为空时不覆盖</param>
        public unsafe static void Write<T>(
            byte* data, int start, int length, T value, 
            EndianEnum endian = EndianEnum.Big, 
            byte? writeValueOverSize = 0x00) where T : struct
        {
            bool big = endian == EndianEnum.Big;

            int size = Marshal.SizeOf(value);
            IntPtr buffer = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(value, buffer, false);
            byte* ptr = (byte*)buffer.ToPointer();
            try
            {
                int min = length > size ? size : length;
                for (int i = 0; i < min; i++)
                {
                    // 遍历方向: 低地址 => 高地址
                    byte b = *(ptr + i);

                    if (big)
                    {
                        byte* temp = data + start + (length - 1 - i);
                        *temp = b;
                    }
                    else
                    {
                        byte* temp = data + start + i;
                        *temp = b;
                    }
                }
                if (length > size && writeValueOverSize != null)
                {
                    byte full = writeValueOverSize.Value;
                    for (int i = size; i < length; i++)
                    {
                        if (big)
                        {
                            byte* temp = data + start + (length - 1 - i);
                            temp = &full;
                        }
                        else
                        {
                            byte* temp = data + start + i;
                            temp = &full;
                        }
                    }
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
        #endregion
    }
}
