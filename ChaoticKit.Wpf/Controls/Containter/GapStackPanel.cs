using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ChaoticKit.Wpf.Controls.Containter
{
    public class GapStackPanel : StackPanel
    {


        public double Gap
        {
            get { return (double)GetValue(GapProperty); }
            set { SetValue(GapProperty, value); }
        }

        public static readonly DependencyProperty GapProperty =
            DependencyProperty.Register(nameof(Gap), typeof(double), typeof(GapStackPanel), 
                new FrameworkPropertyMetadata(2d, FrameworkPropertyMetadataOptions.AffectsMeasure));


        protected override Size MeasureOverride(Size constraint)
        {
            var totalSize = new Size();
            var isHorizontal = Orientation == Orientation.Horizontal;
            var children = InternalChildren;
            var count = children.Count;

            for (int i = 0; i < count; i++)
            {
                var child = children[i];
                child.Measure(constraint);
                var childSize = child.DesiredSize;

                if (isHorizontal)
                {
                    totalSize.Width += childSize.Width;
                    totalSize.Height = Math.Max(totalSize.Height, childSize.Height);
                    // 如果不是最后一个，才加上 Gap
                    if (i < count - 1) totalSize.Width += Gap;
                }
                else
                {
                    totalSize.Width = Math.Max(totalSize.Width, childSize.Width);
                    totalSize.Height += childSize.Height;
                    // 如果不是最后一个，才加上 Gap
                    if (i < count - 1) totalSize.Height += Gap;
                }
            }
            return totalSize;
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            var isHorizontal = Orientation == Orientation.Horizontal;
            var children = InternalChildren;
            var count = children.Count;
            double x = 0, y = 0;

            for (int i = 0; i < count; i++)
            {
                var child = children[i];
                var childSize = child.DesiredSize;

                if (isHorizontal)
                {
                    child.Arrange(new Rect(x, 0, childSize.Width, arrangeSize.Height));
                    x += childSize.Width + Gap; // 加上间距移动坐标
                }
                else
                {
                    child.Arrange(new Rect(0, y, arrangeSize.Width, childSize.Height));
                    y += childSize.Height + Gap; // 加上间距移动坐标
                }
            }
            return arrangeSize;
        }
    }
}
