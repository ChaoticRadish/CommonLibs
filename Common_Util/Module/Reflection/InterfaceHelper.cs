using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Module.Reflection
{
    public static class InterfaceHelper
    {
        /// <summary>
        /// 取得接口方法所有需要实现的方法
        /// </summary>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static IEnumerable<MethodInfo> GetAllMethodsToImplement(Type interfaceType)
        {
            if (!interfaceType.IsInterface)
                throw new ArgumentException("传入类型不是一个接口类型", nameof(interfaceType));

            /* 使用 AI 生成 */

            // 1. 获取当前接口声明的所有方法
            var methodInfos = new List<MethodInfo>(interfaceType.GetMethods());

            // 2. 递归获取所有父接口的方法并加入列表
            foreach (var parentInterface in interfaceType.GetInterfaces())
            {
                methodInfos.AddRange(parentInterface.GetMethods());
            }

            // 3. 去重：按方法签名分组 (方法名 + 返回类型 + 参数类型)
            // 因为 IB : IA，假如都有 void Foo()，实际上只需要实现一个 Foo，但是要把它映射到 IA.Foo 和 IB.Foo 两个槽位上
            // 这里我们返回的是去重后的方法列表，用于生成 IL
            var distinctMethods = methodInfos
                .GroupBy(m => new { m.Name, m.ReturnType, Params = m.GetParameters().Select(p => p.ParameterType).ToArray() })
                .Select(g => g.First()) // 每组取一个代表
                .ToArray();

            return distinctMethods;
        }
    }
}
