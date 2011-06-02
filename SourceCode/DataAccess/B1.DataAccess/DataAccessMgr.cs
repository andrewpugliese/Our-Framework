using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.Objects;
using System.Xml;
using System.Reflection;

using Microsoft.Practices.EnterpriseLibrary.Data;

using B1.Configuration;
using B1.ILoggingManagement;
using B1.Core;
using B1.CacheManagement;
using B1.IDataAccess;

namespace B1.DataAccess
{
    /// <summary>
    /// Defined as a class instead of a struct because I needed
    /// to define a lock on the object before access.
    /// </summary>
    internal class UniqueIdCacheInfo
    {
        internal string UniqueIdKey;
        internal Int64 UniqueIdBlockHead;
        internal Int64 UniqueIdBlockTail;
        internal UInt32 UniqueIdBlockRemaining;
        internal UInt32 CacheBlockSize;
    }

    /// <summary>
    /// Main class for managing the DataAccess Application Block interface
    /// for a particular connection key (connection string).  
    /// This class provides a database independant interface for Building, Executing and Caching DbCommands.
    /// It also provides methods for accessing the database catalog for metadata about 
    /// database tables, columns, indexes and constraints that can be used for generating dynamic sql.
    /// It also provides methods for generating UniqueIds and SequenceNumbers as was as date time
    /// functions.
    /// 
    /// NOTE: To build DbCommand blocks (multiple dbCommands as a single DbCommand), please review
    /// the DbCommandMgr class which accepts this class in its constructor.
    /// </summary>
    public class DataAccessMgr
    {
#pragma warning disable 1591 // disable the xmlComments warning
        /// <summary>
        /// Enumerates the supported databases
        /// </summary>
        public enum EnumDbType { SqlServer, Oracle, Db2 };
        /// <summary>
        /// Enumerates the different providers which suport the databases
        /// </summary>
        public enum EnumDbProvider { Microsoft, Oracle, IBM };
        /// <summary>
        /// Enumerates the different metrics for measuring date differences
        /// </summary>
        public enum EnumDateDiff { Day, Hour, Minute, Second, MilliSecond };
#pragma warning restore 1591 // restore the xmlComments warning
        private DbCatalogMgr _dbCatalogMgr = null;
        private DataConfigMgr _dataConfigMgr = null;
        private Database _database = null;
        private DataAccessProvider _dbProviderLib = null;
        private string _connectionKey = null;
        private string _dbProviderVersion = null;
        private string _dbName = null;
        private string _dbServerVersion = null;
        private EnumDbType _dbType = EnumDbType.SqlServer; // default
        private EnumDbProvider _dbProvider = EnumDbProvider.Microsoft; // default
        private string _noOpDbCommandText = null;
        private CacheMgr<UniqueIdCacheInfo> _uniqueIdCache 
                = new CacheMgr<UniqueIdCacheInfo>(StringComparer.CurrentCultureIgnoreCase);
        private CacheMgr<DbCommand> _internalDbCmdCache 
                = new CacheMgr<DbCommand>(StringComparer.CurrentCultureIgnoreCase);
        private CacheMgr<DbCommand> _externalDbCmdCache 
                = new CacheMgr<DbCommand>(StringComparer.CurrentCultureIgnoreCase);
        private TimeSpan _timeSpanFromDb = new TimeSpan(0, 0, 0);
        private ILoggingMgr _loggingMgr = null;

        /// <summary>
        /// Main constructor for the DataAccess Manager class.  
        /// It expects a Data Access Application Block Configuration Key or null for the default.
        /// An Interface for exceptions can also be passed which will be used in the event of a DbException.
        /// In any case, the exception will always be thrown so that the caller may take specific
        /// action.
        /// If appConfigSetName and globalConfigSetName parameters are set, then a DataConfigMgr will also
        /// be constructed based on those values; otherwise it will default to an empty collection
        /// </summary>
        /// <param name="connectionKey">string entry in configuration file for connection string</param>
        /// <param name="loggingMgr">Interface for logging exceptions or tracing event messages.</param>
        /// <param name="appConfigSetName">The set of configuration parameters specific to the application set</param>
        /// <param name="globalConfigSetName">The set of configuration parameters that apply to all applications
        /// the IDataAccessProvider interface.</param>
        public DataAccessMgr(string connectionKey
            , ILoggingMgr loggingMgr
            , string appConfigSetName
            , string globalConfigSetName)
        {
            if (connectionKey != null)
            {
                _connectionKey = connectionKey;
                // This call will throw a first chance exception: "Object synchronization method was called 
                // from an unsynchronized block of code."
                // The exception is thrown by the Microsoft.Practices.Unity library.
                // You can have Visual Studio ignore this exception by disabling the SynchronizationLock 
                // exception under the Debug->Exceptions menu.
                _database = DatabaseFactory.CreateDatabase(connectionKey);
            }
            // This call will throw a first chance exception: "Object synchronization method was called 
            // from an unsynchronized block of code."
            // The exception is thrown by the Microsoft.Practices.Unity library.
            // You can have Visual Studio ignore this exception by disabling the SynchronizationLock 
            // exception under the Debug->Exceptions menu.
            else _database = DatabaseFactory.CreateDatabase();

            ObjectFactoryConfiguration dbProviderHelper
                    = AppConfigMgr.GetSection<ObjectFactoryConfiguration>(ObjectFactoryConfiguration.ConfigSectionName); 

            DataTable dbFactories = DbProviderFactories.GetFactoryClasses();
            if (_database is Microsoft.Practices.EnterpriseLibrary.Data.Sql.SqlDatabase)
            {
                _dbProviderLib = ObjectFactory.Create<DataAccessProvider>(string.Format("{0}\\{1}.dll"
                        , string.IsNullOrEmpty(connectionKey) || 
                            dbProviderHelper.GetFactoryObject(connectionKey).AssemblyPath == null 
                            ? Constants.HelperPathSqlServerDefault
                            : dbProviderHelper.GetFactoryObject(connectionKey).AssemblyPath
                        , string.IsNullOrEmpty(connectionKey) ||
                            dbProviderHelper.GetFactoryObject(connectionKey).AssemblyName == null 
                            ? Constants.HelperAssemblySqlServerDefault
                            : dbProviderHelper.GetFactoryObject(connectionKey).AssemblyName)
                        , string.IsNullOrEmpty(connectionKey) ||
                            dbProviderHelper.GetFactoryObject(connectionKey).ObjectClass == null 
                            ? Constants.HelperClassSqlServerDefault
                        : dbProviderHelper.GetFactoryObject(connectionKey).ObjectClass
                    , _database);
                _dbType = EnumDbType.SqlServer;
                _dbProvider = EnumDbProvider.Microsoft;
            }
            else if (_database is Microsoft.Practices.EnterpriseLibrary.Data.GenericDatabase)
            {
                if (_database.DbProviderFactory.ToString() == Constants.DbProviderFactoryOracle)
                {
                    _dbProviderLib = ObjectFactory.Create<DataAccessProvider>(string.Format("{0}\\{1}.dll"
                            , string.IsNullOrEmpty(connectionKey) ||
                                dbProviderHelper.GetFactoryObject(connectionKey).AssemblyPath == null 
                                ? Constants.HelperPathOracleDefault
                                : dbProviderHelper.GetFactoryObject(connectionKey).AssemblyPath
                            , string.IsNullOrEmpty(connectionKey) ||
                                dbProviderHelper.GetFactoryObject(connectionKey).AssemblyName == null 
                                ? Constants.HelperAssemblyOracleDefault
                                : dbProviderHelper.GetFactoryObject(connectionKey).AssemblyName)
                            , string.IsNullOrEmpty(connectionKey) ||
                                dbProviderHelper.GetFactoryObject(connectionKey).ObjectClass == null 
                                ? Constants.HelperClassOracleDefault
                            : dbProviderHelper.GetFactoryObject(connectionKey).ObjectClass
                        , _database);
                    _dbType = EnumDbType.Oracle;
                    _dbProvider = EnumDbProvider.Oracle;
                }
                if (_database.DbProviderFactory.ToString() == Constants.DbProviderFactoryDB2)
                {
                    _dbProviderLib = ObjectFactory.Create<DataAccessProvider>(string.Format("{0}\\{1}.dll"
                            , string.IsNullOrEmpty(connectionKey) ||
                                dbProviderHelper.GetFactoryObject(connectionKey).AssemblyPath == null 
                                ? Constants.HelperPathDB2Default
                                : dbProviderHelper.GetFactoryObject(connectionKey).AssemblyPath
                            , string.IsNullOrEmpty(connectionKey) ||
                                dbProviderHelper.GetFactoryObject(connectionKey).AssemblyName == null 
                                ? Constants.HelperAssemblyDB2Default
                                : dbProviderHelper.GetFactoryObject(connectionKey).AssemblyName)
                        , string.IsNullOrEmpty(connectionKey) ||
                                dbProviderHelper.GetFactoryObject(connectionKey).ObjectClass == null 
                                ? Constants.HelperClassDB2Default
                                : dbProviderHelper.GetFactoryObject(connectionKey).ObjectClass
                        , _database);

                    _dbType = EnumDbType.Db2;
                    _dbProvider = EnumDbProvider.IBM;
                }
            }
            else throw new ExceptionEvent(enumExceptionEventCodes.FunctionNotImplementedForDbType
                            , _database.ToString());

            _dbProviderVersion = _dbProviderLib.Version;
            _dbServerVersion = _dbProviderLib.ServerVersion;
            _dbName = _dbProviderLib.DatabaseName;
            _noOpDbCommandText = _dbProviderLib.NoOpDbCommandText;
            _loggingMgr = loggingMgr;
            _dataConfigMgr = new DataConfigMgr(this);
            _dbCatalogMgr = new DbCatalogMgr(this);

             BuildUniqueIdCommands();

             _timeSpanFromDb = GetServerTime(EnumDateTimeLocale.UTC) - DateTime.UtcNow;
        }

        /// <summary>
        /// Constructor expects a Data Access Application Block Configuration Key or null for the default.
        /// It expects a Data Access Application Block Configuration Key or null for the default.
        /// An Interface for exceptions can also be passed which will be used in the event of a DbException.
        /// In any case, the exception will always be thrown so that the caller may take specific
        /// action.
        /// </summary>
        /// <param name="connectionKey">string entry in configuration file for connection string</param>
        /// <param name="loggingMgr">Interface for logging exceptions or tracing event messages.</param>
        public DataAccessMgr(string connectionKey, ILoggingMgr loggingMgr)
            : this(connectionKey, loggingMgr, null, null)
        {
        }

        /// <summary>
        /// Constructs a new instance for the given connection string
        /// configuration key.  The instance will not have an loggingMgr associated
        /// with it.
        /// </summary>
        /// <param name="connectionKey">A key to be found in the configuration file</param>
        public DataAccessMgr(string connectionKey)
            : this(connectionKey, null, null, null)
        {
        }

        /// <summary>
        /// Constructs a new instance using the default configured values for the database 
        /// application block object.  Assumes the the default key is defined in the configuration
        /// file.
        /// </summary>
        public DataAccessMgr()
            : this(null, null, null, null)
        {

        }

        /// <summary>
        /// Returns an enumeration of the supported database types for the
        /// given string or throws an exception.
        /// </summary>
        /// <param name="dbTypeString">The string representation of the databae type</param>
        /// <returns>The enumeration of the given string</returns>
        public static EnumDbType ConvertToDbType(String dbTypeString)
        {
            String dbType = dbTypeString.ToLower();
            if (dbType == EnumDbType.Oracle.ToString().ToLower())
                return EnumDbType.Oracle;
            else if (dbType == EnumDbType.SqlServer.ToString().ToLower())
                return EnumDbType.SqlServer;
            else if (dbType == EnumDbType.Db2.ToString().ToLower())
                return EnumDbType.Db2;
            else throw new ExceptionEvent(enumExceptionEventCodes.UnsupportedDbType, dbTypeString);
        }

        /// <summary>
        /// Returns the underlying Database object
        /// </summary>
        public Database Database
        {
            get { return _database; }
        }


        /// <summary>
        /// Gets or Sets the underlying Event Manager Interface
        /// NOTE: THIS CAN BE NULL IF IT WAS NOT INITIALIZED
        /// </summary>
        public ILoggingMgr loggingMgr
        {
            get { return _loggingMgr; }
            set { _loggingMgr = value; }
        }

        /// <summary>
        /// Gets or Sets the underlying DataConfigMgr 
        /// NOTE: THIS CAN BE NULL IF IT WAS NOT INITIALIZED
        /// </summary>
        public DataConfigMgr dataConfigMgr
        {
            get
            {
                return _dataConfigMgr;
            }
            set { _dataConfigMgr = value; }
        }

        /// <summary>
        /// Returns the current time from the current machine adjusted with the offset of
        /// the db server time taken at construction.  Does not use a call to the db; but is
        /// not as accurate.
        /// </summary>
        public DateTime DbSynchTime
        {
            get { return DateTime.UtcNow.AddMilliseconds(_timeSpanFromDb.TotalMilliseconds); }
        }

        /// <summary>
        /// Returns the difference in time measured as a TimeSpan between the database
        /// and the server that the application resides.  Time is calculated as universal time.
        /// </summary>
        public TimeSpan DbSynchTimeOffSet
        {
            get { return _timeSpanFromDb; }
        }

        /// <summary>
        /// Returns the Connection Key string used
        /// </summary>
        public string ConnectionKey
        {
            get { return _connectionKey; }
        }

        /// <summary>
        /// Returns an enum indicating whether the Database (e.g. Oracle, SqlServer)
        /// </summary>
        public EnumDbType DatabaseType
        {
            get { return _dbType; }
        }

        /// <summary>
        /// Returns an enum indicating who the provider is for the database (e.g. Microsoft, Oracle)
        /// </summary>
        public EnumDbProvider DatabaseProvider
        {
            get { return _dbProvider; }
        }

        /// <summary>
        /// Returns the interface pointer for the database data provider.
        /// </summary>
        public DataAccessProvider DbProviderLib
        {
            get
            {
                return _dbProviderLib;
            }
        }

        /// <summary>
        /// Version a string of the dll version for the database data provider.
        /// </summary>
        public string DbProviderVersion
        {
            get
            {
                return _dbProviderVersion;
            }
        }

        /// <summary>
        /// Version of the database server.
        /// </summary>
        public string DbServerVersion
        {
            get
            {
                return _dbServerVersion;
            }
        }

        /// <summary>
        /// Name of the database data access manager refers to.
        /// </summary>
        public string DbName
        {
            get
            {
                return _dbName;
            }
        }

        /// <summary>
        /// Returns the instance of the DbCatalogMgr or throws and exception if that was functionality
        /// is not available because the data dictionary was not setup properly.
        /// </summary>
        DbCatalogMgr DbCatalogMgrPtr
        {
            get
            {
                return _dbCatalogMgr;
            }
        }

        internal CacheMgr<DbCommand> InternalDbCmdCache
        {
            get { return _internalDbCmdCache; }
        }

        /// <summary>
        /// Returns the backend specific function for current datetime
        /// as a string e.g. sysdate or getdate() to be used in a seperate command
        /// if ReturnAsAlias is not null, it will be the alias
        /// </summary>
        /// <param name="dbDateType">The format type of the date function</param>
        /// <param name="returnAsAlias">What the return column will be called</param>
        /// <returns>Backend specific function for current date time</returns>
        public string GetDbTimeAs(EnumDateTimeLocale dbDateType, string returnAsAlias)
        {
            return _dbProviderLib.GetDbTimeAs(dbDateType, returnAsAlias) + (string.IsNullOrEmpty(returnAsAlias) 
                            ? "" : " as " + returnAsAlias);
        }

        /// <summary>
        /// Returns the DateTime from the database.
        /// Note: This operation will make a database call.
        /// </summary>
        /// <param name="dbDateType">Enumeration value indicating whether time is local or UTC;
        /// default is UTC.</param>
        /// <returns>The database time</returns>
        public DateTime GetServerTime(EnumDateTimeLocale dbDateType)
        {
            string sql = _dbProviderLib.GetServerTimeCommandText(dbDateType, "Now");
            DataTable dt = this.ExecuteDataSet(BuildSelectDbCommand(sql, null), null, null).Tables[0];
            return Convert.ToDateTime(dt.Rows[0]["Now"]);
        }

        #region DbCommand Methods

        internal string NoOpDbCommandText
        {
            get { return _noOpDbCommandText; }
        }

        /// <summary>
        /// Method to indicate whether the given DbCommand object is a No Operation DbCommand
        /// </summary>
        /// <param name="dbCmd">DAAB DbCommand object</param>
        /// <returns>true or false</returns>
        public bool IsNoOpDbCommand(DbCommand dbCmd)
        {
            return dbCmd.CommandText == _noOpDbCommandText;
        }

        /// <summary>
        /// Returns a dbCommand with the equivalent of a No Operation.
        /// </summary>
        /// <returns>DAAB DbCommand Object with NO DbParameters which corresponds to a NOOP.
        /// Adding a NoOpDbCommand to a CommandBlock has no effect.  It is useful for
        /// having recursive calls to add to a CommandBlock.
        /// The CommandType is Text.</returns>
        public DbCommand BuildNoOpDbCommand()
        {
            if (DatabaseType == DataAccessMgr.EnumDbType.SqlServer)
                return _database.GetSqlStringCommand(_noOpDbCommandText);
            else if (DatabaseType == DataAccessMgr.EnumDbType.Oracle)
                return _database.GetSqlStringCommand(
                    _dbProviderLib.FormatCommandText(_noOpDbCommandText));
            else if (DatabaseType == DataAccessMgr.EnumDbType.Db2)
                return _database.GetSqlStringCommand(_noOpDbCommandText);
            else throw new ExceptionEvent(enumExceptionEventCodes.FunctionNotImplementedForDbType
                            , DatabaseType.ToString());
        }

        /// <summary>
        /// Method to return a copy of the given DbCmd so that the Parameters
        /// will be thread safe.
        /// </summary>
        /// <param name="dbCmd">Data Access Application Block Database Command Object</param>
        /// <returns>New Copy of the given DbCommand</returns>
        public DbCommand CloneDbCommand(DbCommand dbCmd)
        {
            DbCommandMgr cmdMgr = new DbCommandMgr(this);
            cmdMgr.AddDbCommand(dbCmd);
            return cmdMgr.DbCommandBlock;
        }


        /// <summary>
        /// Returns the list of unqualified, unique column names from the given select statement
        /// or * if there are any exceptions.
        /// </summary>
        /// <param name="selectStatement"></param>
        /// <returns>Comma seperated list of column names or * if there is any exception</returns>
        string GetColumnsFromSelectStatement(string selectStatement)
        {
            // split statement based on select and from
            StringBuilder selectColumns = new StringBuilder();
            Dictionary<string, int> columnDupes = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
            try
            {
                string[] columnList = selectStatement.ToLower().Replace(Environment.NewLine, string.Empty)
                        .Split(new string[] { "select" }, StringSplitOptions.RemoveEmptyEntries)[0]
                        .Split(new string[] { "from" }, StringSplitOptions.RemoveEmptyEntries);
                string[] columns = columnList[0].Split(new char[] { ',' });
                foreach (string qualifiedColumn in columns)
                {
                    string column = qualifiedColumn;
                    // remove any qualifier prefixes as they will not be available to the outer select
                    string[] unqualifiedColumn = qualifiedColumn.Split(new char[] { '.' });
                    if (unqualifiedColumn.Length > 1)
                        column = unqualifiedColumn[1];
                    if (columnDupes.ContainsKey(column))
                        columnDupes[column]++;
                    else columnDupes.Add(column, 0);
                    selectColumns.AppendFormat("{0}{1}{2}", selectColumns.Length > 0 ? ", " : ""
                            , column
                            , columnDupes[column] > 0 ? columnDupes[column].ToString() : "");
                }
                return selectColumns.ToString();
            }
            // if there are any exceptions, then just return * to select all columns
            catch
            {
                return "*";
            }
        }

        /// <summary>
        /// Function takes any select statement and will turn it into a select statement
        /// that will return only the number of rows defined by parameter BufferSize.
        /// If BufferSize is a string, then it will be assumed be a bind variable.
        /// If it is an Int, then the constant will be used.
        /// NOTE: If for some executions you want a full result set without rewriting query
        ///         set BufferSize Param Value = 0;
        ///         Value CANNOT BE SET TO NULL
        /// DB2 USERS: In order to implement a dynamic buffer size, the row_number() function was applied
        /// however, this column would then be returned in the result set as (row_num);  This function
        /// will attempt to remove it from the statement.  In order to do this, we require a unique set
        /// a column names so if there are joins with the same column then they must be uniquely aliased.
        /// </summary>
        /// <param name="selectStatement">A valid SQL select statement with UNIQUE column names</param>
        /// <param name="bufferSize">Limits the number of rows returned.  If the param is a constant number
        /// , then it will be a fixed number of records returned each time.  If the param is a string
        /// , then a parameter will be created with the name equal to the string provided.  This
        /// can be used to change the buffer size for each execution of the dbCommand.  Null indicates
        /// all rows are returned.</param>
        /// <returns>Select statement with max rows</returns>
        public string FormatSQLSelectWithMaxRows(string selectStatement, object bufferSize)
        {
            if (bufferSize == null
                || bufferSize.ToString() == string.Empty)
                return selectStatement;

            if (!(bufferSize is string || bufferSize is int))
                throw new ExceptionEvent(enumExceptionEventCodes.NullOrEmptyParameter
                            , string.Format("Must be non null string or int data type only: BufferSize:{0}", bufferSize));

            if (DatabaseType == DataAccessMgr.EnumDbType.SqlServer)
                return string.Format("set rowcount {0}{2}{1}{2}"
                    , bufferSize is string ? BuildBindVariableName(bufferSize.ToString()) : bufferSize.ToString()
                                , selectStatement
                                , Environment.NewLine);
            else if (DatabaseType == DataAccessMgr.EnumDbType.Oracle)
                return string.Format("select * from ({0}){1} where ({2} = 0 or ({2} > 0 and rownum <= {2})){1}"
                            , selectStatement
                            , Environment.NewLine
                            , bufferSize is string ? BuildBindVariableName(bufferSize.ToString()) : bufferSize.ToString());
            else if (DatabaseType == DataAccessMgr.EnumDbType.Db2)
                return string.Format("select {3} from (select x.*, row_number() over () as row_num from ({0}) x {1} ) y where "
                        + " ({2} = 0 or ({2} > 0 and y.row_num <= {2})){1}"
                            , selectStatement
                            , Environment.NewLine
                            , bufferSize is string ? BuildBindVariableName(bufferSize.ToString()) : bufferSize.ToString()
                            , GetColumnsFromSelectStatement(selectStatement));
            else throw new ExceptionEvent(enumExceptionEventCodes.FunctionNotImplementedForDbType
                            , DatabaseType.ToString());
        }


        /// <summary>
        /// Provides the back-end specific date math operation string
        /// for the given start date (or current datetime if null)
        /// </summary>
        /// <param name="dateDiffInterval"></param>
        /// <param name="duration"></param>
        /// <param name="startDate"></param>
        /// <returns></returns>
        public string FormatDateMathSql(EnumDateDiffInterval dateDiffInterval
                                        , object duration
                                        , string startDate)
        {
            return _dbProviderLib.FormatDateMathSql(dateDiffInterval
                    , duration
                    , startDate);
        }


        /// <summary>
        /// Converts a DynamicSQL string with .Net parameter symbols (e.g. {0}, {1}) in place
        /// of the Db Parameters to a back-end specific parameterized SQL statement based upon
        /// the given parameter collection.
        /// </summary>
        /// <param name="dbParameters">A collection of DbParameters.</param>
        /// <param name="dynamicSQLFormatString">An SQL statement with .Net parameter symbols (e.g. {0}, {1}).</param>
        /// <returns>A back-end compliant parameterized sql statement.</returns>
        public string FormatSQLStringToDynamicSQL(DbParameterCollection dbParameters
            , string dynamicSQLFormatString)
        {
            if (dbParameters != null && dbParameters.Count > 0)
            {
                for (int i = 0; i < dbParameters.Count; i++)
                {
                    string paramPlaceholder = "{" + i.ToString() + "}";

                    if (!dynamicSQLFormatString.Contains(paramPlaceholder))
                        throw new ExceptionEvent(enumExceptionEventCodes.InvalidFormatStringDynamicSQL
                                , string.Format("Sql Parameter {0} in the {1} position was not defined in SQL Text Command {2}"
                                        , dbParameters[i].ParameterName, i, dynamicSQLFormatString));

                    // replace the paramPlaceHolder with the formal parameter name
                    dynamicSQLFormatString = dynamicSQLFormatString.Replace(paramPlaceholder
                                            , BuildBindVariableName(dbParameters[i].ParameterName));
                }

            }
            else if (dynamicSQLFormatString.Contains("{")) // check to see if command requires paramaters
                throw new ExceptionEvent(enumExceptionEventCodes.NullOrEmptyParameter
                                        , string.Format("SQL Command: {0} expected parameters and none were provided."
                                                                            , dynamicSQLFormatString));
            return dynamicSQLFormatString;
        }


        /// <summary>
        /// Returns a structure containing metadata about columns used in an index
        /// </summary>
        /// <param name="columnName">The name of the column</param>
        /// <param name="IsAscending">Whether or not the column is sorted ascending (false = descending)</param>
        /// <returns>DbIndexColumnStructure</returns>
        public DbIndexColumnStructure BuildIndexColumn(string columnName, bool IsAscending)
        {
            DbIndexColumnStructure indexColumn = new DbIndexColumnStructure();
            indexColumn.ColumnName = columnName;
            indexColumn.IsDescending = !IsAscending;
            return indexColumn;
        }

        /// <summary>
        /// Returns a structure containing metadata describing an index column that is sorted ascending
        /// </summary>
        /// <param name="columnName">Name of column</param>
        /// <returns>DbIndexColumnStructure for an ascending index</returns>
        public DbIndexColumnStructure BuildIndexColumnAscending(string columnName)
        {
            return BuildIndexColumn(columnName, true);
        }

        /// <summary>
        /// Returns a structure containing metadata describing an index column that is sorted descending
        /// </summary>
        /// <param name="columnName">Name of column</param>
        /// <returns>DbIndexColumnStructure for an descending index</returns>
        public DbIndexColumnStructure BuildIndexColumnDescending(string columnName)
        {
            return BuildIndexColumn(columnName, false);
        }

        /// <summary>
        /// Builds a non select statement dbCommand (Insert, Update, Delete)
        /// </summary>
        /// <param name="nonQueryStatement">The sql statement</param>
        /// <param name="dbParams">The parameter collection corresponding to the sql statement</param>
        /// <returns>DAAB DbCommand Object with DbParameters (initialized to the values provided
        /// or DbNull.  The CommandType is Text.</returns>
        public DbCommand BuildNonQueryDbCommand(string nonQueryStatement
                                    , DbParameterCollection dbParams)
        {
            if (_dbType == EnumDbType.Oracle)
                nonQueryStatement = _dbProviderLib.FormatCommandText(nonQueryStatement);

            DbCommand cmdNonQuery = _database.GetSqlStringCommand(nonQueryStatement);
            if (dbParams != null)
                foreach (DbParameter dbParam in dbParams)
                    CopyParameterToCollection(cmdNonQuery.Parameters, dbParam);

            if (DatabaseType == EnumDbType.Oracle)
                cmdNonQuery = _dbProviderLib.FormatDbCommand(cmdNonQuery);
            return cmdNonQuery;
        }

        /// <summary>
        /// Builds a Select DbCommand object that is compliant with the back-end database
        /// for the give Select Statement and parameter collection.
        /// </summary>
        /// <param name="selectStatement">A back-end compliant select statement</param>
        /// <param name="dbParams">A DbParameter Collection</param>
        /// <returns>DAAB DbCommand Object with DbParameters (initialized to the values provided
        /// or DbNull.  The CommandType is Text.</returns>
        public DbCommand BuildSelectDbCommand(string selectStatement
                                    , DbParameterCollection dbParams)
        {
            string paramRefCursor = IDataAccess.Constants.RefCursor;
            string selectCommandText = DatabaseType == EnumDbType.Oracle
                                        ? string.Format("open {0} for{1}{2};"
                                                , BuildBindVariableName(paramRefCursor)
                                                , Environment.NewLine
                                                , selectStatement)
                                        : selectStatement;  // no change for SqlServer

            if (_dbType == EnumDbType.Oracle)
                selectCommandText = _dbProviderLib.FormatCommandText(selectCommandText);

            DbCommand cmdSelect = _database.GetSqlStringCommand(selectCommandText);

            if (_dbType == EnumDbType.Oracle) // we must add the refcursor
            {
                CopyParameterToCollection(cmdSelect.Parameters
                        , _dbProviderLib.CreateNewParameter(paramRefCursor
                            , DbType.Object
                            , IDataAccess.Constants.RefCursor
                            , 0
                            , ParameterDirection.Output
                            , DBNull.Value));
            }

            if (dbParams != null)
                foreach (DbParameter dbParam in dbParams)
                    CopyParameterToCollection(cmdSelect.Parameters, dbParam);

            if (DatabaseType == EnumDbType.Oracle)
                return _dbProviderLib.FormatDbCommand(cmdSelect);
            else return cmdSelect;
        }

        /// <summary>
        /// Builds a select DbCommand and parameter collection
        /// which can be executed against the multiple back-end
        /// supported databases.  In addition, the number of rows
        /// returned can be limited to the given buffersize parameter.
        /// Null indicates all rows.
        /// Numeric constant indicates a fixed size across all executions.
        /// String causes a parameter to be created which will allow
        /// programmer to set buffersize dynamically.
        /// 
        /// NOTE: You will need to define the parameters as well
        /// and consider any database specific syntax if you wish to
        /// run it against multiple back ends.
        /// </summary>
        /// <param name="dmlSelect">MetaData Structure describing the select columns and conditions</param>
        /// <param name="bufferSize">Limits the number of rows returned.  If the param is a constant number
        /// , then it will be a fixed number of records returned each time.  If the param is a string
        /// , then a parameter will be created with the name equal to the string provided.  This
        /// can be used to change the buffer size for each execution of the dbCommand.  Null indicates
        /// all rows are returned.</param>
        /// <returns>DAAB DbCommand Object with DbParameters (initialized to the values provided
        /// or DbNull.  The CommandType is Text.</returns>
        public DbCommand BuildSelectDbCommand(DbTableDmlMgr dmlSelect
                                            , object bufferSize)
        {
            Tuple<string, DbParameterCollection> result = BuildSelect(dmlSelect, bufferSize);
            return BuildSelectDbCommand(result.Item1, result.Item2);
        }

        private Tuple<string, DbParameterCollection> BuildSelect(DbTableDmlMgr dmlSelect, object bufferSize)
        {
            if (dmlSelect.Tables == null | dmlSelect.Tables.Count == 0)
                throw new ExceptionEvent(enumExceptionEventCodes.NullOrEmptyParameter
                            , "Cant build select dbcommand with no tables");

            StringBuilder selectClause = new StringBuilder();
            foreach (string columnName in dmlSelect.QualifiedColumns)
                selectClause.AppendFormat("{0}{1}", 
                        selectClause.Length > 0 ? ", " : 
                            dmlSelect.SelectDistinct ? "select distinct " : "select ",
                        columnName);

            if(dmlSelect.CaseColumns.Count > 0)
                selectClause.AppendFormat("{0}{1}{2}", dmlSelect.QualifiedColumns.Count() > 0 ? ", " : "",
                        Environment.NewLine, 
                        BuildCaseStatementsForSelect(dmlSelect));

            selectClause.AppendFormat("{0} from {1} "
                        , Environment.NewLine
                        , BuildJoinClause(dmlSelect));
            
            StringBuilder whereClause = new StringBuilder();
            if (dmlSelect._whereCondition != null)
                whereClause.AppendFormat("where {0}", dmlSelect._whereCondition.ToString(this));

            StringBuilder groupByClause = new StringBuilder();
            if (dmlSelect.GroupByColumns != null && dmlSelect.GroupByColumns.Count > 0)
                foreach (Int16 columnOrder in dmlSelect.GroupByColumns.Keys)
                {
                    DbQualifiedObject<string> column = dmlSelect.GroupByColumns[columnOrder];

                    string tableAlias = column.Alias != null ? column.Alias
                        : dmlSelect.GetTable(column.SchemaName, column.TableName).TableAlias;

                    groupByClause.AppendFormat("{0}{1}.{2}", groupByClause.Length > 0 ? " ," : " group by ",
                            tableAlias, column.DbObject);
                }

            StringBuilder orderByClause = new StringBuilder();
            if (dmlSelect.OrderByColumns != null && dmlSelect.OrderByColumns.Count > 0)
                foreach (Int16 columnOrder in dmlSelect.OrderByColumns.Keys)
                {
                    var column = dmlSelect.OrderByColumns[columnOrder];

                    string tableAlias = column.Alias != null ? column.Alias 
                        : dmlSelect.GetTable(column.SchemaName, column.TableName).TableAlias; 
                    
                    orderByClause.AppendFormat("{0}{1}.{2}{3}", orderByClause.Length > 0 ? " ," : " order by "
                            , tableAlias
                            , column.DbObject.ColumnName
                            , column.DbObject.IsDescending ? " desc" : " asc");
                }
 
            // now add the parameters
            // start with the statement
            StringBuilder selectSQL = new StringBuilder();
            selectSQL.Append(selectClause.ToString());
            if (whereClause.Length > 0)
                selectSQL.AppendFormat("{0}{1}", Environment.NewLine
                                    , whereClause.ToString());
            if (groupByClause.Length > 0)
                selectSQL.AppendFormat("{0}{1}", Environment.NewLine
                                    , groupByClause.ToString());

            if (orderByClause.Length > 0)
                selectSQL.AppendFormat("{0}{1}", Environment.NewLine
                                    , orderByClause.ToString());

            selectSQL.Append(Environment.NewLine);

            string cmdText = FormatSQLSelectWithMaxRows(selectSQL.ToString(), bufferSize);

            DbParameterCollection dbParams = null;
            if (whereClause.Length > 0)
                dbParams = BuildWhereClauseParams(dmlSelect._whereCondition.Parameters.Values);

            // only if BufferSize is a parameter (string) do we need a parameter
            // otherwise it was part of the commandText
            if (bufferSize != null && bufferSize is string && !string.IsNullOrEmpty(bufferSize.ToString()))
                if (dbParams == null)
                    dbParams = CreateNewParameterAndCollection(bufferSize.ToString()
                                        , DbType.Int32
                                        , null
                                        , 0
                                        , ParameterDirection.Input
                                        , DBNull.Value);
                else AddNewParameterToCollection(dbParams
                                        , bufferSize.ToString()
                                        , DbType.Int32
                                        , null
                                        , 0
                                        , ParameterDirection.Input
                                        , DBNull.Value);

            return new Tuple<string,DbParameterCollection>(cmdText, dbParams);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="queryable"></param>
        /// <param name="bufferSize"></param>
        /// <param name="defaultSchema"></param>
        /// <returns></returns>
        public DbCommand BuildSelectDbCommand<TEntity>(IQueryable<TEntity> queryable, object bufferSize, 
                string defaultSchema = null) where TEntity : class
        {
            LinqQueryParser parser = new LinqQueryParser(queryable, this, defaultSchema);

            string select = parser.GetOuterSelect();
            string from = parser.GetOuterFrom();
            if(from == null)
                from = parser.GetDefaultFrom();

            string where = parser.GetOuterWhere();
            string orderBy = parser.GetOuterOrderBy();
            string groupBy = parser.GetOuterGroupBy();

            StringBuilder selectCmd = new StringBuilder(string.Format("{0}{1} FROM {2}", 
                select, 
                Environment.NewLine, 
                from));

            if(!string.IsNullOrWhiteSpace(where))
                selectCmd.AppendFormat("{0}{1}",
                        Environment.NewLine,
                        where);

            if(!string.IsNullOrWhiteSpace(groupBy))
                selectCmd.AppendFormat("{0}{1}",
                        Environment.NewLine, groupBy);

            if(!string.IsNullOrWhiteSpace(orderBy))
                selectCmd.AppendFormat("{0}{1}",
                        Environment.NewLine, orderBy);

            DbParameterCollection dbParams = BuildWhereClauseParams(parser.Parameters);

            string cmdText = FormatSQLSelectWithMaxRows(selectCmd.ToString(), bufferSize);
            
            DbCommand dbCmd = BuildSelectDbCommand(cmdText, dbParams);

            dbCmd.Site = new ParameterSite(parser.Parameters);

            return dbCmd;
        }

        /// <summary>
        /// Creates an update command with a where condition based on the columns in lastModColumns.
        /// If the the passed in DbTableDmlMgr already has a where condition, the where condition will be 
        /// modified to include these additional column predicates. TWO paremeters will be generated for each last 
        /// mod column. One for there where condition, and one for the new value. 
        /// Use BuildParamName(string fieldName, bool isNewValueParam) to generate the names for the new/where paramaters.
        /// </summary>
        /// <param name="dmlChange">DbTableDmlMgr representing table to be updated. Can have where condition.</param>
        /// <param name="lastModColumns">Fully qualified column names.</param>
        /// <returns>DbCommand</returns>
        public DbCommand BuildChangeDbCommand(DbTableDmlMgr dmlChange, params DbQualifiedObject<string>[] lastModColumns)
        {
            if(lastModColumns.Length == 0)
                throw new ExceptionEvent(enumExceptionEventCodes.InvalidParameterValue,
                    "BuildChangeDbCommand Error: lastModFieldValues must contain at least one field/value pair");

            foreach(var lastModCol in lastModColumns)
            {
                if (!dmlChange.ColumnsForUpdateOrInsert.ContainsKey(lastModCol))
                {
                    DbColumnStructure column = DbCatalogGetColumn(lastModCol.SchemaName, lastModCol.TableName, lastModCol.DbObject);
                    dmlChange.AddColumn(lastModCol.DbObject, BuildParamName(lastModCol.DbObject, true));
                }
            }

            Expression lastModKeyExpression = null;
            foreach(var lastModCol in lastModColumns)
            {
                var lastModColFinal = lastModCol;

                Expression partialEqualExpression = DbPredicate.CreatePredicatePart( t => t.Column(lastModColFinal.SchemaName, 
                        lastModColFinal.TableName, lastModColFinal.DbObject) == t.Parameter(lastModColFinal.TableName, 
                        lastModColFinal.DbObject, BuildParamName(lastModColFinal.DbObject)));

                Expression partialNullExpression = DbPredicate.CreatePredicatePart(t => t.Parameter(lastModColFinal.TableName, 
                        lastModColFinal.DbObject, BuildParamName(lastModColFinal.DbObject)) == null && t.Column(lastModColFinal.SchemaName, 
                        lastModColFinal.TableName, lastModColFinal.DbObject) == null);

                Expression partialExpression = Expression.OrElse(partialEqualExpression, partialNullExpression);

                if(lastModKeyExpression == null)
                {
                    lastModKeyExpression = partialExpression;
                }
                else
                {
                    lastModKeyExpression = Expression.AndAlso(lastModKeyExpression, partialExpression);
                }

            }
            
            if (dmlChange._whereCondition == null)
                dmlChange.SetWhereCondition(lastModKeyExpression);
            else
                dmlChange.AddToWhereCondition(ExpressionType.AndAlso, lastModKeyExpression);

            return BuildUpdateDbCommand(dmlChange);
        }

        /// <summary>
        /// Builds an update DbCommand and parameter collection
        /// which can be executed against the multiple back-end
        /// supported databases.  
        /// 
        /// NOTE: You will need to define the parameters as well
        /// and consider any database specific syntax if you wish to
        /// run it against multiple back ends.
        /// </summary>
        /// <param name="dmlUpdate">MetaData Structure describing table(s) that will be used for SQL</param>
        /// <returns>DAAB DbCommand Object with DbParameters (initialized to the values provided
        /// or DbNull.  The CommandType is Text.</returns>
        public DbCommand BuildUpdateDbCommand(DbTableDmlMgr dmlUpdate)
        {
            if (dmlUpdate.ColumnsForUpdateOrInsert == null || dmlUpdate.ColumnsForUpdateOrInsert.Count == 0)
                throw new ExceptionEvent(enumExceptionEventCodes.NullOrEmptyParameter
                            , "Cant build update dbcommand with no columns");


            string updateTable = null;

            DbParameterCollection dbParams = null;

            if(DatabaseType != EnumDbType.SqlServer)
            {
                Tuple<string, DbParameterCollection> selectResult = BuildSelect(dmlUpdate, null);

                updateTable = string.Format("{0}({1}) {2}"
                        , Environment.NewLine
                            , selectResult.Item1
                            , _dbProviderLib.DefaultTableAlias);

                dbParams = selectResult.Item2;
            }
            else
            {
                updateTable = dmlUpdate.MainTable.TableAlias;
                dbParams = _database.GetSqlStringCommand(_noOpDbCommandText).Parameters;
            }

            string updateSet = string.Format("update {0} set ", updateTable);

            
            StringBuilder updateClause = new StringBuilder();
            foreach (KeyValuePair<DbQualifiedObject<string>, object> columnUpdate in dmlUpdate.ColumnsForUpdateOrInsert)
            {
                DbQualifiedObject<string> qualifiedColumn = columnUpdate.Key;
                string columnName = qualifiedColumn.DbObject;
                string columnValue = "";

               if(columnUpdate.Value is DbFunctionStructure)
                {
                    columnValue = ((DbFunctionStructure)columnUpdate.Value).FunctionBody;
                }
                else if(columnUpdate.Value is DbParameter)
                {
                    columnValue = BuildBindVariableName(((DbParameter)columnUpdate.Value).ParameterName);
                }
               else if (columnUpdate.Value is EnumDateTimeLocale)
                {
                    columnValue = GetDbTimeAs((EnumDateTimeLocale)columnUpdate.Value, null);
                }
                else if (columnUpdate.Value is DbConstValue)
                {
                    columnValue = ((DbConstValue)columnUpdate.Value).GetQuotedValue();
                }
                else if(columnUpdate.Value is string)
                {
                    columnValue = BuildBindVariableName((string)columnUpdate.Value);

                    DbColumnStructure column = DbCatalogGetColumn(qualifiedColumn.SchemaName
                                                    , qualifiedColumn.TableName
                                                    , columnName);

                    // only add parameter if it does not exist in the whereCondition
                    // because it will be added in the where clause processing
                    if (!(dmlUpdate._whereCondition != null && dmlUpdate._whereCondition.Parameters.ContainsKey(
                            _dbProviderLib.BuildParameterName(columnName))))
                        AddNewParameterToCollection(dbParams
                                        , (string)columnUpdate.Value
                                        , column.DataTypeGenericDb
                                        , column.DataTypeNativeDb
                                        , column.MaxLength
                                        , ParameterDirection.Input
                                        , DBNull.Value);
                }

                string tableAlias = null;

                if (DatabaseType != EnumDbType.SqlServer)
                    tableAlias = _dbProviderLib.DefaultTableAlias;
                else
                    tableAlias = dmlUpdate.GetTable(qualifiedColumn.SchemaName, qualifiedColumn.TableName).TableAlias;

                // add to update clause
                updateClause.AppendFormat("{0}{1}.{2} = {3}"
                        , updateClause.Length > 0 ? string.Format("{0}, ", Environment.NewLine) : updateSet
                        , tableAlias
                        , columnName
                        , columnValue);
            }

            if(DatabaseType == EnumDbType.SqlServer)
            {
                updateClause.AppendFormat("{0} from {1} "
                            , Environment.NewLine
                            , BuildJoinClause(dmlUpdate));
            
                StringBuilder whereClause = new StringBuilder();
                if(dmlUpdate._whereCondition != null)
                    whereClause.AppendFormat("{0}where {1}" , Environment.NewLine, dmlUpdate._whereCondition.ToString(this));

                if(whereClause.Length > 0)
                {
                    dbParams = BuildWhereClauseParams(dmlUpdate._whereCondition.Parameters.Values, dbParams);
                    updateClause.Append(whereClause);
                }
            }

            updateClause.Append(Environment.NewLine);

            return BuildNonQueryDbCommand(updateClause.ToString(), dbParams);
        }

        
        /// <summary>
        /// Builds a delete DbCommand and parameter collection
        /// which can be executed against the multiple back-end
        /// supported databases.  
        ///  
        /// NOTE: You will need to define the paramenters as well
        /// and consider any database specific syntax if you wish to
        /// run it against multiple back ends.
        /// </summary>
        /// <param name="dmlDelete">MetaData Structure describing the delete conditions</param>
        /// <returns>DAAB DbCommand Object with DbParameters (initialized to the values provided
        /// or DbNull.  The CommandType is Text.</returns>
        public DbCommand BuildDeleteDbCommand(DbTableDmlMgr dmlDelete)
        {
            StringBuilder deleteSQL = new StringBuilder();

            DbParameterCollection dbParams = null;

            if(DatabaseType == EnumDbType.SqlServer)
            {
                deleteSQL.AppendFormat("delete {0} from {1}"
                        , dmlDelete.MainTable.TableAlias
                        , BuildJoinClause(dmlDelete));

                StringBuilder whereClause = new StringBuilder();
                if(dmlDelete._whereCondition != null)
                    whereClause.AppendFormat("{0}where {1}" , Environment.NewLine, 
                            dmlDelete._whereCondition.ToString(this));

                // now add the parameters
                // start with the statement
                if (whereClause.Length > 0)
                    deleteSQL.AppendFormat("{0}{1}", Environment.NewLine
                                        , whereClause.ToString());

                if (whereClause.Length > 0)
                    dbParams = BuildWhereClauseParams(dmlDelete._whereCondition.Parameters.Values);

            }
            else
            {
                dmlDelete.MainTable.SelectColumns.Clear();
                dmlDelete.MainTable.SelectColumns.Add("*");

                Tuple<string, DbParameterCollection> selectResult = BuildSelect(dmlDelete, null);

                deleteSQL.AppendFormat("delete from ({0})"
                        , selectResult.Item1
                        , BuildJoinClause(dmlDelete));

                dbParams = selectResult.Item2;
            }

            deleteSQL.Append(Environment.NewLine);

            return BuildNonQueryDbCommand(deleteSQL.ToString(), dbParams);
        }

       
        /// <summary>
        /// Builds an insert DbCommand object from the given DbTableDml
        /// meta data structure.  It will initialize the DbParameter collection
        /// to what was provided in the meta data and will return a DAAB DbCommand
        /// object of command type = Text.
        /// </summary>
        /// <param name="dmlInsert">DbTable Dml Meta Data structure.</param>
        /// <returns>DAAB DbCommand Object with DbParameters (initialized to the values provided
        /// or DbNull.  The CommandType is Text.</returns>
        public DbCommand BuildInsertDbCommand(DbTableDmlMgr dmlInsert)
        {
            if (dmlInsert.ColumnsForUpdateOrInsert == null || dmlInsert.ColumnsForUpdateOrInsert.Count == 0)
                throw new ExceptionEvent(enumExceptionEventCodes.NullOrEmptyParameter
                            , "Cant build insert dbcommand with no columns");

            // start with the statement
            StringBuilder sqlInsertVars = new StringBuilder();
            StringBuilder sqlInsertVals = new StringBuilder();
            DbParameterCollection dbParams = _database.GetSqlStringCommand(_noOpDbCommandText).Parameters;
            bool firstColumn = true;
            foreach (KeyValuePair<DbQualifiedObject<string>, object> columnUpdate in dmlInsert.ColumnsForUpdateOrInsert)
            {
                DbQualifiedObject<string> qualifiedColumn = columnUpdate.Key;
                string columnName = qualifiedColumn.DbObject;
                string bindVarName = "";

                // dbDateFunctions do not require an insert syntax
                if (columnUpdate.Value is EnumDateTimeLocale)
                    continue;
                DbFunctionStructure? fn = null;
                if (columnUpdate.Value is DbFunctionStructure)
                {
                    fn = (DbFunctionStructure)columnUpdate.Value;
                    if (fn.Value.AutoGenerate) // if it is an identity, or timestamp, or a trigger, then skip
                        continue;
                }
                else if(columnUpdate.Value is string) // parameterName
                {
                    bindVarName = BuildBindVariableName((string)columnUpdate.Value);

                    DbColumnStructure column = DbCatalogGetColumn(dmlInsert.MainTable.SchemaName
                                            , dmlInsert.MainTable.TableName
                                            , columnName);
                    // only add parameter if it does not exist in the whereCondition
                    // because it will be added in the where clause processing
                    if (!(dmlInsert._whereCondition != null && dmlInsert._whereCondition.Parameters.ContainsKey(
                            _dbProviderLib.BuildParameterName(columnName))))
                        AddNewParameterToCollection(dbParams
                                            , columnName
                                            , column.DataTypeGenericDb
                                            , column.DataTypeNativeDb
                                            , column.MaxLength
                                            , ParameterDirection.Input
                                            , DBNull.Value);
                }

                if (firstColumn)
                {
                    // if it is not autogenerated then we need to add it to the insert
                    if (!fn.HasValue || !fn.Value.AutoGenerate)
                    {
                        sqlInsertVars.AppendFormat("insert into {0}.{1} ({2}{3}"
                            , dmlInsert.MainTable.SchemaName
                            , dmlInsert.MainTable.TableName
                            , columnName
                            , Environment.NewLine);

                        // if it is not autogenerated then we need to add the function body insert values
                        sqlInsertVals.AppendFormat("values ({0}{1}"
                                , fn.HasValue && !fn.Value.AutoGenerate
                                    ? fn.Value.FunctionBody : bindVarName
                                , Environment.NewLine);

                        firstColumn = false;
                    }
                }
                else
                {
                    // if it is not autogenerated then we need to add it to the insert
                    if (!fn.HasValue || !fn.Value.AutoGenerate)
                    {
                        sqlInsertVars.AppendFormat(", {0}{1}", columnName, Environment.NewLine);
                        // if there is a functionbody, we need to add it otherwise add a bind variable
                        sqlInsertVals.AppendFormat(", {0}{1}"
                                    , fn.HasValue && !fn.Value.AutoGenerate
                                        ? fn.Value.FunctionBody : bindVarName
                                , Environment.NewLine);
                    }
                }
            }

            if (sqlInsertVars.Length > 0)
            {
                sqlInsertVals.AppendFormat(") {0}", Environment.NewLine);
                sqlInsertVars.AppendFormat(") {0}", Environment.NewLine);
            }

            string sqlInsert = sqlInsertVars.ToString()
                        + Environment.NewLine + sqlInsertVals.ToString() + Environment.NewLine;
           
            // return the new dbCommand
            return BuildNonQueryDbCommand(sqlInsert, dbParams);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityContext"></param>
        /// <param name="insertObject"></param>
        /// <param name="propertyDbFunctions"></param>
        /// <returns></returns>
        internal Tuple<DbCommand, QualifiedEntity> BuildInsertDbCommand(ObjectContext entityContext
            , object insertObject
            , Dictionary<string, object> propertyDbFunctions, bool getRowId = false)
        {
            ObjectParser insertParser = new ObjectParser(entityContext, insertObject, this);

            DbParameterCollection dbParams = _database.GetSqlStringCommand(_noOpDbCommandText).Parameters;

            Tuple<string, List<DbPredicateParameter>> insertSqlAndParams = insertParser.GetInsertSqlAndParams(propertyDbFunctions, getRowId);

            foreach(DbPredicateParameter param in insertSqlAndParams.Item2)
            {
                DbColumnStructure column = DbCatalogGetColumn(insertParser.QualifiedTable.SchemaName, 
                        insertParser.QualifiedTable.EntityName,
                        param.ColumnName);

                AddNewParameterToCollection(dbParams
                    , param.ParameterName
                    , column.DataTypeGenericDb
                    , column.DataTypeNativeDb
                    , column.MaxLength
                    , ParameterDirection.Input
                    , DBNull.Value);
            }

            string insertSql = insertSqlAndParams.Item1;

            if(getRowId && DatabaseType == EnumDbType.Oracle)
            {
                insertSql += string.Format(" {1}returning rowidtochar(rowid) into {0};",
                        BuildBindVariableName(Constants.ParamNewId), Environment.NewLine);

                DbParameter param = AddNewParameterToCollection(dbParams, Constants.ParamNewId, DbType.String,
                        "varchar2", 40, ParameterDirection.Output, DBNull.Value);
            }

            // return the new dbCommand
            DbCommand dbCmd = BuildNonQueryDbCommand(insertSql, dbParams);

            dbCmd.Site = new ParameterSite(insertSqlAndParams.Item2);

            return new Tuple<DbCommand,QualifiedEntity>(dbCmd, insertParser.QualifiedTable);
        }

        internal Tuple<DbCommand, QualifiedEntity> BuildInsertDbCommand(ObjectContext entityContext, object insertObject, bool getRowId)
        {
            return BuildInsertDbCommand(entityContext
                    , insertObject
                    , new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase), getRowId);
        }

        public DbCommand BuildUpdateDbCommand(ObjectContext entityContext
            , object updateObject
            , Dictionary<PropertyInfo, object> propertyDbFunctions)
        {
            ObjectParser updateParser = new ObjectParser(entityContext, updateObject, this);

            DbParameterCollection dbParams = _database.GetSqlStringCommand(_noOpDbCommandText).Parameters;

            Tuple<string, List<DbPredicateParameter>> updateSqlandParams 
                    = updateParser.GetUpdateSqlAndParams(entityContext, updateObject, propertyDbFunctions);

            foreach (DbPredicateParameter param in updateSqlandParams.Item2)
            {
                DbColumnStructure column = DbCatalogGetColumn(updateParser.QualifiedTable.SchemaName,
                        updateParser.QualifiedTable.EntityName,
                        param.ColumnName);

                AddNewParameterToCollection(dbParams
                    , param.ParameterName
                    , column.DataTypeGenericDb
                    , column.DataTypeNativeDb
                    , column.MaxLength
                    , ParameterDirection.Input
                    , param.Value);
            }
            
            // return the new dbCommand
            DbCommand cmdUpdate = BuildNonQueryDbCommand(updateSqlandParams.Item1, dbParams);
            cmdUpdate.Site = new ParameterSite(updateSqlandParams.Item2);
            return cmdUpdate;
        }

        public DbCommand BuildUpdateDbCommand(ObjectContext entityContext, object updateObject)
        {
            return BuildUpdateDbCommand(entityContext
                    , updateObject
                    , new Dictionary<PropertyInfo, object>());
        }


        public DbCommand BuildDeleteDbCommand(ObjectContext entityContext
            , object deleteObject)
        {
            ObjectParser updateParser = new ObjectParser(entityContext, deleteObject, this);

            DbParameterCollection dbParams = _database.GetSqlStringCommand(_noOpDbCommandText).Parameters;

            Tuple<string, List<DbPredicateParameter>> deleteSqlandParams
                    = updateParser.GeDeleteSqlAndParams(entityContext, deleteObject);

            foreach (DbPredicateParameter param in deleteSqlandParams.Item2)
            {
                DbColumnStructure column = DbCatalogGetColumn(updateParser.QualifiedTable.SchemaName,
                        updateParser.QualifiedTable.EntityName,
                        param.ColumnName);

                AddNewParameterToCollection(dbParams
                    , param.ParameterName
                    , column.DataTypeGenericDb
                    , column.DataTypeNativeDb
                    , column.MaxLength
                    , ParameterDirection.Input
                    , param.Value);
            }

            // return the new dbCommand
            DbCommand cmdDelete = BuildNonQueryDbCommand(deleteSqlandParams.Item1, dbParams);
            cmdDelete.Site = new ParameterSite(deleteSqlandParams.Item2);
            return cmdDelete;
        }

        private string BuildCaseStatementsForSelect(DbTableDmlMgr dmlSelect)
        {
            if(dmlSelect.CaseColumns.Count == 0)
                return "";

            StringBuilder sbCase = new StringBuilder();

            foreach(DbCase dbCase in dmlSelect.CaseColumns)
            {
                sbCase.AppendFormat("{0}{1} as {2} {3}",sbCase.Length > 0 ? "," : "", dbCase.ToString(this), 
                        dbCase.Alias, Environment.NewLine); 
            }

            return sbCase.ToString();
        }


        /// <summary>
        /// Builds a Merge DbCommand object from the given DbTableDml
        /// meta data structure.  It will initialize the DbParameter collection
        /// to what was provided in the meta data and will return a DAAB DbCommand
        /// object of command type = Text.
        /// <![CDATA[
        /// Merge command for Microsoft SQL Server:
        /// 
        /// MERGE INTO TableName AS T1
        /// USING (VALUES ( @valFld1, @valFld2 )) AS Source ( Fld1, Fld2 )
        /// ON (T1.KeyFld = @valKeyFld)
        /// WHEN MATCHED THEN
        /// UPDATE SET Fld1 = Source.Fld1, Fld2 = Source.Fld2
        /// WHEN NOT MATCHED THEN
        /// INSERT (KeyFld, Fld1, Fld2) VALUES (@valKeyFld, Source.Fld1, Source.Fld2)
        /// 
        ///
        /// Merge command for Oracle:
        /// 
        /// MERGE INTO TableName T1
        /// USING (SELECT @valFld1 Fld1, @valFld2 Fld2 FROM dual) Source
        /// ON (T1.KeyFld = @valKeyFld)
        /// WHEN MATCHED THEN
        /// UPDATE SET Fld1 = Source.Fld1, Fld2 = Source.Fld2
        /// WHEN NOT MATCHED THEN
        /// INSERT (KeyFld, Fld1, Fld2) VALUES (@valKeyFld, Source.Fld1, Source.Fld2)
        /// 
        ///
        /// Merge command for DB2:
        /// 
        /// MERGE INTO TableName AS T1
        /// USING TABLE(VALUES ( @valFld1, @valFld2 )) AS Source ( Fld1, Fld2 )
        /// ON (T1.KeyFld = @valKeyFld)
        /// WHEN MATCHED THEN
        /// UPDATE SET Fld1 = Source.Fld1, Fld2 = Source.Fld2
        /// WHEN NOT MATCHED THEN
        /// INSERT (KeyFld, Fld1, Fld2) VALUES (@valKeyFld, Source.Fld1, Source.Fld2)
        /// 
        /// ]]>
        /// </summary>
        /// <param name="dmlMerge">DbTable Dml Meta Data structure.</param>
        /// <returns>DAAB DbCommand Object with DbParameters (initialized to the values provided
        /// or DbNull.  The CommandType is Text.</returns>
        public DbCommand BuildMergeDbCommand(DbTableDmlMgr dmlMerge)
        {
            if (dmlMerge.ColumnsForUpdateOrInsert == null || dmlMerge.ColumnsForUpdateOrInsert.Count == 0)
                throw new ExceptionEvent(enumExceptionEventCodes.NullOrEmptyParameter
                            , "Cant build Merge dbcommand with no columns");

            string mergeTable = null;
            string tableAlias = null;
            StringBuilder sqlMerge = new StringBuilder();
            StringBuilder whereClause = new StringBuilder();
            DbParameterCollection dbParams = null;

            mergeTable = string.Format( "{0}.{1}",
                    dmlMerge.MainTable.SchemaName,
                    dmlMerge.MainTable.TableName );

            tableAlias = dmlMerge.GetTable( dmlMerge.MainTable.SchemaName, dmlMerge.MainTable.TableName ).TableAlias;
            dbParams = _database.GetSqlStringCommand( _noOpDbCommandText ).Parameters;

            if (dmlMerge._whereCondition != null)
                whereClause.Append( dmlMerge._whereCondition.ToString( this ) );

            if (whereClause.Length > 0)
            {
                dbParams = BuildWhereClauseParams( dmlMerge._whereCondition.Parameters.Values, dbParams );
            }

            int columnNum = 0;
            bool bColumnPresentInWhereClause;
            string columnName;
            string columnValue;
            StringBuilder updateClause = new StringBuilder();
            StringBuilder insertColumns = new StringBuilder();
            StringBuilder insertValues = new StringBuilder();
            DbQualifiedObject<string> qualifiedColumn;
            Dictionary<DbQualifiedObject<string>, object> columnsCollection = null;

            foreach (DbTableColumnType columnType in Enum.GetValues(typeof(DbTableColumnType)))
            {
                if (columnType == DbTableColumnType.None)
                    continue;

                if (columnType == DbTableColumnType.ForInsertOnly)
                    columnsCollection = dmlMerge.ColumnsForInsert;
                else if (columnType == DbTableColumnType.ForUpdateOnly)
                    columnsCollection = dmlMerge.ColumnsForUpdate;
                else
                    columnsCollection = dmlMerge.ColumnsForUpdateOrInsert;

                foreach (KeyValuePair<DbQualifiedObject<string>, object> columnMerge in columnsCollection)
                {
                    qualifiedColumn = columnMerge.Key;
                    columnName = qualifiedColumn.DbObject;
                    columnValue = "";

                    columnNum++;
                    bColumnPresentInWhereClause = false;

                    if (columnMerge.Value is DbFunctionStructure)
                    {
                        columnValue = ((DbFunctionStructure)columnMerge.Value).FunctionBody;
                    }
                    else if (columnMerge.Value is DbParameter)
                    {
                        columnValue = BuildBindVariableName( ((DbParameter)columnMerge.Value).ParameterName );
                    }
                    else if (columnMerge.Value is EnumDateTimeLocale)
                    {
                        columnValue = GetDbTimeAs( (EnumDateTimeLocale)columnMerge.Value, null );
                    }
                    else if (columnMerge.Value is DbConstValue)
                    {
                        columnValue = ((DbConstValue)columnMerge.Value).GetQuotedValue();
                    }
                    else if (columnMerge.Value is string)
                    {
                        columnValue = BuildBindVariableName( (string)columnMerge.Value );

                        DbColumnStructure column = DbCatalogGetColumn( qualifiedColumn.SchemaName
                                                        , qualifiedColumn.TableName
                                                        , columnName );

                        if (dmlMerge._whereCondition != null && dmlMerge._whereCondition.Parameters.ContainsKey(
                                _dbProviderLib.BuildParameterName( columnName ) ))
                        {
                            bColumnPresentInWhereClause = true;
                        }
                        else
                        {
                            AddNewParameterToCollection( dbParams
                                                , (string)columnMerge.Value
                                                , column.DataTypeGenericDb
                                                , column.DataTypeNativeDb
                                                , column.MaxLength
                                                , ParameterDirection.Input
                                                , DBNull.Value );
                        }
                    }

                    if (columnType != DbTableColumnType.ForUpdateOnly)
                    {
                        insertColumns.AppendFormat( "{0}{1}"
                                , columnNum > 1 ? string.Format( "{0}, ", Environment.NewLine ) : ""
                                , columnName );

                        insertValues.AppendFormat( "{0}{1}"
                            , columnNum > 1 ? string.Format( "{0}, ", Environment.NewLine ) : ""
                            , columnValue );
                    }

                    if (columnType != DbTableColumnType.ForInsertOnly)
                    {
                        if (!bColumnPresentInWhereClause)
                        {
                            updateClause.AppendFormat( "{0}{1} = {2}"
                                    , updateClause.Length > 0 ? string.Format( "{0}, ", Environment.NewLine ) : "UPDATE SET "
                                    , columnName, columnValue );
                        }
                    }
                }
            }

            if (DatabaseType == EnumDbType.Oracle)
            {
                sqlMerge.AppendFormat( "MERGE INTO {0} {1}{2}", mergeTable, tableAlias, Environment.NewLine );
                sqlMerge.AppendFormat( "USING (SELECT 1 DummyCol FROM dual) Source{0}", Environment.NewLine );
            }
            else
            {
                sqlMerge.AppendFormat( "MERGE INTO {0} AS {1}{2}", mergeTable, tableAlias, Environment.NewLine );
                sqlMerge.AppendFormat( "USING (VALUES ( 1 )) AS Source ( DummyCol ){0}", Environment.NewLine );
            }

            sqlMerge.AppendFormat( "ON ({0}){1}", whereClause.ToString(), Environment.NewLine );

            if (updateClause.Length > 0)
            {
                sqlMerge.AppendFormat( "WHEN MATCHED THEN{0}{1}{2}", Environment.NewLine, updateClause, Environment.NewLine );
            }

            if (insertColumns.Length > 0)
            {
                sqlMerge.AppendFormat( "WHEN NOT MATCHED THEN{0} INSERT ( {1} ){2}VALUES ( {3} )",
                        Environment.NewLine, insertColumns, Environment.NewLine, insertValues );
            }

            sqlMerge.AppendFormat( "{0};{1}", Environment.NewLine, Environment.NewLine );

            // return the new dbCommand
            return BuildNonQueryDbCommand(sqlMerge.ToString(), dbParams);
        }

        private string BuildJoinClause(DbTableDmlMgr joinMgr)
        {
            StringBuilder joinClause = new StringBuilder();

            foreach(var tableList in joinMgr.Tables.Values)
                foreach(DbTableJoin join in tableList)
                {
                    joinClause.AppendFormat("{0}{1} {2}.{3} {4}", Environment.NewLine, 
                            DbTableJoin.GetJoinStringFromType(join.JoinType),
                            join.SchemaName, join.TableName, join.TableAlias);

                    if(join.JoinPredicate == null 
                        && (join.JoinType != DbTableJoinType.None
                            && join.JoinType != DbTableJoinType.Cross))
                        throw new ExceptionEvent(enumExceptionEventCodes.InvalidParameterValue, "Join does not have predicate");

                    if(join.JoinPredicate != null)
                        joinClause.AppendFormat(" ON {0}", join.JoinPredicate.ToString(this));
                }

            return joinClause.ToString();
        }

        private string BuildWhereClause(Dictionary<string, object> ColumnValues)
        {
            StringBuilder whereClause = new StringBuilder();
            foreach (string columnName in ColumnValues.Keys)
            {
                string whereOperator = "="; // default operator is equality
                bool includeNullCheck = false;
                string columnValue = BuildBindVariableName(columnName);

                // is is a non equality comparison
                if (ColumnValues[columnName] is DbComparisonOperatorStructure)
                {
                    DbComparisonOperatorStructure comparOp = (DbComparisonOperatorStructure)ColumnValues[columnName];
                    includeNullCheck = comparOp.IncludeNullCheck;
                    if (comparOp.OperatorEnum == ComparisonOperatorEnum.In
                        || comparOp.OperatorEnum == ComparisonOperatorEnum.NotIn)
                    {
                        StringBuilder inValues = new StringBuilder();
                        foreach (DbBindVariableStructure bv in comparOp.OperatorValues)
                            inValues.AppendFormat("{0}{1}", inValues.Length > 0 ? ", " : "( "
                                , bv.VariableOrConst);
                        inValues.Append(" )");
                        columnValue = inValues.ToString();
                        whereOperator = comparOp.OperatorString;
                    }
                    else if (comparOp.OperatorEnum == ComparisonOperatorEnum.Between
                        || comparOp.OperatorEnum == ComparisonOperatorEnum.NotBetween)
                    {
                        StringBuilder betweenValues = new StringBuilder();
                        foreach (DbBindVariableStructure bv in comparOp.OperatorValues)
                            betweenValues.AppendFormat("{0}{1}", betweenValues.Length > 0 ? " and " : ""
                                , bv.VariableOrConst);
                        columnValue = betweenValues.ToString();
                        whereOperator = comparOp.OperatorString;
                    }
                    else
                    {
                        whereOperator = comparOp.OperatorString;
                        if (comparOp.OperatorValues != null)
                            columnValue = comparOp.OperatorValues[0].VariableOrConst;
                    }
                }

                // if a dbFunction, then value is function body provided
                if (ColumnValues[columnName] is DbFunctionStructure)
                    columnValue = ((DbFunctionStructure)ColumnValues[columnName]).FunctionBody;

                // if a bind variable, then value is the variable defined
                if (ColumnValues[columnName] is DbBindVariableStructure)
                    columnValue = ((DbBindVariableStructure)ColumnValues[columnName]).VariableOrConst;

                // build the where clause
                if (includeNullCheck)   // should this include a null comparison
                    whereClause.AppendFormat("{0}{1}(({2} is null and {3} is null) or ({2} is not null and {2} {4} {3})) "
                        , Environment.NewLine
                        , whereClause.Length > 0 ? string.Format("{0} and ", Environment.NewLine) : "where "
                        , columnName
                        , columnValue
                        , whereOperator);
                // do not check for null
                else whereClause.AppendFormat("{0}{1} {2} {3}"
                        , whereClause.Length > 0 ? string.Format("{0} and ", Environment.NewLine) : "where "
                        , columnName
                        , whereOperator
                        , columnValue);
            }

            return whereClause.ToString();
        }

        internal DbParameterCollection BuildWhereClauseParams(string SchemaName
                    , string TableName
                    , Dictionary<string, object> ColumnValues)
        {
            return BuildWhereClauseParams(null, SchemaName, TableName, ColumnValues);
        }

        internal DbParameterCollection BuildWhereClauseParams(DbParameterCollection DbParams
                    , string SchemaName
                    , string TableName
                    , Dictionary<string, object> ColumnValues)
        {
            foreach (string columnName in ColumnValues.Keys)
            {
                string paramName = columnName;
                object columnValue = ColumnValues[columnName];
                // is is a non equality comparison
                if (ColumnValues[columnName] is DbComparisonOperatorStructure)
                {
                    DbComparisonOperatorStructure comparOp = (DbComparisonOperatorStructure)ColumnValues[columnName];
                    if (comparOp.OperatorEnum == ComparisonOperatorEnum.In
                        || comparOp.OperatorEnum == ComparisonOperatorEnum.NotIn)
                    {
                        foreach (DbBindVariableStructure bv in comparOp.OperatorValues)
                            if (!string.IsNullOrEmpty(bv.BindToColumnType))
                            {
                                DbColumnStructure column = DbCatalogGetColumn(SchemaName
                                                    , TableName
                                                    , bv.BindToColumnType);
                                if (DbParams == null)
                                    DbParams = CreateNewParameterAndCollection(bv.VariableOrConst
                                                        , column.DataTypeGenericDb
                                                        , column.DataTypeNativeDb
                                                        , column.MaxLength
                                                        , ParameterDirection.Input
                                                        , DBNull.Value);
                                else AddNewParameterToCollection(DbParams
                                                        , bv.VariableOrConst
                                                        , column.DataTypeGenericDb
                                                        , column.DataTypeNativeDb
                                                        , column.MaxLength
                                                        , ParameterDirection.Input
                                                        , DBNull.Value);
                            }
                        continue;   // we can skip to next column
                    }
                    else if (comparOp.OperatorEnum == ComparisonOperatorEnum.Between
                        || comparOp.OperatorEnum == ComparisonOperatorEnum.NotBetween)
                    {
                        StringBuilder betweenValues = new StringBuilder();
                        foreach (DbBindVariableStructure bv in comparOp.OperatorValues)
                            if (!string.IsNullOrEmpty(bv.BindToColumnType))
                            {
                                DbColumnStructure column = DbCatalogGetColumn(SchemaName
                                                    , TableName
                                                    , bv.BindToColumnType);

                                if (DbParams == null)
                                    DbParams = CreateNewParameterAndCollection(bv.VariableOrConst
                                                        , column.DataTypeGenericDb
                                                        , column.DataTypeNativeDb
                                                        , column.MaxLength
                                                        , ParameterDirection.Input
                                                        , DBNull.Value);
                                else AddNewParameterToCollection(DbParams
                                                        , bv.VariableOrConst
                                                        , column.DataTypeGenericDb
                                                        , column.DataTypeNativeDb
                                                        , column.MaxLength
                                                        , ParameterDirection.Input
                                                        , DBNull.Value);
                            }
                        continue;   // we can skip to next column
                    }
                    else if (comparOp.OperatorValues != null
                                && !string.IsNullOrEmpty(comparOp.OperatorValues[0].BindToColumnType))
                    {
                        columnValue = comparOp.OperatorValues[0].VariableOrConst;
                    }
                    else if (comparOp.OperatorValues == null
                            || string.IsNullOrEmpty(comparOp.OperatorValues[0].BindToColumnType))
                        continue;
                }

                // if a dbFunction, then value is function body provided
                if (ColumnValues[columnName] is DbFunctionStructure)
                    continue;

                if (ColumnValues[columnName] is DbBindVariableStructure)
                {
                    DbBindVariableStructure bvs = (DbBindVariableStructure)ColumnValues[columnName];
                    if (!string.IsNullOrEmpty(bvs.BindToColumnType))
                    {
                        columnValue = DBNull.Value;
                        paramName = bvs.VariableOrConst;
                    }
                }

                {
                    DbColumnStructure column = DbCatalogGetColumn(SchemaName, TableName, paramName);
                    if (DbParams == null)
                        DbParams = CreateNewParameterAndCollection(paramName
                                            , column.DataTypeGenericDb
                                            , column.DataTypeNativeDb
                                            , column.MaxLength
                                            , ParameterDirection.Input
                                            , columnValue);
                    else AddNewParameterToCollection(DbParams
                                            , paramName
                                            , column.DataTypeGenericDb
                                            , column.DataTypeNativeDb
                                            , column.MaxLength
                                            , ParameterDirection.Input
                                            , columnValue);
                }
            }
            return DbParams;
        }

        private DbParameterCollection BuildWhereClauseParams(IEnumerable<DbPredicateParameter> parameters, 
                DbParameterCollection dbParams = null)
        {
            if(dbParams == null)
                dbParams = _database.GetSqlStringCommand(_noOpDbCommandText).Parameters;

            foreach (DbPredicateParameter parameter in parameters)
            {
                if(parameter.Paramater != null)
                    dbParams.Add(parameter.Paramater);
                else if(parameter.ColumnName != null && parameter.TableName != null)
                {
                    DbColumnStructure column = DbCatalogGetColumn(parameter.SchemaName
                                                    , parameter.TableName
                                                    , parameter.ColumnName);

                    AddNewParameterToCollection(dbParams
                                        , parameter.ParameterName
                                        , column.DataTypeGenericDb
                                        , column.DataTypeNativeDb
                                        , column.MaxLength
                                        , ParameterDirection.Input
                                        , parameter.Value == null ? DBNull.Value : parameter.Value);
                }
            }
         
            return dbParams;
        }

        void BuildUniqueIdCommands()
        {
            DbCommand dbCmd = _database.GetStoredProcCommand(Constants.USP_UniqueIdsGetNextBlock);
            DiscoverParameters(dbCmd, true);
            _internalDbCmdCache.Add(Constants.USP_UniqueIdsGetNextBlock, dbCmd);

            DataTable sequences = ExecuteDataSet(BuildSelectDbCommand(Constants.SQL_Select_UniqueIds, null), null, null).Tables[0];
            foreach (DataRow sequence in sequences.Rows)
            {
                string key = sequence[Constants.UniqueIdKey].ToString();
                UInt32 cacheBlock = Convert.ToUInt32(sequence[Constants.CacheBlockSize]);
                UniqueIdCacheInfo uci = new UniqueIdCacheInfo();
                uci.CacheBlockSize = cacheBlock;
                uci.UniqueIdBlockRemaining = 0;
                uci.UniqueIdKey = key;
                _uniqueIdCache.Set(key, uci);
            }

            // build the dbCommand params
            DbColumnStructure seqKey = DbCatalogGetColumn(Constants.SCHEMA_CORE, Constants.TABLE_UniqueIds, Constants.UniqueIdKey);
            DbColumnStructure seqVal = DbCatalogGetColumn(Constants.SCHEMA_CORE, Constants.TABLE_UniqueIds, Constants.UniqueIdValue);
            DbParameterCollection dbParams = CreateNewParameterAndCollection(BuildParamName(Constants.UniqueIdValue, true)
                                                    , DbType.Int64
                                                    , seqVal.DataTypeNativeDb
                                                    , seqVal.MaxLength
                                                    , ParameterDirection.Input
                                                    , DBNull.Value);
            AddNewParameterToCollection(dbParams
                                                , Constants.UniqueIdKey
                                                , DbType.String
                                                , seqKey.DataTypeNativeDb
                                                , seqKey.MaxLength
                                                , ParameterDirection.Input
                                                , DBNull.Value);
            AddNewParameterToCollection(dbParams
                                                , Constants.UniqueIdValue
                                                , DbType.Int64
                                                , seqVal.DataTypeNativeDb
                                                , seqVal.MaxLength
                                                , ParameterDirection.Input
                                                , DBNull.Value);

            // build the dbCommand from the DynamicSQL FormatString for returning cached ids
            dbCmd = BuildNonQueryDbCommand(FormatSQLStringToDynamicSQL(dbParams
                                                                , Constants.SQL_Update_UniqueIds_SetUniqueId)
                                                            , dbParams);
            _internalDbCmdCache.Add(Constants.SQL_Update_UniqueIds_SetUniqueId, dbCmd);

            // build the dbCommand from the DynamicSQL FormatString for setting Max and Rollover Id values
            dbParams = CreateNewParameterAndCollection(BuildParamName(Constants.MaxIdValue)
                                                    , DbType.Int64
                                                    , seqVal.DataTypeNativeDb
                                                    , seqVal.MaxLength
                                                    , ParameterDirection.Input
                                                    , DBNull.Value);
            AddNewParameterToCollection(dbParams
                                                , Constants.RolloverIdValue
                                                , DbType.Int64
                                                , seqVal.DataTypeNativeDb
                                                , seqVal.MaxLength
                                                , ParameterDirection.Input
                                                , DBNull.Value);
            AddNewParameterToCollection(dbParams
                                                , Constants.UniqueIdKey
                                                , DbType.String
                                                , seqKey.DataTypeNativeDb
                                                , seqKey.MaxLength
                                                , ParameterDirection.Input
                                                , DBNull.Value);

            dbCmd = BuildNonQueryDbCommand(FormatSQLStringToDynamicSQL(dbParams
                                                                , Constants.SQL_Update_UniqueIds_SetMaxAndRolloverId)
                                                            , dbParams);

            _internalDbCmdCache.Add(Constants.SQL_Update_UniqueIds_SetMaxAndRolloverId, dbCmd);

            DbTableDmlMgr dmlSelect = DbCatalogGetTableDmlMgr(Constants.SCHEMA_CORE, Constants.TABLE_UniqueIds);
            dmlSelect.SetWhereCondition( (j) => j.Column(Constants.TABLE_UniqueIds, Constants.UniqueIdKey) == 
                    j.Parameter(Constants.TABLE_UniqueIds, Constants.UniqueIdKey, BuildParamName(Constants.UniqueIdKey)));
            dbCmd = BuildSelectDbCommand(dmlSelect, null);
            _internalDbCmdCache.Add(dmlSelect.MainTable.FullyQualifiedName, dbCmd);
        }

        /// <summary>
        /// Stores the given dbCmd into a cache associated with the given key
        /// NOTE: The cacheKey cannot be empty or null.
        /// In addition, if cacheKey already exists in cache, it will overwrite the existing item.
        /// If not, it will add it to the cache.
        /// </summary>
        /// <param name="cacheKey">String to asociate dbCommand with</param>
        /// <param name="dbCmd">The dbCommand object</param>
        public void DbCmdCacheSet(string cacheKey, DbCommand dbCmd)
        {
            _externalDbCmdCache.Set(cacheKey, dbCmd);
        }

        /// <summary>
        /// Function to create a DbCommand from the given commandText and add the array of
        /// DbParameters to the new dbCommand.  Once the DbCommand is created, then it is
        /// added to the DbCommandCache.
        /// NOTE: The cacheKey and commandText cannot be empty or null.
        /// In addition, if cacheKey already exists in cache, it will overwrite the existing item.
        /// If not, it will add it to the cache.
        /// </summary>
        /// <param name="cacheKey">String that will be used to retrieve the commandText dbCommand later</param>
        /// <param name="commandText">A valid DbCommand command text</param>
        /// <param name="parameterSet">Array of DbParameters to add to the new DbCommand</param>
        public void DbCmdCacheSet(string cacheKey
                        , string commandText
                        , params DbParameter[] parameterSet)
        {
            if (!string.IsNullOrEmpty(cacheKey))
            {
                if (!string.IsNullOrEmpty(commandText))
                {
                    DbCommand dbCmd = _database.GetSqlStringCommand(commandText);
                    if (parameterSet != null)
                        foreach (DbParameter dbParam in parameterSet)
                            dbCmd.Parameters.Add(dbParam);
                    DbCmdCacheSet(cacheKey, dbCmd);
                }
                else throw new ExceptionEvent(enumExceptionEventCodes.NullOrEmptyParameter
                        , "commandText cannot be null or empty.");
            }
            else throw new ExceptionEvent(enumExceptionEventCodes.NullOrEmptyParameter
                    , "cacheKey cannot be null or empty.");
        }

        /// <summary>
        /// Will discover the parameters of the given stored procedure name and store
        /// them in the cache under the cache key
        /// </summary>
        /// <param name="cacheKey">Unique key to lookup procedure later</param>
        /// <param name="storedProcedure">Fully qualified stored procedure name</param>
        public void DbCmdCacheSet(string cacheKey
                        , string storedProcedure)
        {
            if (!string.IsNullOrEmpty(cacheKey))
            {
                if (!string.IsNullOrEmpty(storedProcedure))
                {
                    DbCommand dbCmd = Database.GetStoredProcCommand(storedProcedure);
                    DbCmdCacheSet(cacheKey, dbCmd);
                }
                else throw new ExceptionEvent(enumExceptionEventCodes.NullOrEmptyParameter
                        , "commandText cannot be null or empty.");
            }
            else throw new ExceptionEvent(enumExceptionEventCodes.NullOrEmptyParameter
                    , "StoredProcedure cannot be null or empty.");
        }


        /// <summary>
        /// Returns the DbCommand associated with the given cacheKey
        /// NOTE: The cacheKey cannot be empty or null and it must exist in the Dbcommand Cache
        /// </summary>
        /// <param name="cacheKey">String pointing to the cached DbCommand</param>
        /// <returns>A clone of the cached DbCommand</returns>
        public DbCommand DbCmdCacheGet(string cacheKey)
        {
            if (!string.IsNullOrEmpty(cacheKey))
                if (_externalDbCmdCache.Exists(cacheKey))
                    return CloneDbCommand((DbCommand)_externalDbCmdCache.Get(cacheKey));
                else throw new ExceptionEvent(enumExceptionEventCodes.InvalidParameterValue
                        , string.Format("The cacheKey: {0} could not be found in the DbCommand cache.", cacheKey));
            else throw new ExceptionEvent(enumExceptionEventCodes.NullOrEmptyParameter
                    , "cacheKey cannot be null or empty.");
        }

        /// <summary>
        /// Returns a clone of the DbCommand that is either found in the cache
        /// or created using the given function.
        /// NOTE: The cacheKey cannot be empty or null
        /// </summary>
        /// <param name="cacheKey">String pointing to the cached DbCommand</param>
        /// <param name="createDbCmdFunc"></param>
        /// <returns>A clone of the cached DbCommand</returns>
        public DbCommand DbCmdCacheGetOrAdd(string cacheKey, Func<DataAccessMgr, DbCommand> createDbCmdFunc)
        {
            if (!string.IsNullOrEmpty(cacheKey))
                return CloneDbCommand((DbCommand)_externalDbCmdCache.GetOrAdd(cacheKey, () => createDbCmdFunc(this)));
            else throw new ExceptionEvent(enumExceptionEventCodes.NullOrEmptyParameter
                    , "cacheKey cannot be null or empty.");
        }

        /// <summary>
        /// Removes the dbCommand that is associated with the given cacheKey from the cache
        /// NOTE: The cacheKey cannot be empty or null.
        /// </summary>
        /// <param name="cacheKey">The key that points to the item to remove</param>
        /// <returns>Boolean indicated whether or not the item was found and removed.</returns>
        public bool DbCmdCacheRemove(string cacheKey)
        {
            if (!string.IsNullOrEmpty(cacheKey))
                return _externalDbCmdCache.Remove(cacheKey);
            else throw new ExceptionEvent(enumExceptionEventCodes.NullOrEmptyParameter
                    , "cacheKey cannot be null or empty.");
        }

        /// <summary>
        /// Returns a bool indicating whether the given cacheKey exists in the DbCommand cache.
        /// NOTE: The cacheKey cannot be empty or null.
        /// </summary>
        /// <param name="cacheKey">The key that points to the item to check</param>
        /// <returns>Boolean indicated whether or not the item was found.</returns>
        public bool DbCmdCacheExists(string cacheKey)
        {
            if (!string.IsNullOrEmpty(cacheKey))
                return _externalDbCmdCache.Exists(cacheKey);
            else throw new ExceptionEvent(enumExceptionEventCodes.NullOrEmptyParameter
                    , "cacheKey cannot be null or empty.");
        }


#endregion

        #region Parameter Methods

        /// <summary>
        /// Method used to retrieve the value of an out parameter referred to 
        /// by the given parameter name.
        /// NOTE: Numeric Return Value from Oracle Driver (ODP.NET) must be 
        /// Cast to OracleDecimal, then to .Net Decimal before they can be converted
        /// by the caller.
        /// </summary>
        /// <param name="dbCommand">DbCommand object</param>
        /// <param name="paramName">The name of the parameter to test</param>
        /// <returns>The out param's value as an object</returns>
        public object GetOutParamValue(DbCommand dbCommand, string paramName)
        {
            return _dbProviderLib.GetOutParamValue(dbCommand, paramName);
        }

        /// <summary>
        /// With Oracle ODP.NET, we must call built in function IsNull instead of comparing to DBNull.Value
        /// This method provides a consistent interface for testing out params for null
        /// </summary>
        /// <param name="dbCommand">DbCommand object</param>
        /// <param name="paramName">The name of the parameter to test</param>
        /// <returns>Boolean indicating if the parameter's value is null</returns>
        public bool IsOutParamValueNull(DbCommand dbCommand, string paramName)
        {
            return _dbProviderLib.IsOutParamValueNull(dbCommand, paramName);
        }

        /// <summary>
        /// Returns the Data Access Application Block's dataType for the given
        /// database dataType.
        /// </summary>
        /// <param name="nativeDataType">Database specific dataType</param>
        /// <returns>Data Access Application Block DataType equivalent</returns>
        public DbType GetGenericDbTypeFromNativeDataType(string nativeDataType)
        {
            return _dbProviderLib.GetGenericDbTypeFromNativeDataType(nativeDataType);
        }

        /// <summary>
        /// Returns the Data Access Application Block's dataType for the given
        /// database numeric dataType.
        /// </summary>
        /// <param name="nativeDataType">Database specific dataType</param>
        /// <param name="size">Numeric size of the dataType</param>
        /// <param name="scale">Numeric scale of the dataType</param>
        /// <returns>Data Access Application Block DataType equivalent</returns>
        public DbType GetGenericDbTypeFromNativeDataType(string nativeDataType, Int16 size, Int16 scale)
        {
            return _dbProviderLib.GetGenericDbTypeFromNativeDataType(nativeDataType, size, scale);
        }

        /// <summary>
        /// Returns the .Net dataType for the given
        /// database numeric dataType.
        /// </summary>
        /// <param name="nativeDataType">.Net specific dataType</param>
        /// <param name="size">Numeric size of the dataType</param>
        /// <param name="scale">Numeric scale of the dataType</param>
        /// <returns>.Net DataType equivalent</returns>
        public string GetDotNetDataTypeFromNativeDataType(string nativeDataType, Int16 size, Int16 scale)
        {
            return _dbProviderLib.GetDotNetDataTypeFromNativeDataType(nativeDataType, size, scale);
        }

        /// <summary>
        /// Returns the .Net dataType for the given
        /// database dataType.
        /// </summary>
        /// <param name="nativeDataType">.Net specific dataType</param>
        /// <returns>.Net DataType equivalent</returns>
        public string GetDotNetDataTypeFromNativeDataType(string nativeDataType)
        {
            return _dbProviderLib.GetDotNetDataTypeFromNativeDataType(nativeDataType);
        }

        /// <summary>
        /// Method to populate the given DbCmd with parameter definitions and
        /// depending on given boolean will remove the Return Value parameter if it exists.
        /// </summary>
        /// <param name="dbCmd">DbCommand Object</param>
        /// <param name="removeReturnValue">Indicates whether or not to remove the returnValue parameter</param>
        public void DiscoverParameters(DbCommand dbCmd, bool removeReturnValue)
        {
            if (dbCmd.CommandType == CommandType.StoredProcedure)
            {
                string cacheKey = dbCmd.CommandText + removeReturnValue.ToString();
                if (!_externalDbCmdCache.Exists(cacheKey))
                {
                    _dbProviderLib.DeriveParameters(dbCmd);
                    DbParameterCollection paramCollection = dbCmd.Parameters;
                    string returnValueParam = Constants.Return_Value;
                    foreach (DbParameter dbParam in paramCollection)
                    {
                        dbParam.Value = DBNull.Value;
                        if (dbParam.Direction == ParameterDirection.ReturnValue)
                            returnValueParam = dbParam.ParameterName;
                    }

                    if (removeReturnValue
                        && dbCmd.Parameters.Contains(BuildParamName(returnValueParam)))
                        dbCmd.Parameters.Remove(dbCmd.Parameters[BuildParamName(returnValueParam)]);

                    _externalDbCmdCache.Set(cacheKey, dbCmd);
                }
                else
                {
                    DbCommand cachedCmd = DbCmdCacheGet(cacheKey);
                    foreach (DbParameter dbParam in cachedCmd.Parameters)
                        if (dbCmd.Parameters.Contains(dbParam.ParameterName))
                            continue;
                        else dbCmd.Parameters.Add(dbParam);
                }
            }
        }


        /// <summary>
        /// Method to populate the given DbCmd with parameter definitions.
        /// </summary>
        /// <param name="dbCmd">DbCommand object</param>
        public void DiscoverParameters(DbCommand dbCmd)
        {
            DiscoverParameters(dbCmd, false);
        }

        /// <summary>
        /// Discovers the parameters for the given stored procedure name and indicator as 
        /// to whether or not to remove the ReturnValue parameter.
        /// </summary>
        /// <param name="procedureName">Fully qualified procedure name</param>
        /// <param name="removeReturnValue">Indicates whether to remove ReturnValue parameter</param>
        /// <returns>DbParameterCollection of paramters</returns>
        public DbParameterCollection DiscoverParameters(string procedureName, bool removeReturnValue)
        {
            DbCommand dbCmd = _database.GetStoredProcCommand(procedureName);
            DiscoverParameters(dbCmd, removeReturnValue);
            return CloneDbCommand(dbCmd).Parameters;
        }

        /// <summary>
        /// Creates a new parameter collection (from a new empty DbCommand)
        /// and adds a newly created parameter to the collection.
        /// </summary>
        /// <param name="paramName">Name of the new parameter.</param>
        /// <param name="paramType">DbType of the new parameter.</param>
        /// <param name="nativeDbType">DbType of the new parameter at the native database level e.g. Varchar instead of string.</param>
        /// <param name="maxLength">Maximum length of the new parameter (for strings).</param>
        /// <param name="paramDirection">Direction of the new parameter.</param>
        /// <param name="paramValue">Value of the new parameter.</param>
        /// <returns>Returns the newly create parameter collection containing the newly created parameter.</returns>
        public DbParameterCollection CreateNewParameterAndCollection(string paramName
            , DbType paramType
            , string nativeDbType
            , Int32 maxLength
            , ParameterDirection paramDirection
            , object paramValue)
        {
            DbParameterCollection parameterCollection
                = _database.GetSqlStringCommand(_noOpDbCommandText).Parameters;    // create a new set of parameters
            AddNewParameterToCollection(parameterCollection
                                , paramName
                                , paramType
                                , nativeDbType
                                , maxLength
                                , paramDirection
                                , paramValue);
            return parameterCollection;
        }

        /// <summary>
        /// Function will create a copy of the given DbParameter and add it to the
        /// given collection.
        /// </summary>
        /// <param name="dbParameters">A collection of DbParameters.</param>
        /// <param name="dbParam">A DbParameter.</param>
        /// <returns>Returns the given parameter.</returns>
        internal DbParameter CopyParameterToCollection(DbParameterCollection dbParameters
            , DbParameter dbParam)
        {
            if (dbParameters.Contains(dbParam.ParameterName))
                throw new ExceptionEvent(enumExceptionEventCodes.DbParameterExistsInCollection
                            , string.Format("Parameter: {0} already belongs to this collection; use Set to change value."
                                , dbParam.ParameterName));
            return _dbProviderLib.CopyParameterToCollection(dbParameters, dbParam);
        }


        /// <summary>
        /// Function will create a new parameter with the given properties
        /// and add it to the given collection.
        /// </summary>
        /// <param name="dbParameters">A collection of DbParameters.</param>
        /// <param name="paramName">Name of the new parameter (WITHOUT ANY BACKEND SPECIFIC SYMBOL; e.g. @).</param>
        /// <param name="paramType">DbType of the new parameter.</param>
        /// <param name="nativeDbType">NativeDbType of the new parameter at backend database e.g. varchar instead of string.</param>
        /// <param name="maxLength">Maximum length of the new parameter (for strings).</param>
        /// <param name="paramDirection">Direction of the new parameter.</param>
        /// <param name="paramValue">Value of the new parameter.</param>
        /// <returns>Returns the newly created parameter.</returns>
        public DbParameter AddNewParameterToCollection(DbParameterCollection dbParameters
            , string paramName
            , DbType paramType
            , string nativeDbType
            , Int32 maxLength
            , ParameterDirection paramDirection
            , object paramValue)
        {
            return _dbProviderLib.CopyParameterToCollection(dbParameters
                        , _dbProviderLib.CreateNewParameter(paramName
                            , paramType
                            , nativeDbType
                            , maxLength
                            , paramDirection
                            , paramValue));
        }

        /// <summary>
        /// Function will create a new parameter with the given properties
        /// </summary>
        /// <param name="paramName">Name of the new parameter.</param>
        /// <param name="paramType">DbType of the new parameter.</param>
        /// <param name="nativeDbType">NativeDbType of the new parameter at backend database e.g. varchar instead of string.</param>
        /// <param name="maxLength">Maximum length of the new parameter (for strings).</param>
        /// <param name="paramDirection">Direction of the new parameter.</param>
        /// <param name="paramValue">Value of the new parameter.</param>
        /// <returns>Returns the newly created parameter.</returns>
        public DbParameter CreateParameter(string paramName
            , DbType paramType
            , string nativeDbType
            , Int32 maxLength
            , ParameterDirection paramDirection
            , object paramValue)
        {
            return _dbProviderLib.CreateNewParameter(paramName
                    , paramType
                    , nativeDbType
                    , maxLength
                    , paramDirection
                    , paramValue);
        }



        internal DbParameterCollection CloneParameterCollection(DbParameterCollection dbParameters)
        {
            return _dbProviderLib.CloneParameterCollection(dbParameters);
        }

        /// <summary>
        /// Returns the proper parameter name based upon back end db type.
        /// For commands that Set a Value only where its current value is a specific value
        /// e.g. Set x = 1 where x = 2
        /// We have to name 1 of the parameters differently, we have chosen the SetParam (NewValue)
        /// If IsNewValueParam is true, we will use a special suffix
        /// NOTE: For SQLServer this is the same as BindVariable, but not so for oracle.
        /// </summary>
        /// <param name="variableName">ColumnName or VariableName to become
        /// a parameter (WITHOUT ANY BACKEND SPECIFIC SYMBOL; e.g. @)</param>
        /// <param name="isNewValueParam">Indicates whether is part of a Set clause and a Where clause</param>
        /// <returns>string representation of the back-end specific parameter</returns>
        public string BuildParamName(string variableName, bool isNewValueParam)
        {
            return isNewValueParam ? _dbProviderLib.BuildParameterName(variableName + Constants.ParamSetValueSuffix)
                                    : _dbProviderLib.BuildParameterName(variableName);
        }

        /// <summary>
        /// Returns the proper parameter name based upon back end db type.
        /// NOTE: For SQLServer this is the same as BindVariable, but not so for oracle.
        /// </summary>
        /// <param name="variableName">ColumnName or VariableName to become a parameter
        ///  (WITHOUT ANY BACKEND SPECIFIC SYMBOL; e.g. @)</param>
        /// <returns>string representation of the back-end specific parameter</returns>
        public string BuildParamName(string variableName)
        {
            return BuildParamName(variableName, false);
        }

        /// <summary>
        /// Returns a proper bind variable name based upon back end db type.
        /// NOTE: For SQLServer this is the same as ParamName, but not so for oracle.
        /// </summary>
        /// <param name="fieldName"> ColumnName or VariableName to become a parameter
        /// (WITHOUT ANY BACKEND SPECIFIC SYMBOL; e.g. @)</param>
        /// <returns>Bind variable name for back-end stored procedure or function</returns>
        public string BuildBindVariableName(string fieldName)
        {
            return _dbProviderLib.BuildBindVariableName(fieldName);
        }

#endregion

        #region Database Catalog Methods (Data Dictionary)
 
        /// <summary>
        /// Returns the DbColumn (database catalog data) for the given 
        /// database table name, Schema and, Table.
        /// </summary>
        /// <param name="schemaName"></param>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <returns>Database Catalog Meta Data for a column of table Structure</returns>
        public DbColumnStructure DbCatalogGetColumn(string schemaName, string tableName, string columnName)
        {
            return _dbCatalogMgr.GetDbColumn(schemaName, tableName, columnName);
        }


        /// <summary>
        /// Returns the DbTable (database catalog data) for the given 
        /// database table name.
        /// </summary>
        /// <param name="schemaName"></param>
        /// <param name="tableName"></param>
        /// <returns>Database Catalog Meta Data for a table Structure</returns>
        public DbTableStructure DbCatalogGetTable(string schemaName, string tableName)
        {
            return _dbCatalogMgr.GetDbTable(schemaName, tableName);
        }

        /// <summary>
        /// Returns the DbTable (database catalog data) for the given 
        /// fully qualified database table name.
        /// </summary>
        /// <param name="fullyQualifiedTableName">SchemaName.TableName</param>
        /// <returns>Database Catalog Meta Data for a table Structure</returns>
        public DbTableStructure DbCatalogGetTable(string fullyQualifiedTableName)
        {
            return _dbCatalogMgr.GetDbTable(fullyQualifiedTableName);
        }
        
        /// <summary>
        /// Returns a DbTableDmlMgr class used for defining Dynamic sql. The meta data of the table that is passed in 
        /// is included in the instance.
        /// </summary>
        /// <param name="schemaName"></param>
        /// <param name="tableName"></param>
        /// <param name="selectColumns">Columns to include in a select, if this will be used for a select.
        /// If non are included, all will be returned for a select.</param>
        /// <returns>Meta Data structure (with empty collection structures) to be used for building dynamic sql></returns>
        public DbTableDmlMgr DbCatalogGetTableDmlMgr(string schemaName, string tableName, params object[] selectColumns)
        {
            DbTableStructure tableStructure = _dbCatalogMgr.GetDbTable(schemaName, tableName);

            return new DbTableDmlMgr(tableStructure, selectColumns);
        }

        /// <summary>
        /// Returns a DbTableDmlMgr class used for defining Dynamic sql. The meta data of the table that is passed in 
        /// is included in the instance.
        /// </summary>
        /// <param name="fullyQualifiedTableName">SchemaName.TableName</param>
        /// <param name="selectColumns">Columns to include in a select, if this will be used for a select.
        /// If non are included, all will be returned for a select.</param>
        /// <returns>Meta Data structure (with empty collection structures) to be used for building dynamic sql></returns>
        public DbTableDmlMgr DbCatalogGetTableDmlMgr(string fullyQualifiedTableName, params object[] selectColumns)
        {
            DbTableStructure tableStructure = _dbCatalogMgr.GetDbTable(fullyQualifiedTableName);

            return new DbTableDmlMgr(tableStructure, selectColumns);
        }

        #endregion

        #region Sequencing, UniqueIds

        /// <summary>
        /// Method to return the Next Unique Number for the given Key
        /// and a BlockSize.  The block size allows the caller to retrieve
        /// a contiguous sequence of numbers (in a single operation).  
        /// <para>The returned value is the high limit of the sequence of numbers.
        /// The caller can just sequence descending for the given block size of numbers 
        /// to obtain each unique number.</para>
        /// <para>For example:  If current number = 27 and caller asks for blocksize = 50
        /// , then 77 will be returned.  Caller can use 77, 76, 75, ... 29, 28
        /// </para>
        /// <para>MaxIdValue is used to control the limit of a unique id. When RolloverIdValue 
        /// is given, that value is returned with MaxIdValue is exceeded; otherwise and exception
        /// is raised.</para>
        /// </summary>
        /// <param name="uniqueIdKey">The key that will point to the value</param>
        /// <param name="blockSize">The number of values to allocate continguously</param>
        /// <param name="MaxIdValue">The maximum value that can be returned</param>
        /// <param name="RolloverIdValue">The value to return once MaxIdValue has been exceeded.</param>
        /// <returns>The high value of the block size</returns>
        /// <remarks>The unique ids are stored in a database table: Core.UniqueIds
        /// <para>Each key is given its own row and can have its own value, cacheBlockSize, maxValue, and rolloverValue</para>
        /// </remarks>
        public Int64 GetNextUniqueId(string uniqueIdKey
                , UInt32 blockSize
                , Int64? MaxIdValue = null
                , Int64? RolloverIdValue = null)
       {
            if (string.IsNullOrEmpty(uniqueIdKey))
                throw new ExceptionEvent(enumExceptionEventCodes.NullOrEmptyParameter, Constants.UniqueIdKey);
            if (blockSize == 0)
                throw new ExceptionEvent(enumExceptionEventCodes.InvalidParameterValue
                            , string.Format("{0} cannot be zero", Constants.CacheBlockSize));

            UniqueIdCacheInfo uci;
            // yes, now see if the key belongs to the current cache
            // we must lock the cache in case we need to add a new entry
            lock (_uniqueIdCache)
            {
                if (_uniqueIdCache.Exists(uniqueIdKey))
                    uci = (UniqueIdCacheInfo)_uniqueIdCache.Get(uniqueIdKey);
                else
                {
                    uci = new UniqueIdCacheInfo();
                    uci.UniqueIdKey = uniqueIdKey;
                    uci.UniqueIdBlockRemaining = 0;
                    // if this is the first time we have seen this key
                    // perform a dummy request to the db (BlockSize = 0)
                    // to find the default CacheBlockSize
                    Int64 newSeq = GetNextUniqueIdBlock(uniqueIdKey, 0, MaxIdValue, RolloverIdValue);
                    DbCommand dbCmd = _internalDbCmdCache.Get(Constants.SCHEMA_CORE + "." + Constants.TABLE_UniqueIds);
                    dbCmd.Parameters[BuildParamName(Constants.UniqueIdKey)].Value = uniqueIdKey;
                    DbCommandMgr dbCmdMgr = new DbCommandMgr(this);
                    dbCmdMgr.AddDbCommand(dbCmd);
                    DataTable sequenceData = dbCmdMgr.ExecuteDataTable();
                    if (sequenceData.Rows.Count > 0)
                        uci.CacheBlockSize = Convert.ToUInt32(sequenceData.Rows[0][Constants.CacheBlockSize]);
                    else uci.CacheBlockSize = 0;// default; new id keys are not cached.
                    _uniqueIdCache.Add(uniqueIdKey, uci);
                }
            }
            if (uci.CacheBlockSize != 0) // only if there is caching for this key
            {
                lock (uci)  // lets have a more granular lock on the key
                {
                    // if we have enough id numbers remaining
                    if (uci.UniqueIdBlockRemaining >= blockSize)
                    {
                        // reduce the number by the block size
                        uci.UniqueIdBlockRemaining -= blockSize;
                        // assign the number; only the head changes
                        uci.UniqueIdBlockHead += blockSize;
                    }
                    else
                    {
                        // othwerwise we need to obtain a new block
                        // which we want to get the cacheSize or the requested size
                        UInt32 blockSizeMax = blockSize >
                                                    uci.CacheBlockSize
                                                ? blockSize : uci.CacheBlockSize;
                        // get the new id block based upon the greater block size
                        Int64 newId = GetNextUniqueIdBlock(uniqueIdKey, blockSizeMax, MaxIdValue, RolloverIdValue);
                        
                        // if the new id was a rollover value then we start over
                        // by taking the new id and size
                        bool isRollover = newId <= uci.UniqueIdBlockTail;

                        // determine if there is a gap found between what our last
                        // id was (current tail) and the newly received id
                        bool gapFound = !(newId - uci.UniqueIdBlockTail == blockSizeMax);
                        // set the new tail
                        uci.UniqueIdBlockTail = newId;

                        // calc ids remaining 
                        // if it is a rollover then the remaining is the blocksize
                        if (isRollover)
                            uci.UniqueIdBlockRemaining = blockSizeMax;

                        // which is the CacheBlockSize minus the requested blocksize
                        // if there were no gaps then we can add any existing remaining ids
                        if (!gapFound)
                            uci.UniqueIdBlockRemaining += blockSizeMax - blockSize;
                        // otherwise we have to take whatever is remaining
                        else uci.UniqueIdBlockRemaining = blockSizeMax - blockSize;
                        // calculate the new head 
                        uci.UniqueIdBlockHead = uci.UniqueIdBlockTail - uci.UniqueIdBlockRemaining;
                    }

                    _uniqueIdCache.Set(uniqueIdKey, uci); // update the cache
                    // return the new id
                    return uci.UniqueIdBlockHead;
                }
            }
            return GetNextUniqueIdBlock(uniqueIdKey, blockSize, MaxIdValue, RolloverIdValue);
        }

        /// <summary>
        /// Method to return the Next Unique Number for the given Key
        /// and a BlockSize.  The block size allows the caller to retrieve
        /// a contiguous sequence of numbers (in a single operation).  
        /// <para>The returned value is the high limit of the sequence of numbers.
        /// The caller can just sequence descending for the given block size of numbers 
        /// to obtain each unique number.</para>
        /// <para>For example:  If current number = 27 and caller asks for blocksize = 50
        /// , then 77 will be returned.  Caller can use 77, 76, 75, ... 29, 28
        /// </para>
        /// </summary>
        /// <param name="uniqueIdKey">The key that will point to the value</param>
        /// <param name="blockSize">The number of values to allocate continguously</param>
        /// <returns>The high value of the block size</returns>
        /// <remarks>The unique ids are stored in a database table: Core.UniqueIds
        /// <para>Each key is given its own row and can have its own value, cacheBlockSize, maxValue, and rolloverValue</para>
        /// </remarks>
        public Int64 GetNextUniqueId(string uniqueIdKey
                , UInt32 blockSize)
        {
            return GetNextUniqueId(uniqueIdKey, blockSize, null, null);
        }

        /// <summary>
        /// Retuns a unique number that can sequences across multiple threads and processses as well as 
        /// time synchronized physical servers for the given sequence key string.
        /// <para>The sequence key corresponds to a tie-breaker value that will be significant
        /// when two threads generate a number at the same millisecond.</para>
        /// <para>For more details about the number format:</para>
        /// <para><seealso cref="B1.Core.Functions.GetSequenceNumber(System.DateTime, Int64)"/></para>
        /// <para>For more details about the tie-breaker number:</para>
        /// <para><seealso cref="B1.DataAccess.DataAccessMgr.GetNextUniqueId"/></para>
        /// </summary>
        /// <param name="sequenceKey">any string provided by the caller to associate with sequence number.</param>
        /// <returns>Unique Sequence Number that will correlate between threads and machines (e.g. 1111714372358100001)</returns>
        public Int64 GetNextSequenceNumber(string sequenceKey)
        {
            // gets the tie-breaker number for the sequenceKey
            Int32 uniqueNumber = Convert.ToInt32(GetNextUniqueId(sequenceKey, 1, 99999, 1));
            // return a formatted sequence number using the uniqueNumber for a tie-breaker
            return Functions.GetSequenceNumber(DbSynchTime, uniqueNumber);
        }

        /// <summary>
        /// Function to return any remaining cached UniqueId numbers if caching UniqueId numbers
        /// and the last obtained UniqueId value is still there (no other requests were made)
        /// NOTE: If caching UniqueId numbers is true, then this method will reset the current
        /// UniqueIdValue for ht UniqueIdKey if the Value was not changed from when it was originally
        /// retrieved.
        /// </summary>
        public void ClearUniqueIdsCache()
        {
            // now check the cache for any sequences that were used
            DbCommandMgr dbCmdMgr = new DbCommandMgr(this);
            List<UniqueIdCacheInfo> uciReturned = new List<UniqueIdCacheInfo>();
            lock (_uniqueIdCache)
            {
                foreach (string cacheKey in _uniqueIdCache.Keys)
                {
                    UniqueIdCacheInfo uci = (UniqueIdCacheInfo)_uniqueIdCache.Get(cacheKey);
                    if (uci.UniqueIdBlockRemaining != 0)  // if we have remaining, try to return them
                    {
                        DbCommand dbCmd = _internalDbCmdCache.Get(Constants.SQL_Update_UniqueIds_SetUniqueId);
                        dbCmd.Parameters[BuildParamName(Constants.UniqueIdValue)].Value = uci.UniqueIdBlockTail;
                        dbCmd.Parameters[BuildParamName(Constants.UniqueIdValue, true)].Value = uci.UniqueIdBlockHead;
                        dbCmd.Parameters[BuildParamName(Constants.UniqueIdKey)].Value = uci.UniqueIdKey;
                        dbCmdMgr.AddDbCommand(dbCmd);
                        uciReturned.Add(uci);
                    }
                }
                if (!dbCmdMgr.IsNoOpDbCommand)
                    dbCmdMgr.ExecuteNonQuery();

                // if we have successfully retuned the sequences to the db
                // clear the memory cache.
                foreach (UniqueIdCacheInfo uci in uciReturned)
                {
                    uci.UniqueIdBlockRemaining = 0;
                    _uniqueIdCache.Set(uci.UniqueIdKey, uci);
                }
            }
        }

        /// <summary>
        /// Method to return the Next Unique Number for the given Key
        /// and a BlockSize.  The block size allows the caller to retrieve
        /// a contiguous sequence of numbers (in a single operation).  
        /// <para>The returned value is the high limit of the sequence of numbers.
        /// The caller can just sequence descending for the given block size of numbers 
        /// to obtain each unique number.</para>
        /// <para>For example:  If current number = 27 and caller asks for blocksize = 50
        /// , then 77 will be returned.  Caller can use 77, 76, 75, ... 29, 28
        /// </para>
        /// </summary>
        /// <param name="uniqueIdKey">The key that will point to the value</param>
        /// <param name="blockSize">The number of values to allocate continguously</param>
        /// <returns>The high value of the block size</returns>
        /// <remarks>The unique ids are stored in a database table: Core.UniqueIds
        /// <para>Each key is given its own row and can have its own value, cacheBlockSize, maxValue, and rolloverValue</para>
        /// </remarks>
        Int64 GetNextUniqueIdBlock(string uniqueIdKey
                , UInt32 blockSize
                , Int64? MaxIdValue = null
                , Int64? RolloverIdValue = null)
        {
            if (MaxIdValue.HasValue && RolloverIdValue.HasValue && RolloverIdValue.Value > MaxIdValue.Value)
                throw new ExceptionEvent(enumExceptionEventCodes.InvalidParameterValue
                    , string.Format("RolloverIdValue: {0} cannot be greater than MaxIdvalue: {1}"
                        , MaxIdValue.Value
                        , RolloverIdValue.Value));

            DbCommand dbCmd = _internalDbCmdCache.Get(Constants.USP_UniqueIdsGetNextBlock);
            dbCmd.Parameters[BuildParamName(Constants.UniqueIdKey)].Value
                                            = uniqueIdKey;
            dbCmd.Parameters[BuildParamName(Constants.BlockAmount)].Value = blockSize;
            ExecuteNonQuery(dbCmd, null, null);
            Int64 newId = Convert.ToInt64(GetOutParamValue(dbCmd, Constants.UniqueIdValue));
            if (MaxIdValue.HasValue)
            {
                if (newId > MaxIdValue.Value)
                {
                    if (!RolloverIdValue.HasValue)
                        throw new ExceptionEvent(enumExceptionEventCodes.InvalidParameterValue
                            , string.Format("MaxIdvalue: {0} exceeded without a RolloverIdValue defined", MaxIdValue.Value));

                    dbCmd = _internalDbCmdCache.Get(Constants.SQL_Update_UniqueIds_SetMaxAndRolloverId);
                    dbCmd.Parameters[BuildParamName(Constants.UniqueIdKey)].Value
                                                    = uniqueIdKey;
                    dbCmd.Parameters[BuildParamName(Constants.MaxIdValue)].Value = MaxIdValue.Value;
                    dbCmd.Parameters[BuildParamName(Constants.RolloverIdValue)].Value = RolloverIdValue.Value;
                    ExecuteNonQuery(dbCmd, null, null);
                    return GetNextUniqueIdBlock(uniqueIdKey, blockSize, MaxIdValue.Value, null);
                }
            }
            return newId;
        }

        #endregion

        #region Execution Methods


        /// <summary>
        /// Will look for the DbCommand associated with the stored procedure
        /// in the cache; if it is not found it will create and store a DbCommand
        /// after discovering its parameters.  It will then set the param values
        /// and execute the DbCommand.
        /// If a DbException is raised and a logger class had been provided,
        /// the method will attempt to Log a debug text version of the dbCommand
        /// that is backend specific or just log the exception.
        /// In either case, the exception will be thrown.        
        /// NOTE: storedProcedure cannot be null or empty.
        /// </summary>
        /// <param name="storedProcedure">Fully qualified stored procedure name</param>
        /// <param name="dbTrans">A valid DbTransaction or null</param>
        /// <param name="parameterNameValues">A set of parameter names and values or null. 
        /// Example: "FirstName", "Ernest", "LastName", "Hemingway"</param>
        /// <returns>The return value of the Execute</returns>
        public int ExecuteNonQuery(string storedProcedure
                        , DbTransaction dbTrans
                        , params object[] parameterNameValues)
        {
             DbCommand dbCommand;
             if (!string.IsNullOrEmpty(storedProcedure))
                 dbCommand = _database.GetStoredProcCommand(storedProcedure);
             else throw new ExceptionEvent(enumExceptionEventCodes.NullOrEmptyParameter
                     , "storedProcedure cannot be null or empty.");
            DiscoverParameters(dbCommand, true);
            return ExecuteNonQuery(dbCommand, dbTrans, parameterNameValues);
        }

        /// <summary>
        /// Executes the given DbCommand object after setting the given parameter values.
        /// If a DbException is raised and a logger class had been provided,
        /// the method will attempt to Log a debug text version of the dbCommand
        /// that is backend specific or just log the exception.
        /// In either case, the exception will be thrown.        
        /// </summary>
        /// <param name="dbCommand">DbCommand object</param>
        /// <param name="dbTrans">A valid DbTransaction or null</param>
        /// <param name="parameterNameValues">A set of parameter names and values or null. 
        /// Example: "FirstName", "Ernest", "LastName", "Hemingway"</param>
        /// <returns>The return value of the Execute</returns>
        public int ExecuteNonQuery(DbCommand dbCommand
                                , DbTransaction dbTrans
                                , params object[] parameterNameValues)
        {
            try
            {
                UpdateParameterValues(dbCommand);

                SetParameterValues(dbCommand, parameterNameValues);

                // dbCmdDebug will not have any runtime overhead and is used only when you are debugging
                // and there is an exception executing the dbCommand.
                // Then if you would like to see a formatted representation of the SQL with parameter declarataions
                // (except binary objects unfortunately), then right click on the dbCmdDebug object.
                // there is a property that will return a formatted string.  
                DbCommandDebug dbCmdDebug = new DbCommandDebug(dbCommand, _dbProviderLib.GetCommandDebugScript);

                if (dbTrans != null)
                    return _database.ExecuteNonQuery(dbCommand, dbTrans);
                else return _database.ExecuteNonQuery(dbCommand);
            }
            catch (Exception e)
            {
                // create a new exception event object and log it when loggingMgr is available
                // always throw new event to caller.
                throw CreateAndLogExceptionEvent(e, dbCommand);
            }
        }


        /// Returns a collection of type T based upon the results of the given DbCommand query.
        /// </summary>
        /// <typeparam name="T">Type to create</typeparam>
        /// <param name="dbCommand">DAAB DbCommand object for select</param>
        /// <param name="dbTrans">Database transaction object or null</param>
        /// <param name="parameterNameValues">A set of parameter names and values or null. 
        /// Example: "FirstName", "Ernest", "LastName", "Hemingway"</param>
        /// <returns>Collection of Type T</returns>
        public IEnumerable<T> ExecuteCollection<T>(ObjectContext context
                        , params EntityState[] entityStates) where T : new()
        {
            List<T> collection = new List<T>();
            foreach (EntityState entityState in entityStates)
                foreach (ObjectStateEntry ose in context.ObjectStateManager.GetObjectStateEntries(entityState))
                    collection.Add((T)ose.Entity);
            return collection;
        }

        /// Returns a collection of type T based upon the results of the given DbCommand query.
        /// </summary>
        /// <typeparam name="T">Type to create</typeparam>
        /// <param name="dbCommand">DAAB DbCommand object for select</param>
        /// <param name="dbTrans">Database transaction object or null</param>
        /// <param name="parameterNameValues">A set of parameter names and values or null. 
        /// Example: "FirstName", "Ernest", "LastName", "Hemingway"</param>
        /// <returns>Collection of Type T</returns>
        public IEnumerable<T> ExecuteCollection<T>(DbCommand dbCommand
                        , DbTransaction dbTrans
                        , params object[] parameterNameValues) where T : new()
        {
            return ExecuteCollection<T>(dbCommand, dbTrans, null, parameterNameValues);
        }

        /// <summary>
        /// Returns a collection of type T based upon the results of the given DbCommand query.
        /// </summary>
        /// <typeparam name="T">Type to create</typeparam>
        /// <param name="dbCommand">DAAB DbCommand object for select</param>
        /// <param name="dbTrans">Database transaction object or null</param>
        /// <param name="dataReaderHandler">An optional function accepting a datareader, dictionary of 
        /// properties for type T, an object context and entity set name, will populate an IEnumerable of T.
        /// To allow programmer to write custom handler.
        /// <para>If null, a default handler will be used.</para></param>
        /// <param name="parameterNameValues">A set of parameter names and values or null. 
        /// Example: "FirstName", "Ernest", "LastName", "Hemingway"</param>
        /// <returns>Collection of Type T</returns>
        public IEnumerable<T> ExecuteCollection<T>(DbCommand dbCommand
                        , DbTransaction dbTrans
                        , Func<IDataReader, List<KeyValuePair<int, System.Reflection.PropertyInfo>>,
                                IEnumerable<T>> dataReaderHandler
                        , params object[] parameterNameValues) where T : new()
        {
            UpdateParameterValues(dbCommand);

            // dbCmdDebug will not have any runtime overhead and is used only when you are debugging
            // and there is an exception executing the dbCommand.
            // Then if you would like to see a formatted representation of the SQL with parameter declarataions
            // (except binary objects unfortunately), then right click on the dbCmdDebug object.
            // there is a property that will return a formatted string.  
            DbCommandDebug dbCmdDebug = new DbCommandDebug(dbCommand, _dbProviderLib.GetCommandDebugScript);
            using (IDataReader rdr = ExecuteReader(dbCommand, dbTrans, parameterNameValues))
            {
                // Loop throught the columns in the resultset and lookup the properties and ordinals 
                List<KeyValuePair<int, System.Reflection.PropertyInfo>> props =
                        new List<KeyValuePair<int, System.Reflection.PropertyInfo>>();
                Type t = typeof(T);
                for (int i = 0; i < rdr.FieldCount; i++)
                {
                    string fieldName = rdr.GetName(i);
                    // Ignore case of the property name
                    System.Reflection.PropertyInfo pinfo = t.GetProperties()
                        .Where(p => p.Name.ToLower() == fieldName.ToLower()).FirstOrDefault();
                    if (pinfo != null)
                        props.Add(new KeyValuePair<int, System.Reflection.PropertyInfo>(i, pinfo));
                }

                if(dataReaderHandler == null)
                {
                    List<T> items = new List<T>();
                    while (rdr.Read())
                    {
                        
                        T obj = new T();
                        props.ForEach(kv => kv.Value.SetValue(obj, 
                                GetValueOrNull(Convert.ChangeType(rdr.GetValue(kv.Key), kv.Value.PropertyType)), null));
                        items.Add(obj);
                    }
                    return items;
                }
                else
                    return dataReaderHandler(rdr, props);
            }
        }


        /// <summary>
        /// Returns a collection of type T based upon the results of the given DbCommand query.
        /// <para>This function will update the given ObjectContext for the given entity set name and 
        /// will change the state of the context for the entity to unchanged when successful.</para>
        /// </summary>
        /// <typeparam name="T">Type to create</typeparam>
        /// <param name="dbCommand">DAAB DbCommand object for select</param>
        /// <param name="dbTrans">Database transaction object or null</param>
        /// <param name="context">An Entity Framework context object that will be affected</param>
        /// <param name="entitySetName">The entity set name within the context object to affect changes</param>
        /// <param name="dataReaderHandler">An optional function accepting a datareader, dictionary of 
        /// properties for type T, an object context and entity set name, will populate an IEnumerable of T and
        /// update the object context.  To allow programmer to write custom handler.
        /// <para>If null, a default handler will be used.</para></param>
        /// <param name="parameterNameValues">A set of parameter names and values or null. 
        /// Example: "FirstName", "Ernest", "LastName", "Hemingway"</param>
        /// <returns>Collection of Type T</returns>
        public IEnumerable<T> ExecuteContext<T>(DbCommand dbCommand
                        , DbTransaction dbTrans
                        , ObjectContext context
                        , Func<IDataReader, List<KeyValuePair<int, System.Reflection.PropertyInfo>>
                                , ObjectContext
                                , string 
                                , IEnumerable<T>> dataReaderHandler
                        , params object[] parameterNameValues) where T : new()
        {
            UpdateParameterValues(dbCommand);

            // dbCmdDebug will not have any runtime overhead and is used only when you are debugging
            // and there is an exception executing the dbCommand.
            // Then if you would like to see a formatted representation of the SQL with parameter declarataions
            // (except binary objects unfortunately), then right click on the dbCmdDebug object.
            // there is a property that will return a formatted string.  
            DbCommandDebug dbCmdDebug = new DbCommandDebug(dbCommand, _dbProviderLib.GetCommandDebugScript);
            using (IDataReader rdr = ExecuteReader(dbCommand, dbTrans, parameterNameValues))
            {
                // Loop throught the columns in the resultset and lookup the properties and ordinals 
                List<KeyValuePair<int, System.Reflection.PropertyInfo>> props =
                        new List<KeyValuePair<int, System.Reflection.PropertyInfo>>();
                Type t = typeof(T);
                for (int i = 0; i < rdr.FieldCount; i++)
                {
                    string fieldName = rdr.GetName(i);
                    // Ignore case of the property name
                    System.Reflection.PropertyInfo pinfo = t.GetProperties()
                        .Where(p => p.Name.ToLower() == fieldName.ToLower()).FirstOrDefault();
                    if (pinfo != null)
                        props.Add(new KeyValuePair<int, System.Reflection.PropertyInfo>(i, pinfo));
                }

                string entitySetName = ObjectParser.GetEntitySetName(context, typeof(T));

                if (dataReaderHandler == null)
                {
                    List<T> items = new List<T>();
                    while (rdr.Read())
                    {

                        T obj = new T();
                        props.ForEach(kv => kv.Value.SetValue(obj,
                                GetValueOrNull(Convert.ChangeType(rdr.GetValue(kv.Key), kv.Value.PropertyType)), null));
                        items.Add(obj);
                        
                        context.AttachTo(entitySetName, obj);
                        context.ObjectStateManager.ChangeObjectState(obj, EntityState.Unchanged);
                    }
                    return items;
                }
                else
                    return dataReaderHandler(rdr, props, context, entitySetName);
            }
        }


        /// <summary>
        /// Returns a collection of type T based upon the results of the given DbCommand query.
        /// <para>This function will update the given ObjectContext for the given entity set name and 
        /// will change the state of the context for the entity to unchanged when successful.</para>
        /// </summary>
        /// <typeparam name="T">Type to create</typeparam>
        /// <param name="dbCommand">DAAB DbCommand object for select</param>
        /// <param name="dbTrans">Database transaction object or null</param>
        /// <param name="context">An Entity Framework context object that will be affected</param>
        /// <param name="entitySetName">The entity set name within the context object to affect changes</param>
        /// properties for type T, will populate an IEnumerable of T. 
        /// If null, an IEnumerable of T will be generated in the order recieved by the datareader</param>
        /// <param name="parameterNameValues">A set of parameter names and values or null. 
        /// Example: "FirstName", "Ernest", "LastName", "Hemingway"</param>
        /// <returns>Collection of Type T</returns>
        public IEnumerable<T> ExecuteContext<T>(DbCommand dbCommand
                , DbTransaction dbTrans
                , ObjectContext context
                , params object[] parameterNameValues) where T : new()
        {
            return ExecuteContext<T>(dbCommand, dbTrans, context, null, parameterNameValues);
        }

        /// <summary>
        /// If an object is DBNull will return null, otherwise returns the object.
        /// </summary>
        /// <param name="dbValue"></param>
        /// <returns></returns>
        public static object GetValueOrNull(object dbValue)
        {
            return dbValue != DBNull.Value ? dbValue : null;
        }

        /// <summary>
        /// Will look for the DbCommand associated with the stored procedure
        /// in the cache; if it is not found it will create and store a DbCommand
        /// after discovering its parameters.  It will then set the param values
        /// and execute the DbCommand.  It will not rename any of the dataset's datatables.
        /// If a DbException is raised and a logger class had been provided,
        /// the method will attempt to Log a debug text version of the dbCommand
        /// that is backend specific or just log the exception.
        /// In either case, the exception will be thrown.        
        /// NOTE: storedProcedure cannot be null or empty.
        /// </summary>
        /// <param name="storedProcedure">Fully qualified stored procedure name</param>
        /// <param name="dbTrans">A valid DbTransaction or null</param>
        /// <param name="parameterNameValues">A set of parameter names and values or null. 
        /// Example: "FirstName", "Ernest", "LastName", "Hemingway"</param>
        /// <returns>The dataset of the DbCommand execution</returns>
        public DataSet ExecuteDataSet(string storedProcedure
                        , DbTransaction dbTrans
                        , params object[] parameterNameValues)
        {
            DbCommand dbCmd;
            if (!string.IsNullOrEmpty(storedProcedure))
                dbCmd = _database.GetStoredProcCommand(storedProcedure);
            else throw new ExceptionEvent(enumExceptionEventCodes.NullOrEmptyParameter
                    , "storedProcedure cannot be null or empty.");
            DiscoverParameters(dbCmd, true);
                
            return ExecuteDataSet(dbCmd, dbTrans, null, parameterNameValues);
        }

        /// <summary>
        /// Executes the given DbCommand object after setting the given parameter values.
        /// If a DbException is raised and a logger class had been provided,
        /// the method will attempt to Log a debug text version of the dbCommand
        /// that is backend specific or just log the exception.
        /// In either case, the exception will be thrown.        
        /// </summary>
        /// <param name="dbCommand">DbCommand object</param>
        /// <param name="dbTrans">A valid DbTransaction or null</param>
        /// <param name="tableNames">An arrary names to rename the dataset's tables</param>
        /// <param name="parameterNameValues">A set of parameter names and values or null. 
        /// Example: "FirstName", "Ernest", "LastName", "Hemingway"</param>
        /// <returns>The dataset of the DbCommand execution</returns>
        public DataSet ExecuteDataSet(DbCommand dbCommand
                        , DbTransaction dbTrans
                        , List<string> tableNames
                        , params object[] parameterNameValues)
        {
            try
            {
                UpdateParameterValues(dbCommand);

                SetParameterValues(dbCommand, parameterNameValues);

                // dbCmdDebug will not have any runtime overhead and is used only when you are debugging
                // and there is an exception executing the dbCommand.
                // Then if you would like to see a formatted representation of the SQL with parameter declarataions
                // (except binary objects unfortunately), then right click on the dbCmdDebug object.
                // there is a property that will return a formatted string.  
                DbCommandDebug dbCmdDebug = new DbCommandDebug(dbCommand, _dbProviderLib.GetCommandDebugScript);

                DataSet returnValue = null;     
                if (dbTrans != null)
                    returnValue = _database.ExecuteDataSet(dbCommand, dbTrans);
                else returnValue = _database.ExecuteDataSet(dbCommand);

                if (tableNames != null)
                    if (tableNames.Count != returnValue.Tables.Count)
                        throw new ExceptionEvent(enumExceptionEventCodes.DataSetTableNamesMismatchWithResultSet
                                        , string.Format("The TableNames count: {0} provided did not match with the table "
                                            + "count returned: {1} in the dataset"
                                                , tableNames.Count, returnValue.Tables.Count));
                    else
                    {
                        int tableCount = 0;
                        foreach (DataTable table in returnValue.Tables)
                            table.TableName = tableNames[tableCount++];
                    }
                return returnValue;
            }
            catch (Exception e)
            {
                // create a new exception event object and log it when loggingMgr is available
                // always throw new event to caller.
                throw CreateAndLogExceptionEvent(e, dbCommand);
            }
        }


        /// <summary>
        /// Will look for the DbCommand associated with the stored procedure
        /// in the cache; if it is not found it will create and store a DbCommand
        /// after discovering its parameters.  It will then set the param values
        /// and execute the DbCommand.
        /// If a DbException is raised and a logger class had been provided,
        /// the method will attempt to Log a debug text version of the dbCommand
        /// that is backend specific or just log the exception.
        /// In either case, the exception will be thrown.        
        /// NOTE: storedProcedure cannot be null or empty.
        /// </summary>
        /// <param name="storedProcedure">Fully qualified stored procedure name</param>
        /// <param name="dbTrans">A valid DbTransaction or null</param>
        /// <param name="parameterNameValues">A set of parameter names and values or null. 
        /// Example: "FirstName", "Ernest", "LastName", "Hemingway"</param>
        /// <returns>The return value of the Execute</returns>
        public object ExecuteScalar(string storedProcedure
                        , DbTransaction dbTrans
                        , params object[] parameterNameValues)
        {
            DbCommand dbCommand;
            if (!string.IsNullOrEmpty(storedProcedure))
                dbCommand = _database.GetStoredProcCommand(storedProcedure);
            else throw new ExceptionEvent(enumExceptionEventCodes.NullOrEmptyParameter
                    , "storedProcedure cannot be null or empty.");
            DiscoverParameters(dbCommand, true);

            return ExecuteScalar(dbCommand, dbTrans, parameterNameValues);
        }

        /// <summary>
        /// Executes the given DbCommand object after setting the given parameter values.
        /// If a DbException is raised and a logger class had been provided,
        /// the method will attempt to Log a debug text version of the dbCommand
        /// that is backend specific or just log the exception.
        /// In either case, the exception will be thrown.        
        /// </summary>
        /// <param name="dbCommand">DbCommand object</param>
        /// <param name="dbTrans">A valid DbTransaction or null</param>
        /// <param name="parameterNameValues">A set of parameter names and values or null. 
        /// Example: "FirstName", "Ernest", "LastName", "Hemingway"</param>
        /// <returns>The return value of the Execute</returns>
        public object ExecuteScalar(DbCommand dbCommand
                                , DbTransaction dbTrans
                        , params object[] parameterNameValues)
        {
            try
            {
                SetParameterValues(dbCommand, parameterNameValues);

                // dbCmdDebug will not have any runtime overhead and is used only when you are debugging
                // and there is an exception executing the dbCommand.
                // Then if you would like to see a formatted representation of the SQL with parameter declarataions
                // (except binary objects unfortunately), then right click on the dbCmdDebug object.
                // there is a property that will return a formatted string.  
                DbCommandDebug dbCmdDebug = new DbCommandDebug(dbCommand, _dbProviderLib.GetCommandDebugScript);

                if (dbTrans != null)
                    return _database.ExecuteScalar(dbCommand, dbTrans);
                else return _database.ExecuteScalar(dbCommand);
            }
            catch (Exception e)
            {
                // create a new exception event object and log it when loggingMgr is available
                // always throw new event to caller.
                throw CreateAndLogExceptionEvent(e, dbCommand);
            }
        }

        /// <summary>
        /// Will look for the DbCommand associated with the stored procedure
        /// in the cache; if it is not found it will create and store a DbCommand
        /// after discovering its parameters.  It will then set the param values
        /// and execute the DbCommand.
        /// If a DbException is raised and a logger class had been provided,
        /// the method will attempt to Log a debug text version of the dbCommand
        /// that is backend specific or just log the exception.
        /// In either case, the exception will be thrown.        
        /// NOTE: storedProcedure cannot be null or empty.
        /// A dataReader requires that a connection remain open and there is no
        /// control over whether the client using the reader will close it.
        /// So it is recommended to use the overload function ExecuteReader which
        /// accepts a function as a parameter.  Then the function consumes the dataReader
        /// and the ExecuteReader function closes the dataReader.
        /// </summary>
        /// <param name="storedProcedure">Fully qualified stored procedure name</param>
        /// <param name="dbTrans">A valid DbTransaction or null</param>
        /// <param name="parameterNameValues">A set of parameter names and values or null. 
        /// Example: "FirstName", "Ernest", "LastName", "Hemingway"</param>
        /// <returns>A data reader object</returns>
        public IDataReader ExecuteReader(string storedProcedure
                        , DbTransaction dbTrans
                        , params object[] parameterNameValues)
        {
            DbCommand dbCommand;
            if (!string.IsNullOrEmpty(storedProcedure))
                dbCommand = _database.GetStoredProcCommand(storedProcedure);
            else throw new ExceptionEvent(enumExceptionEventCodes.NullOrEmptyParameter
                    , "storedProcedure cannot be null or empty.");
            DiscoverParameters(dbCommand, true);

            return ExecuteReader(dbCommand, dbTrans, parameterNameValues);
        }

        /// <summary>
        /// Executes the given DbCommand object after setting the given parameter values.
        /// If a DbException is raised and a logger class had been provided,
        /// the method will attempt to Log a debug text version of the dbCommand
        /// that is backend specific or just log the exception.
        /// In either case, the exception will be thrown.        
        /// NOTE:
        /// A dataReader requires that a connection remain open and there is no
        /// control over whether the client using the reader will close it.
        /// So it is recommended to use the overload function ExecuteReader which
        /// accepts a function as a parameter.  Then the function consumes the dataReader
        /// and the ExecuteReader function closes the dataReader.
        /// </summary>
        /// <param name="dbCommand">DbCommand object</param>
        /// <param name="dbTrans">A valid DbTransaction or null</param>
        /// <param name="parameterNameValues">A set of parameter names and values or null. 
        /// Example: "FirstName", "Ernest", "LastName", "Hemingway"</param>
        /// <returns>A data reader object</returns>
        public IDataReader ExecuteReader(DbCommand dbCommand
                                , DbTransaction dbTrans
                        , params object[] parameterNameValues)
        {
            try
            {
                UpdateParameterValues(dbCommand);

                SetParameterValues(dbCommand, parameterNameValues);

                // dbCmdDebug will not have any runtime overhead and is used only when you are debugging
                // and there is an exception executing the dbCommand.
                // Then if you would like to see a formatted representation of the SQL with parameter declarataions
                // (except binary objects unfortunately), then right click on the dbCmdDebug object.
                // there is a property that will return a formatted string.  
                DbCommandDebug dbCmdDebug = new DbCommandDebug(dbCommand, _dbProviderLib.GetCommandDebugScript);

                if (dbTrans != null)
                    return _database.ExecuteReader(dbCommand, dbTrans);
                else return _database.ExecuteReader(dbCommand);
            }
            catch (Exception e)
            {
                // create a new exception event object and log it when loggingMgr is available
                // always throw new event to caller.
                throw CreateAndLogExceptionEvent(e, dbCommand);
            }
        }

        /// <summary>
        /// Will look for the DbCommand associated with the stored procedure
        /// in the cache; if it is not found it will create and store a DbCommand
        /// after discovering its parameters.  It will then set the param values
        /// and execute the DbCommand.
        /// NOTE: storedProcedure cannot be null or empty.
        /// It returns the result of the dataReaderHandler 
        /// function delegate.  It will be given the DataReader and after its execution,
        /// the DataReader will be destroyed. This prevents from caller to have an active
        /// DataReader where they can leave a connection open. If there are errors in the
        /// delegate functions, the datareader is still closed.
        /// If a DbException is raised and a logger class had been provided,
        /// the method will attempt to Log a debug text version of the dbCommand
        /// that is backend specific or just log the exception.
        /// In either case, the exception will be thrown.        
        /// </summary>
        /// <param name="storedProcedure">Fully qualified stored procedure name</param>
        /// <param name="dataReaderHandler">Delegate which will be called to consume the DataReader.</param>
        /// <param name="dbTrans">A valid DbTransaction or null</param>
        /// <param name="parameterNameValues">A set of parameter names and values or null. 
        /// Example: "FirstName", "Ernest", "LastName", "Hemingway"</param>
        /// <returns>A data reader object</returns>
        public T ExecuteReader<T>(string storedProcedure
                        , DbTransaction dbTrans
                        , Func<IDataReader, T> dataReaderHandler
                        , params object[] parameterNameValues)
        {
            DbCommand dbCommand;
            if (!string.IsNullOrEmpty(storedProcedure))
                dbCommand = _database.GetStoredProcCommand(storedProcedure);
            else throw new ExceptionEvent(enumExceptionEventCodes.NullOrEmptyParameter
                    , "storedProcedure cannot be null or empty.");
            DiscoverParameters(dbCommand, true);

            return ExecuteReader(dbCommand, dbTrans, dataReaderHandler, parameterNameValues);
        }

        /// <summary>
        /// Executes the given DbCommand and returns the result of the dataReaderHandler 
        /// function delegate.  It will be given the DataReader and after its execution,
        /// the DataReader will be destroyed. This prevents from caller to have an active
        /// DataReader where they can leave a connection open. If there are errors in the
        /// delegate functions, the datareader is still closed.
        /// If a DbException is raised and a logger class had been provided,
        /// the method will attempt to Log a debug text version of the dbCommand
        /// that is backend specific or just log the exception.
        /// In either case, the exception will be thrown.
        /// </summary>
        /// <param name="dbCommand">Database Command Object to execute.</param>
        /// <param name="dataReaderHandler">Delegate which will be called to consume the DataReader.</param>
        /// <param name="dbTrans">A valid DbTransaction or null</param>
        /// <param name="parameterNameValues">A set of parameter names and values or null. 
        /// Example: "FirstName", "Ernest", "LastName", "Hemingway"</param>
        /// <returns>Returns the result of the DataReaderConsumerFunction.</returns>      
        public T ExecuteReader<T>(DbCommand dbCommand
            , DbTransaction dbTrans
            , Func<IDataReader, T> dataReaderHandler
            , params object[] parameterNameValues)
        {
            try
            {

                // dbCmdDebug will not have any runtime overhead and is used only when you are debugging
                // and there is an exception executing the dbCommand.
                // Then if you would like to see a formatted representation of the SQL with parameter declarataions
                // (except binary objects unfortunately), then right click on the dbCmdDebug object.
                // there is a property that will return a formatted string.  
                DbCommandDebug dbCmdDebug = new DbCommandDebug(dbCommand, _dbProviderLib.GetCommandDebugScript);

                using (IDataReader dbRdr = (IDataReader)ExecuteReader(dbCommand
                        , dbTrans
                        , parameterNameValues))
                {
                    return (T)dataReaderHandler(dbRdr);
                }
            }
            catch (Exception e)
            {
                // create a new exception event object and log it when loggingMgr is available
                // always throw new event to caller.
                throw CreateAndLogExceptionEvent(e, dbCommand);
            }
        }


        /// <summary>
        /// Will look for the DbCommand associated with the stored procedure
        /// in the cache; if it is not found it will create and store a DbCommand
        /// after discovering its parameters.  It will then set the param values
        /// and execute the DbCommand.
        /// If a DbException is raised and a logger class had been provided,
        /// the method will attempt to Log a debug text version of the dbCommand
        /// that is backend specific or just log the exception.
        /// In either case, the exception will be thrown.        
        /// NOTE: storedProcedure cannot be null or empty.
        /// An xmlReader requires that a connection remain open and there is no
        /// control over whether the client using the reader will close it.
        /// So it is recommended to use the overload function ExecuteXmlReader which
        /// accepts a function as a parameter.  Then the function consumes the xmlReader
        /// and the ExecuteReader function closes the xmlReader.
        /// </summary>
        /// <param name="storedProcedure">Fully qualified stored procedure name</param>
        /// <param name="dbTrans">A valid DbTransaction or null</param>
        /// <param name="parameterNameValues">A set of parameter names and values or null. 
        /// Example: "FirstName", "Ernest", "LastName", "Hemingway"</param>
        /// <returns>An XmlReader object</returns>
        public XmlReader ExecuteXmlReader(string storedProcedure
                        , DbTransaction dbTrans
                        , params object[] parameterNameValues)
        {
            DbCommand dbCommand;
            if (!string.IsNullOrEmpty(storedProcedure))
                dbCommand = _database.GetStoredProcCommand(storedProcedure);
            else throw new ExceptionEvent(enumExceptionEventCodes.NullOrEmptyParameter
                    , "storedProcedure cannot be null or empty.");
            DiscoverParameters(dbCommand, true);

            return ExecuteXmlReader(dbCommand, dbTrans, parameterNameValues);
        }

        /// <summary>
        /// Executes the given DbCommand object after setting the given parameter values.
        /// If a DbException is raised and a logger class had been provided,
        /// the method will attempt to Log a debug text version of the dbCommand
        /// that is backend specific or just log the exception.
        /// In either case, the exception will be thrown.        
        /// NOTE:
        /// An xmlReader requires that a connection remain open and there is no
        /// control over whether the client using the reader will close it.
        /// So it is recommended to use the overload function ExecuteXmlReader which
        /// accepts a function as a parameter.  Then the function consumes the xmlReader
        /// and the ExecuteXmlReader function closes the xmlReader.
        /// </summary>
        /// <param name="dbCommand">DbCommand object</param>
        /// <param name="dbTrans">A valid DbTransaction or null</param>
        /// <param name="parameterNameValues">A set of parameter names and values or null. 
        /// Example: "FirstName", "Ernest", "LastName", "Hemingway"</param>
        /// <returns>A xmlReader object</returns>
        public XmlReader ExecuteXmlReader(DbCommand dbCommand
                                , DbTransaction dbTrans
                        , params object[] parameterNameValues)
        {
            try
            {
                SetParameterValues(dbCommand, parameterNameValues);

                // dbCmdDebug will not have any runtime overhead and is used only when you are debugging
                // and there is an exception executing the dbCommand.
                // Then if you would like to see a formatted representation of the SQL with parameter declarataions
                // (except binary objects unfortunately), then right click on the dbCmdDebug object.
                // there is a property that will return a formatted string.  
                DbCommandDebug dbCmdDebug = new DbCommandDebug(dbCommand, _dbProviderLib.GetCommandDebugScript);

                return _dbProviderLib.ExecuteXmlReader(dbCommand, dbTrans);
            }
            catch (Exception e)
            {
                // create a new exception event object and log it when loggingMgr is available
                // always throw new event to caller.
                throw CreateAndLogExceptionEvent(e, dbCommand);
            }
        }


        /// <summary>
        /// Will look for the DbCommand associated with the stored procedure
        /// in the cache; if it is not found it will create and store a DbCommand
        /// after discovering its parameters.  It will then set the param values
        /// and execute the DbCommand.
        /// Executes the given DbCommand and returns the result of the xmlReaderHandler 
        /// function delegate.  It will be given the DataReader and after its execution,
        /// the xmlReader will be destroyed. This prevents from caller to have an active
        /// xmlReader where they can leave a connection open. If there are errors in the
        /// delegate functions, the xmlareader is still closed.
        /// If a DbException is raised and a logger class had been provided,
        /// the method will attempt to Log a debug text version of the dbCommand
        /// that is backend specific or just log the exception.
        /// In either case, the exception will be thrown.
        /// NOTE: storedProcedure cannot be null or empty.
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storedProcedure">Fully qualified stored procedure name</param>
        /// <param name="xmlReaderHandler">Delegate which will be called to consume the xmlReader.</param>
        /// <param name="dbTrans">A valid DbTransaction or null</param>
        /// <param name="parameterNameValues">A set of parameter names and values or null. 
        /// Example: "FirstName", "Ernest", "LastName", "Hemingway"</param>
        /// <returns>Returns the result of the DataReaderConsumerFunction.</returns>      
        public T ExecuteXmlReader<T>(string storedProcedure
            , Func<XmlReader, T> xmlReaderHandler
            , DbTransaction dbTrans
            , params object[] parameterNameValues)
        {
            DbCommand dbCommand;
            if (!string.IsNullOrEmpty(storedProcedure))
                dbCommand = _database.GetStoredProcCommand(storedProcedure);
            else throw new ExceptionEvent(enumExceptionEventCodes.NullOrEmptyParameter
                    , "storedProcedure cannot be null or empty.");
            DiscoverParameters(dbCommand, true);
            return ExecuteXmlReader(dbCommand, xmlReaderHandler, dbTrans, parameterNameValues);
        }

        /// <summary>
        /// Executes the given DbCommand and returns the result of the xmlReaderHandler 
        /// function delegate.  It will be given the DataReader and after its execution,
        /// the xmlReader will be destroyed. This prevents from caller to have an active
        /// xmlReader where they can leave a connection open. If there are errors in the
        /// delegate functions, the xmlareader is still closed.
        /// If a DbException is raised and a logger class had been provided,
        /// the method will attempt to Log a debug text version of the dbCommand
        /// that is backend specific or just log the exception.
        /// In either case, the exception will be thrown.
        /// </summary>
        /// <param name="dbCommand">Database Command Object to execute.</param>
        /// <param name="xmlReaderHandler">Delegate which will be called to consume the xmlReader.</param>
        /// <param name="dbTrans">A valid DbTransaction or null</param>
        /// <param name="parameterNameValues">A set of parameter names and values or null. 
        /// Example: "FirstName", "Ernest", "LastName", "Hemingway"</param>
        /// <returns>Returns the result of the DataReaderConsumerFunction.</returns>      
        public T ExecuteXmlReader<T>(DbCommand dbCommand
            , Func<XmlReader, T> xmlReaderHandler
            , DbTransaction dbTrans
            , params object[] parameterNameValues)
        {
            try
            {
                // dbCmdDebug will not have any runtime overhead and is used only when you are debugging
                // and there is an exception executing the dbCommand.
                // Then if you would like to see a formatted representation of the SQL with parameter declarataions
                // (except binary objects unfortunately), then right click on the dbCmdDebug object.
                // there is a property that will return a formatted string.  
                DbCommandDebug dbCmdDebug = new DbCommandDebug(dbCommand, _dbProviderLib.GetCommandDebugScript);

                using (XmlReader xmlRdr = (XmlReader)ExecuteXmlReader(dbCommand
                        , dbTrans
                        , parameterNameValues))
                {
                    return (T)xmlReaderHandler(xmlRdr);
                }
            }
            catch (Exception e)
            {
                // create a new exception event object and log it when loggingMgr is available
                // always throw new event to caller.
                throw CreateAndLogExceptionEvent(e, dbCommand);
            }
        }

        /// <summary>
        /// Returns the rowcount of the given database table name
        /// </summary>
        /// <param name="tableName">Fully qualified table name</param>
        /// <returns>Number of rows found in table.</returns>
        public Int64 ExecuteRowCount(string tableName)
        {

            if (!string.IsNullOrEmpty(tableName))
            {
                string selectRowcount = "select count(*) from " + tableName;
                return Convert.ToInt64(ExecuteScalar(_database.GetSqlStringCommand(selectRowcount)
                    , (DbTransaction)null, null));
            }
            else throw new ExceptionEvent(enumExceptionEventCodes.NullOrEmptyParameter
                    , "tableName cannot be null or empty.");
        }

        /// <summary>
        /// Updates the entity in the database, and sets the object to unchanged in the context. 
        /// The DbCmdIn is optional. If it is passed in, it will be used and any parameters will be changed to the 
        /// value of the updateObject. If it is not passed in, a new DbCommand will be created with the parameters pointing
        /// to the properties of the updateObject instance.
        /// </summary>
        /// <param name="entityContext">Context to update</param>
        /// <param name="updateObject">Entity to update</param>
        /// <param name="dbTransaction">Transacition. Can be null. Ignored if NULL</param>
        /// <param name="dbCmdIn">Optional. See summary.</param>
        /// <returns></returns>
        public Tuple<ObjectContext, DbCommand> UpdateEntity(ObjectContext entityContext, object updateObject,
                DbTransaction dbTransaction = null, DbCommand dbCmdIn = null)
        {
            return UpdateEntity(entityContext, updateObject,
                    new Dictionary<PropertyInfo, object>(), dbTransaction, dbCmdIn);
        }

        /// <summary>
        /// Updates the entity in the database, and sets the object to unchanged in the context. 
        /// The DbCmdIn is optional. If it is passed in, it will be used and any parameters will be changed to the 
        /// value of the updateObject. If it is not passed in, a new DbCommand will be created with the parameters pointing
        /// to the properties of the updateObject instance.
        /// </summary>
        /// <param name="entityContext">Context to update</param>
        /// <param name="updateObject">Entity to update</param>
        /// <param name="propertyDbFunctions">DB Functions to be evaluated for that particular column</param>
        /// <param name="dbTransaction">Transacition. Can be null. Ignored if NULL</param>
        /// <param name="dbCmdIn">Optional. See summary.</param>
        /// <returns></returns>
        public Tuple<ObjectContext, DbCommand> UpdateEntity(ObjectContext entityContext, object updateObject,
                Dictionary<PropertyInfo, object> propertyDbFunctions, DbTransaction dbTransaction = null, 
                DbCommand dbCmdIn = null)
        {
            DbCommand dbCmd = null;

            if (dbCmdIn != null)
            {
                dbCmd = dbCmdIn;
                ObjectParser.RemapDbCommandParameters(dbCmd, updateObject);
            }
            else
            {
                DbCommandMgr cmdMgr = new DbCommandMgr(this);
                cmdMgr.AddDbCommand(BuildUpdateDbCommand(entityContext, updateObject, propertyDbFunctions));
                ObjectParser entity = new ObjectParser(entityContext, updateObject, this);

                object[] columns = new object[propertyDbFunctions.Count()];
                int i = 0;
                foreach (PropertyInfo pi in propertyDbFunctions.Keys)
                    columns[i++] = entity.QualifiedTable.GetColumnName(pi.Name);

                DbTableDmlMgr dmlMgr = DbCatalogGetTableDmlMgr(entity.QualifiedTable.SchemaName + "."
                        + entity.QualifiedTable.EntityName
                        , columns);

                foreach (EntityKeyMember ek in 
                        entityContext.ObjectStateManager.GetObjectStateEntry(updateObject).EntityKey.EntityKeyValues)
                {
                    string columnName = entity.QualifiedTable.GetColumnName(ek.Key);

                    string paramName = BuildBindVariableName(LinqTableMgr.BuildParamName(ek.Key
                            , new List<DbPredicateParameter>(), this));

                    Expression exp = DbPredicate.CreatePredicatePart(t => t.Column(columnName) ==
                            t.Function(paramName));
                    dmlMgr.SetOrAddWhereCondition(ExpressionType.AndAlso, exp);
                }
                dbCmd = BuildSelectDbCommand(dmlMgr, null);
                cmdMgr.AddDbCommand(dbCmd);
                dbCmd = cmdMgr.DbCommandBlock;
            }

            UpdateEntitiesFromReader(updateObject, ExecuteReader(dbCmd, dbTransaction));
            entityContext.ObjectStateManager.ChangeObjectState(updateObject, EntityState.Unchanged);

            return new Tuple<ObjectContext, DbCommand>(entityContext, dbCmd);
        }

       

        public Tuple<ObjectContext, DbCommand> InsertEntity(ObjectContext entityContext, object insertObject,
                 DbTransaction dbTransaction, DbCommand dbCmdIn = null)
        {
            return InsertEntity(entityContext, insertObject, new Dictionary<string, object>(), dbTransaction, dbCmdIn);
        }

        public Tuple<ObjectContext, DbCommand> InsertEntity(ObjectContext entityContext, object insertObject,
                Dictionary<string, object> propertyDbFunctions, DbTransaction dbTransaction, DbCommand dbCmdIn = null)
        {
            DbCommand dbCmdInsert = null;

            if(dbCmdIn != null)
            {
                dbCmdInsert = dbCmdIn;
                ObjectParser.RemapDbCommandParameters(dbCmdInsert, insertObject);
            }
            else
            {
                // Find better way to do this.
                QualifiedEntity qualifiedEntity = new ObjectParser(entityContext, insertObject, this).QualifiedTable;

                DbTableStructure table = DbCatalogGetTable(qualifiedEntity.SchemaName, qualifiedEntity.EntityName);
          
                bool bGetRowFromId = false;
                List<string> autoGeneratedColumns = new List<string>();
                List<string> selectColumns = new List<string>();
                List<string> functionColumns = propertyDbFunctions.Select(
                        kvp => qualifiedEntity.GetColumnName(kvp.Key)).ToList();
                foreach(var propColumnName in qualifiedEntity._propertyToColumnMap)
                {
                    string columnName = propColumnName.Value;
                    string propertyName = propColumnName.Key;

                    DbColumnStructure column = DbCatalogGetColumn(qualifiedEntity.SchemaName, qualifiedEntity.EntityName, 
                            columnName);

                    if(table.PrimaryKeyColumns.ContainsKey(columnName) && (column.IsAutoGenerated || column.IsComputed 
                            || propertyDbFunctions.ContainsKey(propertyName)))
                    {
                        bGetRowFromId = true;
                    }

                    if(column.IsAutoGenerated || column.IsComputed || 
                            functionColumns.Contains(columnName, StringComparer.CurrentCultureIgnoreCase))
                        selectColumns.Add(columnName);

                    if(column.IsAutoGenerated)
                        autoGeneratedColumns.Add(columnName);
                }

                bool getRowId = DatabaseType == EnumDbType.Oracle && bGetRowFromId;

                Tuple<DbCommand, QualifiedEntity> insertResult = BuildInsertDbCommand(entityContext, insertObject,
                    propertyDbFunctions, getRowId);

                dbCmdInsert = insertResult.Item1;

                // If we already have all the data, and there is nothing to select, skip select creation.
                if(selectColumns.Count > 0)
                {
                    DbTableDmlMgr dmlSelect = new DbTableDmlMgr(table, selectColumns.ToArray());
                    DbCommand dbCmdSelect = null;

                    if(bGetRowFromId)
                    {
                        if(DatabaseType == EnumDbType.Oracle)
                        {
                            string bindVarName = BuildBindVariableName(Constants.ParamNewId);

                            dmlSelect.SetWhereCondition(t => t.Function("rowid") == t.Function(bindVarName));
                        }
                        else if(DatabaseType == EnumDbType.SqlServer)
                        {
                            dbCmdInsert.CommandText += string.Format(" {1}set @{0} = SCOPE_IDENTITY();", 
                                    Constants.ParamNewId, Environment.NewLine);

                            DbParameter param = AddNewParameterToCollection(dbCmdInsert.Parameters, 
                                    Constants.ParamNewId, DbType.Int64, 
                                    "numeric(18,0)", 18, ParameterDirection.Input, DBNull.Value);
                            
                        }

                        if(DatabaseType == EnumDbType.Db2 || DatabaseType == EnumDbType.SqlServer)
                        {
                            foreach(string columnName in table.PrimaryKeyColumns.Keys)
                            {
                                string propertyName = qualifiedEntity._propertyToColumnMap.Where(
                                        kvp => kvp.Value.ToLower() == columnName.ToLower()).First().Key;

                                string paramName = null;
                                Expression exp = null;

                                if(autoGeneratedColumns.Count(c => c.ToLower() == columnName.ToLower()) > 0)
                                {
                                    if(DatabaseType == EnumDbType.Db2)
                                        paramName = "IDENTITY_VAL_LOCAL()";
                                    else
                                        paramName = BuildBindVariableName(Constants.ParamNewId);
                                }
                                else
                                    paramName = BuildBindVariableName(LinqTableMgr.BuildParamName(propertyName,
                                            new List<DbPredicateParameter>(), this));

                                
                                exp = DbPredicate.CreatePredicatePart(t => t.Column(columnName) ==
                                        t.Function(paramName));

                                dmlSelect.SetOrAddWhereCondition(ExpressionType.AndAlso, exp);
                            }
                        }
                    }
                    else // select by primary key
                    {
                        foreach(string columnName in table.PrimaryKeyColumns.Keys)
                        {
                            string propertyName = qualifiedEntity._propertyToColumnMap.Where(
                                    kvp => kvp.Value.ToLower() == columnName.ToLower()).First().Key;

                            string paramName = BuildBindVariableName(LinqTableMgr.BuildParamName(propertyName, 
                                    new List<DbPredicateParameter>(), this));
                          
                            Expression exp = DbPredicate.CreatePredicatePart( t => t.Column(columnName) ==
                                    t.Function(paramName));

                            dmlSelect.SetOrAddWhereCondition(ExpressionType.AndAlso, exp);
                        }
                    }

                    dbCmdSelect = BuildSelectDbCommand(dmlSelect, null);

                    DbCommandMgr cmdMgr = new DbCommandMgr(this);
                    cmdMgr.AddDbCommand(dbCmdInsert);
                    cmdMgr.AddDbCommand(dbCmdSelect);
                    dbCmdInsert = cmdMgr.DbCommandBlock;
                }
            }
            
            UpdateEntitiesFromReader(insertObject, ExecuteReader(dbCmdInsert, dbTransaction));

            entityContext.AttachTo(ObjectParser.GetEntitySetName(entityContext, insertObject), insertObject);

            entityContext.ObjectStateManager.ChangeObjectState(insertObject, EntityState.Unchanged);

            return new Tuple<ObjectContext, DbCommand>(entityContext, dbCmdInsert);
        }

        /// <summary>
        /// Deletes the entity from the database, and removes the object from the context. 
        /// The DbCmdIn is optional. If it is passed in, it will be used and any parameters will be changed to the 
        /// value of the deleteObject. If it is not passed in, a new DbCommand will be created with the parameters pointing
        /// to the properties of the deleteObject instance.
        /// </summary>
        /// <param name="entityContext">Context to remove object from</param>
        /// <param name="deleteObject">Object to remove from context</param>
        /// <param name="dbTransaction">Transacition. Can be null. Ignored if NULL</param>
        /// <param name="dbCmdIn">Optional. See summary.</param>
        /// <returns></returns>
        public Tuple<ObjectContext, DbCommand> DeleteEntity(ObjectContext entityContext, object deleteObject,
                DbTransaction dbTransaction = null, DbCommand dbCmdIn = null)
        {
            DbCommand dbCmd = null;

            if (dbCmdIn != null)
            {
                dbCmd = dbCmdIn;
                // if the command is not new, then we need to bind the command's parameters
                // to the new object
                ObjectParser.RemapDbCommandParameters(dbCmd, deleteObject);
            }
            else
            {
                dbCmd = BuildDeleteDbCommand(entityContext, deleteObject);
                ObjectParser entity = new ObjectParser(entityContext, deleteObject, this);
            }

            ExecuteNonQuery(dbCmd, dbTransaction, null);
            entityContext.Detach(deleteObject);

            return new Tuple<ObjectContext, DbCommand>(entityContext, dbCmd);
        }

        
        /// <summary>
        /// Updates the entity object with FIRST ROW of data from the data reader.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        internal object UpdateEntitiesFromReader(object entity, IDataReader reader)
        {
            return UpdateEntitiesFromReader(new List<object> { entity }, reader);
        }

        /// <summary>
        /// Updates the entity objects with data from the data reader.
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        internal List<object> UpdateEntitiesFromReader(List<object> entities, IDataReader reader)
        {
            if(entities.Count == 0)
                return entities;

            Dictionary<Type, List<KeyValuePair<int, System.Reflection.PropertyInfo>>> typeProps = 
                    new Dictionary<Type, List<KeyValuePair<int, PropertyInfo>>>();

            foreach(object entity in entities)
            {
                Type t = entity.GetType();

                List<KeyValuePair<int, System.Reflection.PropertyInfo>> props;

                if(typeProps.ContainsKey(t))
                    props = typeProps[t];
                else
                {
                    // Loop throught the columns in the resultset and lookup the properties and ordinals 
                    props = new List<KeyValuePair<int, System.Reflection.PropertyInfo>>();

                    for(int i = 0; i < reader.FieldCount; i++)
                    {
                        string fieldName = reader.GetName(i);
                        // Ignore case of the property name
                        System.Reflection.PropertyInfo pinfo = t.GetProperties()
                                .Where(p => p.Name.ToLower() == fieldName.ToLower()).FirstOrDefault();
                        if(pinfo != null)
                            props.Add(new KeyValuePair<int, System.Reflection.PropertyInfo>(i, pinfo));
                    }
                }

                if(reader.Read())
                {
                    props.ForEach(kv => kv.Value.SetValue(entity,
                            GetValueOrNull(Convert.ChangeType(reader.GetValue(kv.Key), kv.Value.PropertyType)), null));
                }
            }

            return entities;
        }

        /// <summary>
        /// Function will attempt to prepare a database specific string for debugging
        /// by calling the backend specific library if available and defined.
        /// If the debug script is prepared it will be logged; otherwise the exception
        /// is simply logged.
        /// </summary>
        /// <param name="e">The caught exception</param>
        /// <param name="dbCommand">The database command being executed</param>
        /// <returns>A new ExceptionEvent object with the ReferenceNumber set when event logging is on</returns>
        internal ExceptionEvent CreateAndLogExceptionEvent(Exception e, DbCommand dbCommand)
        {
            StringBuilder msg = new StringBuilder();
            msg.AppendFormat("Excecuting DbCommandText: {0}",
                _dbProviderLib.GetCommandDebugScript(dbCommand));
            ExceptionEvent excEvent = new ExceptionEvent(enumExceptionEventCodes.UnknownException
                                            , msg.ToString()
                                            , e);
            if (_loggingMgr != null)
                excEvent.ReferenceNumber = _loggingMgr.WriteToLog(excEvent);

            return excEvent;
        }

        /// <summary>
        /// Sets the parameters in the dbCommand object.
        /// </summary>
        /// <param name="dbCommand">DbCommand object containing parameters that need values.</param>
        /// <param name="parameterNameValues">A set of parameter names and values or null. 
        /// Example: "FirstName", "Ernest", "LastName", "Hemingway"</param>
        private void SetParameterValues(DbCommand dbCommand, params object[] parameterNameValues)
        {
            foreach (DbParameter dbParam in dbCommand.Parameters)
                if (dbParam.Value == null)
                    dbParam.Value = DBNull.Value;

            if(parameterNameValues == null || parameterNameValues.Count() == 0)
                return;

            if(parameterNameValues.Count() % 2 != 0)
                throw new ExceptionEvent(enumExceptionEventCodes.InvalidParameterValue, 
                        "Name/value parameters are not evenly matched.");

            for (int i = 0; i < parameterNameValues.Count(); i++)
            {
                string paramName = (string)parameterNameValues[i];
                object paramValue = parameterNameValues[i + 1] == null ? DBNull.Value : parameterNameValues[i + 1];
                dbCommand.Parameters[paramName].Value = paramValue;
                    
                i++;
            }
        }

        /// <summary>
        /// Looks for special ISite implementation to use lambda member access to access any closures
        /// that were associated with the parameter.
        /// </summary>
        /// <param name="dbCommand"></param>
        private void UpdateParameterValues(DbCommand dbCommand)
        {
            if(dbCommand.Site != null && dbCommand.Site is ParameterSite)
            {
                List<DbPredicateParameter> parameters = (List<DbPredicateParameter>)dbCommand.Site.GetService(null);

                for(int i = 0; i < dbCommand.Parameters.Count; i++)
                {
                    DbPredicateParameter parameter = parameters.FirstOrDefault( 
                            p => p.ParameterName.ToLower() == dbCommand.Parameters[i].ParameterName.ToLower());

                    if(parameter != null)
                        dbCommand.Parameters[i].Value = parameter.MemberAccess.DynamicInvoke();
                }
            }
        }   

        #endregion
    }
}
