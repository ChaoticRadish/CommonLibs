using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Common_Wpf.Controls
{
    /// <summary>
    /// ContentControl 改进版01, 实现了一些接口, 增加了一些属性, UI需另外再实现
    /// <para>· 实现 <see cref="INotifyPropertyChanged"/> 接口</para>
    /// </summary>
    public abstract class MbContentControl01 : ContentControl, INotifyPropertyChanged
    {
        #region 通知
        public event PropertyChangedEventHandler? PropertyChanged;
        public event PropertyChangingEventHandler? PropertyChanging;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        protected virtual void OnPropertyChanging([CallerMemberName] string propertyName = "")
        {
            if (this.PropertyChanging != null)
                this.PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
        }

        #endregion
    }
}
