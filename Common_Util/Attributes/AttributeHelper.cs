using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Attributes
{
    public static class AttributeHelper
    {
        /// <summary>
        /// 取得类型的指定自定义特性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="inherit">是否从祖先中搜索</param>
        /// <returns></returns>
        public static T? GetCustomAttribute<T>(Type type, bool inherit) where T : System.Attribute
        {
            if (type == null)
            {
                return null;
            }
            object[] atts = type.GetCustomAttributes(inherit);
            Type typeInput = typeof(T);
            foreach (object att in atts)
            {
                if (att.GetType() == typeInput)
                {
                    return (T)att;
                }
            }
            return null;
        }
        /// <summary>
        /// 判断是否存在指定的特性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        public static bool ExistCustomAttribute<T>(MemberInfo property) where T : System.Attribute
        {
            return property.GetCustomAttribute<T>() != null;
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
        public static bool ExistCustomAttribute<T>(MemberInfo memberInfo,
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
