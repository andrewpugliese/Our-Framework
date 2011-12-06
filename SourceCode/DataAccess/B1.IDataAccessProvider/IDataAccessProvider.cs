using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Xml;

using Microsoft.Practices.EnterpriseLibrary.Data;

using B1.Core;

namespace B1.IDataAccess
{
    /// <summary>
    /// The interface that defines the methods and properties needed by the DataAccessManager
    /// for the database provider's specific functions or syntax is implemented as an abstract class.
    /// This allows us to minimize code by implementing common code as part of the abstract class
    /// as well as virtual methods/properties.
    /// </summary>
    public abstract class DataAccessProvider
    {
        protected Database _database = null;
        protected string _serverVersion = null;
        protected Int16 _serverMajorVersion = 0;
        protected Int16 _serverMinorVersion = 0;
        protected string _dbName = null;
        protected string _version = null;
        protected DataTable _dbFactories = null;

        /// <summary>
        /// Default constructor for the class that requires a Data Access Application Block Database object.
        /// </summary>
        /// <param name="database">DAAB Database object</param>
        public DataAccessProvider(Database database)
        {
            _database = database;
            using (DbConnection con = _database.CreateConnection())
            {
                con.Open();
                _serverVersion = con.ServerVersion;
                string[] serverVersion = _serverVersion.Split(new char[] { '.' });
                _serverMajorVersion = Convert.ToInt16(serverVersion[0]);
                _serverMinorVersion = Convert.ToInt16(serverVersion[1]);
                _dbName = con.Database;
            }
            _dbFactories = DbProviderFactories.GetFactoryClasses();
        }

        /// <summary>
        /// Returns the database server version string.
        /// </summary>
        public string ServerVersion { get { return _serverVersion; } }

        /// <summary>
        /// Returns the database server major version number.
        /// </summary>
        public Int16 ServerMajorVersion { get { return _serverMajorVersion; } }

        /// <summary>
        /// Returns the database server minor version number.
        /// </summary>
        public Int16 ServerMinorVersion { get { return _serverMinorVersion; } }

        /// <summary>
        /// Returns the database provider dll version string.
        /// </summary>
        public string Version { get { return _version != null ? _version : ProviderVersion(); } }

        /// <summary>
        /// Returns the database name string.
        /// </summary>
        public string DatabaseName { get { return _dbName; } }

        /// <summary>
        /// Returns the back-end compliant syntax for a command that performs no operation.
        /// e.g: -- in SqlServer and Db2.
        /// </summary>
        public virtual string NoOpDbCommandText { get { return Constants.NoOpDbCommandText; } }

        /// <summary>
        /// Returns a string to be used as an alias when joining tables (e.g. T )
        /// </summary>
        public virtual string DefaultTableAlias { get { return Constants.DefaultTableAlias; } }

        /// <summary>
        /// Returns the string character that prefixes parameters for the specific back-end
        /// e.g. @ in SqlServer and Db2
        /// </summary>
        public virtual string ParameterPrefix { get { return Constants.ParameterPrefix; } }

        /// <summary>
        /// Returns the string character that prefixes bind variables for the specific back-end
        /// e.g. @ in SqlServer and Db2
        /// </summary>
        public virtual string BindValuePrefix { get { return Constants.BindValuePrefix; } }

        /// <summary>
        /// Supports the Version property
        /// </summary>
        /// <returns>Version number string for the Database Provider DLL</returns>
        protected abstract string ProviderVersion();

        /// <summary>
        /// Derives the parameters of the given DbCommand object
        /// </summary>
        /// <param name="dbCmd">A DAAB DbCommand object</param>
        public abstract void DeriveParameters(DbCommand dbCmd);

        /// <summary>
        /// Adjusts the given command text so that it is back-end compliant.
        /// e.g. wraps in Begin / End block
        /// </summary>
        /// <param name="commandText">Command Text of a DAAB DbCommand object</param>
        /// <returns></returns>
        public virtual string FormatCommandText(string commandText)
        {
            return commandText;
        }
        
        /// <summary>
        /// Provides an opportunity to make any property settings to the given DbCommand object.
        /// e.g. InitialLOBFetchSize for Oracle
        /// </summary>
        /// <param name="dbCmd">A DAAB DbCommand object</param>
        /// <returns>A DAAB DbCommand object</returns>
        public virtual DbCommand FormatDbCommand(DbCommand dbCmd)
        {
            return dbCmd;
        }

        /// <summary>
        /// Returns the back-end compliant sql fragment for getting the row count for the last operation.
        /// This is not the same as COUNT(*);  It is more like @@RowCount of SQLServer
        /// </summary>
        /// <param name="rowCountParam">A parameter name to store the result of the rowcount function</param>
        /// <returns>A code fragment which will store the rowcount into the given parameter</returns>
        public abstract string FormatRowCountSql(string rowCountParam);

        /// <summary>
        /// Returns the back-end compliant sql fragment for performing Date Arithametic.
        /// Depending on the parameters, the function will add (Days, Hours, ... , milliseconds)
        /// </summary>
        /// <param name="dateDiffInterval">Enumeration of the possible intervals (Days, Hours, Minutes.. MilliSeconds)</param>
        /// <param name="duration">If duration is a string, it will be parameterized; otherwise it will be a constant</param>
        /// <param name="startDate">If startDate is a string, it will be assumed to be a column name;
        /// if it is a dateEnumeration, then it can be either UTC, Local or default.</param>
        /// <returns>A code fragment which will perform the appropriate date add operation.</returns>
        public abstract string FormatDateMathSql(EnumDateDiffInterval dateDiffInterval
                , object duration
                , object startDate);

        /// <summary>
        /// Returns a back-end compliant sql syntax for beggining a command block in a transaction.
        /// Note this is used for Compound SQL where multiple statements are formatted in a single
        /// DbCommand.CommandText.
        /// </summary>
        /// <param name="tranCount">Used for nested transactions as a comment for readability</param>
        /// <returns>A code fragment which will perform the appropriate operation.</returns>
        public abstract string BeginTransaction(Int32 tranCount);

        /// <summary>
        /// Returns a back-end compliant sql syntax for completing a command block in a transaction.
        /// Note this is used for Compound SQL where multiple statements are formatted in a single
        /// DbCommand.CommandText.
        /// </summary>
        /// <param name="tranCount">Used for nested transactions as a comment for readability</param>
        /// <returns>A code fragment which will perform the appropriate operation.</returns>
        public abstract string CommitTransaction(Int32 tranCount);

        /// <summary>
        /// Returns the back-end compliant parameter name (with ParameterPrefix)
        /// </summary>
        /// <param name="paramName">Parameter Name (with or without prefix)</param>
        /// <returns>Parameter name with prefix</returns>
        public virtual string BuildParameterName(string paramName)
        {
            return paramName.Contains(ParameterPrefix) ? paramName : ParameterPrefix + paramName;
        }

        /// <summary>
        /// Returns the back-end compliant variable name (with BindVariablePrefix)
        /// </summary>
        /// <param name="variableName">Variable name (with or without prefix)</param>
        /// <returns>Variable name with prefix)</returns>
        public virtual string BuildBindVariableName(string variableName)
        {
            return variableName.Contains(BindValuePrefix) ? variableName : BindValuePrefix + variableName;
        }

        /// <summary>
        /// Creates a DbParameter from the given attributes.
        /// </summary>
        /// <param name="paramName">The name of the parameter</param>
        /// <param name="paramType">The DAAB data type of the parameter</param>
        /// <param name="nativeDbType">The back-end specific data type</param>
        /// <param name="maxLength">The maximum length of the param for out parameters; 0 otherwise</param>
        /// <param name="paramDirection">The parameter direction</param>
        /// <param name="paramValue">The value of the parameter.</param>
        /// <returns>New DbParameter object</returns>
        public abstract DbParameter CreateNewParameter(string paramName
                , DbType paramType
                , string nativeDbType
                , Int32 maxLength
                , ParameterDirection paramDirection
                , object paramValue);

        /// <summary>
        /// Returns a clone of the given parameter
        /// </summary>
        /// <param name="dbParam">The DbParameter to clone</param>
        /// <returns>A copy of the DbParameter</returns>
        public abstract DbParameter CloneParameter(DbParameter dbParam);

        /// <summary>
        /// Returns a clone of the given DbParameter collection.
        /// </summary>
        /// <param name="dbParameters">The collection to clone</param>
        /// <returns>A copy of the DbParameter collection</returns>
        public abstract DbParameterCollection CloneParameterCollection(DbParameterCollection dbParameters);

        /// <summary>
        /// Returns a copy of the given DbParameter that was added to the given collection.
        /// </summary>
        /// <param name="dbParameters">A DbParameter collection to add the parameter clone to</param>
        /// <param name="dbParam">A DbParameter to clone</param>
        /// <returns>The DbParameter clone</returns>
        public abstract DbParameter CopyParameterToCollection(DbParameterCollection dbParameters
                , DbParameter dbParam);
        
        /// <summary>
        /// Returns a boolean indicating if the two parameters are equivalent
        /// (same direction, type, and value);  Out params are always false.
        /// </summary>
        /// <param name="param1">DbParameter1</param>
        /// <param name="param2">DbParameter2</param>
        /// <returns>true or false</returns>
        public abstract bool CompareParamEquality(DbParameter param1, DbParameter param2);

        /// <summary>
        /// Returns the back-end compliant sql syntax for calling the given
        /// stored procedure with the given parameters.
        /// </summary>
        /// <param name="storedProcedure">The stored procedure to call</param>
        /// <param name="dbParameters">The DbParameter collection to use as arguments</param>
        /// <returns>A code fragment which will perform the appropriate operation.</returns>
        public abstract string GenerateStoredProcedureCall(string storedProcedure
                , DbParameterCollection dbParameters);

        /// <summary>
        /// Method used to retrieve the value of an out parameter referred to 
        /// by the given parameter name.
        /// NOTE: Numeric Return Value from Oracle Driver (ODP.NET) must be 
        /// Cast to OracleDecimal, then to .Net Decimal before they can be converted
        /// by the caller.
        /// This method provides a consistent interface for testing out params for null
        /// </summary>
        /// <param name="dbCommand">DbCommand object</param>
        /// <param name="paramName">The name of the parameter to test</param>
        /// <returns>The out param's value as an object</returns>
        public virtual object GetOutParamValue(DbCommand dbCommand, string paramName)
        {
            return dbCommand.Parameters[BuildParameterName(paramName)].Value;
        }

        /// <summary>
        /// With Oracle ODP.NET, we must call built in function IsNull instead of comparing to DBNull.Value
        /// This method provides a consistent interface for testing out params for null
        /// </summary>
        /// <param name="dbCommand">DbCommand object</param>
        /// <param name="paramName">The name of the parameter to test</param>
        /// <returns>Boolean indicating if the parameter's value is null</returns>
        public virtual bool IsOutParamValueNull(DbCommand dbCommand, string paramName)
        {
            return dbCommand.Parameters[BuildParameterName(paramName)].Value == DBNull.Value
                   || dbCommand.Parameters[BuildParameterName(paramName)].Value == null;
        }

        /// <summary>
        /// Returns the backend specific function for current datetime
        /// to be used in an sql command.
        /// if ReturnAsAlias is not null, it will be the alias for the function
        /// </summary>
        /// <param name="dbDateType">The format type of the date function(local, UTC, Default (UTC))</param>
        /// <param name="returnAsAlias">What the return column will be called</param>
        /// <returns>Backend specific function for current date time including milliseconds</returns>
        public abstract string GetDbTimeAs(EnumDateTimeLocale dbDateType, string returnAsAlias);

        /// <summary>
        /// Returns the command text for a DbCommand to obtain the DateTime from the database.
        /// Note: This operation will make a database call.
        /// if ReturnAsAlias is not null, it will be the alias
        /// </summary>
        /// <param name="dbDateType">Enumeration value indicating whether time is local or UTC;
        /// default is UTC.</param>
        /// <param name="returnAsAlias">What the return column will be called</param>
        /// <returns>Back-end compliant command text for returning server time</returns>
        public abstract string GetServerTimeCommandText(EnumDateTimeLocale dbDateType, string returnAsAlias);

        /// <summary>
        /// Returns the Data Access Application Block's dataType for the given
        /// database native dataType.
        /// </summary>
        /// <param name="nativeDataType">Database specific dataType</param>
        /// <returns>Data Access Application Block DataType equivalent</returns>
        public abstract DbType GetGenericDbTypeFromNativeDataType(string nativeDataType);

        /// <summary>
        /// Returns the Data Access Application Block's dataType for the given
        /// database numeric dataType.
        /// NOTE: This if for Oracle Only; For other Db's, the size and scale
        /// parameters will be ignored.
        /// </summary>
        /// <param name="nativeDataType">Database specific dataType</param>
        /// <param name="size">Numeric size of the dataType</param>
        /// <param name="scale">Numeric scale of the dataType</param>
        /// <returns>Data Access Application Block DataType equivalent</returns>
        public virtual DbType GetGenericDbTypeFromNativeDataType(string nativeDataType, Int16 size, Int16 scale)
        {
            return GetGenericDbTypeFromNativeDataType(nativeDataType);
        }

        /// <summary>
        /// Returns the .Net dataType for the given
        /// database native dataType.
        /// </summary>
        /// <param name="nativeDataType">String representation of the database native data type</param>
        /// <returns>String representation of the .Net data type</returns>
        public abstract string GetDotNetDataTypeFromNativeDataType(string nativeDataType);

        /// <summary>
        /// Returns the .Net dataType for the given
        /// database numeric dataType.
        /// NOTE: This if for Oracle Only
        /// </summary>
        /// <param name="nativeDataType">String representation of the database native data type</param>
        /// <param name="size">Numeric size of the dataType</param>
        /// <param name="scale">Numeric scale of the dataType</param>
        /// <returns>String representation of the .Net data type</returns>
        public virtual string GetDotNetDataTypeFromNativeDataType(string nativeDataType, Int16 size, Int16 scale)
        {
            return GetDotNetDataTypeFromNativeDataType(nativeDataType);
        }

        /// <summary>
        /// Returns and XmlReader object from the database command object
        /// </summary>
        /// <param name="dbCommand">DAAB DbCommand Object</param>
        /// <param name="dbTran">DbTransaction or null</param>
        /// <returns>XmlReader</returns>
        public abstract XmlReader ExecuteXmlReader(DbCommand dbCommand
                , DbTransaction dbTran);

        /// <summary>
        /// Returns a back-end compliant script that can be executed in an interactive editor
        /// such as Management Studio or SQLDeveloper for the given DbCommand.
        /// Since the DbCommands are parameterized, the command text will only contain bind variables
        /// This function will provide variable declarations and initalizations so that the results
        /// can be tested.
        /// </summary>
        /// <param name="dbCmd">DAAB DbCommand object</param>
        /// Returns a back-end compliant script that can be executed in an interactive editor
        public abstract string GetCommandDebugScript(DbCommand dbCmd);

        /// <summary>
        /// Returns a boolean indicating whether or not the given dbException is for a primary key constraint
        /// </summary>
        /// <param name="dbe">DbException object</param>
        /// <returns>True if dbException is a primary key violation</returns>
        public abstract bool IsPrimaryKeyViolation(DbException dbe);
    }
}
