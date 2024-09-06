using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Extensions
{
    public static class StructExtensions
    {
        /// <summary>
        /// 将结构体转换为Byte[]
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="structObj"></param>
        /// <returns></returns>
        public static byte[] ToByteArray<T>(this T structObj) where T : struct
        {
            int datasize = Marshal.SizeOf(typeof(T));
            IntPtr ptr = Marshal.AllocHGlobal(datasize);
            Marshal.StructureToPtr(structObj, ptr, false);
            byte[] data = new byte[datasize];
            Marshal.Copy(ptr, data, 0, datasize);
            Marshal.FreeHGlobal(ptr);
            return data;
        }

    }
}
