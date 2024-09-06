using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util
{
    /// <summary>
    /// 反射帮助类
    /// </summary>
    public static class ReflectionHelper
    {
        /// <summary>
        /// 判断方法的输入参数, 参数类型, 返回类型, 是否均一致
        /// </summary>
        /// <param name="method"></param>
        /// <param name="paramTypes">null值可代表无参</param>
        /// <param name="returnType"></param>
        /// <returns></returns>
        public static bool IsMatch(MethodInfo method, Type[]? paramTypes = null, Type? returnType = null)
        {
            return _isMatch_ParamCount(method, paramTypes) && _isMatch_ParamType(method, paramTypes) && _isMatch_ReturnType(method, returnType);
        }


        /// <summary>
        /// 判断方法的输入参数, 参数类型, 是否均一致
        /// </summary>
        /// <param name="method"></param>
        /// <param name="paramTypes">null值可代表无参</param>
        /// <returns></returns>
        public static bool IsMatchAnyReturn(MethodInfo method, Type[]? paramTypes = null)
        {
            return _isMatch_ParamCount(method, paramTypes) && _isMatch_ParamType(method, paramTypes);
        }


        private static bool _isMatch_ParamCount(MethodInfo method, Type[]? paramTypes = null)
        {
            ParameterInfo[] paramters = method.GetParameters();
            if (paramters.Length > 0)
            {
                if (paramTypes == null || paramters.Length != paramTypes.Length)
                {
                    return false;
                }
            }
            else
            {
                if (paramTypes != null && paramTypes.Length > 0)
                {
                    return false;
                }
            }
            return true;
        }
        private static bool _isMatch_ParamType(MethodInfo method, Type[]? paramTypes = null)
        {
            ParameterInfo[] paramters = method.GetParameters();
            if (paramTypes != null)
            {
                for (int i = 0; i < paramters.Length; i++)
                {
                    if (paramters[i].ParameterType != paramTypes[i])
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        private static bool _isMatch_ReturnType(MethodInfo method, Type? returnType)
        {
            if (!(returnType == method.ReturnType || (returnType == null && method.ReturnType == typeof(void))))
            {
                return false;
            }
            return true;
        }
    }
}
