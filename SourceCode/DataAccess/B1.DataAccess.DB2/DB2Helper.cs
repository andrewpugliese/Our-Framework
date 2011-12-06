using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Xml;
using System.Threading;

using Microsoft.Practices.EnterpriseLibrary.Data;
using IBM.Data.DB2;

using B1.ILoggingManagement;
using B1.IDataAccess;
using B1.Core;

namespace B1.DataAccess.DB2
{
    /// <summary>
    /// Provides the Db2 Specific functionality as defined in the IDataAccessProvider interface.
    /// </summary>
    public class DB2Helper : DataAccessProvider
    {
        /// <summary>
        /// Constructs the Db2Helper class
        /// </summary>
        /// <param name="database">DAAB Database object for a Db2 database</param>
        public DB2Helper(Database database)
            : base(database)
        {
        }

        /// <summary>
        /// Returns the string version of the dll loaded for this provider
        /// </summary>
        /// <returns>The version string of the registered factories for this provider</returns>
        protected override string ProviderVersion()
        {
            DataRow[] dbFactory = _dbFactories.Select("InvariantName = 'IBM.Data.DB2'");
            _version = dbFactory[0]["AssemblyQualifiedName"].ToString().Split('=')[1].Split(',')[0];
            return _version;
        }

        /// <summary>
        /// Derives the parameters of the given DbCommand object
        /// </summary>
        /// <param name="dbCmd">A DAAB DbCommand object</param>
        public override void DeriveParameters(DbCommand dbCmd)
        {
            DB2Command db2Cmd = (DB2Command)dbCmd;
            db2Cmd.Connection = (DB2Connection)_database.CreateConnection();
            try
            {
                db2Cmd.Connection.Open();
                DB2CommandBuilder.DeriveParameters(db2Cmd);
                DB2CommandBuilder db = new DB2CommandBuilder();
            }
            finally
            {
                db2Cmd.Connection.Close();
                db2Cmd.Connection.Dispose();
            }
        }

        /// <summary>
        /// Returns a PL/SQL compliant command block by wrapping it with a begin end clause.
        /// Replaces newline character with a space
        /// </summary>
        /// <param name="newCommandText">The current command text.</param>
        /// <returns>PL/SQL compliant command block</returns>
        public override string FormatCommandText(string newCommandText)
        {
            return string.Format("BEGIN {0} END", newCommandText);
        }

        /// <summary>
        /// Returns the back-end compliant sql fragment for getting the row count for the last operation.
        /// This is not the same as COUNT(*);  It is more like @@RowCount of SQLServer
        /// </summary>
        /// <param name="rowCountParam">A parameter name to store the result of the rowcount function</param>
        /// <returns>A code fragment which will store the rowcount into the given parameter</returns>
        public override string FormatRowCountSql(string rowCountParam)
        {
            return string.Format("GET DIAGNOSTICS {0} := ROW_COUNT {1};", BuildBindVariableName(rowCountParam), Environment.NewLine);
        }
        /// <summary>
        /// Returns the Db2 compliant sql fragment for performing Date Arithametic.
        /// Depending on the parameters, the function will add (Days, Hours, ... , milliseconds)
        /// NOTE: DB2 does not support milliseconds; instead it supports microseconds.  So the value
        /// provided for milliseconds will be multiplied by 100 and defined as microseconds.
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
            // db2 does not have milliseconds, substitute milliseconds with Microseconds
            return string.Format("{0} + {1} {2}", startDateParam
                , dateDiffInterval == EnumDateDiffInterval.MilliSecond ? string.Format("(100 * {0})", durationParam) : durationParam
                , dateDiffInterval == EnumDateDiffInterval.MilliSecond ? "Microsecond" : dateDiffInterval.ToString());
        }

        /// <summary>
        /// Returns and XmlReader object from the database command object
        /// </summary>
        /// <param name="oracleCommand">Database Command Object</param>
        /// <param name="dbTran">Transaction or null</param>
        /// <returns>XmlReader</returns>
        public override XmlReader ExecuteXmlReader(DbCommand dbCommand
                , DbTransaction dbTran)
        {
            DB2Command db2Command = (DB2Command)dbCommand;
            using (DB2Connection con = (DB2Connection)_database.CreateConnection())
            {
                con.Open();
                db2Command.Connection = con;
                if (dbTran != null)
                    db2Command.Transaction = (DB2Transaction)dbTran;
                return db2Command.ExecuteXmlReader();
            }
        }

        /// <summary>
        /// Returns the statements to start a new transaction within a dbCommand block
        /// NOTE: DB2 Users, this statement has NO AFFECT and does NOT begin a new transaction block
        /// </summary>
        /// <returns></returns>
        public override string BeginTransaction(Int32 tranCount)
        {
            return string.Format("BEGIN ATOMIC /* tran_{0} */{1}", tranCount, Environment.NewLine);
        }

        /// <summary>
        /// Returns the end keyword of a transaction block
        /// NOTE: DB2 Users, this statement has NO AFFECT and does NOT commit a transaction block
        /// </summary>
        /// <returns></returns>
        public override string CommitTransaction(Int32 tranCount)
        {
            return string.Format("END; /* tran_{0} */{1}", tranCount, Environment.NewLine);
        }

        /// <summary>
        /// Returns the backend specific function for current datetime
        /// as a string e.g. sysdate or getdate() to be used in a seperate command
        /// if ReturnAsAlias is not null, it will be the alias
        /// </summary>
        /// <param name="dbDateType">The format type of the date function</param>
        /// <param name="returnAsAlias">What the return column will be called</param>
        /// <returns>Backend specific function for current date time</returns>
        public override string GetDbTimeAs(EnumDateTimeLocale dbDateType, string returnAsAlias)
        {
            string dbTime = dbDateType == EnumDateTimeLocale.Local ? "current_timestamp" : "current_timestamp - current_timezone";
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
            return string.Format("select {0} from sysibm.dual", GetDbTimeAs(dbDateType, returnAsAlias));
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
            , string nativeDbType
            , Int32 maxLength
            , ParameterDirection paramDirection
            , object paramValue)
        {
            DB2Type? db2Type = null;
            if (!string.IsNullOrEmpty(nativeDbType))
                db2Type = GetDB2DataTypeFromNativeDataType(nativeDbType, paramType);

            if (!paramName.Contains(Constants.ParameterPrefix))
                paramName = Constants.ParameterPrefix + paramName;
            DB2Parameter newParam = new DB2Parameter(paramName, db2Type);
            newParam.Value = paramValue;
            newParam.Direction = paramDirection;
            return ValidateParam(newParam, maxLength, paramType);
        }

        private static DB2Parameter ValidateParam(DB2Parameter dbParam, Int32 size, DbType dbType)
        {
            if (dbParam.Direction == ParameterDirection.InputOutput
                || dbParam.Direction == ParameterDirection.Output)
                dbParam.Size = size;

            if (dbParam.Value == null)
            {
                dbParam.Value = DBNull.Value;
                dbParam.DbType = dbType;
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
            DB2Parameter db2Param = (DB2Parameter)dbParam;
            DB2Parameter cloneParam = new DB2Parameter(db2Param.ParameterName, db2Param.DB2Type);
            cloneParam.ArrayLength = db2Param.ArrayLength;
            cloneParam.DB2TypeOutput = db2Param.DB2TypeOutput;
            cloneParam.Direction = db2Param.Direction;
            cloneParam.Value = db2Param.Value;
            cloneParam.DbType = db2Param.DbType;
            cloneParam.Scale = db2Param.Scale;
            cloneParam.SourceColumn = db2Param.SourceColumn;
            cloneParam.SourceColumnNullMapping = db2Param.SourceColumnNullMapping;
            cloneParam.SourceVersion = db2Param.SourceVersion;
            cloneParam.Precision = db2Param.Precision;
            cloneParam.ParameterName = db2Param.ParameterName;
            cloneParam.IsDefault = db2Param.IsDefault;
            cloneParam.IsNullable = db2Param.IsNullable;
            cloneParam.IsUnassigned = db2Param.IsUnassigned;

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
            DB2ParameterCollection db2Parameters = (DB2ParameterCollection)dbParameters;
            DB2Parameter db2Param = (DB2Parameter)dbParam;

            if (db2Parameters.Contains(db2Param.ParameterName))
                throw new ExceptionEvent(enumExceptionEventCodes.DbParameterExistsInCollection
                        , string.Format("Parameter {0} already belongs to this collection; use Set to change value."
                            , db2Param.ParameterName));
            db2Parameters.Add(CloneParameter(db2Param));
            return db2Parameters[db2Param.ParameterName];
        }

        /// <summary>
        /// Returns a clone of the given DbParameter collection.
        /// </summary>
        /// <param name="dbParameters">The collection to clone</param>
        /// <returns>A copy of the DbParameter collection</returns>
        public override DbParameterCollection CloneParameterCollection(DbParameterCollection dbParameters)
        {
            DB2ParameterCollection srcDb2Parameters = (DB2ParameterCollection)dbParameters;
            DB2ParameterCollection tgtDb2Parameters = (DB2ParameterCollection)
                _database.GetSqlStringCommand(Constants.NoOpDbCommandText).Parameters;
            foreach (DB2Parameter dbdParam in dbParameters)
                CopyParameterToCollection(tgtDb2Parameters, dbdParam);
            return tgtDb2Parameters;
        }

        /// <summary>
        /// Return the DB2 specific statement for executing a stored procedure
        /// </summary>
        /// <param name="storedProcedure">Name of stored procedure.</param>
        /// <param name="dbParameters">DB2Parameter collection</param>
        /// <returns>An DB2 compliant statement for executing the given stored procedure and parameters.</returns>
        public override string GenerateStoredProcedureCall(string storedProcedure
                                , DbParameterCollection dbParameters)
        {
            DB2ParameterCollection db2Parameters = (DB2ParameterCollection)dbParameters;

            StringBuilder commandText = new StringBuilder();
            if (db2Parameters != null && db2Parameters.Count > 0)
            {
                commandText.AppendFormat("call {0}", storedProcedure);
                bool firstParam = true;
                foreach (DB2Parameter param in db2Parameters)
                {
                    if (param.Direction == ParameterDirection.ReturnValue)
                        commandText.Insert(0, BuildBindVariableName(param.ParameterName) + " = ");
                    commandText.AppendFormat("{0} {1}"
                        , firstParam ? "(" : ", "
                        , BuildBindVariableName(param.ParameterName));
                    firstParam = false;
                }
                if (!firstParam)
                    commandText.Append(")");
            }
            commandText.Append(";");
            return commandText.ToString();
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
            DB2Parameter db2Param1 = (DB2Parameter)dbParam1;
            DB2Parameter db2Param2 = (DB2Parameter)dbParam2;
            switch (db2Param1.DB2Type)
            {
                case DB2Type.SmallInt:
                    return Convert.ToInt16(db2Param1.Value) == Convert.ToInt16(db2Param2.Value);
                case DB2Type.Integer:
                    return Convert.ToInt32(db2Param1.Value) == Convert.ToInt32(db2Param2.Value);
                case DB2Type.BigInt:
                    return Convert.ToInt64(db2Param1.Value) == Convert.ToInt64(db2Param2.Value);
                case DB2Type.Decimal:
                case DB2Type.Numeric:
                    return Convert.ToDecimal(db2Param1.Value) == Convert.ToDecimal(db2Param2.Value);
                case DB2Type.Real:
                case DB2Type.Real370:
                    return Convert.ToSingle(db2Param1.Value) == Convert.ToSingle(db2Param2.Value);
                case DB2Type.Time:
                case DB2Type.DecimalFloat:
                case DB2Type.Double:
                case DB2Type.Float:
                    return Convert.ToDouble(db2Param1.Value) == Convert.ToDouble(db2Param2.Value);
                case DB2Type.Char:
                case DB2Type.Clob:
                case DB2Type.VarChar:
                case DB2Type.Xml:
                case DB2Type.Graphic:
                case DB2Type.VarGraphic:
                case DB2Type.LongVarChar:
                case DB2Type.LongVarGraphic:
                case DB2Type.DbClob:
                case DB2Type.RowId:
                    return db2Param1.Value.ToString().ToLower() == db2Param2.Value.ToString().ToLower();
                case DB2Type.Date:
                case DB2Type.Timestamp:
                case DB2Type.TimeStampWithTimeZone:
                    return Convert.ToDateTime(db2Param1.Value) == Convert.ToDateTime(db2Param2.Value);
                default:
                    return db2Param1.Value == db2Param2.Value;
            }
        }


        private static DB2Type GetDB2DataTypeFromNativeDataType(string nativeDataType, DbType paramType)
        {
            nativeDataType = nativeDataType.ToLower();
            if (nativeDataType == Constants.DataTypeChar)
                return DB2Type.Char;
            if (nativeDataType == Constants.DataTypeClob)
                return DB2Type.Clob;
            if (nativeDataType == Constants.DataTypeLongVarChar)
                return DB2Type.LongVarChar;
            if (nativeDataType == Constants.DataTypeGraphic)
                return DB2Type.Graphic;
            if (nativeDataType == Constants.DataTypeVarChar)
                return DB2Type.VarChar;
            if (nativeDataType == Constants.DataTypeVarGraphic)
                return DB2Type.VarGraphic;
            if (nativeDataType == Constants.DataTypeLongVarGraphic)
                return DB2Type.LongVarGraphic;
            if (nativeDataType == Constants.DataTypeRowId)
                return DB2Type.RowId;
            if (nativeDataType == Constants.DataTypeDbClob)
                return DB2Type.DbClob;
            if (nativeDataType == Constants.DataTypeDate)
                return DB2Type.Date;
            if (nativeDataType == Constants.DataTypeTimeStamp)
                return DB2Type.Timestamp;
            if (nativeDataType == Constants.DataTypeTimeStampTZ)
                return DB2Type.TimeStampWithTimeZone;
            if (nativeDataType == Constants.DataTypeXml)
                return DB2Type.Xml;
            if (nativeDataType == Constants.DataTypeTime)
                return DB2Type.Time;
            if (nativeDataType == Constants.DataTypeSmallInt)
                return DB2Type.SmallInt;
            if (nativeDataType == Constants.DataTypeInt)
                return DB2Type.Integer;
            if (nativeDataType == Constants.DataTypeBigInt)
                return DB2Type.BigInt;
            if (nativeDataType == Constants.DataTypeNumeric)
                return DB2Type.Numeric;
            if (nativeDataType == Constants.DataTypeDecimal)
                return DB2Type.Decimal;
            if (nativeDataType == Constants.DataTypeReal)
                return DB2Type.Real;
            if (nativeDataType == Constants.DataTypeReal370)
                return DB2Type.Real370;
            if (nativeDataType == Constants.DataTypeDouble)
                return DB2Type.Double;
            if (nativeDataType == Constants.DataTypeDecimalFloat)
                return DB2Type.DecimalFloat;
            if (nativeDataType == Constants.DataTypeFloat)
                return DB2Type.Float;
            if (nativeDataType == Constants.DataTypeBinary)
                return DB2Type.Binary;
            if (nativeDataType == Constants.DataTypeBinaryXml)
                return DB2Type.BinaryXml;
            if (nativeDataType == Constants.DataTypeLongVarBinary)
                return DB2Type.LongVarBinary;
            if (nativeDataType == Constants.DataTypeBlob)
                return DB2Type.Blob;

            throw new ExceptionEvent(enumExceptionEventCodes.InvalidParameterValue
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
            if (nativeDataType == Constants.DataTypeChar
                || nativeDataType == Constants.DataTypeClob
                || nativeDataType == Constants.DataTypeLongVarChar
                || nativeDataType == Constants.DataTypeGraphic
                || nativeDataType == Constants.DataTypeVarChar
                || nativeDataType == Constants.DataTypeVarGraphic
                || nativeDataType == Constants.DataTypeLongVarGraphic
                || nativeDataType == Constants.DataTypeRowId
                || nativeDataType == Constants.DataTypeDbClob)
                return DbType.String;

            if (nativeDataType == Constants.DataTypeDate
                || nativeDataType == Constants.DataTypeTimeStamp
                || nativeDataType == Constants.DataTypeTimeStampTZ)
                return DbType.DateTime;

            if (nativeDataType == Constants.DataTypeXml)
                return DbType.Xml;

            if (nativeDataType == Constants.DataTypeTime)
                return DbType.DateTimeOffset;

            if (nativeDataType == Constants.DataTypeSmallInt)
                return DbType.Int16;

            if (nativeDataType == Constants.DataTypeInt)
                return DbType.Int32;

            if (nativeDataType == Constants.DataTypeBigInt)
                return DbType.Int64;

            if (nativeDataType == Constants.DataTypeNumeric
                 || nativeDataType == Constants.DataTypeDecimal)
                return DbType.Decimal;

            if (nativeDataType == Constants.DataTypeReal
                || nativeDataType == Constants.DataTypeReal370)
                return DbType.Single;

            if (nativeDataType == Constants.DataTypeDouble
                || nativeDataType == Constants.DataTypeDecimalFloat
                || nativeDataType == Constants.DataTypeFloat)
                return DbType.Double;

            if (nativeDataType == Constants.DataTypeBinary
                || nativeDataType == Constants.DataTypeBinaryXml
                || nativeDataType == Constants.DataTypeLongVarBinary
                || nativeDataType == Constants.DataTypeBlob)
                return DbType.Binary;

            throw new ExceptionEvent(enumExceptionEventCodes.InvalidParameterValue
                , string.Format("nativeDataType; {0} was not defined as a DotNetType.", nativeDataType));

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
            if (nativeDataType == Constants.DataTypeChar
                || nativeDataType == Constants.DataTypeClob
                || nativeDataType == Constants.DataTypeLongVarChar
                || nativeDataType == Constants.DataTypeGraphic
                || nativeDataType == Constants.DataTypeVarChar
                || nativeDataType == Constants.DataTypeVarGraphic
                || nativeDataType == Constants.DataTypeLongVarGraphic
                || nativeDataType == Constants.DataTypeXml
                || nativeDataType == Constants.DataTypeRowId
                || nativeDataType == Constants.DataTypeDbClob)
                return IDataAccess.Constants.SystemString;

            if (nativeDataType == Constants.DataTypeDate
                || nativeDataType == Constants.DataTypeTimeStamp
                || nativeDataType == Constants.DataTypeTimeStampTZ)
                return IDataAccess.Constants.SystemDateTime;

            if (nativeDataType == Constants.DataTypeTime)
                return IDataAccess.Constants.SystemTimeSpan;

            if (nativeDataType == Constants.DataTypeSmallInt)
                return IDataAccess.Constants.SystemInt16;

            if (nativeDataType == Constants.DataTypeInt)
                return IDataAccess.Constants.SystemInt32;

            if (nativeDataType == Constants.DataTypeBigInt)
                return IDataAccess.Constants.SystemInt64;

            if (nativeDataType == Constants.DataTypeNumeric
                 || nativeDataType == Constants.DataTypeDecimal)
                return IDataAccess.Constants.SystemDecimal;

            if (nativeDataType == Constants.DataTypeReal
                || nativeDataType == Constants.DataTypeReal370)
                return IDataAccess.Constants.SystemSingle;

            if (nativeDataType == Constants.DataTypeDouble
                || nativeDataType == Constants.DataTypeDecimalFloat
                || nativeDataType == Constants.DataTypeFloat)
                return IDataAccess.Constants.SystemDouble;

            if (nativeDataType == Constants.DataTypeBinary
                || nativeDataType == Constants.DataTypeBinaryXml
                || nativeDataType == Constants.DataTypeLongVarBinary
                || nativeDataType == Constants.DataTypeBlob)
                return IDataAccess.Constants.SystemByteArray;

            // if we still did not find a match
            throw new ExceptionEvent(enumExceptionEventCodes.InvalidParameterValue
                , string.Format("nativeDataType; {0} was not defined as a DotNetType.", nativeDataType));
        }

        /// <summary>
        /// Returns a DB2 compliant script declaring the parameters of the DB2Command
        /// setting their values and executing the parameterized sql statement so that the
        /// command can be debugged in Management Studio.
        /// </summary>
        /// <param name="db2Cmd"></param>
        /// <returns></returns>
        public override String GetCommandDebugScript(DbCommand dbCmd)
        {
            try
            {
                DB2Command db2Cmd = (DB2Command)dbCmd;
                StringBuilder sb = new StringBuilder();
                StringBuilder sbDeclare = new StringBuilder();
                StringBuilder sbParamList = new StringBuilder();
                SortedDictionary<string, DB2Parameter> cmdParams = new SortedDictionary<string
                        , DB2Parameter>(StringComparer.CurrentCultureIgnoreCase);
                foreach (DB2Parameter param in db2Cmd.Parameters)
                    cmdParams.Add(BuildBindVariableName(param.ParameterName), param);

                sbDeclare.AppendFormat("--SET TERMINATOR / {0}BEGIN {0}", Environment.NewLine);
                foreach (string paramName in cmdParams.Keys)
                {
                    DB2Parameter param = cmdParams[paramName];
                    sbDeclare.AppendFormat("declare {0} {1}; {2}"
                        , paramName
                        , GetParamTypeDecl(param)
                        , Environment.NewLine);
                    if (param.Direction != ParameterDirection.Output
                        && param.Value != null
                        && param.Value != DBNull.Value)
                        sb.AppendFormat("set {0} = {1}; {2}"
                            , paramName
                            , param.DB2Type == DB2Type.Char
                                || param.DB2Type == DB2Type.Date
                                || param.DB2Type == DB2Type.LongVarChar
                                || param.DB2Type == DB2Type.Timestamp
                                || param.DB2Type == DB2Type.TimeStampWithTimeZone
                                || param.DB2Type == DB2Type.Time
                                || param.DB2Type == DB2Type.VarChar
                                || param.DB2Type == DB2Type.Xml
                                ? string.Format("'{0}'"
                                    , (param.DB2Type == DB2Type.Date
                                        || param.DB2Type == DB2Type.Time
                                        || param.DB2Type == DB2Type.Timestamp
                                        || param.DB2Type == DB2Type.TimeStampWithTimeZone)
                                    ? Convert.ToDateTime(param.Value).ToString("yyyy-MM-dd HH:mm:ss.fff")
                                    : param.Value.ToString().Replace("'", "''"))
                                : param.Value
                            , Environment.NewLine);
                    if (db2Cmd.CommandType == CommandType.StoredProcedure)
                        sbParamList.AppendFormat("{0}{1}"
                            , paramName
                            , sbParamList.Length > 0 ? ", " : "");
                }
                if (db2Cmd.CommandType == CommandType.StoredProcedure)
                {
                    sb.AppendFormat("call {0} ({1}); {2}"
                        , db2Cmd.CommandText
                        , sbParamList.ToString()
                        , Environment.NewLine);
                }
                else sb.AppendFormat("{0} {1}END {1}", db2Cmd.CommandText, Environment.NewLine);
                return sbDeclare.ToString() + sb.ToString();
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

        private static String GetParamTypeDecl(DB2Parameter db2Param)
        {
            if (db2Param.DB2Type == DB2Type.VarChar
                || db2Param.DB2Type == DB2Type.Char
                || db2Param.DB2Type == DB2Type.LongVarChar
                || db2Param.DB2Type == DB2Type.Xml)
                return string.Format("{0}({1})"
                    , db2Param.DB2Type
                    , db2Param.Size <= 0 ? "500" : db2Param.Size.ToString());
            if (db2Param.DB2Type == DB2Type.Decimal
                && db2Param.Precision != 0
                || db2Param.Scale != 0)
                return string.Format("{0}({1}, {2})"
                    , db2Param.DB2Type
                    , db2Param.Precision
                    , db2Param.Scale);
            return string.Format("{0}", db2Param.DB2Type);
        }

        /// <summary>
        /// Returns a boolean indicating whether or not the given dbException is for a primary key constraint
        /// </summary>
        /// <param name="dbe">DbException object</param>
        /// <returns>True if dbException is a primary key violation</returns>
        public override bool IsPrimaryKeyViolation(DbException dbException)
        {
            DB2Exception db2Exception = (DB2Exception)dbException;
            if (db2Exception.ErrorCode == DB2.Constants.DBError_UniqueConstraintViolation)
                return true;
            return false;
        }

    }
}
