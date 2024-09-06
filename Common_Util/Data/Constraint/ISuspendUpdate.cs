using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Constraint
{
    /// <summary>
    /// 可暂挂更新的一个东西
    /// </summary>
    public interface ISuspendUpdate
    {
        /// <summary>
        /// 暂挂更新
        /// </summary>
        void SuspendUpdate();

        /// <summary>
        /// 恢复更新
        /// </summary>
        void ResumeUpdate();
    }

    /// <summary>
    /// 暂挂更新的作用域, 实例化时立即暂挂传入对象 (<see cref="ISuspendUpdate.SuspendUpdate"/>)
    /// <para>作用域对象被处理时 (<see cref="IDisposable.Dispose"/>), 恢复更新 (<see cref="ISuspendUpdate.ResumeUpdate"/>)</para>
    /// <para>使用示例: </para>
    /// <code> 
    /// using (SuspendUpdateScope scope = new(ISuspendUdateObj)) 
    /// { 
    ///     ... 
    /// } 
    /// </code>
    /// </summary>
    public class SuspendUpdateScope : IDisposable
    {
        private readonly ISuspendUpdate _parent;

        public SuspendUpdateScope(ISuspendUpdate parent)
        {
            _parent = parent; 
            parent.SuspendUpdate();
        }

        public void Dispose()
        {
            _parent.ResumeUpdate();
        }
    }
}
