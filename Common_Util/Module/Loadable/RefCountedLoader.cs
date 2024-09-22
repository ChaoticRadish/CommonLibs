using Common_Util.Interfaces.Behavior;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Module.Loadable
{
    /// <summary>
    /// 引用计数的加卸载器
    /// </summary>
    /// <remarks>
    /// 根据引用计数调用 <typeparamref name="TLoadable"/> 的 <see cref="ILoadable.Load"/> 或 <see cref="ILoadable.Unload"/> <br/>
    /// 0 => 1 : 将调用 <see cref="ILoadable.Load"/> 加载数据 <br/>
    /// 1 => 0 : 将调用 <see cref="ILoadable.Unload"/> 卸载数据 <br/>
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    /// <param name="loadableInstance">可加载对象的实例</param>
    /// <param name="obtainDataFunc">处于加载完成状态下, 从可加载对象获取加载后数据的方法</param>
    public sealed class RefCountedLoader<TLoadable, TData>(TLoadable loadableInstance, Func<TLoadable, TData> obtainDataFunc) where TLoadable : ILoadable
    {
        /// <summary>
        /// 当前引用计数
        /// </summary>
        public int ReferenceCount { get => referenceCount; } 
        private int referenceCount;
        private readonly TLoadable loadableInstance = loadableInstance;
        private readonly object locker = new();
        private readonly Func<TLoadable, TData> ObtainDataFunc = obtainDataFunc;
        
        /// <summary>
        /// 更新计数
        /// </summary>
        /// <param name="increment"><see langword="true"/> => +1; <see langword="false"/> => -1; </param>
        private void UpdateReferenceCount(bool increment)
        {
            lock (locker)
            {
                if (increment)
                {
                    if (referenceCount == 0)
                    {
                        loadableInstance.Load();
                    }
                    referenceCount++;
                }
                else
                {
                    referenceCount--;
                    if (referenceCount == 0)
                    {
                        loadableInstance.Unload();
                    }
                }
            }
        }

        /// <summary>
        /// 获取数据引用对象, 需记得使用 <see cref="LoadedReference{TData}.Dispose"/> 释放引用, 让计数得以复位
        /// </summary>
        /// <returns></returns>
        public LoadedReference<TData> Obtain()
        {
            UpdateReferenceCount(true);
            return new()
            {
                OnDisposing = () => UpdateReferenceCount(false),
                Data = ObtainDataFunc.Invoke(loadableInstance),
            };
        }


    }


}
