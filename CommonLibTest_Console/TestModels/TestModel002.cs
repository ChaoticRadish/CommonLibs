using Common_Util.Attributes.General;
using Common_Util.Data.Structure.Pair;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.TestModels
{
    internal class TestModel002
    {
        [DefaultValue("999", "123456")]
        public string ABC { get; set; } = string.Empty;

        [DefaultValue("999; 123456; 999; 12aqw")]
        public List<string>? Test { get; set; }

        public Type? T { get; set; }

        [DefaultValue("aaa:bbb")]
        public GroupIdPair Pair { get; set; }
    }
}
