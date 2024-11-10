using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Extensions
{
    public static class MethodInfoExtensions
    {

        /// <summary>
        /// 尝试获取 <paramref name="genericParam"/> 在方法 <paramref name="method"/> 的泛型形参列表中的索引
        /// </summary>
        /// <param name="method"></param>
        /// <param name="genericParam">准备查找的泛型形参, 如果传入值不是泛型形参, 将抛出异常</param>
        /// <returns>如果未能在类型的泛型中找到目标泛型形参, 则返回 -1 </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GenericParamIndexOf(this MethodInfo method, Type genericParam)
        {
            if (!genericParam.IsGenericParameter)
            {
                throw new ArgumentException($"传入参数 {genericParam} 不是泛型形参", nameof(genericParam));
            }
            if (!method.IsGenericMethod)
            {
                return -1;
            }
            MethodInfo methodDefinition = method.IsGenericMethodDefinition ? method : method.GetGenericMethodDefinition();
            Type[] gParams = methodDefinition.GetGenericArguments();
            int index = 0;
            foreach (Type param in gParams)
            {
                if (param.Equals(genericParam)) return index;
                index++;
            }
            return -1;
        }
    }
}
