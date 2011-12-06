using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace B1.Wpf.Controls
{
    interface INumericPlusMinus
    {
        string Label { get; set; }
        int Max { get; set; }
        int Min { get; set; }
        int Plus { get; set; }
        int Minus { get; set; }
        int? Value { get; set; } 
    }
}
