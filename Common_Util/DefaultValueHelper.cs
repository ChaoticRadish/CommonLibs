using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util
{
    /// <summary>
    /// 默认值帮助类
    /// </summary>
    public static class DefaultValueHelper
    {
        /// <summary>
        /// 尝试运行 <paramref name="action"/>, 并将运行结果转换为字符串, 发生异常时, 捕获并返回异常信息
        /// </summary>
        /// <typeparam name="T">执行结果类型</typeparam>
        /// <param name="action">需执行内容</param>
        /// <param name="exStr">发生异常时将返回此字符串</param>
        /// <param name="nullObjStr">如果执行结果对象为 <see langword="null"/>, 返回此字符串</param>
        /// <param name="objToStr">自定义执行结果对象转换为字符串的方法, 如果此参数为 <see langword="null"/>, 将使用 <see cref="object.ToString"/>, 并在结果为 <see langword="null"/> 的时候取 <paramref name="nullObjStr"/></param>
        /// <returns></returns>
        public static string TryRunToString<T>(Func<T> action, string exStr, string nullObjStr = "<null>", Func<T, string>? objToStr = null)
        {
            try
            {
                T obj = action();
                if (objToStr == null)
                {
                    return obj?.ToString() ?? nullObjStr;
                }
                else
                {
                    return objToStr(obj);
                }
            }
            catch
            {
                return exStr;
            }
        }
        /// <summary>
        /// 尝试运行 <paramref name="action"/>, 并将运行结果转换为字符串, 发生异常时, 捕获并返回异常信息
        /// </summary>
        /// <typeparam name="T">执行结果类型</typeparam>
        /// <param name="action">需执行内容</param>
        /// <param name="exToStr">发生异常时将异常转换为字符串的方法, 如果为 <see langword="null"/>, 将取 <see cref="Exception.Message"/></param>
        /// <param name="nullObjStr">如果执行结果对象为 <see langword="null"/>, 返回此字符串</param>
        /// <param name="objToStr">自定义执行结果对象转换为字符串的方法, 如果此参数为 <see langword="null"/>, 将使用 <see cref="object.ToString"/>, 并在结果为 <see langword="null"/> 的时候取 <paramref name="nullObjStr"/></param>
        /// <returns></returns>
        public static string TryRunToString<T>(Func<T> action, Func<Exception, string>? exToStr = null, string nullObjStr = "<null>", Func<T, string>? objToStr = null)
        {
            try
            {
                T obj = action();
                if (objToStr == null)
                {
                    return obj?.ToString() ?? nullObjStr;
                }
                else
                {
                    return objToStr(obj);
                }
            }
            catch (Exception ex)
            {
                if (exToStr == null)
                {
                    return ex.Message;
                }
                else
                {
                    return exToStr(ex);
                }
            }
        }
    }
}
