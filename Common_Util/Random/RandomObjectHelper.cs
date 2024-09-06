using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Common_Util.Attributes.Random;
using Common_Util.Extensions;

namespace Common_Util.Random
{
    /// <summary>
    /// <para>本帮组类的制作情况: </para>
    /// <para>int属性: 完成</para>
    /// <para>bool属性: 完成</para>
    /// <para>array属性: 完成</para>
    /// <para>list属性: 完成</para>
    /// <para>double属性: 完成</para>
    /// <para>object属性: 完成</para>
    /// </summary>
    public static class RandomObjectHelper
    {
        #region 设置属性的值
        /// <summary>
        /// 为对象的指定属性赋随机值
        /// </summary>
        /// <param name="obj">被赋值对象</param>
        /// <param name="propertyInfo">属性</param>
        /// <param name="random"></param>
        public static void SetRandomValue(object obj, PropertyInfo propertyInfo, System.Random? random = null)
        {
            random ??= new System.Random();

            if (obj == null || propertyInfo == null)
            {
                return;
            }

            if (typeof(int) == propertyInfo.PropertyType)
            {
                SetRandomIntValue(obj, propertyInfo, random);
            }
            else if (typeof(bool) == propertyInfo.PropertyType)
            {
                propertyInfo.SetValue(obj, RandomValueTypeHelper.RandomBool(random), null);
            }
            else if (typeof(double) == propertyInfo.PropertyType)
            {
                SetRandomDoubleValue(obj, propertyInfo, random);
            }
            else if (typeof(float) == propertyInfo.PropertyType)
            {
                SetRandomFloatValue(obj, propertyInfo, random);
            }
            else if (typeof(long) == propertyInfo.PropertyType)
            {
                SetRandomLongValue(obj, propertyInfo, random);
            }
            else if (typeof(string) == propertyInfo.PropertyType)
            {
                SetRandomStringValue(obj, propertyInfo, random);
            }
            else if (typeof(IList).IsAssignableFrom(propertyInfo.PropertyType))
            {
                SetRandomListValue(obj, propertyInfo, random);
            }
            else if (propertyInfo.PropertyType.IsClass)
            {
                propertyInfo.SetValue(obj, GetObject(propertyInfo.PropertyType, random), null);
            }

        }

        #region 设置值的具体实现
        /// <summary>
        /// 为对象的指定属性赋随机值, 不会判断, 需要确保输入的参数不为空
        /// </summary>
        /// <param name="obj">被赋值对象</param>
        /// <param name="propertyInfo">属性</param>
        /// <param name="random"></param>
        private static void SetRandomIntValue(object obj, PropertyInfo propertyInfo, System.Random random)
        {
            // 取值范围
            int min = 0;
            int max = 100;

            IntRangeAttribute? range = propertyInfo.GetCustomAttribute<IntRangeAttribute>();
            if (range != null)
            {
                min = range.Min;
                max = range.Max;
            }
            propertyInfo.SetValue(obj, RandomValueTypeHelper.RandomInt(min, max, random), null);
        }
        /// <summary>
        /// 为对象的指定属性赋随机值, 不会判断, 需要确保输入的参数不为空
        /// </summary>
        /// <param name="obj">被赋值对象</param>
        /// <param name="propertyInfo">属性</param>
        /// <param name="random"></param>
        private static void SetRandomLongValue(object obj, PropertyInfo propertyInfo, System.Random random)
        {
            // 取值范围
            long min = 0;
            long max = 100;

            LongRangeAttribute? range = propertyInfo.GetCustomAttribute<LongRangeAttribute>();
            if (range != null)
            {
                min = range.Min;
                max = range.Max;
            }
            propertyInfo.SetValue(obj, RandomValueTypeHelper.GetLong(min, max, random), null);
        }
        /// <summary>
        /// 为对象的指定属性赋随机值, 不会判断, 需要确保输入的参数不为空
        /// </summary>
        /// <param name="obj">被赋值对象</param>
        /// <param name="propertyInfo">属性</param>
        /// <param name="random"></param>
        private static void SetRandomFloatValue(object obj, PropertyInfo propertyInfo, System.Random random)
        {
            // 取值范围
            float min = 0;
            float max = 100;

            FloatRangeAttribute? range = propertyInfo.GetCustomAttribute<FloatRangeAttribute>();
            if (range != null)
            {
                min = range.Min;
                max = range.Max;
            }
            propertyInfo.SetValue(obj, RandomValueTypeHelper.GetFloat(min, max, random), null);
        }
        /// <summary>
        /// 为对象的指定属性赋随机值, 不会判断, 需要确保输入的参数不为空
        /// </summary>
        /// <param name="obj">被赋值对象</param>
        /// <param name="propertyInfo">属性</param>
        /// <param name="random"></param>
        private static void SetRandomDoubleValue(object obj, PropertyInfo propertyInfo, System.Random random)
        {
            // 取值范围
            double min = 0;
            double max = 100;

            DoubleRangeAttribute? range = propertyInfo.GetCustomAttribute<DoubleRangeAttribute>();
            if (range != null)
            {
                min = range.Min;
                max = range.Max;
            }
            propertyInfo.SetValue(obj, RandomValueTypeHelper.GetDouble(min, max, random), null);
        }
        /// <summary>
        /// 为对象的指定属性赋随机值, 不会判断, 需要确保输入的参数不为空
        /// </summary>
        /// <param name="obj">被赋值对象</param>
        /// <param name="propertyInfo">属性</param>
        /// <param name="random"></param>
        private static void SetRandomStringValue(object obj, PropertyInfo propertyInfo, System.Random random)
        {
            // 取值范围
            int min = 0;
            int max = 100;

            CountRangeAttribute? range = propertyInfo.GetCustomAttribute<CountRangeAttribute>();
            if (range != null)
            {
                min = range.Min;
                max = range.Max;
            }
            propertyInfo.SetValue(
            obj,
            RandomStringHelper.GetRandomEnglishString(
            RandomValueTypeHelper.RandomInt(min, max, random),
            random),
            null);
        }
        /// <summary>
        /// 为对象的指定属性赋随机值, 不会判断, 需要确保输入的参数不为空
        /// </summary>
        /// <param name="obj">被赋值对象</param>
        /// <param name="propertyInfo">属性</param>
        /// <param name="random"></param>
        private static void SetRandomListValue(object obj, PropertyInfo propertyInfo, System.Random random)
        {
            // 取值范围
            int min = 0;
            int max = 100;
            CountRangeAttribute? range = propertyInfo.GetCustomAttribute<CountRangeAttribute>();
            if (range != null)
            {
                min = range.Min;
                max = range.Max;
            }
            propertyInfo.SetValue(obj, GetList(propertyInfo.PropertyType, random, min, max), null);
        }

        #endregion

        #endregion
        /// <summary>
        /// 取得随机的指定类型的对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="random"></param>
        /// <returns></returns>
        public static object? GetObject(Type type, System.Random? random = null)
        {
            random ??= new System.Random();

            if (typeof(IList).IsAssignableFrom(type))
            {
                return GetList(type, random, 0, 100);
            }
            else
            {
                if (type.IsValueType)
                {
                    return RandomValueTypeHelper.Random(type, random);
                }
                else
                {
                    if (TypeHelper.ExistNonParamPublicConstructor(type))
                    {// 检查是否有无参构造函数
                        object output = CreateInstance(type);

                        foreach (PropertyInfo propertyInfo in type.GetProperties())
                        {// 遍历公共属性
                            if (!propertyInfo.ExistCustomAttribute<IgnoreRandomAttribute>())
                            {
                                // 无忽略随机生成的特性, 则设置值
                                SetRandomValue(output, propertyInfo, random);
                            }
                        }

                        return output;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }
        /// <summary>
        /// 取得由随机指定类型的对象构成的列表
        /// </summary>
        /// <param name="type"></param>
        /// <param name="random"></param>
        /// <param name="minCount"></param>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        public static IList? GetList(Type listType, System.Random? random = null, int minCount = 0, int maxCount = 100)
        {
            if (!typeof(IList).IsAssignableFrom(listType))
            {
                return null;
            }

            random ??= new System.Random();

            // 生成数量
            int count = RandomValueTypeHelper.GetNonnegativeInt(minCount, maxCount, random);

            IList? list = null;
            if (listType.IsArray)
            {
                Type? elementType = listType.GetElementType();
                if (elementType == null)
                {
                    throw new Exception("未能获取到输入类型的元素类型");
                }
                if (elementType != null)
                {
                    Array array = Array.CreateInstance(elementType, count);
                    for (int i = 0; i < count; i++)
                    {
                        array.SetValue(GetObject(elementType, random), i);
                    }
                    list = array;
                }
            }
            else
            {
                list = (IList)CreateInstance(listType);
                if (list != null)
                {
                    Type[] genericArguments = listType.GetGenericArguments();
                    if (genericArguments.Length > 0)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            list.Add(GetObject(genericArguments[0], random));
                        }
                    }
                }
            }

            return list;
        }


        #region 泛型方法
        /// <summary>
        /// 取得随机的T对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="random"></param>
        /// <returns></returns>
        public static T? GetObject<T>(System.Random? random = null) where T : new()
        {
            object? obj = GetObject(typeof(T), random);
            return obj == null ? default : (T)obj;
        }
        public static List<T>? GetList<T>(System.Random? random = null, int minCount = 0, int maxCount = 100) where T : new()
        {
            IList? list = GetList(typeof(List<T>), random, minCount, maxCount);
            return list == null ? default : (List<T>)list;
        }
        #endregion

        #region 内部私有方法
        /// <summary>
        /// 使用输入类型的无参构造函数实例化一个对象, 如果实例化失败将抛出异常
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static object CreateInstance(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            object? output = Activator.CreateInstance(type);
            if (output == null)
            {
                throw new Exception($"未能使用 Activator.CreateInstance({type.Name}) 实例化对象");
            }
            return output;
        }
        #endregion
    }
}
