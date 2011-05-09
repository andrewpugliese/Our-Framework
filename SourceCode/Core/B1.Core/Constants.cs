using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace B1.Core
{
#pragma warning disable 1591 // disable the xmlComments warning because the following constants are mostly used as string literals
    /// <summary>
    /// Enumerates different options for a datetime value.
    /// Default 
    /// Local
    /// UTC: Universal Time.
    /// </summary>
    public enum EnumDateTimeLocale { Default, Local, UTC };
    /// <summary>
    /// Enumerates the different options for calculating a datetime difference
    /// </summary>
    public enum EnumDateDiffInterval { Day, Hour, Minute, Second, MilliSecond };

#pragma warning restore 1591 // disable the xmlComments warning

    /// <summary>
    /// Global constants to be used by the libraries and application.
    /// It is recommended to use constants for literals as this supports
    /// compiler supplied functionality of refactoring and finding references.
    /// </summary>
    public class Constants    
    {
        /// <summary>
        /// Framework's Major Version
        /// </summary>
        public const int B1FrameworkMajorVersion = 1;
        /// <summary>
        /// Framework's Minor Version
        /// </summary>
        public const int B1FrameworkrMinorVersion = 13;
        /// <summary>
        /// Frameworks Major . Minor Version
        /// </summary>
        public static readonly string B1FrameworkVersion = B1FrameworkMajorVersion.ToString() + "." + B1FrameworkrMinorVersion.ToString();
   }
}
