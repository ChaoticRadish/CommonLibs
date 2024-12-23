using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Common_Winform.Preview.Pdf
{
    public partial class AdobePdfPreviewer : UserControl, IPdfPreviewer
    {
        public AdobePdfPreviewer()
        {
            InitializeComponent();
        }

        public string? FileName { get; private set; }

        public void Show(string fileName)
        {
            axAcropdf.src = fileName;
        }

        public void Clear()
        {
        }

        public bool SupplyMoveToTop => false;

        public bool SupplyMoveToBottom => false;

        public bool SupplyMoveToPage => false;

        public bool SupplyGetPageCode => false;

        public bool SupplyGetTotalPage => false;

        public int GetPageCode()
        {
            return 0;
        }

        public int GetTotalPage()
        {
            return 0;
        }

        public void MoveToBottom()
        {
        }

        public void MoveToPage(int page)
        {
        }

        public void MoveToTop()
        {
        }

    }
}
