using Common_Winform.Attributes;
using Common_Winform.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CommonLibTest_Winform
{
    public partial class MainForm : MainFormTemplate01  // 暂时先用这个来测试, 因为目前没别的了
    {
        public MainForm()
        {
            InitializeComponent();
        }

        [PageConfig(Index = 1, ShowName = "PDF 预览测试")]
        TestPages.Preview.Pdf001 Preview_Pdf001 { get; set; } = new TestPages.Preview.Pdf001();
    }
}
