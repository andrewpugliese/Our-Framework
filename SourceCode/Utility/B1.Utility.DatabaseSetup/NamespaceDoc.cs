using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace B1.Utility.DatabaseSetup
{
    /// <summary>
    /// This namespace is contains classes for setting up a new database or altering an existing one:
    /// <para>
    /// <list type="bullet">
    /// <item>
    /// Database Setup Utility User Interface
    /// <see cref="B1.Utility.DatabaseSetup.DbSetupMgr"/>
    /// <seealso cref="B1.Utility.DatabaseSetup.DbSetupMgr"/>
    /// </item>
    /// <item>
    /// Setup Utility Worker Class to parse .cmd files and process .sql files
    /// <see cref="B1.Utility.DatabaseSetup.DbSetupWkr"/>
    /// <seealso cref="B1.Utility.DatabaseSetup.DbSetupWkr"/>
    /// </item>
    /// </list>
    /// </para>
    /// In addition, it also contains classes for testing and demonstrating the functionality of the framework classes:
    /// <list type="bullet">
    /// <item>
    /// Implements methods for testing Insert, Update, Delete and Merge Compound SQL DbCommands
    /// in a multi-threaded fashion.
    /// <see cref="B1.Utility.DatabaseSetup.TestDataAccessMgr"/>
    /// </item>
    /// <item>
    /// as well as references to other classes in other namespaces
    /// <list type="bullet">
    /// <item>
    /// The Database Access Layer for back-end independant functionality
    /// <seealso cref="B1.DataAccess.DataAccessMgr"/>
    /// </item>
    /// <item>
    /// The Layer for thread safe, efficient caching of data, objects
    /// <seealso cref="B1.CacheManagement.CacheMgr"/>
    /// </item>
    /// <item>
    /// A common class for Exception objects
    /// <seealso cref="B1.ILoggingManagement.ExceptionEvent"/>
    /// </item>
    /// <item>
    /// A common set of classes for logging and tracing functionality
    /// <seealso cref="B1.ILoggingManagement"/>
    /// </item>
    /// <item>
    /// A common set of classes for security and cryptography functionality
    /// <seealso cref="B1.Cryptography"/>
    /// </item>
    /// </list>
    /// </item>
    /// </list>
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
        // This class is only needed for SandCastle and its ability to create HTML documenatation for
        // a given namespace.
    }
}
