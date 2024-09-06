using Common_Util.Attributes.Random;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Wpf.Models
{
    /// <summary>
    /// 测试实体001
    /// <para>拥有很多个字段, 主要是字符串</para>
    /// </summary>
    public class TestModel001 : ITestModel, INotifyPropertyChanged
    {

        private string strA = string.Empty;
        private string strB = string.Empty;
        private string strF = string.Empty;

        public int Id { get; set; }
        public bool B { get; set; }

        [IntRange(8, 15)]
        public string StrA
        {
            get => strA;
            set
            {
                strA = value;
                OnPropertyChanged(nameof(StrA));
            }
        }
        [IntRange(10, 15)]
        public string StrB 
        { 
            get => strB; 
            set
            {
                strB = value;
                OnPropertyChanged(nameof(StrB));
            }
        }
        [IntRange(12, 15)]
        public string StrC { get; set; } = string.Empty;
        [IntRange(15, 15)]
        public string StrD { get; set; } = string.Empty;
        [IntRange(1, 15)]
        public string StrE { get; set; } = string.Empty;
        [IntRange(2, 15)]
        public string StrF 
        { 
            get => strF; 
            set 
            {
                strF = value; 
                OnPropertyChanged(nameof(StrF));
            } 
        }
        [IntRange(3, 15)]
        public string StrG { get; set; } = string.Empty;
        [IntRange(4, 15)]
        public string StrH { get; set; } = string.Empty;
        public string StrI { get; set; } = string.Empty;

        public float? F 
        {
            get => f;
            set
            {
                f = value;
                OnPropertyChanged(nameof(F));
            }
        }
        private float? f;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class TestModelList001 : List<TestModel001>
    {

    }
}
