using Common_Util.Extensions;
using Common_Winform.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Winform.Pages
{
    [ToolboxItem(true)]
    public class PageBase : UserControl
    {
        #region 状态
        [Category("CVII_自定义_状态"), DisplayName("是否只读")]
        public bool ReadOnly
        {
            get
            {
                return _isReadOnly;
            }
            set
            {
                if (_firstSetReadOnly)
                {
                    _isReadOnly = value;
                    OnReadOnlyChanged(_isReadOnly);
                    ReadOnlyChanged?.Invoke(_isReadOnly);
                }
                else if (_isReadOnly != value)
                {
                    _isReadOnly = value;
                    OnReadOnlyChanged(_isReadOnly);
                    ReadOnlyChanged?.Invoke(_isReadOnly);
                }
            }
        }
        private bool _isReadOnly = false;
        private bool _firstSetReadOnly = true;


        /// <summary>
        /// 当前容器传递的字体, 比如布局传递给页面的字体
        /// </summary>
        [Category("CVII_自定义_状态"), DisplayName("当前容器传递的字体")]
        public Font? ContainerFont { get; set; }
        #endregion

        #region 属性
        /// <summary>
        /// 页面索引
        /// </summary>
        [Category("CVII_自定义"), DisplayName("页面索引")]
        public int PageIndex { get; set; }
        /// <summary>
        /// 写日志时, 控件需使用什么名字
        /// </summary>
        [Category("CVII_自定义"), DisplayName("日志使用名称")]
        public string LogName
        {
            get
            {
                if (!string.IsNullOrEmpty(logName))
                {
                    return logName;
                }
                if (!string.IsNullOrEmpty(Name))
                {
                    return Name;
                }
                else
                {
                    return GetType().Name;
                }
            }
            set
            {
                logName = value;
            }
        }
        private string logName = string.Empty;
        #endregion

        #region 事件
        public delegate void ReadOnlyChangedHandle(bool newState);
        [Category("CVII_自定义"), DisplayName("只读状态变更")]
        public event ReadOnlyChangedHandle? ReadOnlyChanged;
        /// <summary>
        /// 只读状态变更时触发
        /// </summary>
        /// <param name="newState">新的状态(只读时为true)</param>
        protected virtual void OnReadOnlyChanged(bool newState) { }
        #endregion

        public PageBase()
        {
        }

        #region 初始化
        /// <summary>
        /// 使用指定的页面配置初始化页面
        /// </summary>
        /// <param name="pageConfig"></param>
        public virtual void Init(PageConfigAttribute? pageConfig = null)
        {
            if (pageConfig != null)
            {
                PageIndex = pageConfig.Index;
                Name = pageConfig.ShowName;
                LogName = pageConfig.LogName.WhenEmptyDefault(pageConfig.ShowName.WhenEmptyDefault(string.Empty));
            }
        }
        #endregion

        public virtual void LogInfo(string message)
        {
            Debug.WriteLine("[" + LogName + "] <Info> " + message);
        }
        public virtual void LogError(string message, Exception ex)
        {
            Debug.WriteLine("[" + LogName + "] <Error> " + message + "\n" + ex.Message);
        }
    }
}
