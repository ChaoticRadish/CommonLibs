using Common_Winform.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Common_Winform.Container
{
    public partial class ScrollPanel : Panel
    {
        public ScrollPanel()
        {
            InitializeComponent();

            InnerPanel = new Panel();
            InnerPanel.ControlAdded += InnerPanel_ControlAdded;
            base.Controls.Add(InnerPanel);
        }


        #region 子控件
        private Panel InnerPanel;
        #endregion

        #region 控件列表
        public new ControlCollection Controls 
        {
            get
            {
                return InnerPanel.Controls;
            }
        }

        #endregion

        #region 参数

        [Category("CVII_自定义_参数"), DisplayName("滚动方向(垂直/水平)")]
        public OrientationEnum ScrollOrientation { get; set; } = OrientationEnum.Vertical;
        [Category("CVII_自定义_参数"), DisplayName("尺寸计算修正量")]
        public int SizeCalcCorrection { get; set; } = 5;
        #endregion

        #region 状态
        [Category("CVII_自定义_状态"), DisplayName("当前水平偏移量")]
        public int CurrentOffsetHorizontal { get; private set; }
        [Category("CVII_自定义_状态"), DisplayName("当前垂直偏移量")]
        public int CurrentOffsetVertical { get; private set; }
        #endregion

        #region 尺寸计算
        private int CalcInnerNeedWidth()
        {
            int output = 0;

            foreach (Control control in InnerPanel.Controls)
            {
                int need = control.Location.X + control.Width;
                if (need > output)
                {
                    output = need;
                }
            }

            if (output < 0) { output = 0; }
            output += SizeCalcCorrection;
            return output;
        }
        private int CalcInnerNeedHeight()
        {
            int output = 0;

            foreach (Control control in InnerPanel.Controls)
            {
                int need = control.Location.Y + control.Height;
                if (need > output)
                {
                    output = need;
                }
            }

            if (output < 0) { output = 0; }
            output += SizeCalcCorrection;
            return output;
        }
        #endregion

        #region 滚动效果和内部Panel自适应效果
        public void UpdateInnerPanelSize()
        {
            InnerPanel.SuspendLayout();
            switch (ScrollOrientation)
            {
                case OrientationEnum.Horizontal:
                    InnerPanel.Width = CalcInnerNeedWidth();
                    InnerPanel.Height = Height;
                    UpdateInnerPanelLocation_Horizontal();
                    break;
                case OrientationEnum.Vertical:
                    InnerPanel.Width = Width;
                    InnerPanel.Height = CalcInnerNeedHeight();
                    UpdateInnerPanelLocation_Vertical();
                    break;
            }
            InnerPanel.ResumeLayout();
        }
        private void UpdateInnerPanelLocation_Horizontal()
        {
            CurrentOffsetVertical = 0;
            if (CurrentOffsetHorizontal > 0)
            {
                CurrentOffsetHorizontal = 0;
            }
            if (InnerPanel.Width < Width)
            {
                CurrentOffsetHorizontal = 0;
                InnerPanel.Location = new Point();
            }
            else if (InnerPanel.Width + CurrentOffsetHorizontal < Width)
            {
                CurrentOffsetHorizontal = Width - InnerPanel.Width;
                InnerPanel.Location = new Point(CurrentOffsetHorizontal, 0);
            }
            else
            {
                InnerPanel.Location = new Point(CurrentOffsetHorizontal, 0);
            }
        }
        private void UpdateInnerPanelLocation_Vertical()
        {
            CurrentOffsetHorizontal = 0;
            if (CurrentOffsetVertical > 0)
            {
                CurrentOffsetVertical = 0;
            }
            if (InnerPanel.Height < Height)
            {
                CurrentOffsetVertical = 0;
                InnerPanel.Location = new Point();
            }
            else if (InnerPanel.Height + CurrentOffsetVertical < Height)
            {
                CurrentOffsetVertical = Height - InnerPanel.Height;
                InnerPanel.Location = new Point(0, CurrentOffsetVertical);
            }
            else
            {
                InnerPanel.Location = new Point(0, CurrentOffsetVertical);
            }
        }
        #endregion

        #region 子控件事件

        private void InnerPanel_ControlAdded(object? sender, ControlEventArgs e)
        {
            UpdateInnerPanelSize();
        }

        #endregion

        #region 重载
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible)
            {
                UpdateInnerPanelSize();
            }
        }
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (!IsHandleCreated) return;

            SuspendLayout();

            UpdateInnerPanelSize();

            ResumeLayout();
        }
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            switch (ScrollOrientation)
            {
                case OrientationEnum.Horizontal:
                    CurrentOffsetHorizontal += e.Delta;
                    UpdateInnerPanelLocation_Horizontal();
                    break;
                case OrientationEnum.Vertical:
                    CurrentOffsetVertical += e.Delta;
                    UpdateInnerPanelLocation_Vertical();
                    break;
            }
        }
        #endregion
    }
}
