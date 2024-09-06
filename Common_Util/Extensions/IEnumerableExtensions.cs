using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Extensions
{
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// 调用集合内的所有项的<see cref="IDisposable.Dispose"/>方法, 释放所有资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        public static void DisposeAll<T>(this IEnumerable<T> array) 
            where T : IDisposable
        {
            foreach (T t in array) t?.Dispose();
        }

        /// <summary>
        /// 将输入的数组, 转换为byte[]
        /// </summary>
        /// <typeparam name="T"></typeparam>
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

        /// <summary>
        /// 将byte[]转换为T类型的数组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static T[] ByteArrayToObject<T>(this byte[] data)
        {
            return ByteArrayExtensions.ToObject<T>(data);
        }

    }
}
