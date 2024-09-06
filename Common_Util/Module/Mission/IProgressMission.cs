using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Module.Mission
{
    /// <summary>
    /// 有进度信息的任务的接口
    /// </summary>
    public interface IProgressMission
    {
        /// <summary>
        /// 进度级别
        /// </summary>
        int ProgressLevel { get; set; }

        /// <summary>
        /// 任务名
        /// </summary>
        string MissionName { get; set; }

        /// <summary>
        /// 所在容器 (可以没有或暂时没有)
        /// </summary>
        IProgressMissionContainer? Container { get; set; }

        /// <summary>
        /// 任务主体
        /// </summary>
        Action MissionBody { get; set; }
    }
    /// <summary>
    /// 有进度信息的任务的接口的拓展方法
    /// </summary>
    public static class IProgressMissionExtension
    {
        /// <summary>
        /// 更新进度信息
        /// </summary>
        /// <param name="mission"></param>
        /// <param name="progress"></param>
        /// <param name="info"></param>
        public static void UpdateProgress(this IProgressMission mission, float progress, string info)
        {
            UpdateProgress(mission, mission == null ? 1 : mission.ProgressLevel, progress, info);
        }

        /// <summary>
        /// 更新进度信息
        /// </summary>
        /// <param name="mission"></param>
        /// <param name="level"></param>
        /// <param name="progress"></param>
        /// <param name="info"></param>
        public static void UpdateProgress(this IProgressMission mission, int level, float progress, string info)
        {
            if (mission == null || mission.Container == null)
            {
                return;
            }
            else
            {
                mission.Container.OnProgressUpdate(new MissionProgress()
                {
                    Level = level,
                    Progress = progress,
                    Info = string.IsNullOrEmpty(mission.MissionName) ? info : mission.MissionName + ": " + info,
                });
            }
        }

        /// <summary>
        /// 运行任务
        /// </summary>
        /// <param name="mission"></param>
        public static void Run(this IProgressMission mission)
        {
            if (mission == null || mission.MissionBody == null)
            {
                return;
            }
            mission.MissionBody.Invoke();
        }
        /// <summary>
        /// 异步运行任务
        /// </summary>
        /// <param name="mission"></param>
        public static Task RunAsync(this IProgressMission mission)
        {
            if (mission == null || mission.MissionBody == null)
            {
                return Task.CompletedTask;
            }
            return Task.Run(mission.MissionBody);
        }
    }
}
