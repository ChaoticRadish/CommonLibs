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
    public partial class AskOkCancelForm01 : Form, IAskOkCancelForm
    {
        public AskOkCancelForm01()
        {
            InitializeComponent();
        }

        public string Title { get => Text; set => Text = value; }

        public string ShowingText { get => Shower_主要信息.Text; set => Shower_主要信息.Text = value; }
    }
}
