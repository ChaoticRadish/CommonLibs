using Common_Util.Extensions;
using Common_Util.Module.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.DynamicIL
{
    internal class ValueTuple001() : TestBase("测试反射创建元组")
    {
        protected override void RunImpl()
        {
            Type type;
            WritePair(type = typeof(ValueTuple<,>).MakeGenericType(typeof(string), typeof(int)));
            WritePair(ValueTupleHelper.Unpack(type).FullInfoString());
            WritePair(type = typeof(ValueTuple<>).MakeGenericType(typeof(string)));
            WritePair(ValueTupleHelper.Unpack(type).FullInfoString());
            WritePair(type = (1,2,3,4,5,6,7,8).GetType());
            WritePair(ValueTupleHelper.Unpack(type).FullInfoString());
            WritePair(type = (1,2,3,4,5,6,7,8,9,10).GetType());
            WritePair(ValueTupleHelper.Unpack(type).FullInfoString());

            List<Type> range = [typeof(string), typeof(int), typeof(byte), typeof(float), typeof(bool)];
            List<Type> types = new List<Type>();
            foreach (var i in 20.ForUntil())
            {
                types.Add(range.Random());
                WritePair(ValueTupleHelper.MakeValueTupleType(types));
            }
        }
    }
}
