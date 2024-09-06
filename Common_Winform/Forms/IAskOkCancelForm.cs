using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Winform.Forms
{
    public interface IAskOkCancelForm
    {
        string Title { get; set; }

        string ShowingText { get; set; }

        FormStartPosition StartPosition { get; set; }
        

        DialogResult ShowDialog();

        DialogResult ShowDialog(IWin32Window parent);


    }
}
