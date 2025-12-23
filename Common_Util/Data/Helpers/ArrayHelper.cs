using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Helpers
{
    public static class ArrayHelper
    {
        /// <summary>
        /// 创建一个长度为 <paramref name="length"/> 的数组, 并使用 <paramref name="initValue"/> 作为其中所有位置的值
        /// </summary>
        /// <remarks>使用时需要注意当 <typeparamref name="T"/> 为引用类型时, 所有位置上都会是同一个对象的引用</remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="length"></param>
        /// <param name="initValue"></param>
        /// <returns></returns>
        public static T[] CreateArray<T>(int length, T initValue)
        {
            if (length <= 0) return [];
            T[] values = new T[length];
            for (int i = 0; i < length; i++)
            {
                values[i] = initValue;
            }
            return values;
        }
        /// <summary>
        /// 使用输入的工厂方法创建一个长度为 <paramref name="length"/> 的数组, 并使用指定的工厂方法初始化其中所有位置的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="length"></param>
        /// <param name="initValueFactory"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] CreateArray<T>(int length, Func<T> initValueFactory)
        {
            if (length <= 0) return [];
            T[] values = new T[length];
            for (int i = 0; i < length; i++)
            {
                values[i] = initValueFactory();
            }
            return values;
        }
        /// <summary>
        /// 使用输入的工厂方法创建一个长度为 <paramref name="length"/> 的数组, 并使用指定的工厂方法初始化其中所有位置的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="length"></param>
        /// <param name="initValueFactory">入参为数组即将填充位置的索引</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] CreateArray<T>(int length, Func<int, T> initValueFactory)
        {
            if (length <= 0) return [];
            T[] values = new T[length];
            for (int i = 0; i < length; i++)
            {
                values[i] = initValueFactory(i);
            }
            return values;
        }
    }
}
