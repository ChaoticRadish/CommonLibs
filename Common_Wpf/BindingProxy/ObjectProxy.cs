using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Common_Wpf.BindingProxy
{
    public class ObjectProxy : Freezable
    {
        protected override Freezable CreateInstanceCore()
        {
            return new ObjectProxy();
        }



        public object Data
        {
            get { return GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Data.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register(nameof(Data), typeof(object), typeof(ObjectProxy), new PropertyMetadata(null));


    }
}
