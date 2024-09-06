using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Wpf.Models
{
    public class TestModel004 : ITestModel
    {
        public required string Value { get; set; }
        public override string ToString()
        {
            return $"{nameof(TestModel004)} => {Value}";
        }
    }
}
