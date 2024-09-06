using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Extensions
{
    public static class ActionExtensions
    {
        /// <summary>
        /// 使用<see cref="Task.Run(Action)"/>运行这个委托
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Task? TaskRun(this Action action)
        {
            if (action == null) return null;
            return Task.Run(action);
        }
        /// <summary>
        /// 使用<see cref="Task.Run(Action)"/>运行这个委托
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Task? TaskRun<T>(this Action<T> action, T arg)
        {
            if (action == null) return null;
            return Task.Run(() =>
            {
                action(arg);    
            });
        }
        /// <summary>
        /// 使用<see cref="Task.Run(Action)"/>运行这个委托
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Task? TaskRun<T1, T2>(this Action<T1, T2> action, T1 arg1, T2 arg2)
        {
            if (action == null) return null;
            return Task.Run(() =>
            {
                action(arg1, arg2);
            });
        }


    }
}
