using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Excel.NPOI.Helper
{
    public static class TableReadHelper
    {
        public static DataTable ImportAsTable(string fileName, string? worksheetName = null)
        {
            DataTable table = new DataTable();

            IWorkbook workbook;
            using (FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                if (Path.GetExtension(fileName).ToLower().Equals(".xlsx"))
                {
                    workbook = new XSSFWorkbook(file);
                }
                else
                {
                    workbook = new HSSFWorkbook(file);
                }
            }
            ISheet sheet;
            if (!string.IsNullOrEmpty(worksheetName))
            {
                sheet = workbook.GetSheet(worksheetName);
            }
            else
            {
                sheet = workbook.GetSheetAt(0);
            }

            IRow headerRow = sheet.GetRow(0);
            int headerColumnCount = headerRow.LastCellNum;

            for (int c = 0; c < headerColumnCount; c++)
            {// 循环获取列名
                ICell headerCell = headerRow.GetCell(c);
                if (headerCell == null)
                {
                    continue;
                }
                string? cellValue = headerCell.ToString();
                if (string.IsNullOrEmpty(cellValue) || table.Columns.Contains(cellValue))
                {
                    continue;
                }
                DataColumn column = table.Columns.Add(headerCell.ToString());

                for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++)
                {
                    if (i > table.Rows.Count)
                    {
                        table.Rows.Add();
                    }
                    ICell? cell = sheet.GetRow(i)?.GetCell(c);
                    if (cell != null)
                    {
                        string? v = cell.ToString();
                        if (!string.IsNullOrEmpty(v))
                        {
                            table.Rows[i - 1][column] = v;
                        }
                    }
                }

            }
            return table;
        }
    }
}
