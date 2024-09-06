using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Common_Wpf.BindingProxy
{
    public class BoolProxy : Freezable
    {
        protected override Freezable CreateInstanceCore()
        {
            return new BoolProxy();
        }

        public bool Data
        {
            get { return (bool)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Data.
        // This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(bool),
            typeof(BoolProxy), new PropertyMetadata(false));
    }
}
