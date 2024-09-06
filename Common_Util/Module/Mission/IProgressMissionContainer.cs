using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Module.Mission
{
    /// <summary>
    /// 进度条任务容器接口
    /// </summary>
    public interface IProgressMissionContainer
    {
        /// <summary>
        /// 任务
        /// </summary>
        IProgressMission Mission { get; set; }

        /// <summary>
        /// 启动时触发
        /// </summary>
        void OnStart();

        /// <summary>
        /// 结束时触发
        /// </summary>
        void OnFinish();

        /// <summary>
        /// 进度更新触发
        /// </summary>
        /// <param name="progress"></param>
        void OnProgressUpdate(MissionProgress progress);
    }
    /// <summary>
    /// 进度条任务容器接口的拓展方法
    /// </summary>
    public static class IProgressMissionContainerExtension
    {
        /// <summary>
        /// 运行任务
        /// </summary>
        /// <param name="mission"></param>
        public static void Run(this IProgressMissionContainer container)
        {
            if (container == null || container.Mission == null)
            {
                return;
            }
            container.Mission.Container = container;
            container.OnStart();
            container.Mission.Run();
            container.OnFinish();
        }
        /// <summary>
        /// 异步运行任务
        /// </summary>
        /// <param name="mission"></param>
        public static Task RunAsync(this IProgressMissionContainer container)
        {
            if (container == null || container.Mission == null)
            {
                return Task.CompletedTask; 
            }
            container.Mission.Container = container;
            return Task.Run(() =>
            {
                container.OnStart();
                container.Mission.Run();
                container.OnFinish();
            });
        }
    }
}
