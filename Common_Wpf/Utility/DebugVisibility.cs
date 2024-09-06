using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Common_Wpf.Utility
{
    /// <summary>
    /// 区分 Debug 以及 Release 为依赖依赖对象设置 <see cref="UIElement.Visibility"/> 值
    /// <para>使用这些属性将覆盖 <see cref="UIElement.Visibility"/> 属性, </para>
    /// <para>也就是将其设置 <see cref="Visibility.Visible"/> 的同时, 设置 <see cref="DebugProperty"/> = False, 会使其在 Debug 下被隐藏</para>
    /// </summary>
    public static class DebugVisibility
    {
        /// <summary>
        /// Debug 下是否可视, null => 可视
        /// </summary>
        public static readonly DependencyProperty DebugProperty = DependencyProperty.RegisterAttached(
            "Debug", typeof(bool?), typeof(DebugVisibility), new PropertyMetadata(default(bool?), IsVisibleChangedCallback));

        /// <summary>
        /// Release 下是否可视, null => 可视
        /// </summary>
        public static readonly DependencyProperty ReleaseProperty = DependencyProperty.RegisterAttached(
            "Release", typeof(bool?), typeof(DebugVisibility), new PropertyMetadata(default(bool?), IsVisibleChangedCallback));

        /// <summary>
        /// 如果不可视, 需要设置为什么值, 默认为 <see cref="Visibility.Collapsed"/>
        /// </summary>
        public static readonly DependencyProperty HiddenValueProperty = DependencyProperty.RegisterAttached(
            "HiddenValue", typeof(Visibility), typeof(DebugVisibility), new PropertyMetadata(Visibility.Collapsed, IsVisibleChangedCallback));

        private static void IsVisibleChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not FrameworkElement fe)
                return;

            bool visible;
#if DEBUG
            visible = GetDebugVisible(d) ?? true;
#else
            visible = GetReleaseVisible(d) ?? true;
#endif
            fe.Visibility = visible ? Visibility.Visible : GetHiddenValue(d);
        }

        public static void SetDebugVisible(DependencyObject element, bool? value)
        {
            element.SetValue(DebugProperty, value);
        }

        public static bool? GetDebugVisible(DependencyObject element)
        {
            return (bool?)element.GetValue(DebugProperty);
        }

        public static void SetReleaseVisible(DependencyObject element, bool? value)
        {
            element.SetValue(ReleaseProperty, value);
        }

        public static bool? GetReleaseVisible(DependencyObject element)
        {
            return (bool?)element.GetValue(ReleaseProperty);
        }

        public static void SetHiddenValue(DependencyObject element, Visibility value)
        {
            element.SetValue(HiddenValueProperty, value);
        }

        public static Visibility GetHiddenValue(DependencyObject element)
        {
            return (Visibility)element.GetValue(HiddenValueProperty);
        }
    }
}
