using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Common_Wpf.Controls.LayoutPanel
{
    /// <summary>
    /// <see langword="null"/> 值感知的内容控件, 在根据绑定内容是否为 <see langword="null"/> 使用不同的数据模板
    /// </summary>
    public class NullAwareContentControl : MbContentControl01
    {
        static NullAwareContentControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(NullAwareContentControl),
                new FrameworkPropertyMetadata(typeof(NullAwareContentControl)));
        }

        public DataTemplate NullTemplate
        {
            get { return (DataTemplate)GetValue(NullTemplateProperty); }
            set { SetValue(NullTemplateProperty, value); }
        }
        public static readonly DependencyProperty NullTemplateProperty =
            DependencyProperty.Register(
                "NullTemplate",
                typeof(DataTemplate),
                typeof(NullAwareContentControl),
                new PropertyMetadata(null)
            );
    }
}
