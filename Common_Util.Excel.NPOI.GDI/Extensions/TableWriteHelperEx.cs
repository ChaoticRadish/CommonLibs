using Common_Util.Excel.NPOI.Helper;
using Common_Util.GDI.Images;
using NPOI.SS.UserModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Excel.NPOI.GDI.Extensions
{
    public static class TableWriteHelperEx
    {
        public static void UseGDI(this TableWriteHelper _)
        {
            TableWriteHelper.AddSetValueExtension(_setValueExImpl);
        }
        #region 设置值扩展方法的实现: GDI图片
        private static bool _setValueExImpl(object value, Func<TableWriteHelper.SetValueExtensionContext> getContextFunc)
        {
            Type type = value.GetType();
            if (typeof(System.Drawing.Image).IsAssignableFrom(type))
            {
                new _setValueExImplBody(getContextFunc()).SetImageValue([(System.Drawing.Image)value]);
                return true;
            }
            else if (typeof(ImageFileInfo).IsAssignableFrom(type))
            {
                new _setValueExImplBody(getContextFunc()).SetImageValue([(ImageFileInfo)value]);
                return true;
            }
            else
            {// 其他情况, 需要判断是否列表
                if (typeof(IList).IsAssignableFrom(type))
                {
                    if (type.IsArray)
                    {
                        if (typeof(System.Drawing.Image).IsAssignableFrom(type.GetElementType()))
                        {
                            List<System.Drawing.Image> imageList = new List<System.Drawing.Image>();
                            foreach (object obj in (Array)value)
                            {
                                imageList.Add((System.Drawing.Image)obj);
                            }
                            new _setValueExImplBody(getContextFunc()).SetImageValue(imageList);
                            return true;
                        }
                        else if (typeof(ImageFileInfo).IsAssignableFrom(type.GetElementType()))
                        {
                            List<ImageFileInfo> imageList = new List<ImageFileInfo>();
                            foreach (object obj in (Array)value)
                            {
                                imageList.Add((ImageFileInfo)obj);
                            }
                            new _setValueExImplBody(getContextFunc()).SetImageValue(imageList);
                            return true;
                        }
                    }
                    else
                    {
                        Type[] genericArguments = type.GetGenericArguments();
                        if (genericArguments.Length > 0 &&
                            typeof(System.Drawing.Image).IsAssignableFrom(genericArguments[0]))
                        {
                            List<System.Drawing.Image> imageList = new List<System.Drawing.Image>();
                            foreach (object obj in (IList)value)
                            {
                                imageList.Add((System.Drawing.Image)obj);
                            }
                            new _setValueExImplBody(getContextFunc()).SetImageValue(imageList);
                            return true;
                        }
                        else if (genericArguments.Length > 0 &&
                            typeof(ImageFileInfo).IsAssignableFrom(genericArguments[0]))
                        {
                            List<ImageFileInfo> imageList = new List<ImageFileInfo>();
                            foreach (object obj in (IList)value)
                            {
                                imageList.Add((ImageFileInfo)obj);
                            }
                            new _setValueExImplBody(getContextFunc()).SetImageValue(imageList);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private struct _setValueExImplBody(TableWriteHelper.SetValueExtensionContext context)
        {
            public ICell Cell = context.Cell ?? throw new NullReferenceException($"上下文的 {nameof(context.Cell)} 不能为 null");

            public IWorkbook Workbook = context.Workbook ?? throw new NullReferenceException($"上下文的 {nameof(context.Workbook)} 不能为 null");

            public ISheet Sheet = context.Sheet ?? throw new NullReferenceException($"上下文的 {nameof(context.Sheet)} 不能为 null");

            public IDrawing Drawing = context.Drawing ?? throw new NullReferenceException($"上下文的 {nameof(context.Drawing)} 不能为 null");


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
            /// 设置单元格的值为图片
            /// </summary>
            /// <param name="cell"></param>
            /// <param name="imageFiles"></param>
            public void SetImageValue(List<ImageFileInfo> imageFiles)
            {
                if (imageFiles == null)
                {
                    return;
                }
                System.Drawing.Image? image = null;
                SetImageValue(imageFiles.Count,
                    (index) =>
                    {
                        ImageFileInfo file = imageFiles[index];
                        if (file != null && file.Exist)
                        {
                            // 获取图片
                            image = BitmapHelper.GetBitmap(file.Path);
                            if (image != null)
                            {
                                if (file.ImageSizeLimit > 0)
                                {// 如果需要限制图片的尺寸
                                    image = BitmapHelper.ScaleShorterSmallThen(image, file.ImageSizeLimit);
                                }
                            }
                            return image;
                        }
                        else
                        {
                            return null;
                        }
                    },
                    (index) =>
                    {
                        if (image != null)
                        {
                            image.Dispose();
                            image = null;
                        }
                    });
            }
            /// <summary>
            /// 设置单元格的值为图片
            /// </summary>
            /// <param name="draing"></param>
            /// <param name="cell"></param>
            /// <param name="images"></param>
            public void SetImageValue(List<System.Drawing.Image> images)
            {
                if (images == null)
                {
                    return;
                }
                SetImageValue(images.Count,
                    (index) =>
                    {
                        return images[index];
                    }, null);
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="cell"></param>
            /// <param name="count"></param>
            /// <param name="beforeSet">本方法返回使用的位图, 传入当前的Index</param>
            /// <param name="afterSet">完成图片设置后执行的内容, 传入当前的Index</param>
            public void SetImageValue(
                int count,
                Func<int, System.Drawing.Image?> beforeSet, Action<int>? afterSet)
            {
                if (count <= 0 || beforeSet == null)
                {
                    return;
                }

                // 基本信息
                double cellWidth;
                double cellHeight;
                double gapWidth;
                double totalWidth = 0;
                double hScale;
                double vScale;
                cellWidth = Sheet!.GetColumnWidth(Cell.ColumnIndex) / 35;
                cellHeight = GetRow(Cell.RowIndex).Height / 15;
                gapWidth = cellHeight / 15;
                hScale = 1024 / cellWidth;
                vScale = 256 / cellHeight;
                hScale *= (Sheet.GetColumnWidth(Cell.ColumnIndex) / 256 / 5 * 6 - 20 / 2) * 100;
                vScale *= cellHeight * 100;
                // 循环设置
                for (int i = 0; i < count; i++)
                {
                    // 设置前的取得数据
                    System.Drawing.Image? image = beforeSet.Invoke(i);
                    if (image == null)
                    {
                        continue;
                    }
                    totalWidth += gapWidth;
                    // 在单元格中的尺寸
                    System.Drawing.Size size = Common_Util.Maths.SizeCalcUtil.ZoomToHeight(image.Size, (int)(cellHeight - gapWidth * 2));
                    double xScale = (double)size.Width / (cellWidth - totalWidth);
                    double yScale = (double)size.Height / cellHeight;
                    double left = totalWidth;
                    double right = cellWidth - totalWidth - size.Width;
                    double top = gapWidth / 2;
                    double bottom = gapWidth / 2;
                    if (right < 0)
                    {
                        right = 0;
                        left = cellWidth - size.Width;
                        xScale = 1;
                    }

                    SetImageValue(image,
                        left, top,
                        xScale, yScale, hScale, vScale);

                    totalWidth += size.Width;

                    // 设置完成执行函数
                    afterSet?.Invoke(i);
                }

            }

            void SetImageValue(System.Drawing.Image image,
                double left, double top,
                double xS, double yS, double hS, double vS)
            {
                int imageId = Workbook.AddPicture(BitmapHelper.ImageToByteArray(image), PictureType.PNG);
                IClientAnchor anchor = Workbook.GetCreationHelper().CreateClientAnchor();
                anchor.AnchorType = AnchorType.MoveDontResize;
                anchor.Col1 = Cell.ColumnIndex;
                anchor.Col2 = Cell.ColumnIndex + 1;
                anchor.Row1 = Cell.RowIndex;
                anchor.Row2 = Cell.RowIndex + 1;
                anchor.Dx1 = (int)(left * hS);
                anchor.Dy1 = (int)(top * vS);

                IPicture picture = Drawing!.CreatePicture(anchor, imageId);
                picture.Resize(xS, 1);
            }

        }

        #endregion

    }
}
