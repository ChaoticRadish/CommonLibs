using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Winform.Pages.Layout
{
    public interface ITabPageLayout<ItemType> : IMultiPageLayout<ItemType>
        where ItemType : MultiPageLayoutItem, new()

    {
        void Init();

    }
}
