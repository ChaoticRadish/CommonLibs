using Common_Util.GDI;
using Common_Winform.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Winform.Controls
{
    /// <summary>
    /// 简易状态的Label
    /// </summary>
    [DefaultEvent(nameof(Click))]
    public class LabelSampleState01 : UserControl
    {
        public LabelSampleState01() 
        {
            DoubleBuffered = true;
        }

        [Category("CVII_自定义"), DisplayName("当前状态")]
        public SampleStateEnum State 
        {
            get => _state;
            set
            {
                _state = value;
                Invalidate();
            }
        }
        private SampleStateEnum _state = SampleStateEnum.UnChecked;

        [Category("CVII_自定义"), DisplayName("显示文本")]
        public string? ShowingText
        {
            get => Text;
            set => Text = value;
        }
        [Category("CVII_自定义"), DisplayName("显示文本 (Text)")]
        public new string? Text
        {
            get => _text;
            set
            {
                _text = value;
                Invalidate();
            }
        }
        private string? _text;


        #region 绘制参数
        [Category("CVII_自定义_绘制"), DisplayName("状态颜色(前景色): 选中")]
        public Color StateForeColor_Checked { get; set; } = Color.Black;
        [Category("CVII_自定义_绘制"), DisplayName("状态颜色(背景色): 选中")]
        public Color StateBackColor_Checked { get; set; } = ColorConvert.FromString("#2b8769");

        [Category("CVII_自定义_绘制"), DisplayName("状态颜色(前景色): 未选中")]
        public Color StateForeColor_UnChecked { get; set; } = Color.Black;
        [Category("CVII_自定义_绘制"), DisplayName("状态颜色(背景色): 未选中")]
        public Color StateBackColor_UnChecked { get; set; } = ColorConvert.FromString("#94d1c3");

        [Category("CVII_自定义_绘制"), DisplayName("状态颜色(前景色): 只读")]
        public Color StateForeColor_ReadOnly { get; set; } = Color.LightGray;
        [Category("CVII_自定义_绘制"), DisplayName("状态颜色(背景色): 只读")]
        public Color StateBackColor_ReadOnly { get; set; } = Color.DarkGray;

        [Category("CVII_自定义_绘制"), DisplayName("状态颜色(前景色): 停用")]
        public Color StateForeColor_Disable { get; set; } = Color.Gray;
        [Category("CVII_自定义_绘制"), DisplayName("状态颜色(背景色): 停用")]
        public Color StateBackColor_Disable { get; set; } = Color.LightGray;

        #endregion

        [Category("CVII_自定义_绘制"), DisplayName("背景颜色")]
        public new Color BackColor
        {
            get
            {
                return State switch
                {
                    SampleStateEnum.Checked => StateBackColor_Checked,
                    SampleStateEnum.UnChecked => StateBackColor_UnChecked,
                    SampleStateEnum.ReadOnly => StateBackColor_ReadOnly,
                    SampleStateEnum.Disable => StateBackColor_Disable,
                    _ => Color.White,
                };
            }
        }
        [Category("CVII_自定义_绘制"), DisplayName("前景颜色")]
        public new Color ForeColor
        {
            get
            {
                return State switch
                {
                    SampleStateEnum.Checked => StateForeColor_Checked,
                    SampleStateEnum.UnChecked => StateForeColor_UnChecked,
                    SampleStateEnum.ReadOnly => StateForeColor_ReadOnly,
                    SampleStateEnum.Disable => StateForeColor_Disable,
                    _ => Color.White,
                };
            }
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
            e.Graphics.Clear(BackColor);
            if (!string.IsNullOrEmpty(Text) && Width > 3 && Height > 3)
            {
                using (Brush brush = new SolidBrush(ForeColor))
                {
                    e.Graphics.DrawString(
                        Text, Font, brush, 
                        new Rectangle()
                        {
                            X = 1, Y = 1, Width = Width - 2, Height = Height - 2
                        }, 
                        new StringFormat()
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center,
                        });
                }
            }
        }



        #region 事件
        protected override void OnClick(EventArgs e)
        {
            if (State == SampleStateEnum.Disable) return;
            base.OnClick(e);
        }
        #endregion
    }
}
