using Common_Util.Module.Mission;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.Streaming;
using NPOI.XSSF.UserModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Excel.NPOI.Helper
{
    public class TableWriteHelper : IDisposable, IProgressMission
    {
        public int ProgressLevel { get; set; }
        public IProgressMissionContainer? Container { get; set; }
        public Action MissionBody { get; set; }
        public string MissionName { get; set; } = string.Empty;
        /// <summary>
        /// 实例化
        /// </summary>
        public TableWriteHelper()
        {
            MissionBody = () =>
            {
                Save();
            };
        }

        #region 参数
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; private set; } = "默认标题";
        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; private set; } = "默认文件名";
        /// <summary>
        /// 设置路径
        /// </summary>
        public string DirPath { get; private set; } = ".";
        /// <summary>
        /// 格式
        /// </summary>
        public FormatEnum Format { get; private set; } = FormatEnum.XSSF;
        /// <summary>
        /// 只读
        /// </summary>
        public bool ReadOnly { get; private set; } = true;
        /// <summary>
        /// 工作表配置
        /// </summary>
        private SheetOption? SheetOption;
        /// <summary>
        /// 列配置
        /// </summary>
        private readonly Dictionary<int, ColumnOption> ColumnOptions = new Dictionary<int, ColumnOption>();
        /// <summary>
        /// 列字体配置
        /// </summary>
        private readonly Dictionary<int, IFont> ColumnFontOptions = new Dictionary<int, IFont>();
        /// <summary>
        /// 数据
        /// </summary>
        private List<object> Datas = new List<object>();
        #endregion

        #region 属性设置
        /// <summary>
        /// 设置导出的表格是否只读
        /// </summary>
        /// <param name="b"></param>
        public void SetReadonly(bool b)
        {
            ReadOnly = b;
        }
        /// <summary>
        /// 设置格式
        /// </summary>
        /// <param name="format"></param>
        public void SetFormat(FormatEnum format)
        {
            Format = format;
        }
        /// <summary>
        /// 格式枚举
        /// </summary>
        public enum FormatEnum
        {
            /// <summary>
            /// HSSFWorkbook:是操作Excel2003以前（包括2003）的版本，扩展名是.xls
            /// </summary>
            HSSF,
            /// <summary>
            /// XSSFWorkbook:是操作Excel2007后的版本，扩展名是.xlsx
            /// </summary>
            XSSF,
            /// <summary>
            /// SXSSFWorkbook:是操作Excel2007后的版本，扩展名是.xlsx
            /// </summary>
            SXSSF,
        }

        #endregion

        #region 标题及文件名
        /// <summary>
        /// 设置标题及文件名
        /// </summary>
        /// <param name="title"></param>
        public void SetTitle(string title, string? fileName = null)
        {
            if (title == null)
            {
                title = string.Empty;
            }
            if (fileName == null)
            {
                fileName = title;
            }
            Title = title;
            FileName = fileName;
        }
        /// <summary>
        /// 设置文件名
        /// </summary>
        /// <param name="dir"></param>
        public void SetDir(string dir)
        {
            if (dir == null)
            {
                dir = "";
            }
            DirPath = dir;
        }
        #endregion

        #region 配置
        /// <summary>
        /// 以类型配置数据及列
        /// </summary>
        /// <typeparam name="Item"></typeparam>
        public void SetOption<Item>()
        {
            Type type = typeof(Item);
            SetOption(type);
        }
        /// <summary>
        /// 以类型配置数据及列
        /// </summary>
        /// <param name="itemType"></param>
        public void SetOption(Type itemType)
        {
            SheetOption sheetOption;

            // 工作表配置
            Attributes.SheetNameAttribute? sheetNameAttribute
                = itemType.GetCustomAttribute<Attributes.SheetNameAttribute>();
            Attributes.UseTitleAttribute? useTitleAttribute
                = itemType.GetCustomAttribute<Attributes.UseTitleAttribute>();
            Attributes.TitleSizeAttribute? titleSizeAttribute
                = itemType.GetCustomAttribute<Attributes.TitleSizeAttribute>();
            Attributes.HeadSizeAttribute? headSizeAttribute
                = itemType.GetCustomAttribute<Attributes.HeadSizeAttribute>();
            Attributes.UseSummeryAttribute? useSummaryAttribute
                = itemType.GetCustomAttribute<Attributes.UseSummeryAttribute>();
            Attributes.SummarySizeAttribute? summarySizeAttribute
                = itemType.GetCustomAttribute<Attributes.SummarySizeAttribute>();
            Attributes.RowBackColorAttribute? rowBackColorAttribute
                = itemType.GetCustomAttribute<Attributes.RowBackColorAttribute>();
            Attributes.DataRowSizeAttribute? dataRowSizeAttribute
                = itemType.GetCustomAttribute<Attributes.DataRowSizeAttribute>();
            sheetOption = new SheetOption()
            {
                Name = sheetNameAttribute?.Name ?? string.Empty,
                UseTitle = useTitleAttribute != null,
                TitleHeight = titleSizeAttribute == null ? 50 : titleSizeAttribute.Height,
                TitleFontHeight = titleSizeAttribute == null ? 40 : titleSizeAttribute.FontHeight,
                HeadHeight = headSizeAttribute == null ? 25 : headSizeAttribute.Height,
                HeadFontHeight = headSizeAttribute == null ? 16 : headSizeAttribute.FontHeight,
                UseSummery = useSummaryAttribute != null,
                SummaryHeight = summarySizeAttribute == null ? 25 : summarySizeAttribute.Height,
                SummaryFontHeight = summarySizeAttribute == null ? 16 : summarySizeAttribute.FontHeight,
                OddRowBackColor
                    = rowBackColorAttribute == null ?
                    System.Drawing.Color.FromArgb(255, 220, 220, 220) : rowBackColorAttribute.OddRowBackColor,
                EvenRowBackColor
                    = rowBackColorAttribute == null ?
                    System.Drawing.Color.White : rowBackColorAttribute.EvenRowBackColor,
                HeadRowBackColor
                    = rowBackColorAttribute == null ?
                    System.Drawing.Color.FromArgb(255, 170, 170, 170) : rowBackColorAttribute.HeadRowBackColor,
                SummaryRowBackColor
                    = rowBackColorAttribute == null ?
                    System.Drawing.Color.FromArgb(255, 190, 190, 190) : rowBackColorAttribute.SummaryRowBackColor,
                DataOddRowHeight
                    = dataRowSizeAttribute == null ?
                    18 : dataRowSizeAttribute.OddRowHeight,
                DataEvenRowHeight
                    = dataRowSizeAttribute == null ?
                    18 : dataRowSizeAttribute.EvenRowHeight,
            };


            // 列配置读取为列表
            List<ColumnOption> options = new List<ColumnOption>();
            PropertyInfo[] propertyInfos = itemType.GetProperties();
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                Attributes.InvisableAttribute? invisable
                    = propertyInfo.GetCustomAttribute<Attributes.InvisableAttribute>();
                if (invisable != null) continue;

                Attributes.IndexColumnAttribute? indexColumnAttribute
                    = propertyInfo.GetCustomAttribute<Attributes.IndexColumnAttribute>();
                bool isIndexColumn = indexColumnAttribute != null;
                int indexStartValue = indexColumnAttribute == null ? 1 : indexColumnAttribute.IndexStartValue;

                Attributes.ColumnIndexAttribute? indexAttribute
                    = propertyInfo.GetCustomAttribute<Attributes.ColumnIndexAttribute>();
                int index = indexAttribute == null ? 0 : indexAttribute.Index;
                if (index < 0) continue;

                Attributes.ColumnNameAttribute? columnNameAttribute
                    = propertyInfo.GetCustomAttribute<Attributes.ColumnNameAttribute>();
                string columnName = columnNameAttribute == null ? propertyInfo.Name : columnNameAttribute.Name;

                Attributes.WrapTextAttribute? wrapTextAttribute
                    = propertyInfo.GetCustomAttribute<Attributes.WrapTextAttribute>();
                bool warpText = wrapTextAttribute != null;

                Attributes.ColumnLayoutAttribute? columnLayoutAttribute
                    = propertyInfo.GetCustomAttribute<Attributes.ColumnLayoutAttribute>();
                int columnWidth = columnLayoutAttribute == null ? 15 : columnLayoutAttribute.ColumnWidth;
                int maxColumnWidth = columnLayoutAttribute == null ? 15 : columnLayoutAttribute.MaxColumnWidth;
                bool autoColumnWidth = columnLayoutAttribute == null ? false : columnLayoutAttribute.AutoColumnWidth;
                HorizontalAlignment horizontalAlignment
                    = columnLayoutAttribute == null ?
                    HorizontalAlignment.Center : columnLayoutAttribute.HorizontalAlignment;
                VerticalAlignment verticalAlignment
                    = columnLayoutAttribute == null ?
                    VerticalAlignment.Top : columnLayoutAttribute.VerticalAlignment;

                Attributes.ColumnFontAttribute? columnFontAttribute
                    = propertyInfo.GetCustomAttribute<Attributes.ColumnFontAttribute>();
                bool isbold = columnFontAttribute == null ? false : columnFontAttribute.IsBold;
                float fontHeight = columnFontAttribute == null ? 14 : columnFontAttribute.Height;

                Attributes.SummaryAttribute? summaryAttribute
                    = propertyInfo.GetCustomAttribute<Attributes.SummaryAttribute>();
                SummaryMethodEnum summaryMethod
                    = summaryAttribute == null ? SummaryMethodEnum.Empty : summaryAttribute.SummaryMethod;
                Func<List<object?>, string>? customSummaryMethod = summaryAttribute?.CustomSummaryMethod;
                string summaryStringFormat = summaryAttribute == null ? "{result}" : summaryAttribute.SummaryStringFormat;


                ColumnOption columnOption = new ColumnOption()
                {
                    Index = index,
                    IsIndexColumn = isIndexColumn,
                    IndexStartValue = indexStartValue,
                    BindingPropertyName = propertyInfo.Name,
                    ColumnName = columnName,
                    WrapText = warpText,
                    ColumnWidth = columnWidth * 256,
                    MaxColumnWidh = maxColumnWidth * 256,
                    AutoColumnWidth = autoColumnWidth,
                    HorizontalAlignment = horizontalAlignment,
                    VerticalAlignment = verticalAlignment,
                    FontIsBold = isbold,
                    FontHeight = fontHeight,
                    SummaryMethod = summaryMethod,
                    SummaryStringFormat = summaryStringFormat,
                    CustomSummaryMethod = customSummaryMethod,
                };
                options.Add(columnOption);
            }

            SetOption(sheetOption, options);
        }
        /// <summary>
        /// 以输入配置数据及列
        /// </summary>
        /// <param name="sOption"></param>
        /// <param name="cOptions"></param>
        public void SetOption(SheetOption sOption, List<ColumnOption> cOptions)
        {
            // 设置表配置
            SheetOption = sOption;
            // 整理列配置列表
            int GetNextUnusedNumber()   // 取得下一个未被使用的数字的方法
            {
                int output = 0;
                while (ColumnOptions.ContainsKey(output))
                {
                    output++;
                }
                return output;
            }
            //清空列配置
            ColumnOptions.Clear();
            foreach (ColumnOption option in cOptions)
            {
                if (ColumnOptions.ContainsKey(option.Index))
                {
                    option.Index = GetNextUnusedNumber();
                }
                ColumnOptions.Add(option.Index, option);
            }
        }
        #endregion

        #region 写数据
        /// <summary>
        /// 清空数据
        /// </summary>
        public void ClearData()
        {
            Datas.Clear();
        }
        /// <summary>
        /// 设置数据
        /// </summary>
        /// <typeparam name="Item"></typeparam>
        /// <param name="datas">写入数据</param>
        public void WriteData(IEnumerable<object> datas)
        {
            if (datas != null)
            {
                foreach (object data in datas)
                {
                    Datas.Add(data);
                }
            }
        }
        /// <summary>
        /// 裁剪
        /// </summary>
        /// <param name="start"></param>
        /// <param name="count"></param>
        public void Limit(int start, int count)
        {
            Datas = Datas.Skip(start >= 0 ? start : 0).Take(count >= 0 ? count : 0).ToList();
        }
        #endregion

        #region 写汇总列

        #endregion


        #region 释放
        /// <summary>
        /// 是否回收完毕
        /// </summary>
        public bool Disposed { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~TableWriteHelper()
        {
            Dispose(false);
        }
        /// <summary>
        /// 是否需要释放那些实现IDisposable接口的托管对象
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (Disposed) return; //如果已经被回收，就中断执行
            if (disposing)
            {
                //TODO:释放那些实现IDisposable接口的托管对象
            }
            //TODO:释放非托管资源，设置对象为null
            Drawing = null;
            Sheet = null;
            Workbook = null;

            Disposed = true;
        }
        #endregion

        #region 保存
        #region 保存过程中的临时对象
        private IWorkbook? Workbook;
        private ISheet? Sheet;
        private IDrawing? Drawing;
        #endregion
        /// <summary>
        /// 保存到指定路径
        /// </summary>
        /// <param name="dirPath">所属文件夹路径c</param>
        public bool Save()
        {
            this.UpdateProgress(0, "正在检查参数是否完整");
            //---- 检查参数是否足够
            if (SheetOption == null || ColumnOptions.Count == 0)
            {
                this.UpdateProgress(0, "参数不足");
                return false;
            }
            // 列索引列表
            List<int> columnIndexList = ColumnOptions.Keys.ToList();

            this.UpdateProgress(2f, "正在检查路径是否可用");
            //---- 检查路径是否可用
            string fullFilePath;
            try
            {
                DirectoryInfo info = new DirectoryInfo(DirPath);
                if (!info.Exists)
                {
                    this.UpdateProgress(4f, "路径不存在");
                    return false;
                }
                StringBuilder builder = new StringBuilder();
                builder.Append(info.FullName).Append("/").Append(FileName);
                switch (Format)
                {
                    case FormatEnum.HSSF:
                        this.UpdateProgress(5f, "文件格式: xls");
                        builder.Append(".xls");
                        break;
                    default:
                    case FormatEnum.XSSF:
                    case FormatEnum.SXSSF:
                        this.UpdateProgress(5f, "文件格式: xlsx");
                        builder.Append(".xlsx");
                        break;
                }
                fullFilePath = builder.ToString();
                new FileInfo(fullFilePath);
            }
            catch (Exception e)
            {
                this.UpdateProgress(10f, "检查路径是否可用时发生异常: " + e.Message);
                return false;
            }

            this.UpdateProgress(10f, "实例化工作簿");
            //---- 工作簿
            switch (Format)
            {
                case FormatEnum.HSSF:
                    this.UpdateProgress(11f, "HSSFWorkbook");
                    Workbook = new HSSFWorkbook();
                    break;
                case FormatEnum.SXSSF:
                    this.UpdateProgress(11f, "SXSSFWorkbook");
                    Workbook = new SXSSFWorkbook();
                    break;

                default:
                case FormatEnum.XSSF:
                    this.UpdateProgress(11f, "XSSFWorkbook");
                    Workbook = new XSSFWorkbook();
                    break;
            }
            //---- 工作表
            this.UpdateProgress(12f, "实例化工作表");
            Sheet = string.IsNullOrEmpty(SheetOption.Name) ?
                Workbook.CreateSheet() : Workbook.CreateSheet(SheetOption.Name);
            //---- 画廊
            Drawing = Sheet.CreateDrawingPatriarch();

            // 当前行索引
            int rowIndex = 0;
            //---- 标题行
            if (SheetOption.UseTitle)
            {
                this.UpdateProgress(12f, "写入标题行");
                // 合并后的区域
                CellRangeAddress titleRegion
                    = new CellRangeAddress(0, 0, 0, columnIndexList.Max());
                // 合并
                Sheet.AddMergedRegion(titleRegion);
                // 取得单元格
                IRow titleRow = GetRow(rowIndex);
                ICell titleCell = GetCell(titleRow, rowIndex);
                // 样式
                titleRow.HeightInPoints = SheetOption.TitleHeight;

                ICellStyle titleStyle = Workbook.CreateCellStyle();
                IFont titlefont = GetFontAsSetting(new FontSetting()
                {
                    IsBold = true,
                    Height = SheetOption.HeadFontHeight,
                });
                titleStyle.SetFont(titlefont);
                titleStyle.BorderLeft = BorderStyle.None;
                titleStyle.BorderRight = BorderStyle.None;
                titleStyle.BorderTop = BorderStyle.Double;
                titleStyle.BorderBottom = BorderStyle.Double;
                titleStyle.Alignment = HorizontalAlignment.Center;
                titleStyle.VerticalAlignment = VerticalAlignment.Center;

                SetRegionCellStyle(titleRegion, titleStyle);

                // 赋值
                titleCell.SetCellValue(Title);

                // 行索引自增
                rowIndex++;
            }


            //---- 列头行
            this.UpdateProgress(14f, "写入列头行");
            IRow headRow = GetRow(rowIndex);
            // 行高
            headRow.HeightInPoints = SheetOption.HeadHeight;
            ColumnFontOptions.Clear();
            // 赋值
            for (int i = 0; i < ColumnOptions.Count; i++)
            {
                // 列索引
                int columnIndex = columnIndexList[i];
                // 列配置
                ColumnOption option = ColumnOptions[columnIndex];

                // 取得单元格
                ICell cell = GetCell(headRow, columnIndex);
                //样式 
                ICellStyle style = GetCellStyleAsColor(new StyleColor(SheetOption.HeadRowBackColor));
                IFont font = GetFontAsSetting(new FontSetting()
                {
                    Height = SheetOption.HeadFontHeight,
                });
                style.SetFont(font);
                style.BorderLeft = columnIndex == 0 ? BorderStyle.Medium : BorderStyle.DashDot;
                style.BorderRight = columnIndex == columnIndexList.Max() ? BorderStyle.Medium : BorderStyle.DashDot;
                style.BorderTop = BorderStyle.Medium;
                style.BorderBottom = BorderStyle.Medium;
                style.Alignment = option.HorizontalAlignment;
                style.VerticalAlignment = option.VerticalAlignment;
                style.WrapText = option.WrapText;
                cell.CellStyle = style;
                // 赋值
                cell.SetCellValue(option.ColumnName);

                // 设置列宽
                if (option.AutoColumnWidth)
                {
                    try
                    {
                        Sheet.AutoSizeColumn(columnIndex);
                    }
                    catch { }
                }
                else
                {
                    Sheet.SetColumnWidth(columnIndex, option.ColumnWidth);
                }

                // 提前实例化列字体
                ColumnFontOptions.Add(option.Index, GetFontAsSetting(new FontSetting()
                {
                    Height = option.FontHeight,
                    IsBold = option.FontIsBold,
                }));
            }
            // 行索引自增
            rowIndex++;


            //---- 数据行
            int dataRowStartIndex = rowIndex;
            this.UpdateProgress(18f, "写入数据行");
            // 通用设置对象
            ICellStyle oddRowStyle = GetCellStyleAsColor(new StyleColor(SheetOption.OddRowBackColor));
            ICellStyle EvenRowStyle = GetCellStyleAsColor(new StyleColor(SheetOption.EvenRowBackColor));
            foreach (object data in Datas)
            {
                IRow row = GetRow(rowIndex);

                // 是奇数行
                bool isOddRow = (rowIndex - dataRowStartIndex) % 2 != 0;

                if (isOddRow)
                {
                    row.HeightInPoints = SheetOption.DataOddRowHeight;
                }
                else
                {
                    row.HeightInPoints = SheetOption.DataEvenRowHeight;
                }

                // 当前进度
                float progressNow = 20f + (rowIndex - dataRowStartIndex) / Datas.Count * (90f - 20f);
                // 两行间的进度差
                float progressDetail = (90f - 20f) / Datas.Count;
                this.UpdateProgress(progressNow, $"写入数据行: ( {rowIndex - dataRowStartIndex} / {Datas.Count} )");

                for (int c = 0; c < ColumnOptions.Count; c++)
                {
                    // 列索引
                    int columnIndex = columnIndexList[c];

                    this.UpdateProgress(progressNow + (float)c / ColumnOptions.Count * progressDetail, $"单元格: {ColumnOptions[columnIndex].Index} ( {columnIndex} / {ColumnOptions.Count} )");
                    // 列配置
                    ColumnOption option = ColumnOptions[columnIndex];
                    // 单元格
                    ICell cell = GetCell(row, columnIndex);


                    // 样式
                    ICellStyle style = isOddRow ? oddRowStyle : EvenRowStyle;
                    IFont font = ColumnFontOptions[option.Index];
                    style.SetFont(font);
                    style.BorderLeft = BorderStyle.DashDot;
                    style.BorderRight = BorderStyle.DashDot;
                    style.BorderTop = BorderStyle.Dotted;
                    style.BorderBottom = BorderStyle.Dotted;
                    style.Alignment = option.HorizontalAlignment;
                    style.VerticalAlignment = option.VerticalAlignment;
                    style.WrapText = option.WrapText;

                    // 赋值
                    if (option.IsIndexColumn)
                    {
                        cell.SetCellValue((rowIndex - dataRowStartIndex + option.IndexStartValue).ToString());
                    }
                    else
                    {
                        object? value = GetPropertyValue(option.BindingPropertyName, data);
                        if (value != null)
                        {
                            // 单元格赋值
                            SetValue(cell, value);
                        }
                    }

                    cell.CellStyle = style;
                }
                rowIndex++;
            }
            //---- 汇总行
            if (SheetOption.UseSummery)
            {
                this.UpdateProgress(90f, $"写入汇总行");
                // 此时rowInex是汇总行的行索引
                IRow summaryRow = GetRow(rowIndex);
                // 行高
                summaryRow.HeightInPoints = SheetOption.SummaryHeight;



                // 赋值
                for (int i = 0; i < ColumnOptions.Count; i++)
                {
                    // 列索引
                    int columnIndex = columnIndexList[i];
                    // 列配置
                    ColumnOption option = ColumnOptions[columnIndex];

                    // 取得单元格
                    ICell cell = GetCell(summaryRow, columnIndex);
                    //样式 
                    ICellStyle style = GetCellStyleAsColor(new StyleColor(SheetOption.SummaryRowBackColor));
                    IFont font = GetFontAsSetting(new FontSetting()
                    {
                        Height = SheetOption.SummaryFontHeight,
                    });
                    style.SetFont(font);
                    style.BorderLeft = columnIndex == 0 ? BorderStyle.Medium : BorderStyle.DashDot;
                    style.BorderRight = columnIndex == columnIndexList.Max() ? BorderStyle.Medium : BorderStyle.DashDot;
                    style.BorderTop = BorderStyle.Medium;
                    style.BorderBottom = BorderStyle.Medium;
                    style.Alignment = option.HorizontalAlignment;
                    style.VerticalAlignment = option.VerticalAlignment;
                    style.WrapText = option.WrapText;
                    cell.CellStyle = style;

                    // 取得值列表
                    List<object?> columnDatas = new List<object?>();
                    if (option.IsIndexColumn)
                    {
                        for (int index = option.IndexStartValue; index < option.IndexStartValue + Datas.Count; index++)
                        {
                            columnDatas.Add(index);
                        }
                    }
                    else
                    {
                        foreach (object data in Datas)
                        {
                            Type type = data.GetType();
                            // 属性信息
                            PropertyInfo? propertyInfo = type.GetProperty(option.BindingPropertyName);
                            // 取得数据
                            object? propertyValue = propertyInfo?.GetValue(data, null);

                            columnDatas.Add(propertyValue);
                        }
                    }
                    // 赋值
                    cell.SetCellValue(GetSummaryString(option, columnDatas));
                }

            }


            //---- 重新调整数据区域单元格尺寸
            this.UpdateProgress(92f, $"重新调整数据区域单元格尺寸");
            for (int c = 0; c < ColumnOptions.Count; c++)
            {
                // 列索引
                int columnIndex = columnIndexList[c];
                // 列配置
                ColumnOption option = ColumnOptions[columnIndex];

                // 设置列宽
                if (option.AutoColumnWidth)
                {
                    try
                    {
                        Sheet.AutoSizeColumn(columnIndex);
                    }
                    catch { }
                }
                else
                {
                    Sheet.SetColumnWidth(columnIndex, option.ColumnWidth);
                }
                if (Sheet.GetColumnWidth(columnIndex) > option.MaxColumnWidh)
                {
                    Sheet.SetColumnWidth(columnIndex, option.MaxColumnWidh);
                }
            }


            //---- 保存
            FileStream? fs = null;
            try
            {
                this.UpdateProgress(95f, $"保存到文件");
                fs = new FileStream(fullFilePath, FileMode.OpenOrCreate);
                Workbook.Write(fs); // 向打开的这个Excel文件中写入表单并保存。  
            }
            catch
            {
                this.UpdateProgress(96f, $"保存是失败");
                return false;
            }
            finally
            {
                this.UpdateProgress(100f, $"保存完成");
                fs?.Close();
                Workbook?.Close();
                GC.Collect();
            }
            return true;
        }

        #region 表格填充
        /// <summary>
        /// 取得指定属性的对应值
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private object? GetPropertyValue(string propertyName, object data)
        {
            bool isExpandoObject = data is System.Dynamic.ExpandoObject;
            if (isExpandoObject)
            {
                var dic = (IDictionary<string, object>)data;
                return dic.TryGetValue(propertyName, out object? value) ? value : null;
            }
            else
            {
                Type type = data.GetType();
                // 属性信息
                PropertyInfo? propertyInfo = type.GetProperty(propertyName);
                if (propertyInfo != null)
                {
                    // 取得数据
                    return propertyInfo.GetValue(data, null);
                }
                else
                {
                    return null;
                }
            }
        }

        #endregion

        #region 私有方法

        private short LastHSSFColorIndex = 8;
        /// <summary>
        /// 取得颜色是输入值的单元格样式
        /// </summary>
        /// <param name="workbook"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        private ICellStyle GetCellStyleAsColor(StyleColor color)
        {
            ICellStyle output = Workbook!.CreateCellStyle();
            switch (Format)
            {
                case FormatEnum.HSSF:

                    HSSFPalette palette = ((HSSFWorkbook)Workbook).GetCustomPalette();
                    short GetColorIndex(StyleColor c)
                    {
                        HSSFColor hSSFColor = palette.FindColor(c.R, c.G, c.B);
                        if (hSSFColor == null)
                        {
                            palette.SetColorAtIndex(LastHSSFColorIndex, c.R, c.G, c.B);
                            if (LastHSSFColorIndex < 64)
                            {
                                LastHSSFColorIndex++;
                            }
                            return LastHSSFColorIndex;
                        }
                        else
                        {
                            return hSSFColor.Indexed;
                        }
                    }
                    output.FillForegroundColor = GetColorIndex(color);
                    output.FillPattern = FillPattern.SolidForeground;

                    break;
                case FormatEnum.SXSSF:
                // break;
                default:
                case FormatEnum.XSSF:
                    XSSFCellStyle xssfStyle = (XSSFCellStyle)output;
                    xssfStyle.SetFillForegroundColor(new XSSFColor(color.ToRgbBytes()));
                    xssfStyle.FillPattern = FillPattern.SolidForeground;
                    break;
            }
            return output;
        }
        private IFont GetFontAsSetting(FontSetting setting)
        {
            IFont font = Workbook!.CreateFont();
            font.FontHeightInPoints = setting.Height;
            font.IsBold = setting.IsBold;
            return font;
        }

        /// <summary>
        /// 设置区域的单元格格式
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="region"></param>
        /// <param name="style"></param>
        private void SetRegionCellStyle(CellRangeAddress region, ICellStyle style)
        {
            for (int r = region.FirstRow; r <= region.LastRow; r++)
            {
                IRow row = GetRow(r);
                for (int c = region.FirstColumn; c <= region.LastColumn; c++)
                {
                    ICell cell = GetCell(row, c);
                    cell.CellStyle = style;
                }
            }
        }
        /// <summary>
        /// 获取行, 如果不存在, 创建
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        private IRow GetRow(int rowIndex)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(rowIndex, 0, $"输入行索引 ({nameof(rowIndex)})");

            IRow? output = Sheet!.GetRow(rowIndex);
            output ??= Sheet.CreateRow(rowIndex);
            return output;
        }
        /// <summary>
        /// 获取单元格, 如果不存在, 创建
        /// </summary>
        /// <param name="row"></param>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        private ICell GetCell(IRow row, int columnIndex)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(columnIndex, 0, $"输入列索引 ({nameof(columnIndex)})");

            ICell? output = row.GetCell(columnIndex);
            output ??= row.CreateCell(columnIndex);
            return output;
        }

        /// <summary>
        /// 设置单元格的值
        /// </summary>
        /// <param name="workbook"></param>
        /// <param name="sheet"></param>
        /// <param name="cell"></param>
        /// <param name="value"></param>
        private void SetValue(ICell cell, object value)
        {
            if (value == null)
            {
                return;
            }
            Type type = value.GetType();
            if (type == typeof(DateTime))
            {
                cell.SetCellValue((DateTime)value);
                return;
            }
            else if (type == typeof(bool))
            {
                cell.SetCellValue((bool)value);
                return;
            }
            else if (type == typeof(int))
            {
                cell.SetCellValue((int)value);
                return;
            }
            else if (type == typeof(short))
            {
                cell.SetCellValue((short)value);
                return;
            }
            else if (type == typeof(long))
            {
                cell.SetCellValue((long)value);
                return;
            }
            else if (type == typeof(float))
            {
                cell.SetCellValue((float)value);
                return;
            }
            else if (type == typeof(double))
            {
                cell.SetCellValue((double)value);
                return;
            }
            else if (type == typeof(IRichTextString))
            {
                cell.SetCellValue((bool)value);
                return;
            }
            foreach (SetValueExtensionHandler handler in SetValueExtensions)
            {
                if (handler.Invoke(value, () => _getSetValueExtensionContext(cell)))
                {
                    return;
                }
            }
            cell.SetCellValue(value.ToString());
        }

        private SetValueExtensionContext _getSetValueExtensionContext(ICell cell)
        {
            return new(cell)
            {
                Workbook = Workbook,
                Sheet = Sheet,
                Drawing = Drawing,
            };
        }
        public class SetValueExtensionContext(ICell cell)
        {
            public ICell Cell { get; } = cell;
            public IWorkbook? Workbook { get; set; }
            public ISheet? Sheet { get; set; }
            public IDrawing? Drawing { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value">即将被写入的值</param>
        /// <param name="getContextHandler">取得写入时的上下文数据的方法. 需要使用时再调用</param>
        /// <returns>true: 标记已处理完成, 无需再做其他检查以及操作</returns>
        public delegate bool SetValueExtensionHandler(object value, Func<SetValueExtensionContext> getContextHandler);
        private static HashSet<SetValueExtensionHandler> SetValueExtensions = new();
        /// <summary>
        /// 添加设置单元格值的扩展检查与写入操作
        /// </summary>
        /// <param name="handler"></param>
        public static void AddSetValueExtension(SetValueExtensionHandler handler)
        {
            SetValueExtensions.Add(handler);
        }


        /// <summary>
        /// 整理成汇总的字符串
        /// </summary>
        /// <param name="option"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        private string GetSummaryString(ColumnOption option, List<object?> values)
        {
            if (string.IsNullOrEmpty(option.SummaryStringFormat))
            {
                return "";
            }
            bool needResult = false;
            string extraInfo = string.Empty; // 标记中的额外的的字符串信息
            string resultEzCheck = Common_Util.String.StringAnalysis.AnalysisToStringForPair(
                   option.SummaryStringFormat,
                   (source) =>
                   {
                       if (!string.IsNullOrEmpty(source))
                       {
                           string model = source[..ColumnOption.SUMMARY_FLAG_STRING.Length];
                           if (model.Equals(ColumnOption.SUMMARY_FLAG_STRING, StringComparison.InvariantCultureIgnoreCase))
                           {
                               needResult = true;
                               extraInfo = model[ColumnOption.SUMMARY_FLAG_STRING.Length..];
                           }
                       }
                       return string.Empty;
                   });
            if (!needResult)
            {// 不需要结果
                return resultEzCheck;
            }
            string? result = null;
            // 根据汇总方法取得字符串
            switch (option.SummaryMethod)
            {
                case SummaryMethodEnum.Count:
                    int count = 0;
                    foreach (object? obj in values)
                    {
                        if (obj != null)
                        {
                            count++;
                        }
                    }
                    result = count.ToString();
                    break;
                case SummaryMethodEnum.GroupCount:
                    Dictionary<Type, List<object>> map = new Dictionary<Type, List<object>>();
                    foreach (object? obj in values)
                    {
                        if (obj == null) continue;
                        Type type = obj.GetType();
                        if (map.ContainsKey(type))
                        {
                            List<object> list = map[type];
                            bool existSame = false;
                            foreach (object listItem in list)
                            {
                                if (listItem.Equals(obj))
                                {
                                    existSame = true;
                                    break;
                                }
                            }
                            if (!existSame)
                            {
                                list.Add(obj);
                            }
                        }
                        else
                        {
                            map.Add(type, new List<object>() { obj });
                        }
                    }
                    int groupCount = map.Sum(item => item.Value.Count);
                    result = groupCount.ToString();
                    break;
                case SummaryMethodEnum.Sum:
                    {
                        double sum = ToNumberList(option, values).Sum();
                        if (int.TryParse(extraInfo, out int i))
                        {
                            result = Math.Round(sum, i).ToString();
                        }
                        else
                        {
                            result = sum.ToString();
                        }
                    }
                    break;
                case SummaryMethodEnum.Min:
                    {
                        List<double> numbers = ToNumberList(option, values);
                        if (numbers.Count == 0)
                        {
                            result = 0.ToString();
                        }
                        else
                        {
                            result = ToNumberList(option, values).Min().ToString();
                        }
                    }
                    break;
                case SummaryMethodEnum.Max:
                    {
                        List<double> numbers = ToNumberList(option, values);
                        if (numbers.Count == 0)
                        {
                            result = 0.ToString();
                        }
                        else
                        {
                            result = ToNumberList(option, values).Max().ToString();
                        }
                    }
                    break;
                case SummaryMethodEnum.Average:
                    {// 平均数
                        List<double> numbers = ToNumberList(option, values);
                        double average;
                        if (numbers.Count == 0)
                        {
                            average = 0;
                        }
                        else
                        {
                            average = numbers.Average();
                        }
                        if (int.TryParse(extraInfo, out int i))
                        {
                            result = Math.Round(average, i).ToString();
                        }
                        else
                        {
                            result = average.ToString();
                        }
                    }
                    break;
                case SummaryMethodEnum.Median:
                    {// 中位数
                        double median = 0;
                        List<double> numbers = ToNumberList(option, values);
                        numbers.Sort();
                        if (numbers.Count == 0)
                        {
                            median = 0;
                        }
                        else if (numbers.Count % 2 == 0)
                        {
                            median = (numbers[numbers.Count / 2] + numbers[numbers.Count / 2 - 1]) / 2;
                        }
                        else if (numbers.Count % 1 == 0)
                        {
                            median = numbers[numbers.Count / 2];
                        }
                        if (int.TryParse(extraInfo, out int i))
                        {
                            result = Math.Round(median, i).ToString();
                        }
                        else
                        {
                            result = median.ToString();
                        }
                    }
                    break;
                case SummaryMethodEnum.Custom:
                    result = option.CustomSummaryMethod?.Invoke(values);
                    break;
                case SummaryMethodEnum.Empty:
                    break;
            }
            if (result == null) return string.Empty;
            // 格式化
            return Common_Util.String.StringAnalysis.AnalysisToStringForPair(
                   option.SummaryStringFormat,
                   (source) =>
                   {
                       if (!string.IsNullOrEmpty(source))
                       {
                           string model = source[..ColumnOption.SUMMARY_FLAG_STRING.Length];
                           if (model.Equals(ColumnOption.SUMMARY_FLAG_STRING, StringComparison.InvariantCultureIgnoreCase))
                           {
                               return result;
                           }
                       }
                       return string.Empty;
                   }); ;
        }

        /// <summary>
        /// 将输入的对象列表转化为数字列表
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private List<double> ToNumberList(ColumnOption option, List<object?> list)
        {
            List<double> output = new List<double>();
            if (option.IsIndexColumn)
            {
                double d = option.IndexStartValue;
                foreach (object? obj in list)
                {
                    output.Add(d);
                    d++;
                }
            }
            else
            {
                foreach (object? obj in list)
                {
                    if (obj != null && double.TryParse(obj.ToString(), out double d))
                    {
                        output.Add(d);
                    }
                }
            }
            return output;
        }



        #endregion

        #endregion
    }
}
