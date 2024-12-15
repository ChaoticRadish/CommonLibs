using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Extensions
{
    public static class MemberInfoExtensions
    {
        /// <summary>
        /// 判断是否存在指定的特性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        public static bool ExistCustomAttribute<T>(this MemberInfo property) where T : System.Attribute
        {
            return Attributes.AttributeHelper.ExistCustomAttribute<T>(property);
        }

        /// <summary>
        /// 判断是否存在指定的特性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        /// <param name="attribute">如果存在特性, 在此参数返回, 反之返回null</param>
        /// <returns></returns>
        public static bool ExistCustomAttribute<T>(this MemberInfo property, [NotNullWhen(true)] out T? attribute) where T : System.Attribute
        {
            T? temp = null;
            bool result = Attributes.AttributeHelper.ExistCustomAttribute<T>(property, (attr) => temp = attr);
            attribute = temp;
            return result;
        }


        /// <summary>
        /// 检查输入的<see cref="MemberInfo"/>是否存在输入类型的特性, 存在的话做点什么
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="memberInfo"></param>
        /// <param name="ifExistDo">如果存在, 执行这个方法</param>
        /// <param name="elseDo">如果不存在, 执行这个方法</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool ExistCustomAttribute<T>(this MemberInfo memberInfo,
            Action<T>? ifExistDo = null,
            Action? elseDo = null) where T : System.Attribute
        {
            if (memberInfo == null)
            {
                throw new ArgumentNullException(nameof(memberInfo));
            }

            T? att = memberInfo.GetCustomAttribute<T>();
            if (att != null)
            {
                ifExistDo?.Invoke(att);
            }
            else
            {
                elseDo?.Invoke();
            }
            return att != null;
        }
    }
}
