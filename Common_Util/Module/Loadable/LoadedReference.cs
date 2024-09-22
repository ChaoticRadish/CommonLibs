using Common_Util.Data.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Module.Loadable
{
    /// <summary>
    /// 经过加载取得数据的引用对象
    /// </summary>
    /// <typeparam name="TData">加载取得的数据类型</typeparam>
    public sealed class LoadedReference<TData> : IDisposable
    {
        public LoadedReference()
        {

        }
        /// <summary>
        /// 实例化一个包装了另一个可释放对象的实例, 在释放此对象时, 会调用 <paramref name="beWrapper"/> 的 <see cref="IDisposable.Dispose"/> 方法
        /// </summary>
        /// <param name="beWrapper"></param>
        public LoadedReference(IDisposable beWrapper)
        {
            this.beWrapper = beWrapper;
        }

        /// <summary>
        /// 加载取得的数据
        /// </summary>
        public TData Data { get => initWrapper.Value; set => initWrapper.Value = value; }
        private NeedInitObjectImmutable<TData> initWrapper;



        /// <summary>
        /// 释放时需要执行的内容
        /// </summary>
        public required Action OnDisposing { get; set; }

        private readonly IDisposable? beWrapper;

        public void Dispose()
        {
            OnDisposing.Invoke();
            beWrapper?.Dispose();
        }
    }
}
