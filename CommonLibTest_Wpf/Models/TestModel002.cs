using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Wpf.Models
{
    public class TestModel002 : NotifyTestModelBase, IOtherTestModel
    {
        private static int _i;

        public TestModel002()
        {
            I = _i++;
            A = _i.ToString();
        }

        public int I { get; set; }

        public string A { get; set; } = string.Empty;


        public override string ToString()
        {
            return $"{I} => {A}";
        }
    }
}
