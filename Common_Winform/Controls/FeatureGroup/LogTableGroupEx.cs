using Common_Util.Attributes.General;
using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common_Winform.Controls.FeatureGroup
{
    public static class LogTableGroupEx
    {
        /// <summary>
        /// 设置枚举日志输出器作为类别
        /// </summary>
        /// <param name="table"></param>
        /// <param name="code"></param>
        /// <param name="name"></param>
        /// <param name="show"></param>
        public static void SetEnumLoggerType(this LogTableGroup table, Enum code, string? name = null, bool show = true)
        {
            Type type = code.GetType();
            FieldInfo? field = type.GetField(code.ToString());
            if (field != null && field.ExistCustomAttribute<LoggerAttribute>(out var attr) && attr != null)
            {
                table.SetType(attr.Category, name ?? attr.Category, show);
            }
        }
    }
}
