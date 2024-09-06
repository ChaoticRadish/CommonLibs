using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Winform.Controls.ValueBox
{
    /// <summary>
    /// 包含提示信息的文本框
    /// </summary>
    public class HintTextBox : TextBox
    {
        [Browsable(true)]
        [Category("CVII_自定义_参数"), Description("提示文本")]
        public string HintText
        {
            get => hintText;
            set
            {
                hintText = value;
                Invalidate();
            }
        }
        private string hintText = string.Empty;


        [Browsable(true)]
        [Category("CVII_自定义_参数"), Description("提示文本颜色")]
        public Color HintColor
        {
            get => hintColor;
            set
            {
                hintColor = value;
                Invalidate();
            }
        }
        private Color hintColor = Color.LightGray;

        [Browsable(true)]
        [Category("CVII_自定义_参数"), Description("提示文本与边框的内侧间距")]
        public float HintBorderGap { get => hintBorderGap; set => hintBorderGap = value; }
        private float hintBorderGap = 1.5f;

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            switch (m.Msg)
            {
                case (int)Common_Util.Enums.WMMsgCodeEnum.WM_PAINT:
                    RePaint();
                    break;
            }
        }

        protected void RePaint()
        {
            Graphics g = CreateGraphics();
            /*g.Clear(Color.FromArgb(0, 255, 255, 255));
            // 绘制内部背景色
            using (Brush brush = new SolidBrush(BackColor))
            {
                g.FillRectangle(brush, new RectangleF()
                {
                    X = BorderGap * 1,
                    Y = BorderGap * 1,
                    Width = Width - BorderGap * 2,
                    Height = Height - BorderGap * 2
                });
            }
            // 绘制边框
            using (Pen pen = new Pen(BorderColor))
            {
                g.DrawRectangle(pen, new Rectangle()
                {
                    X = (int)(BorderGap * 1),
                    Y = (int)(BorderGap * 1),
                    Width = (int)(Width - BorderGap * 2),
                    Height = (int)(Height - BorderGap * 2)
                });
            }
            // 绘制文字
            if (!string.IsNullOrEmpty(Text))
            {
                using (Brush brush = new SolidBrush(ForeColor))
                {
                    g.DrawString(Text, Font, brush, new RectangleF()
                    {
                        X = BorderGap * 2,
                        Y = BorderGap * 2,
                        Width = Width - BorderGap * 4,
                        Height = Height - BorderGap * 4
                    });
                }
            }*/
            // 绘制提示文字
            if (!Focused && Text.IsEmpty() && !hintText.IsEmpty())
            {
                using (Brush brush = new SolidBrush(HintColor))
                {
                    g.DrawString(HintText, Font, brush, 
                        new RectangleF()
                        {
                            X = HintBorderGap * 1,
                            Y = HintBorderGap * 1,
                            Width = Width - HintBorderGap * 2,
                            Height = Height - HintBorderGap * 2
                        },
                        new StringFormat()
                        {
                            Alignment = StringAlignment.Near,
                            LineAlignment = StringAlignment.Center,
                        });
                }
            }
        }
    }
}
