using Common_Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibTest_Console.CSharp
{
    internal class Enum002() : TestBase($"测试 {nameof(EnumHelper.All)} 和取得枚举类型中名字匹配的项")
    {
        protected override void RunImpl()
        {
            WritePair(Enums.CCC == Enums.DDD);
            WriteLine("测试1");
            foreach (var item in EnumHelper.All<Enums>())
            {
                WriteLine($"{(int)item}. {item}");
            }
            WriteLine("测试2");
            foreach (var item in EnumHelper.All<Enums>().Distinct())
            {
                WriteLine($"{(int)item}. {item}");
            }


            WritePair($"{EnumHelper.TryGetMatchName<Enums>(nameof(Enums.AAA), out var _e1)} {_e1}");
            WritePair($"{EnumHelper.TryGetMatchName<Enums>(nameof(Enums.BBB), out var _e2)} {_e2}");
            WritePair($"{EnumHelper.TryGetMatchName<Enums>(nameof(Enums.CCC), out var _e3)} {_e3}");
            WritePair($"{EnumHelper.TryGetMatchName<Enums>(nameof(Enums.DDD), out var _e4)} {_e4}");
            WritePair($"{EnumHelper.TryGetMatchName<Enums>("ddd", out var _e5)} {_e5}");
            WritePair($"{EnumHelper.TryGetMatchName<Enums>("DDD", out var _e6)} {_e6}");
        }

        private enum Enums : int    
        {
            AAA = 0,
            BBB = 1,
            CCC = 2,
            DDD = 2,
        }
    }
}
