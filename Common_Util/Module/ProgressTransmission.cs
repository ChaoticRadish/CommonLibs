using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Module
{
    /// <summary>
    /// 进度的传动器
    /// </summary>
    public class ProgressTransmission<MessageType>
    {
        /// <summary>
        /// 进度的总量
        /// </summary>
        public float Total { get; set; }
        /// <summary>
        /// 当前进度
        /// </summary>
        public float ProgressNow { get; set; }
        /// <summary>
        /// 进度比例
        /// </summary>
        public float ProgressScale { get => ProgressNow / (Total == 0 ? 1 : Total); }

        /// <summary>
        /// 更新进度 (进度自增1)
        /// </summary>
        public void UpdateProgress(MessageType message)
        {
            UpdateProgress(ProgressNow + 1, message);
        }
        /// <summary>
        /// 更新进度 (设置为指定进度)
        /// </summary>
        /// <param name="newProgress"></param>
        public void UpdateProgress(float newProgress, MessageType message)
        {
            ProgressNow = newProgress;

            ProgressUpdatedCallback?.Invoke(this, message);
        }
        /// <summary>
        /// 进度完成
        /// </summary>
        public void Done(MessageType message)
        {
            ProgressNow = Total;

            ProgressDoneCallback?.Invoke(this, message);
        }
        /// <summary>
        /// 进度失败
        /// </summary>
        public void Failed(MessageType message)
        {
            ProgressNow = 0;

            ProgressFailedCallback?.Invoke(this, message);
        }

        #region 回调
        /// <summary>
        /// 进度更新的委托类型
        /// </summary>
        /// <param name="transmission">进度传动器</param>

        public delegate void ProgressUpdatedDelegate(ProgressTransmission<MessageType> transmission, MessageType message);
        /// <summary>
        /// 进度更新的回调方法
        /// </summary>
        public ProgressUpdatedDelegate? ProgressUpdatedCallback { get; set; }


        /// <summary>
        /// 进度完成的委托类型
        /// </summary>
        /// <param name="transmission"></param>

        public delegate void ProgressDoneDelegate(ProgressTransmission<MessageType> transmission, MessageType message);
        /// <summary>
        /// 进度更新的回调方法
        /// </summary>
        public ProgressDoneDelegate? ProgressDoneCallback { get; set; }

        /// <summary>
        /// 进度失败的委托类型
        /// </summary>
        /// <param name="transmission"></param>
        public delegate void ProgressFailedDelegate(ProgressTransmission<MessageType> transmission, MessageType message);
        /// <summary>
        /// 进度失败的回调函数
        /// </summary>
        public ProgressFailedDelegate? ProgressFailedCallback { get; set; }
        #endregion
    }
}
