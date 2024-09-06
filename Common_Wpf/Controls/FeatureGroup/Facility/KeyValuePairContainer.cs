using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Common_Wpf.Controls.FeatureGroup
{
    /// <summary>
    /// 键值对容器, 设置内容将被显示到值区域
    /// </summary>
    public class KeyValuePairContainer : MbContentControl01
    {
        static KeyValuePairContainer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(KeyValuePairContainer), 
                new FrameworkPropertyMetadata(typeof(KeyValuePairContainer)));
        }

        #region 设置

        /// <summary>
        /// Key部分的宽度设置
        /// </summary>
        public GridLength KeyWidth
        {
            get { return (GridLength)GetValue(KeyWidthProperty); }
            set { SetValue(KeyWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for KeyWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty KeyWidthProperty =
            DependencyProperty.Register(
                "KeyWidth", 
                typeof(GridLength), 
                typeof(KeyValuePairContainer), 
                new PropertyMetadata(GridLength.Auto)
                );



        /// <summary>
        /// Key部分的最小宽度设置
        /// </summary>
        public double KeyMinWidth
        {
            get { return (double)GetValue(KeyMinWidthProperty); }
            set { SetValue(KeyMinWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for KeyMinProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty KeyMinWidthProperty =
            DependencyProperty.Register(
                "KeyMinWidth", 
                typeof(double),
                typeof(KeyValuePairContainer),
                new PropertyMetadata(0d));


        /// <summary>
        /// Key部分的最大宽度设置
        /// </summary>
        public double KeyMaxWidth
        {
            get { return (double)GetValue(KeyMaxWidthProperty); }
            set { SetValue(KeyMaxWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for KeyMaxWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty KeyMaxWidthProperty =
            DependencyProperty.Register(
                "KeyMaxWidth",
                typeof(double),
                typeof(KeyValuePairContainer), 
                new PropertyMetadata(double.PositiveInfinity)
                );




        /// <summary>
        /// Value部分的宽度设置
        /// </summary>
        public GridLength ValueWidth
        {
            get { return (GridLength)GetValue(ValueWidthProperty); }
            set { SetValue(ValueWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ValueWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueWidthProperty =
            DependencyProperty.Register(
                "ValueWidth", 
                typeof(GridLength),
                typeof(KeyValuePairContainer),
                new PropertyMetadata(new GridLength(1, GridUnitType.Star))
                );



        /// <summary>
        /// Value部分的最小宽度设置
        /// </summary>
        public double ValueMinWidth
        {
            get { return (double)GetValue(ValueMinWidthProperty); }
            set { SetValue(ValueMinWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ValueMinWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueMinWidthProperty =
            DependencyProperty.Register(
                "ValueMinWidth", 
                typeof(double), 
                typeof(KeyValuePairContainer), 
                new PropertyMetadata(0d)
                );



        /// <summary>
        /// Value部分的最大宽度设置
        /// </summary>
        public double ValueMaxWidth
        {
            get { return (double)GetValue(ValueMaxWidthProperty); }
            set { SetValue(ValueMaxWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ValueMaxWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueMaxWidthProperty =
            DependencyProperty.Register(
                "ValueMaxWidth",
                typeof(double),
                typeof(KeyValuePairContainer), 
                new PropertyMetadata(double.PositiveInfinity)
                );





        /// <summary>
        /// Key 文本
        /// </summary>
        public string KeyText
        {
            get { return (string)GetValue(KeyTextProperty); }
            set { SetValue(KeyTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for KeyText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty KeyTextProperty =
            DependencyProperty.Register("KeyText", typeof(string), typeof(KeyValuePairContainer), new PropertyMetadata(string.Empty)
            {
                PropertyChangedCallback = (s, e) =>
                {
                    if (s is KeyValuePairContainer container)
                    {
                        if (e.NewValue is string str)
                        {
                            string trim = str.Trim();
                            if (trim != str)
                            {
                                container.SetCurrentValue(KeyTextProperty, trim);
                            }
                        }
                    }
                }
            });



        #endregion

        #region 模板

        /// <summary>
        /// Key 区域的模板
        /// </summary>
        public ControlTemplate? KeyTemplate
        {
            get { return (ControlTemplate?)GetValue(KeyTemplateProperty); }
            set { SetValue(KeyTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for KeyTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty KeyTemplateProperty =
            DependencyProperty.Register("KeyTemplate", typeof(ControlTemplate), typeof(KeyValuePairContainer),
                new PropertyMetadata(null)
                {
                    PropertyChangedCallback = (s, e) =>
                    {
                        if (s is KeyValuePairContainer container)
                        {
                            if (e.NewValue is ControlTemplate template)
                            {
                                var obj = template.LoadContent();
                                if (obj is FrameworkElement element)
                                {
                                    element.DataContext = container.DataContext;
                                }
                            }
                        }
                    }
                });




        #endregion
    }
}
