using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Wpf.Models
{
    /// <summary>
    /// 实现了以下接口的测试实体
    /// <para><see cref="INotifyPropertyChanged"/></para>
    /// <para><see cref="INotifyPropertyChanging"/></para>
    /// </summary>
    public abstract class NotifyTestModelBase : ITestModel, INotifyPropertyChanged, INotifyPropertyChanging
    {
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
    }
}
