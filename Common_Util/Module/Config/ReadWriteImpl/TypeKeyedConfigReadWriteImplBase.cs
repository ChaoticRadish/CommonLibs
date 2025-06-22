using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Module.Config
{
    /// <summary>
    /// 实现 <see cref="ITypeKeyedConfigReadWriteImpl"/> 的默认基类, 可用所有具有公共无参构造方法的类型
    /// </summary>
    public abstract class TypeKeyedConfigReadWriteImplBase : ITypeKeyedConfigReadWriteImpl
    {
        public bool IsAvailable(Type type)
        {
            return type.HavePublicEmptyCtor();
        }
        /// <summary>
        /// 创建特定类型的实例
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected virtual object CreateInstance(Type type)
        {
            return Activator.CreateInstance(type) ?? throw new InvalidOperationException("创建实例失败! ")
            {
                Data = { ["Type"] = type }
            };
        }

        public abstract bool SaveConfig(Type key, object config);
        public abstract bool TryLoadConfig(Type key, [NotNullWhen(true)] out object? config);

        /// <summary>
        /// 获取输入类型的配置, 获取失败时抛出异常
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual T LoadConfig<T>()
        {
            return ((ITypeKeyedConfigReadWriteImpl)this).DefaultLoadConfig<T>();
        }
        /// <summary>
        /// 保存输入类型的配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="config">配置数据对象</param>
        public void SaveConfig<T>(T config)
        {
            ((ITypeKeyedConfigReadWriteImpl)this).DefaultSaveConfig(config);
        }
    }
}
