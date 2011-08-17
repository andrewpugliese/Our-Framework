﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.225
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace B1.ILoggingManagement {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class EventCodes {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal EventCodes() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("B1.ILoggingManagement.EventCodes", typeof(EventCodes).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The applications unique numeric code was not found in the AppMaster table for the given AppId.
        /// </summary>
        internal static string AppCodeNotFound {
            get {
                return ResourceManager.GetString("AppCodeNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A dataset was returned from the database and a collection of table names was provided but the count of the returned tables does not match what was supplied to the function..
        /// </summary>
        internal static string DataSetTableNamesMismatchWithResultSet {
            get {
                return ResourceManager.GetString("DataSetTableNamesMismatchWithResultSet", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The core catalog data structures were not available in the database so the catalog operations cannot be performed.  Please ask DBA to check existences of CatalogTables, CatalogColumns, etc..
        /// </summary>
        internal static string DbCatalogMgrNotAvailable {
            get {
                return ResourceManager.GetString("DbCatalogMgrNotAvailable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The given parameter could not be found in the given dbCommandText and it needs to be replaced.  This could indicate an invalid sql statement or the parameter deliminiters string does not include a required token..
        /// </summary>
        internal static string DbCommandBlockParameterReplacementFailed {
            get {
                return ResourceManager.GetString("DbCommandBlockParameterReplacementFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The given dbParameter already belongs to the dbParameter collection..
        /// </summary>
        internal static string DbParameterExistsInCollection {
            get {
                return ResourceManager.GetString("DbParameterExistsInCollection", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The database table was not defined with an index that had the required columns..
        /// </summary>
        internal static string DbTableIndexNotFound {
            get {
                return ResourceManager.GetString("DbTableIndexNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The database table was not defined with a primary key and the operation required a table with a primary key or some other alternative index which was not supplied..
        /// </summary>
        internal static string DbTablePrimaryKeyUndefined {
            get {
                return ResourceManager.GetString("DbTablePrimaryKeyUndefined", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The primary target of the EventLog could not be written to and the failover file could also not be written to for the given critical prioirty event..
        /// </summary>
        internal static string EventLogFailoverFileWriteFailed {
            get {
                return ResourceManager.GetString("EventLogFailoverFileWriteFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The given EventLog source does not exist and the program did not have privileges to create it.  Please create it manually..
        /// </summary>
        internal static string EventLogSourceCreateFailed {
            get {
                return ResourceManager.GetString("EventLogSourceCreateFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The given found could not be found; check spelling of name or path..
        /// </summary>
        internal static string FileNotFound {
            get {
                return ResourceManager.GetString("FileNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The function was not implemented for the current database type..
        /// </summary>
        internal static string FunctionNotImplementedForDbType {
            get {
                return ResourceManager.GetString("FunctionNotImplementedForDbType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Once an Oracle DbCommandBlock has been formatted (by accessing the DbCommandBlock property, it cannot have any new commands added to it..
        /// </summary>
        internal static string InvalidChangeToOracleDbCommandBlock {
            get {
                return ResourceManager.GetString("InvalidChangeToOracleDbCommandBlock", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The format string used to generate a dynamic sql command did not have the proper parameters.
        /// </summary>
        internal static string InvalidFormatStringDynamicSQL {
            get {
                return ResourceManager.GetString("InvalidFormatStringDynamicSQL", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The value of the parameter is not valid.
        /// </summary>
        internal static string InvalidParameterValue {
            get {
                return ResourceManager.GetString("InvalidParameterValue", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The was not configuration value defined for the given key which required a value.
        /// </summary>
        internal static string MissingRequiredConfigurationValue {
            get {
                return ResourceManager.GetString("MissingRequiredConfigurationValue", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A parameter was passed with a null or empty value..
        /// </summary>
        internal static string NullOrEmptyParameter {
            get {
                return ResourceManager.GetString("NullOrEmptyParameter", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The given TaskQueueCode was not by the taskCompletion delegate.  This is an internal error with the TaskProcessingEngine which indicates that a task that just completed was not recorded as being started..
        /// </summary>
        internal static string TaskQueueCodeNotFoundAtCompletion {
            get {
                return ResourceManager.GetString("TaskQueueCodeNotFoundAtCompletion", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The tasksInProcess counter cannot be less than 0.  This is an internal error in the TaskProcessingEngine..
        /// </summary>
        internal static string TasksInProcessCounterUnderFlow {
            get {
                return ResourceManager.GetString("TasksInProcessCounterUnderFlow", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unexpected error event occurred..
        /// </summary>
        internal static string UnknownException {
            get {
                return ResourceManager.GetString("UnknownException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The database type given is not yet supported..
        /// </summary>
        internal static string UnsupportedDbType {
            get {
                return ResourceManager.GetString("UnsupportedDbType", resourceCulture);
            }
        }
    }
}
