using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;

namespace Common_WpfWinformMix.Winform
{
    /// <summary>
    /// Wpf用户控件在Winform内的宿主
    /// </summary>
    public class UserControlHost01 : System.Windows.Forms.UserControl
    {
        #region 属性
        [Category("CVII_自定义_属性"), Description("此宿主寄生控件的描述")]
        public string Desc { get; set; } = string.Empty;

        #endregion

        public UserControlHost01() 
        {
            ElementHost = new ElementHost();
            ElementHost.Dock = DockStyle.Fill;
            ElementHost.BackColor = Color.Transparent;

            Controls.Add(ElementHost);

        }

        private ElementHost ElementHost { get; }

        public void SetChild(UIElement element)
        {
            if (element is Page)
            {
                Frame frame = new Frame();
                frame.Content = element;
                ElementHost.Child = frame;
            }
            else
            {
                ElementHost.Child = element;
            }
        }
        public void SetChild<T>() where T : UIElement, new()
        {
            SetChild(new T());
        }
        public void SetChild<T>(out T element) where T : UIElement, new()
        {
            element = new T();
            SetChild(element);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;

            if (this.GetService(typeof(IDesignerHost)) != null
                || System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                using (Pen pen = new Pen(ForeColor, 1))
                {
                    g.DrawRectangle(pen, new Rectangle()
                    {
                        X = 0,
                        Y = 0,
                        Width = Width - 1,
                        Height = Height - 1
                    });
                }
                if (Desc.IsNotEmpty())
                {
                    using (Brush brush = new SolidBrush(ForeColor))
                    {
                        g.DrawString($"WPF控件宿主: {Desc}", Font, brush,
                            new RectangleF()
                            {
                                X = 1,
                                Y = 1,
                                Width = Width - 2,
                                Height = Height - 2
                            },
                            new StringFormat()
                            {
                                Alignment = StringAlignment.Center,
                                LineAlignment = StringAlignment.Center,
                            });
                    }
                }
            }
        }
    }
}
