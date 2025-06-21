using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Test.Console
{
    /// <summary>
    /// 测试运行器
    /// </summary>
    public class TestRunner
    {

        public TestRunner()
        {
            init();
        }
        /// <summary>
        /// 扫描所有
        /// </summary>
        private void init()
        {
            string? entryAssemblyName = Assembly.GetEntryAssembly()?.GetName().Name;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (!type.IsInterface && !type.IsAbstract && !type.IsGenericType
                        && type.IsAssignableTo(typeof(ITest)) 
                        && type.HavePublicEmptyCtor())
                    {
                        if (type.FullName == null) continue;
                        if (entryAssemblyName != null && type.FullName.StartsWith(entryAssemblyName))
                        {
                            string shortName = type.FullName[(entryAssemblyName.Length + 1)..];
                            shortName_TestTypes.Add(shortName, type);
                        }
                        fullName_testTypes.Add(type.FullName, type);
                    }
                }
            }
        }
        private Dictionary<string, Type> fullName_testTypes = [];
        private Dictionary<string, Type> shortName_TestTypes = [];

        public void Run(string name)
        {
            Type? type;
            if (shortName_TestTypes.TryGetValue(name, out type) || fullName_testTypes.TryGetValue(name, out type))
            {
                var test = Create(type);
                test.Setup();
                test.Run();
                test.Finish();
            }
        }
        private ITest Create(Type type)
        {
            return (Activator.CreateInstance(type) as ITest) ?? throw new InvalidOperationException($"无法创建测试对象 {type.FullName}");
        }
    }
}
