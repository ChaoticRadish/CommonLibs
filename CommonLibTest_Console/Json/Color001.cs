using Common_Util.Data.Structure.Pair;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CommonLibTest_Console.Json
{
    internal class Color001() : TestBase("RGB 颜色值转换为 JSON")
    {
        protected override void RunImpl()
        {
            RgbaColorB color = 0xFF112233;
            string value = System.Text.Json.JsonSerializer.Serialize(color);
            WritePair(value);
            WritePair(uint.Parse(value).ToString("X2"));
            WritePair(System.Text.Json.JsonSerializer.Deserialize<RgbaColorB>(value));

            WritePair(color.R = 0x66);

            value = System.Text.Json.JsonSerializer.Serialize(color);
            WritePair(value);
            WritePair(uint.Parse(value).ToString("X2"));
            WritePair(System.Text.Json.JsonSerializer.Deserialize<RgbaColorB>(value));
        }
    }
}
