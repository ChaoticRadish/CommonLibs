using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaoticKit.Module.Concurrency
{
    /// <summary>
    /// 封装 <see cref="TaskCompletionSource"/> 实现, 用于异步环境下替代 <see cref="ManualResetEvent"/>
    /// </summary>
    public class AsyncManualResetEvent
    {
        private volatile TaskCompletionSource<bool> _tcs;

        public AsyncManualResetEvent(bool initialState = false)
        {
            _tcs = new TaskCompletionSource<bool>();
            if (initialState) _tcs.SetResult(true);
        }

        public Task WaitAsync() => _tcs.Task;

        public void Set() => _tcs.TrySetResult(true);

        public void Reset()
        {
            while (true)
            {
                var tcs = _tcs;
                if (!tcs.Task.IsCompleted) return;

                var newTcs = new TaskCompletionSource<bool>();
                if (Interlocked.CompareExchange(ref _tcs, newTcs, tcs) == tcs) return;
            }
        }
    }

}
