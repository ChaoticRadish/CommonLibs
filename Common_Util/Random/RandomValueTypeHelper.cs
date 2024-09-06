using System;
using System.Collections.Generic;
using System.Text;

namespace Common_Util.Random
{
    public static class RandomValueTypeHelper
    {
        private static readonly System.Random DefaultRandom = new();

        /// <summary>
        /// 随机布尔值
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        public static bool RandomBool(System.Random? random = null)
        {
            random ??= DefaultRandom;
            return random.Next(0, 2) == 0;
        }
        /// <summary>
        /// 按指定的概率返回true
        /// </summary>
        /// <param name="random"></param>
        /// <param name="probability"></param>
        /// <returns></returns>
        public static bool RandomTrue(double probability = 0.25, System.Random? random = null)
        {
            random ??= DefaultRandom;
            return random.NextDouble() < probability;
        }
        /// <summary>
        /// 按指定的概率返回true
        /// </summary>
        /// <param name="probability"></param>
        /// <returns></returns>
        public static bool RandomTrue(double probability = 0.25)
        {
            return DefaultRandom.NextDouble() < probability;
        }

        /// <summary>
        /// 指定区间内的整数
        /// </summary>
        /// <param name="random"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int RandomInt(int min, int max, System.Random? random = null)
        {
            random ??= DefaultRandom;
            if (min > max) (min, max) = (max, min);
            return min == max ? min : random.Next(min, max);
        }
        /// <summary>
        /// 指定区间内的整数
        /// </summary>
        /// <param name="random"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int GetInt(int min, int max)
        {
            if (min > max) (min, max) = (max, min);
            return min == max ? min : DefaultRandom.Next(min, max);
        }
        /// <summary>
        /// 指定区间内的整数
        /// </summary>
        /// <param name="random"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int GetInt(int min, int max, System.Random? random = null)
        {
            random ??= DefaultRandom;
            if (min > max) (min, max) = (max, min);
            return min == max ? min : random.Next(min, max);
        }
        /// <summary>
        /// 指定区间内的非负整数
        /// </summary>
        /// <param name="random"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int GetNonnegativeInt(int min, int max, System.Random? random = null)
        {
            random ??= DefaultRandom;
            if (min < 0)
            {
                min = 0;
            }
            if (max < 0)
            {
                max = 0;
            }
            if (min > max) (min, max) = (max, min);
            return min == max ? min : random.Next(min, max);
        }

        /// <summary>
        /// 指定区间内的long
        /// </summary>
        /// <param name="random"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static long GetLong(long min, long max, System.Random? random = null)
        {
            random ??= DefaultRandom;
            if (min > max) (min, max) = (max, min);
            return min == max ? min : min + (long)((max - min) * random.NextDouble());
        }
        /// <summary>
        /// 指定区间内的double
        /// </summary>
        /// <param name="random"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static double GetDouble(double min, double max, System.Random? random = null)
        {
            random ??= DefaultRandom;
            if (min > max) (min, max) = (max, min);
            return min == max ? min : random.NextDouble() * (max - min) + min;
        }
        /// <summary>
        /// 指定区间内的float
        /// </summary>
        /// <param name="random"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static float GetFloat(float min, float max, System.Random? random = null)
        {
            random ??= DefaultRandom;
            if (min > max) (min, max) = (max, min);
            return min == max ? min : (float)random.NextDouble() * (max - min) + min;
        }


        #region 默认范围

        public static double DefaultDoubleRandomMin = 0;
        public static double DefaultDoubleRandomMax = 100;

        public static int DefaultIntRandomMin = 0;
        public static int DefaultIntRandomMax = 100;

        public static long DefaultLongRandomMin = 0;
        public static long DefaultLongRandomMax = 100;

        public static float DefaultFloatRandomMin = 0;
        public static float DefaultFloatRandomMax = 100;
        #endregion
        /// <summary>
        /// 使用默认的值类型随机范围生成随机值类型数据
        /// </summary>
        /// <param name="type"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static object? Random(Type type, System.Random? random = null)
        {
            if (!type.IsValueType)
            {
                throw new ArgumentException("输入类型不是值类型", nameof(type));
            }
            random ??= DefaultRandom;
            if (type == typeof(double))
            {
                return GetDouble(DefaultDoubleRandomMin, DefaultDoubleRandomMax, random);
            }
            if (type == typeof(int))
            {
                return GetInt(DefaultIntRandomMin, DefaultIntRandomMax, random);
            }
            if (type == typeof(float))
            {
                return GetFloat(DefaultFloatRandomMin, DefaultFloatRandomMax, random);
            }
            if (type == typeof(long))
            {
                return GetLong(DefaultLongRandomMin, DefaultLongRandomMax, random);
            }
            else
            {
                throw new NotImplementedException($"未实现类型 ({type.Name}) 对应的生成方法");
            }
        }
    }
}
