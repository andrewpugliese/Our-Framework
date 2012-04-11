using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace B1.Wpf.Controls
{
    /// <summary>
    /// Interface definition for the Numeric Plus or Minus control
    /// </summary>
    interface INumericPlusMinus
    {
        /// <summary>
        /// Returns or sets the given label of the value
        /// </summary>
        string Label { get; set; }
        /// <summary>
        /// Returns or sets the maximum allowed value
        /// </summary>
        int Max { get; set; }
        /// <summary>
        /// Returns or sets the minimum allowed value
        /// </summary>
        int Min { get; set; }
        /// <summary>
        /// Returns or sets the delta value when doing a plus (addition)
        /// </summary>
        int Plus { get; set; }
        /// <summary>
        /// Returns or sets the delta value when doing a minus (subtraction)
        /// </summary>
        int Minus { get; set; }
        /// <summary>
        /// Returns or sets the current value (which can be null)
        /// </summary>
        int? Value { get; set; } 
    }
}
