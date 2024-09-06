using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Wpf
{
    class ObjViewer
    {
        #region 事件
        public static event Action? RecordChanged; 
        #endregion

        /// <summary>
        /// 已记录在案的资源
        /// </summary>
        private static ConcurrentDictionary<string, object?> _records = new ConcurrentDictionary<string, object?>();

        /// <summary>
        /// 记录对象
        /// </summary>
        /// <param name="name"></param>
        /// <param name="obj"></param>
        public static void Record(string name, object? obj)
        {
            _records.AddOrUpdate(name, obj, (key, oValue) => obj);

            RecordChanged?.Invoke();
        }
        /// <summary>
        /// 移除指定的对象
        /// </summary>
        /// <param name="name"></param>
        public static void Remove(string name)
        {
            _records.TryRemove(name, out _);

            RecordChanged?.Invoke();
        }
        /// <summary>
        /// 清理记录的所有对象
        /// </summary>
        public static void Clear()
        {
            _records.Clear();

            RecordChanged?.Invoke();
        }
    }
}
