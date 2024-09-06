using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Common_Util.GDI.Extensions;
using Common_Winform.Extensions;

namespace Common_Winform.Controls.FeatureGroup
{
    public partial class WaitingInfoBox : UserControl
    {
        public WaitingInfoBox()
        {
            InitializeComponent();

            DoubleBuffered = true;

        }

        #region 属性
        /// <summary>
        /// 需要显示的信息
        /// </summary>
        public string? Info 
        {
            get => info;
            set
            {
                info = value;
                Refresh();
            }
        }
        private string? info;
        /// <summary>
        /// 转圈圈的圆形数量
        /// </summary>
        public int CirculCount { get; set; }
        #endregion
        #region 状态
        /// <summary>
        /// 载入时间
        /// </summary>
        private DateTime LoadTime { get; set; }
        #endregion

        #region 重载
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadTime = DateTime.Now;
        }
        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            if (Parent != null)
            {
                this.Location = new Point()
                {
                    X = (Parent.Width - Width) / 2,
                    Y = (Parent.Height - Height) / 2,
                };
                BringToFront();
                // SetTopLevel(true);
            }
        }


        #endregion

        #region 定时器
        private void InnerTimer_Tick(object sender, EventArgs e)
        {
            if (Visible)
            {
                Refresh();
            }
        }

        #endregion

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.Clear(BackColor);

            RectangleF infoArea = new RectangleF();
            RectangleF waitingArea = new RectangleF();

            if (Width > Height)
            {
                waitingArea.Height = Height;
                waitingArea.Width = Height;
            }
            else 
            {
                waitingArea.Width = Width;
                waitingArea.Height = Width;
            }
            // 计算位置, 同时绘制文本
            if (!string.IsNullOrEmpty(Info))
            {
                waitingArea.X = 0;
                waitingArea.Y = 0;
                StringFormat format = new StringFormat();
                if (Width > Height)
                {
                    infoArea.X = waitingArea.Width;
                    infoArea.Y = 0;
                    infoArea.Width = Width - waitingArea.Width;
                    infoArea.Height = Height;
                    format.Alignment = StringAlignment.Near;
                    format.LineAlignment= StringAlignment.Center;
                }
                else
                {
                    infoArea.X = 0;
                    infoArea.Y = waitingArea.Width;
                    infoArea.Width = Width;
                    infoArea.Height = Height - waitingArea.Height;
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Center;
                }

                using (SolidBrush brush = new SolidBrush(ForeColor))
                {
                    e.Graphics.DrawString(Info, Font, brush, infoArea, format);

                }
            }
            else
            {
                waitingArea.X = (Width - waitingArea.Width) / 2;
                waitingArea.Y = (Height - waitingArea.Height) / 2;
            }
            // 绘制加载图标
            PointF center = waitingArea.Center();
            int angleCount = CirculCount < 8 ? 8 : CirculCount;
            float perAngle = (float)(2 * Math.PI / angleCount);
            int circulCount = CirculCount < 2 ? 2 : CirculCount;
            int perA = 255 / circulCount;
            int offset = (int)((DateTime.Now - LoadTime).TotalMilliseconds / InnerTimer.Interval) % angleCount;

            float dist = waitingArea.Width / 2 / 3 * 2; // 此处宽与高一致
            float size = waitingArea.Width / 2 / 6;

            for (int i = 0; i < circulCount; i++)
            {
                double sin = Math.Sin(perAngle * (i - offset));
                double cos = Math.Cos(perAngle * (i - offset));
                float x = (float)(center.X + sin * dist);
                float y = (float)(center.Y + cos * dist);
                DrawCircul(e.Graphics, x, y, size, Color.FromArgb(255 - perA * i, ForeColor));
            }
        }

        /// <summary>
        /// 在指定位置画一个实心圆形
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="x">指定中点</param>
        /// <param name="y">指定中点</param>
        /// <param name="size">指定大小</param>
        /// <param name="color">颜色</param>
        private void DrawCircul(Graphics graphics, float x, float y, float size, Color color)
        {
            using (Brush b = new SolidBrush(color))
            {
                DrawCircul(graphics, x, y, size, b);
            }
        }
        /// <summary>
        /// 在指定位置画一个实心圆形
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="x">指定中点</param>
        /// <param name="y">指定中点</param>
        /// <param name="size">指定大小</param>
        /// <param name="brush">画刷</param>
        private void DrawCircul(Graphics graphics, float x, float y, float size, Brush brush)
        {
            graphics.FillEllipse(brush, new RectangleF()
            {
                X = x - size / 2,
                Y = y - size / 2,
                Width = size,
                Height = size,
            });
        }

    }
}
