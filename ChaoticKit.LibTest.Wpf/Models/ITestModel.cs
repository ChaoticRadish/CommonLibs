using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaoticKit.LibTest.Wpf.Models
{
    public interface ITestModel
    {
    }
    public class Collection<T> : ObservableCollection<T> 
        where T : ITestModel, new()
    {
        public void AddRandom(System.Random? random = null)
        {
            ITestModelExtensions.AddRandom(this, random);
        }
        public void AddRandom(int count, System.Random? random = null)
        {
            ITestModelExtensions.AddRandom(this, count, random);
        }
    }
    static class ITestModelExtensions
    {
        /// <summary>
        /// 生成指定数量的随机列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="random"></param>
        /// <param name="minCount"></param>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        public static List<T> RandomList<T>(System.Random? random = null, int minCount = 20, int maxCount = 30) where T : ITestModel, new()
        {
            random ??= new System.Random();
            return ChaoticKit.Random.RandomObjectHelper.GetList<T>(random, minCount, maxCount) ?? new();
        }
        /// <summary>
        /// 生成一个随机对象添加到列表中
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="random"></param>
        public static void AddRandom<T>(this IList<T> list, System.Random? random = null) where T : ITestModel, new()
        {
            random ??= new System.Random();
            var obj = ChaoticKit.Random.RandomObjectHelper.GetObject<T>(random);
            if (obj != null)
            {
                list.Add(obj);
            }
        }
        /// <summary>
        /// 生成指定数量的随机对象添加到列表中
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="count"></param>
        /// <param name="random"></param>
        public static void AddRandom<T>(this IList<T> list, int count, System.Random? random = null) where T : ITestModel, new()
        {
            random ??= new System.Random();
            for (int i = 0; i < count; i++)
            {
                var obj = ChaoticKit.Random.RandomObjectHelper.GetObject<T>(random);
                if (obj != null)
                {
                    list.Add(obj);
                }
            }
        }
    }
}
