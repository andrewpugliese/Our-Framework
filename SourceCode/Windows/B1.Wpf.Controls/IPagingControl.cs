using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace B1.Wpf.Controls
{
    interface IPagingControl
    {
        bool First();

        bool Last();

        bool Previous();

        bool Next();

        bool Refresh();
    }
}
