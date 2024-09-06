using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Relevance
{
    /// <summary>
    /// 从属关系接口 (一对多)
    /// </summary>
    /// <typeparam name="T">使用继承本接口的当前类作为泛型</typeparam>
    public interface ISlave<T> where T : class, ISlave<T>
    {
        /// <summary>
        /// 父
        /// </summary>
        T Father { get; set; }
        /// <summary>
        /// 孩子们
        /// </summary>
        List<T> Children { get; }

    }

    public static class ISlaveExtensions
    {
        /// <summary>
        /// 设置父亲
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="slaver"></param>
        /// <param name="father"></param>
        public static void SetFather<T>(this ISlave<T> slaver, ISlave<T> father) where T : class, ISlave<T>
        {
            slaver.Father = father as T;
            // 如果父的孩子列表里没有本对象, 则添加进去
            if (!father.Children.Contains(slaver))
            {
                father.Children.Add(slaver as T);
            }
        }
        /// <summary>
        /// 移除父亲
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="slaver"></param>
        /// <param name="father"></param>
        public static void RemoveFather<T>(this ISlave<T> slaver) where T : class, ISlave<T>
        {
            // 如果父的孩子列表里有本对象, 则移除
            if (slaver.Father.Children.Contains(slaver))
            {
                slaver.Father.Children.Remove(slaver as T);
            }
            // 设置没有父亲
            slaver.Father = null;
        }


        /// <summary>
        /// 添加孩子
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="slaver"></param>
        /// <param name="children"></param>
        public static void AddChildren<T>(this ISlave<T> slaver, params ISlave<T>[] children) where T : class, ISlave<T>
        {
            if(children != null && children.Length > 0)
            {
                // 遍历孩子
                foreach (ISlave<T> child in children)
                {
                    // 设置孩子的父亲
                    child.Father = slaver as T;
                    // 将孩子添加到孩子列表中
                    if (!slaver.Children.Contains(child))
                    {
                        slaver.Children.Add(child as T);
                    }
                }
            }
        }
        /// <summary>
        /// 丢弃孩子
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="slaver"></param>
        /// <param name="children"></param>
        public static void RemoveChildren<T>(this ISlave<T> slaver, params ISlave<T>[] children) where T : class, ISlave<T>
        {
            if (children != null && children.Length > 0)
            {
                // 遍历孩子
                foreach (ISlave<T> child in children)
                {
                    // 检查孩子是否存在
                    if (slaver.Children.Contains(child))
                    {
                        // 存在则移除
                        slaver.Children.Remove(child as T);
                    }
                    // 移除孩子的父亲
                    child.Father = null;
                }
            }
        }
    }
}
