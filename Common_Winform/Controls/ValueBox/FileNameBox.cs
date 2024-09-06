using Common_Util.Extensions;
using Common_Util.String;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common_Winform.Controls.ValueBox
{
    public class FileNameBox : TextBox
    {
        public FileNameBox()
        {
        }


        #region 配置


        /// <summary>
        /// 当前选择的文件名
        /// </summary>
        [Browsable(true)]
        [Category("CVII_自定义_参数"), Description("未选择状态显示文本")]
        public string NotSelectString { get; set; } = "< 未选择文件 >";

        /// <summary>
        /// 是否检查文件是否存在
        /// </summary>
        [Browsable(true)]
        [Category("CVII_自定义_参数"), Description("检查文件是否存在")]
        public bool CheckFileExists { get; set; }

        /// <summary>
        /// 是否支持多选? 默认不支持.
        /// </summary>
        [Browsable(true)]
        [Category("CVII_自定义_参数"), Description("可否多选")]
        public bool Multiselect { get; set; } = false;

        /// <summary>
        /// 选择文件时的筛选设置
        /// </summary>
        [Browsable(true)]
        [Category("CVII_自定义_参数"), Description("筛选设置")]
        public string Filter { get; set; } = "所有文件 (*.*)|*.*";
        /// <summary>
        /// 默认选择的筛选文件类型索引
        /// </summary>
        [Browsable(true)]
        [Category("CVII_自定义_参数"), Description("默认筛选类型索引")]
        public int FilterIndex { get; set; } = 0;
        /// <summary>
        /// 选择文件时的初始文件目录
        /// </summary>
        [Browsable(true)]
        [Category("CVII_自定义_参数"), Description("选择时的初始目录")]
        public string? InitialDirectory { get; set; }

        /// <summary>
        /// 弹出的文件选择窗口的标题
        /// </summary>
        [Browsable(true)]
        [Category("CVII_自定义_参数"), Description("选择窗口标题")]
        public string Title { get; set; } = "选择文件";

        #endregion

        #region 非设计器参数
        public delegate bool CheckValidHandler(string fileName);
        /// <summary>
        /// 检查文件名有效性的方法
        /// </summary>
        [Browsable(false)]
        public CheckValidHandler? CheckValid { get; set; }
        #endregion

        #region 当前选择文件
        /// <summary>
        /// 当前选择的文件名
        /// </summary>
        [Browsable(true)]
        [Category("CVII_自定义_状态"), Description("当前选择文件")]
        public string? FileName
        {
            get
            {
                return (_fileNames != null && _fileNames.Length > 0) ? _fileNames[0] : null;
            }
            set
            {
                _fileNames = new string?[] { value };
                _updateShowingText();
            }
        }

        /// <summary>
        /// 当前选择的文件名
        /// </summary>
        [Browsable(true)]
        [Category("CVII_自定义_状态"), Description("当前选择文件数组")]
        public string?[] FileNames
        {
            get
            {
                return _fileNames ?? Array.Empty<string>();
            }
            set
            {
                _fileNames = value;
                _updateShowingText();
            }
        }
        private string?[]? _fileNames;

        #endregion

        #region 基类属性隐藏

        [Browsable(true)]
        [Category("CVII_覆盖_参数"), Description("当前文本")]
        public override string Text
        {
            get => base.Text;
            set => _updateShowingText();
        }
        #endregion

        #region 显示
        private bool _onUpdateText = false;
        private void _updateShowingText()
        {
            _onUpdateText = true;

            if (_fileNames.IsEmpty())
            {
                base.Text = NotSelectString;
            }
            else
            {
                if (_fileNames!.Length == 1)
                {
                    base.Text = _fileNames![0] ?? string.Empty;
                }
                else
                {
                    base.Text = StringHelper.Concat(_fileNames!.Select(i => i ?? string.Empty).ToList(), "; ");
                }
            }

            _onUpdateText = false;
        }
        #endregion

        protected override void OnClick(EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Multiselect = Multiselect,
                Filter = Filter,
                FilterIndex = FilterIndex,
                CheckFileExists = CheckFileExists,
                Title = Title,
            };
            if (InitialDirectory.IsNotEmpty())
            {
                dialog.InitialDirectory = InitialDirectory;
                dialog.RestoreDirectory = false;
            }
            else
            {
                dialog.RestoreDirectory = true;
            }

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (Multiselect)
                {
                    if (CheckValid != null)
                    {
                        List<string> valid = new List<string>();
                        foreach (string file in dialog.FileNames)
                        {
                            bool b = CheckValid(dialog.FileName);
                            if (b)
                            {
                                valid.Add(file);
                            }
                        }
                        FileNames = valid.ToArray();
                    }
                    else
                    {
                        FileNames = dialog.FileNames;
                    }
                }
                else
                {
                    if (CheckValid != null)
                    {
                        bool b = CheckValid(dialog.FileName);
                        if (b)
                        {
                            FileName = dialog.FileName;
                        }
                        else
                        {
                            FileName = null;
                        }
                    }
                    else
                    {
                        FileName = dialog.FileName;
                    }
                }
            }

        }
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
        }
        protected override void OnTextChanged(EventArgs e)
        {
            if (!_onUpdateText)
            {
                _updateShowingText();
            }
        }
    }
}
