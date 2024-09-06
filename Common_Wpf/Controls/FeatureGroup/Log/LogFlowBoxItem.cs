using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Common_Wpf.Controls.FeatureGroup
{
    public class LogFlowBoxItem : MbControl01
    {
        static LogFlowBoxItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LogFlowBoxItem), new FrameworkPropertyMetadata(typeof(LogFlowBoxItem)));

        }



        public LogFlowBoxModel ShowingData
        {
            get { return (LogFlowBoxModel)GetValue(ShowingDataProperty); }
            set { SetValue(ShowingDataProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowingData.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowingDataProperty =
            DependencyProperty.Register("ShowingData", typeof(LogFlowBoxModel), typeof(LogFlowBoxItem), 
                new PropertyMetadata(new LogFlowBoxModel()));


    }
}
