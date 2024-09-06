using Common_Util.Attributes.General;
using Common_Util.Data.Extensions;
using Common_Util.Data.Structure.Map;
using Common_Util.Extensions;
using Common_Util.Interfaces.Owner;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.DataStruct
{
    internal class TagOwner001() : TestBase("测试拥有标签的东西的集合的扩展方法")
    {
        protected override void RunImpl()
        {
            TestClass[] test = [
                new("A", "11", "22", "33", "qqq"),
                new("B", "DD", "AA", "1", "qqq"),
                new("C"),
                new("D", "DD", "22", "33", "99"),
                new("E", null, "22", "1", "qqq"),
                new("F", null, null, null),
                new("G", null, null),
                ];

            WritePair(key: "QueryAllNoTag", test.QueryAllNoTag().ToList().FullInfoString(), " => \n");
            WriteEmptyLine();
            WritePair(key: "QueryAllNoTag(\"1\")", test.QueryExistTag("1").ToList().FullInfoString(), " => \n");
            WriteEmptyLine();
            WritePair(key: "QueryWhere(i => i.Count() == 3 && i.All(s => s == null))", test.QueryWhere(i => i.Count() == 3 && i.All(s => s == null)).ToList().FullInfoString(), " => \n");
            WriteEmptyLine();
            WritePair(key: "QueryMatchingTag(\"1\", \"AA\", \"DD\")", test.QueryMatchingTag("1", "AA", "DD").ToList().FullInfoString(), " => \n");
            WriteEmptyLine();

            TagsMap<string, TestClass> map = test.AsTagsMap(i => i.Name);
            WritePair(key: "AsTagsMap => ToList()", map.ToList().FullInfoString(), " => \n");
            WriteEmptyLine();

            map.Values.QueryAllNoTag();
        }

        [InfoToString]
        public class TestClass(string name, params string?[] tags) : ITagsOwner
        {
            public string Name { get; } = name;
            public string?[] Tags { get; } = tags;

            IEnumerable<string?> ITagsOwner.Tags => Tags;

            public override string ToString()
            {
                return $"{Name} [{Common_Util.String.StringHelper.Concat(Tags.Select(i => i ?? "<null>").ToArray(), ", ")}]";
            }


        }
    }
}
