using Common_Util.Excel.NPOI.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Excel.NPOI.SkiaSharp.Extensions
{
    public static class TableWriteHelperEx
    {
        public static void UseSkiaSharp(this TableWriteHelper _)
        {
            TableWriteHelper.AddSetValueExtension(_setValueExImpl);
        }
        private static bool _setValueExImpl(object value, Func<TableWriteHelper.SetValueExtensionContext> getContextFunc)
        {
            // 待实现
            return false;
        }
    }
}
