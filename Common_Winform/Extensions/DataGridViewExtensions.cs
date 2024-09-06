using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common_Winform.Extensions
{
    public static class DataGridViewExtension
    {
        /// <summary>
        /// 快速设置右键点击单元格以打开菜单的方法到dgv
        /// </summary>
        /// <typeparam name="TRowDate"></typeparam>
        /// <param name="dgv"></param>
        /// <param name="menu">指定的菜单</param>
        /// <param name="onlySelectOneRow">是否仅选中一行</param>
        /// <param name="doSomeAfterShowMenu">在显示菜单之前做点什么</param>
        /// <param name="applyToNoCellArea">右键菜单应用于非单元格区域</param>
        /// <param name="doSthAfterShowMenuOnNoCell">右键非单元格区域时做点什么</param>
        /// <param name="setRowItemToTag">将行Item设置到右键菜单的Tag上</param>
        public static void SetRightButtonMenu<TRowDate>(
            this DataGridView dgv, ContextMenuStrip menu,
            Action<TRowDate>? doSomeAfterShowMenu = null, bool onlySelectOneRow = true,
            bool applyToNoCellArea = false, Action? doSthAfterShowMenuOnNoCell = null,
            bool setRowItemToTag = false)
        {
            if (applyToNoCellArea)
            {
                dgv.MouseDown += (sender, e) =>
                {
                    if (e.Button == MouseButtons.Right)
                    {
                        var c = dgv.GetChildAtPoint(e.Location);
                        if (c == null)
                        {// 非单元格区域
                            doSthAfterShowMenuOnNoCell?.Invoke();
                            Form form = dgv.FindForm();
                            menu.Show(form, form.PointToClient(Control.MousePosition));
                        }
                    }
                };
            }
            dgv.CellMouseDown += (sender, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    if (e.RowIndex >= 0 && e.RowIndex < dgv.RowCount
                        && e.ColumnIndex >= 0 && e.ColumnIndex < dgv.ColumnCount)
                    {// 单元格区域
                        if (dgv.Rows[e.RowIndex].DataBoundItem is TRowDate data)
                        {
                            doSomeAfterShowMenu?.Invoke(data);

                            // 仅选中一行
                            if (onlySelectOneRow)
                            {
                                dgv.ClearSelection();
                            }
                            else
                            {
                                if (dgv.SelectedRows.Count == 1)
                                {// 非仅选中一行的情况下, 如果已选择的只有另外的一行, 就也给他清空
                                    dgv.ClearSelection();
                                }
                            }

                            dgv.Rows[e.RowIndex].Selected = true;
                            if (setRowItemToTag)
                            {
                                // 只有右键点击单元格, 切单元格类型与输入泛型参数相同, 才将其设置到菜单的Tag上
                                menu.Tag = data;
                            }
                            Form form = dgv.FindForm();
                            menu.Show(form, form.PointToClient(Control.MousePosition));
                            return;
                        }
                    }
                    else if (applyToNoCellArea)
                    {// 非单元格区域
                        doSthAfterShowMenuOnNoCell?.Invoke();
                        Form form = dgv.FindForm();
                        menu.Show(form, form.PointToClient(Control.MousePosition));
                    }
                    if (setRowItemToTag)
                    {
                        // 其他情况需要将菜单的Tag设为null
                        menu.Tag = null;
                    }
                }
            };
        }


        /// <summary>
        /// 取得第一个被选中的行数据
        /// </summary>
        /// <typeparam name="TRowData">试图将行转换为这个类型的对象</typeparam>
        /// <param name="dgv"></param>
        /// <param name="defaultValue">没有选中的数据行或者转换失败，将返回默认值</param>
        /// <returns></returns>
        public static TRowData? GetFirstSelectedRowDataAs<TRowData>(
            this DataGridView dgv, TRowData? defaultValue = default)
        {
            if (dgv.SelectedRows.Count > 0
                && dgv.SelectedRows[0].DataBoundItem is TRowData data)
            {
                return data;
            }
            else
            {
                return defaultValue;
            }
        }
        /// <summary>
        /// 取得所有被选中的行数据
        /// </summary>
        /// <typeparam name="TRowData"></typeparam>
        /// <param name="dgv"></param>
        /// <returns></returns>
        public static List<TRowData> GetSelectedRowDatasAs<TRowData>(this DataGridView dgv)
        {
            List<TRowData> output = new List<TRowData>();
            foreach (DataGridViewRow row in dgv.SelectedRows)
            {
                if (row.DataBoundItem is TRowData item)
                {
                    output.Add(item);
                }
            }
            return output;
        }

        /// <summary>
        /// 设置是否启用双缓冲，DataGridView的DoubleBuffered属性，默认是对外隐藏的
        /// </summary>
        /// <param name="dgv"></param>
        /// <param name="b"></param>
        public static void SetDoubleBuffered(
            this DataGridView dgv, bool b = true)
        {
            // 开启双缓冲，减少卡顿感
            Type dgvType = dgv.GetType();
            PropertyInfo? pi = dgvType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            pi?.SetValue(dgv, b, null);
        }

        /// <summary>
        /// 检查输入的行列索引对应的单元格, 是否数据单元格(大于0, 且小于RowCount及ColumnCount)
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        public static bool IsDataCell(
            this DataGridView dgv,
            int rowIndex, int columnIndex = 0)
        {
            return rowIndex >= 0 && rowIndex < dgv.RowCount
                && columnIndex >= 0 && columnIndex < dgv.ColumnCount;
        }


        public static DataGridViewColumn AddImageColumn(
            this DataGridView dgv, string columnName, string dataPropertyName, float fillWeight = 100, DataGridViewImageCellLayout imageLayout = DataGridViewImageCellLayout.Zoom, bool readOnly = true)
        {
            DataGridViewImageColumn dataGridViewColumn = new DataGridViewImageColumn();
            dataGridViewColumn.HeaderText = columnName;
            dataGridViewColumn.DataPropertyName = dataPropertyName;
            dataGridViewColumn.Name = columnName;
            dataGridViewColumn.ReadOnly = readOnly;
            dataGridViewColumn.FillWeight = fillWeight;
            dataGridViewColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridViewColumn.ImageLayout = imageLayout;
            dataGridViewColumn.DefaultCellStyle.NullValue = null;
            dgv.Columns.Add(dataGridViewColumn);
            return dataGridViewColumn;
        }
    }
}
