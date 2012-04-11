using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace B1.Wpf.Controls
{
    /// <summary>
    /// Interface definition for paging controls
    /// </summary>
    interface IPagingControl
    {
        /// <summary>
        /// Retrieves the first page of data
        /// </summary>
        /// <returns>Bool indicator of whether there was data that was processed</returns>
        bool First();

        /// <summary>
        /// Retrieves the last page of data
        /// </summary>
        /// <returns>Bool indicator of whether there was data that was processed</returns>
        bool Last();

        /// <summary>
        /// Retrieves the previous page of data
        /// </summary>
        /// <returns>Bool indicator of whether there was data that was processed</returns>
        bool Previous();

        /// <summary>
        /// Retrieves the nextt page of data
        /// </summary>
        /// <returns>Bool indicator of whether there was data that was processed</returns>
        bool Next();

        /// <summary>
        /// Retrieves the current page of data
        /// </summary>
        /// <returns>Bool indicator of whether there was data that was processed</returns>
        bool Refresh();
    }
}
