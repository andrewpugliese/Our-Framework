using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace B1.ILoggingManagement
{
    /// <summary>
    /// All enumerations must have an entry in the EventCodes resource table.
    /// Negative enumerations correspond to programming errors
    /// Positive enumerations correspond to data conditions
    /// </summary>
    public enum enumExceptionEventCodes
    {
        #pragma warning disable 1591 // disable the xmlComments warning because the following constants are mostly used as string literals
        UnknownException = -1
        , FunctionNotImplementedForDbType = -2
        , InvalidFormatStringDynamicSQL = -3
        , InvalidChangeToOracleDbCommandBlock = -4
        , DbCommandBlockParameterReplacementFailed = -5
        , DbParameterExistsInCollection = -6
        , DataSetTableNamesMismatchWithResultSet = -7
        , TaskQueueCodeNotFoundAtCompletion = -8
        , TasksInProcessCounterUnderFlow = -9

        , NullOrEmptyParameter = 1
        , EventLogSourceCreateFailed = 2
        , EventLogFailoverFileWriteFailed = 3
        , UnsupportedDbType = 4
        , InvalidParameterValue = 5
        , FileNotFound = 6
        , MissingRequiredConfigurationValue = 7
        , DbCatalogMgrNotAvailable = 8
        , DbTablePrimaryKeyUndefined = 9
        , DbTableIndexNotFound = 10
        , AppCodeNotFound = 11
        #pragma warning restore 1591 // disable the xmlComments warning
    };
}
