using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Xml;
using System.Threading;

using Microsoft.Practices.EnterpriseLibrary.Data;

using B1.ILoggingManagement;
using B1.IDataAccess;
using B1.Core;

namespace B1.DataAccess.SqlServer
{
    /// <summary>
    /// Provides the SqlServer Specific functionality as defined in the IDataAccessProvider interface.
    /// </summary>
    public class SqlServerHelper : DataAccessProvider
    {
        /// <summary>
        /// Constructs the SqlHelper class
        /// </summary>
        /// <param name="database">DAAB Database object for a SqlServer database</param>
        public SqlServerHelper(Database database)
            : base(database)
        {
        }

        /// <summary>
        /// Returns the string version of the dll loaded for this provider
        /// </summary>
        /// <returns>The version string of the registered factories for this provider</returns>
        protected override string ProviderVersion()
        {
            DataRow[] dbFactory = _dbFactories.Select("InvariantName = 'System.Data.SqlClient'");
            _version = dbFactory[0]["AssemblyQualifiedName"].ToString().Split('=')[1].Split(',')[0];
            return _version;
        }

        /// <summary>
        /// Returns the SqlServer compliant sql fragment for performing Date Arithametic.
        /// Depending on the parameters, the function will add (Days, Hours, ... , milliseconds)
        /// </summary>
        /// <param name="dateDiffInterval">Enumeration of the possible intervals (Days, Hours, Minutes.. MilliSeconds)</param>
        /// <param name="duration">If duration is a string, it will be parameterized; otherwise it will be a constant</param>
        /// <param name="startDate">If startDate is a string, it will be assumed to be a column name;
        /// if it is a dateEnumeration, then it can be either UTC, Local or default.</param>
        /// <returns>A code fragment which will perform the appropriate date add operation.</returns>
        public override string FormatDateMathSql(EnumDateDiffInterval dateDiffInterval
                , object duration
                , object startDate)
        {
            // is the startDate one of the default parameters
            string startDateParam = GetDbTimeAs(EnumDateTimeLocale.Default, null);
            if (startDate is EnumDateTimeLocale)
                startDateParam = GetDbTimeAs((EnumDateTimeLocale)startDate, null);
            if (startDate is string)    // columnName
                startDateParam = startDate.ToString();

            // determine if the Duration parameter should be a bind variable
            // or is a constant
            object durationParam = 0;    // default is 0 duration.
            if (duration is string
                && !string.IsNullOrEmpty(duration.ToString()))
                BuildBindVariableName(duration.ToString());

            durationParam = duration;
            return string.Format("DateAdd({0}, {1}, {2})", dateDiffInterval.ToString(), durationParam, startDateParam);
        }

        /// <summary>
        /// Return the SQLServer specific statement for executing a stored procedure with a CommandBlock
        /// </summary>
        /// <param name="storedProcedure">Name of stored procedure.</param>
        /// <param name="dbParameters">SqlParameter collection</param>
        /// <returns>A SQLServer compliant statement for executing the given stored procedure and parameters.</returns>
        public override string GenerateStoredProcedureCall(string storedProcedure, DbParameterCollection dbParameters)
        {
            SqlParameterCollection sqlParameters = (SqlParameterCollection)dbParameters;
            StringBuilder commandText = new StringBuilder(string.Format("execute {0} ", storedProcedure));
            if (sqlParameters != null && sqlParameters.Count > 0)
            {
                bool firstParam = true;
                foreach (SqlParameter param in sqlParameters)
                {
                    if (param.Direction == ParameterDirection.ReturnValue)
                        commandText.Insert(0, param.ParameterName + " = ");
                    commandText.AppendFormat("{0} {1}{2}"
                            , firstParam ? "" : ", "
                            , param.ParameterName
                            , param.Direction == ParameterDirection.Output
                                || param.Direction == ParameterDirection.InputOutput ? " out" : "");
                    firstParam = false;
                }
            }
            return commandText.ToString();
        }

        /// <summary>
        /// Returns and XmlReader object from the database command object
        /// </summary>
        /// <param name="dbCommand">Database Command Object</param>
        /// <param name="dbTran">Transaction or null</param>
        /// <returns>XmlReader</returns>
        public override XmlReader ExecuteXmlReader(DbCommand dbCommand
                , DbTransaction dbTran)
        {
            SqlCommand sqlCommand = (SqlCommand)dbCommand;
            using (SqlConnection con = (SqlConnection)_database.CreateConnection())
            {
                con.Open();
                sqlCommand.Connection = con;
                if (dbTran != null)
                    sqlCommand.Transaction = (SqlTransaction)dbTran;
                return sqlCommand.ExecuteXmlReader();
            }
        }

        /// <summary>
        /// Returns a SqlServer compliant sql syntax for beggining a command block in a transaction.
        /// Note this is used for Compound SQL where multiple statements are formatted in a single
        /// DbCommand.CommandText.
        /// </summary>
        /// <param name="tranCount">Used for nested transactions as a comment for readability</param>
        /// <returns>A code fragment which will perform the appropriate operation.</returns>
        public override string BeginTransaction(Int32 tranCount)
        {
            return string.Format("begin transaction tran{0} /* tran_{0} */ {1}set xact_abort on{1}"
                , tranCount, Environment.NewLine);
        }

        /// <summary>
        /// Returns a SqlServer compliant sql syntax for completing a command block in a transaction.
        /// Note this is used for Compound SQL where multiple statements are formatted in a single
        /// DbCommand.CommandText.
        /// </summary>
        /// <param name="tranCount">Used for nested transactions as a comment for readability</param>
        /// <returns>A code fragment which will perform the appropriate operation.</returns>
        public override string CommitTransaction(Int32 tranCount)
        {
            return string.Format("commit transaction tran{0} /* tran_{0} */ {1}", tranCount, Environment.NewLine);
        }

        /// <summary>
        /// Derives the parameters of the given DbCommand object
        /// </summary>
        /// <param name="dbCmd">A DAAB DbCommand object</param>
        public override void DeriveParameters(DbCommand dbCmd)
        {
            _database.DiscoverParameters(dbCmd);
        }

        /// <summary>
        /// Returns the backend specific function for current datetime
        /// to be used in an sql command.
        /// if ReturnAsAlias is not null, it will be the alias for the function
        /// </summary>
        /// <param name="dbDateType">The format type of the date function(local, UTC, Default (UTC))</param>
        /// <param name="returnAsAlias">What the return column will be called</param>
        /// <returns>Backend specific function for current date time including milliseconds</returns>
        public override string GetDbTimeAs(EnumDateTimeLocale dbDateType, string returnAsAlias)
        {
            string dbTime = dbDateType == EnumDateTimeLocale.Local ? "getdate()" : "getutcdate()";
            return dbTime + (string.IsNullOrEmpty(returnAsAlias) ? "" : " as " + returnAsAlias);
        }

        /// <summary>
        /// Returns the command text for a DbCommand to obtain the DateTime from the database.
        /// Note: This operation will make a database call.
        /// if ReturnAsAlias is not null, it will be the alias
        /// </summary>
        /// <param name="dbDateType">Enumeration value indicating whether time is local or UTC;
        /// default is UTC.</param>
        /// <param name="returnAsAlias">What the return column will be called</param>
        /// <returns>Back-end compliant command text for returning server time</returns>
        public override string GetServerTimeCommandText(EnumDateTimeLocale dbDateType, string returnAsAlias)
        {
            return string.Format("select {0}", GetDbTimeAs(dbDateType, returnAsAlias));
        }

        /// <summary>
        /// Returns the back-end compliant sql fragment for getting the row count for the last operation.
        /// This is not the same as COUNT(*);  It is more like @@RowCount of SQLServer
        /// </summary>
        /// <param name="rowCountParam">A parameter name to store the result of the rowcount function</param>
        /// <returns>A code fragment which will store the rowcount into the given parameter</returns>
        public override string FormatRowCountSql(string rowCountParam)
        {
            StringBuilder rowCount = new StringBuilder();
            rowCount.AppendFormat("set {0} = @@rowcount {1};", rowCountParam, Environment.NewLine);
            return rowCount.ToString();
        }


        /// <summary>
        /// Returns the .Net dataType for the given
        /// database native dataType.
        /// </summary>
        /// <param name="nativeDataType">String representation of the database native data type</param>
        /// <returns>String representation of the .Net data type</returns>
        public override string GetDotNetDataTypeFromNativeDataType(string nativeDataType)
        {
            nativeDataType = nativeDataType.ToLower();
            if (nativeDataType == Constants.DataTypeBigInt)
                return IDataAccess.Constants.SystemInt64;
            else if (nativeDataType == Constants.DataTypeTinyint)
                return IDataAccess.Constants.SystemByte;
            else if (nativeDataType == Constants.DataTypeBit)
                return IDataAccess.Constants.SystemBoolean;
            else if (nativeDataType == Constants.DataTypeChar
                || nativeDataType == Constants.DataTypeChar
                || nativeDataType == Constants.DataTypeVarChar
                || nativeDataType == Constants.DataTypeNVarChar)
                return IDataAccess.Constants.SystemString;
            else if (nativeDataType == Constants.DataTypeText
                || nativeDataType == Constants.DataTypeNText)
                return IDataAccess.Constants.SystemObject;
            else if (nativeDataType == Constants.DataTypeSmallDateTime
                || nativeDataType == Constants.DataTypeDate
                || nativeDataType == Constants.DataTypeDateTime
                || nativeDataType == Constants.DataTypeDateTime2)
                return IDataAccess.Constants.SystemDateTime;
            else if (nativeDataType == Constants.DataTypeMoney
                || nativeDataType == Constants.DataTypeSmallMoney
                || nativeDataType == Constants.DataTypeDecimal)
                return IDataAccess.Constants.SystemDecimal;
            else if (nativeDataType == Constants.DataTypeInt)
                return IDataAccess.Constants.SystemInt32;
            else if (nativeDataType == Constants.DataTypeReal)
                return IDataAccess.Constants.SystemDouble;
            else if (nativeDataType == Constants.DataTypeSmallInt)
                return IDataAccess.Constants.SystemInt16;
            else if (nativeDataType == Constants.DataTypeUniqueId)
                return IDataAccess.Constants.SystemGuid;
            else throw new ExceptionEvent(enumExceptionEventCodes.InvalidParameterValue
                        , string.Format("nativeDataType; {0} was not defined as a DotNetType.", nativeDataType));

        }


        /// <summary>
        /// Returns the Data Access Application Block's dataType for the given
        /// database native dataType.
        /// </summary>
        /// <param name="nativeDataType">Database specific dataType</param>
        /// <returns>Data Access Application Block DataType equivalent</returns>
        public override DbType GetGenericDbTypeFromNativeDataType(string nativeDataType)
        {
            nativeDataType = nativeDataType.ToLower();
            if (nativeDataType == Constants.DataTypeBigInt)
                return DbType.Int64;
            else if (nativeDataType == Constants.DataTypeTinyint)
                return DbType.Byte;
            else if (nativeDataType == Constants.DataTypeBit)
                return DbType.Boolean;
            else if (nativeDataType == Constants.DataTypeChar
                || nativeDataType == Constants.DataTypeChar
                || nativeDataType == Constants.DataTypeVarChar
                || nativeDataType == Constants.DataTypeNVarChar
                || nativeDataType == Constants.DataTypeText
                || nativeDataType == Constants.DataTypeNText)
                return DbType.String;
            else if (nativeDataType == Constants.DataTypeSmallDateTime
                || nativeDataType == Constants.DataTypeDate
                || nativeDataType == Constants.DataTypeDateTime
                || nativeDataType == Constants.DataTypeDateTime2)
                return DbType.DateTime;
            else if (nativeDataType == Constants.DataTypeMoney
                || nativeDataType == Constants.DataTypeSmallMoney
                || nativeDataType == Constants.DataTypeDecimal)
                return DbType.Decimal;
            else if (nativeDataType == Constants.DataTypeInt)
                return DbType.Int32;
            else if (nativeDataType == Constants.DataTypeReal)
                return DbType.Double;
            else if (nativeDataType == Constants.DataTypeSmallInt)
                return DbType.Int16;
            else if (nativeDataType == Constants.DataTypeUniqueId)
                return DbType.Guid;
            else throw new ExceptionEvent(enumExceptionEventCodes.InvalidParameterValue
                , string.Format("nativeDataType; {0} was not defined as a DotNetType.", nativeDataType));
        }

        /// <summary>
        /// Returns a boolean indicating if the two parameters are equivalent
        /// (same direction, type, and value);  Out params are always false.
        /// </summary>
        /// <param name="param1">DbParameter1</param>
        /// <param name="param2">DbParameter2</param>
        /// <returns>true or false</returns>
        public override bool CompareParamEquality(DbParameter dbParam1, DbParameter dbParam2)
        {
            SqlParameter sqlParam1 = (SqlParameter)dbParam1;
            SqlParameter sqlParam2 = (SqlParameter)dbParam2;
            switch (sqlParam1.SqlDbType)
            {
                case SqlDbType.Money:
                case SqlDbType.Decimal:
                    return Convert.ToDecimal(sqlParam1.Value) == Convert.ToDecimal(sqlParam2.Value);
                case SqlDbType.Int:
                    return Convert.ToInt32(sqlParam1.Value) == Convert.ToInt32(sqlParam2.Value);
                case SqlDbType.SmallInt:
                    return Convert.ToInt16(sqlParam1.Value) == Convert.ToInt16(sqlParam2.Value);
                case SqlDbType.TinyInt:
                    return Convert.ToByte(sqlParam1.Value) == Convert.ToByte(sqlParam2.Value);
                case SqlDbType.BigInt:
                    return Convert.ToInt64(sqlParam1.Value) == Convert.ToInt64(sqlParam2.Value);
                case SqlDbType.Real:
                    return Convert.ToSingle(sqlParam1.Value) == Convert.ToSingle(sqlParam2.Value);
                case SqlDbType.Float:
                    return Convert.ToDouble(sqlParam1.Value) == Convert.ToDouble(sqlParam2.Value);
                case SqlDbType.Char:
                case SqlDbType.NChar:
                case SqlDbType.VarChar:
                case SqlDbType.NVarChar:
                case SqlDbType.Text:
                    return sqlParam1.Value.ToString().ToLower() == sqlParam2.Value.ToString().ToLower();
                case SqlDbType.DateTime:
                    return Convert.ToDateTime(sqlParam1.Value) == Convert.ToDateTime(sqlParam2.Value);
                default:
                    return sqlParam1.Value == sqlParam2.Value;
            }
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
        public override DbParameter CreateNewParameter(string paramName
            , DbType paramType
            , string nativeDataType
            , Int32 maxLength
            , ParameterDirection paramDirection
            , object paramValue)
        {
            if (!paramName.Contains(Constants.ParameterPrefix))
                paramName = Constants.ParameterPrefix + paramName;
            SqlParameter newParam = new SqlParameter(paramName, paramType);
            newParam.Value = paramValue;
            newParam.Direction = paramDirection;
            newParam.DbType = paramType;
            return ValidateParam(newParam, maxLength, paramType);
        }

        private static SqlParameter ValidateParam(SqlParameter dbParam, Int32 size, DbType paramType)
        {
            // Only set the size if parameter is an output param
            if (dbParam.Direction == ParameterDirection.InputOutput
                || dbParam.Direction == ParameterDirection.Output)
                dbParam.Size = size;

            // if we need to reset the value to DbNull.value
            if (dbParam.Value == null)
            {
                dbParam.Value = DBNull.Value;
                // make sure we reset the DAAB dbType
                dbParam.DbType = paramType;
            }
            return dbParam;
        }

        /// <summary>
        /// Returns a clone of the given parameter
        /// </summary>
        /// <param name="dbParam">The DbParameter to clone</param>
        /// <returns>A copy of the DbParameter</returns>
        public override DbParameter CloneParameter(DbParameter dbParam)
        {
            SqlParameter cloneParam = new SqlParameter(dbParam.ParameterName, dbParam.DbType);
            cloneParam.DbType = dbParam.DbType;
            cloneParam.Direction = dbParam.Direction;
            cloneParam.Value = dbParam.Value;
            cloneParam.SourceColumn = dbParam.SourceColumn;
            cloneParam.SourceColumnNullMapping = dbParam.SourceColumnNullMapping;
            cloneParam.SourceVersion = dbParam.SourceVersion;
            cloneParam.ParameterName = dbParam.ParameterName;
            cloneParam.IsNullable = dbParam.IsNullable;

            return ValidateParam(cloneParam, dbParam.Size, dbParam.DbType);
        }

        /// <summary>
        /// Returns a copy of the given DbParameter that was added to the given collection.
        /// </summary>
        /// <param name="dbParameters">A DbParameter collection to add the parameter clone to</param>
        /// <param name="dbParam">A DbParameter to clone</param>
        /// <returns>The DbParameter clone</returns>
        public override DbParameter CopyParameterToCollection(DbParameterCollection dbParameters
            , DbParameter dbParam)
        {
            SqlParameterCollection sqlParameters = (SqlParameterCollection)dbParameters;
            SqlParameter sqlParam = (SqlParameter)dbParam;
            if (sqlParameters.Contains(sqlParam.ParameterName))
                throw new ExceptionEvent(enumExceptionEventCodes.DbParameterExistsInCollection
                        , string.Format("Parameter {0} already belongs to this collection; use Set to change value."
                                , sqlParam.ParameterName));

            sqlParameters.Add(CloneParameter(sqlParam));
            return sqlParameters[sqlParam.ParameterName];
        }

        /// <summary>
        /// Returns a clone of the given DbParameter collection.
        /// </summary>
        /// <param name="dbParameters">The collection to clone</param>
        /// <returns>A copy of the DbParameter collection</returns>
        public override DbParameterCollection CloneParameterCollection(DbParameterCollection dbParameters)
        {
            SqlParameterCollection srcSqlCollection = (SqlParameterCollection)dbParameters;
            SqlParameterCollection tgtSqlCollection = (SqlParameterCollection)
                    _database.GetSqlStringCommand(Constants.NoOpDbCommandText).Parameters; ;
            foreach (SqlParameter dbParam in srcSqlCollection)
                CopyParameterToCollection(tgtSqlCollection, dbParam);
            return tgtSqlCollection;
        }

        /// <summary>
        /// Returns a back-end compliant script that can be executed in an interactive editor
        /// such as Management Studio or SQLDeveloper for the given DbCommand.
        /// Since the DbCommands are parameterized, the command text will only contain bind variables
        /// This function will provide variable declarations and initalizations so that the results
        /// can be tested.
        /// </summary>
        /// <param name="dbCmd">DAAB DbCommand object</param>
        /// Returns a back-end compliant script that can be executed in an interactive editor
        public override String GetCommandDebugScript(DbCommand dbCmd)
        {
            try
            {
                SqlCommand sqlCmd = (SqlCommand)dbCmd;
                StringBuilder sb = new StringBuilder();
                StringBuilder sbParamList = new StringBuilder();
                SortedDictionary<string, SqlParameter> cmdParams = new SortedDictionary<string
                        , SqlParameter>(StringComparer.CurrentCultureIgnoreCase);
                foreach (SqlParameter param in sqlCmd.Parameters)
                    cmdParams.Add(param.ParameterName, param);
                foreach (string paramName in cmdParams.Keys)
                {
                    SqlParameter param = cmdParams[paramName];
                    sb.AppendFormat("declare {0} {1} {2}"
                        , param.ParameterName
                        , GetParamTypeDecl(param)
                        , Environment.NewLine);
                    if (param.Direction != ParameterDirection.Output
                        && param.Value != null
                        && param.Value != DBNull.Value)
                        sb.AppendFormat("set {0} = {1} {2}"
                            , param.ParameterName
                            , param.SqlDbType == SqlDbType.Char
                                || param.SqlDbType == SqlDbType.DateTime
                                || param.SqlDbType == SqlDbType.NChar
                                || param.SqlDbType == SqlDbType.NVarChar
                                || param.SqlDbType == SqlDbType.UniqueIdentifier
                                || param.SqlDbType == SqlDbType.VarChar
                                || param.SqlDbType == SqlDbType.Xml
                                ? string.Format("'{0}'"
                                    , param.SqlDbType == SqlDbType.DateTime
                                    ? Convert.ToDateTime(param.Value).ToString("yyyy-MM-dd HH:mm:ss.fff")
                                    : param.Value.ToString().Replace("'", "''"))
                                : param.Value
                            , Environment.NewLine);
                    if (sqlCmd.CommandType == CommandType.StoredProcedure)
                        sbParamList.AppendFormat("{0}{1}{2}"
                            , param.ParameterName
                            , sbParamList.Length > 0 ? ", " : ""
                            , param.Direction == ParameterDirection.Output ? " out" : "");
                }
                if (sqlCmd.CommandType == CommandType.StoredProcedure)
                {
                    sb.AppendFormat("execute {0} {1} {2}"
                        , sqlCmd.CommandText
                        , sbParamList.ToString()
                        , Environment.NewLine);
                }
                else sb.AppendFormat("{0} {1}", sqlCmd.CommandText, Environment.NewLine);
                return sb.ToString();
            }
            catch (Exception e)
            {
                return string.Format("-- Error while trying to convert command text to debug string; return commandText {0}"
                        + "--Error: {1}{0}{2}{0}"
                        , Environment.NewLine
                        , e.Message
                        , dbCmd.CommandText);
            }
        }

        private String GetParamTypeDecl(SqlParameter sqlParam)
        {
            if (sqlParam.SqlDbType == SqlDbType.VarChar
                || sqlParam.SqlDbType == SqlDbType.NVarChar)
                return string.Format("{0}({1})"
                    , sqlParam.SqlDbType
                    , sqlParam.Size <= 0 ? "max" : sqlParam.Size.ToString());
            if (sqlParam.SqlDbType == SqlDbType.Decimal
                && sqlParam.Precision != 0
                || sqlParam.Scale != 0)
                return string.Format("{0}({1}, {2})"
                    , sqlParam.SqlDbType
                    , sqlParam.Precision
                    , sqlParam.Scale);
            return string.Format("{0}", sqlParam.SqlDbType);
        }

    }
}
