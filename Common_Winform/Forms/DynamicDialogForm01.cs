using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Common_Winform.Forms
{
    public partial class DynamicDialogForm01 : Form
    {
        public DynamicDialogForm01()
        {
            InitializeComponent();

            更新按钮列表();

            ClearBody();
            SetDefault();
        }

        #region 按钮列表


        private Dictionary<DialogResult, Button> 已创建按钮列表 = [];
        private void 更新按钮列表()
        {
            按钮流式布局容器.Controls.Clear();
            已创建按钮列表.Invoke(i => i.Value.Visible = false).Finish();

            var list = NeedResults.OrderByDescending(i => i);
            foreach (var item in list)
            {
                if (已创建按钮列表.TryGetOrAdd(item, () => 创建按钮(item), out Button? button))
                {
                    button.Visible = true;
                    按钮流式布局容器.Controls.Add(button);
                }
                else
                {
                    throw new InvalidOperationException($"未能创建或从字典中获取已有按钮, 对应值: {item}");
                }
            }
        }
        private Button 创建按钮(DialogResult result)
        {
            Button button = new Button()
            {
                Size = new Size(100, 30),
                DialogResult = result,
                Text = 按钮文本映射.GetValueOrDefault(result, result.ToString()),
            };
            return button;
        }
        private static Dictionary<DialogResult, string> 按钮文本映射 = new()
        {
            { DialogResult.None, "无" },
            { DialogResult.OK, "确认" },
            { DialogResult.Cancel, "取消" },
            { DialogResult.Abort, "中止" },
            { DialogResult.Retry, "重试" },
            { DialogResult.Ignore, "忽略" },
            { DialogResult.Yes, "是" },
            { DialogResult.No, "否" },
            { DialogResult.TryAgain, "再试一次" },
            { DialogResult.Continue, "继续" },
        };

        public DialogResult[] NeedResults
        {
            get => dialogResults;
            set
            {
                dialogResults = value ?? [];
                更新按钮列表();
            }
        }
        private DialogResult[] dialogResults = [ DialogResult.OK ];


        #endregion

        #region 主体区域

        /// <summary>
        /// 窗口标题
        /// </summary>
        public string Title
        {
            get => base.Text;
            set => base.Text = value;
        }

        /// <summary>
        /// 默认显示内容文本
        /// </summary>
        public string DefaultText
        {
            get => 默认显示内容.Text;
            set
            {
                默认显示内容.Text = value;
            }
        }

        /// <summary>
        /// 设置控件为窗口的主体
        /// </summary>
        public Control? Body
        {
            get => body;
            set
            {
                body = value;
                if (body == null)
                {
                    ClearBody();
                    SetDefault();
                }
                else
                {
                    ClearBody();
                    SetBody(body);
                }
            }
        }
        private Control? body;
        private void ClearBody()
        {
            显示容器.Controls.Clear();
        }
        private void SetDefault()
        {
            显示容器.Controls.Add(默认显示内容);
        }
        private void SetBody(Control body)
        {
            显示容器.Controls.Add(body);
            // body.Dock = DockStyle.None;
            // body.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            body.Visible = false;
            body.Show();
        }

        #endregion

        #region 静态方法, 常用预设
        /// <summary>
        /// 创建一个多行文本的消息提示框, 只有一个确认按钮
        /// </summary>
        /// <returns></returns>
        public static (DynamicDialogForm01 form, Action<string> setTextAction) MessageBox()
        {
            RichTextBox textBox = new RichTextBox()
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BorderStyle = BorderStyle.FixedSingle,
            };
            DynamicDialogForm01 form = new DynamicDialogForm01()
            {
                NeedResults = [DialogResult.OK],
                Body = textBox,
                StartPosition = FormStartPosition.CenterParent,
            };
            form.Shown += (o, e) =>
            {
                form.BringToFront();
            };

            return (form, (str) => textBox.Text = str);
        }
        #endregion

    }

}
