using Common_Util.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// 取得对象的完整信息
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string FullInfoString(this object obj)
        {
            return FullInfoGetterImpl.GetFullInfo(obj);
        }

        /// <summary>
        /// 取得对象的简略信息
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string BriefInfoString(this object obj)
        {
            return BriefInfoGetterImpl.GetBriefInfo(obj);
        }

        /// <summary>
        /// 完整信息获取器的实现
        /// </summary>
        public static IFullInfoGetter FullInfoGetterImpl { get; set; } = new FullInfoGetterImplDefault();

        /// <summary>
        /// 简略信息获取器的实现
        /// </summary>
        public static IBriefInfoGetter BriefInfoGetterImpl { get; set; } = new BriefInfoGetterImplDefault();

        /// <summary>
        /// 判断对象是否可以被赋值到指定类型, 如果对象为null或不能赋值, 则抛出异常
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T As<T>(this object? obj, string? objInfo)
        {
            string str = objInfo == null ? string.Empty : ("[" + objInfo + "]");
            if (obj == null) throw new ArgumentNullException($"对象{str}为 null ");
            if (obj is T t)
            {
                return t;
            }
            else
            {
                throw new ArgumentException($"对象{str}不能被赋值到类型 {typeof(T).Name}");
            }
        }
    }
}
