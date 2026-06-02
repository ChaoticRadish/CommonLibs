using ChaoticKit.Data.Wrapped;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaoticKit.LibTest.Wpf.Models
{
    public class TestModel003 : ITestModel
    {
        public SuspendableObservableCollection<TestModel002> List { get; set; } = new();
    }
}
