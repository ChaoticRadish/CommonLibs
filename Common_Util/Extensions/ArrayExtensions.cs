using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Extensions
{
    public static class ArrayExtensions
    {
        /// <summary>
        /// 判断数组是否为null或空(长度0)
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static bool IsEmpty([NotNullWhen(false)] this Array? array) 
        {
            return array == null || array.Length == 0;
        }

        #region 结构体数组
        /// <summary>
        /// 将输入的数组, 转换为 <see cref="byte[]"/>
        /// </summary>
        /// <typeparam name="T">
        /// 理论上应该需要是 <see langword="struct"/>, 
        /// 但是关键方法 <see cref="Marshal.StructureToPtr{T}(T, nint, bool)"/> 并没有这么限制, 故这里也不作限制
        /// </typeparam>
        /// <param name="array"></param>
        /// <returns></returns>
        public static byte[] ToBinary<T>(this T[] array)
        {
            int size = Marshal.SizeOf<T>();
            byte[] output = new byte[size * array.Length];
            IntPtr buffer = Marshal.AllocHGlobal(size);
            try
            {
                for (int i = 0; i < array.Length; i++)
                {
                    T t = array[i];
                    if (t != null)
                    {
                        Marshal.StructureToPtr(t, buffer, false);
                        Marshal.Copy(buffer, output, i * size, size);
                    }
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
            return output;
        }


        #endregion


    }
}
