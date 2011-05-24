using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Xml;
using System.Threading;

using Microsoft.Practices.EnterpriseLibrary.Data;
using Oracle.DataAccess.Client;

using B1.ILoggingManagement;
using B1.IDataAccess;
using B1.Core;

namespace B1.DataAccess.OracleDb
{
    /// <summary>
    /// Provides the Oracle Specific functionality as defined in the IDataAccessProvider interface.
    /// </summary>
    public class OracleHelper : DataAccessProvider
    {
        /// <summary>
        /// Constructs the OracleHelper class
        /// </summary>
        /// <param name="database">DAAB Database object for a Oracle database</param>
        public OracleHelper(Database database)
            : base(database)
        {
        }

        /// <summary>
        /// Returns the back-end compliant syntax for a command that performs no operation.
        /// e.g: -- in SqlServer and Db2.
        /// </summary>
        public override string NoOpDbCommandText { get { return Constants.NoOpDbCommandText; } }

        /// <summary>
        /// Returns the string character that prefixes parameters for the specific back-end
        /// e.g. @ in SqlServer and Db2
        /// </summary>
        public override string ParameterPrefix { get { return Constants.ParameterPrefix; } }

        /// <summary>
        /// Returns the string character that prefixes bind variables for the specific back-end
        /// e.g. @ in SqlServer and Db2
        /// </summary>
        public override string BindValuePrefix { get { return Constants.BindValuePrefix; } }

        /// <summary>
        /// Supports the Version property
        /// </summary>
        /// <returns>Version number string for the Database Provider DLL</returns>
        protected override string ProviderVersion()
        {
            DataRow[] dbFactory = _dbFactories.Select("InvariantName = 'Oracle.DataAccess.Client'");
            _version = dbFactory[0]["AssemblyQualifiedName"].ToString().Split('=')[1].Split(',')[0];
            return _version;
        }

        /// <summary>
        /// Adjusts the given command text so that it is back-end compliant.
        /// e.g. wraps in Begin / End block
        /// </summary>
        /// <param name="commandText">Command Text of a DAAB DbCommand object</param>
        /// <returns></returns>
        public override string FormatCommandText(string newCommandText)
        {
            if (!Functions.IsLastCharInText(newCommandText, ';'))
                newCommandText += ";";
            return string.Format("begin {0} end;", newCommandText.Replace(Environment.NewLine, " "));
        }

        /// <summary>
        /// Provides an opportunity to make any property settings to the given DbCommand object.
        /// e.g. InitialLOBFetchSize for Oracle
        /// </summary>
        /// <param name="dbCmd">A DAAB DbCommand object</param>
        /// <returns>A DAAB DbCommand object</returns>
        public override DbCommand FormatDbCommand(DbCommand dbCmd)
        {
            OracleCommand oracleCmd = (OracleCommand)dbCmd;
            oracleCmd.InitialLOBFetchSize = 4000;
            oracleCmd.InitialLONGFetchSize = 4000;
            return oracleCmd;
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
            OracleCommand oracleCommand = (OracleCommand)dbCommand;
            using (OracleConnection con = (OracleConnection)_database.CreateConnection())
            {
                con.Open();
                oracleCommand.Connection = con;
                if (dbTran != null)
                    oracleCommand.Transaction = (OracleTransaction)dbTran;
                return oracleCommand.ExecuteXmlReader();
            }
        }

        /// <summary>
        /// Returns a back-end compliant sql syntax for beggining a command block in a transaction.
        /// Note this is used for Compound SQL where multiple statements are formatted in a single
        /// DbCommand.CommandText.
        /// </summary>
        /// <param name="tranCount">Used for nested transactions as a comment for readability</param>
        /// <returns>A code fragment which will perform the appropriate operation.</returns>
        public override string BeginTransaction(Int32 tranCount)
        {
            return string.Format("begin /* tran_{0} */{1}", tranCount, Environment.NewLine);
        }

        /// <summary>
        /// Returns a back-end compliant sql syntax for completing a command block in a transaction.
        /// Note this is used for Compound SQL where multiple statements are formatted in a single
        /// DbCommand.CommandText.
        /// </summary>
        /// <param name="tranCount">Used for nested transactions as a comment for readability</param>
        /// <returns>A code fragment which will perform the appropriate operation.</returns>
        public override string CommitTransaction(Int32 tranCount)
        {
            StringBuilder tran = new StringBuilder();
            tran.AppendFormat("commit work; /* tran_{0} */{1}", tranCount, Environment.NewLine);
            tran.AppendFormat("Exception {0}", Environment.NewLine);
            tran.AppendFormat("when others then begin {0}", Environment.NewLine);
            tran.AppendFormat("rollback; {0}", Environment.NewLine);
            tran.AppendFormat("raise;{0}", Environment.NewLine);
            tran.AppendFormat("end;{0}", Environment.NewLine);
            tran.AppendFormat("end;{0}", Environment.NewLine);
            return tran.ToString();
        }

        /// <summary>
        /// Derives the parameters of the given DbCommand object
        /// </summary>
        /// <param name="dbCmd">A DAAB DbCommand object</param>
        public override void DeriveParameters(DbCommand dbCmd)
        {
            OracleCommand oracleCmd = (OracleCommand)dbCmd;
            oracleCmd.Connection = (OracleConnection)_database.CreateConnection();
            try
            {
                oracleCmd.Connection.Open();
                OracleCommandBuilder.DeriveParameters(oracleCmd);
            }
            finally
            {
                oracleCmd.Connection.Close();
                oracleCmd.Connection.Dispose();
            }
        }

        /// <summary>
        /// Returns the back-end compliant sql fragment for getting the row count for the last operation.
        /// This is not the same as COUNT(*);  It is more like @@RowCount of SQLServer
        /// </summary>
        /// <param name="rowCountParam">A parameter name to store the result of the rowcount function</param>
        /// <returns>A code fragment which will store the rowcount into the given parameter</returns>
        public override string FormatRowCountSql(string rowCountParam)
        {
            return string.Format("{0} := sql%rowcount {1};", BuildBindVariableName(rowCountParam), Environment.NewLine);
        }


        /// <summary>
        /// Returns the back-end compliant sql fragment for performing Date Arithametic.
        /// Depending on the parameters, the function will add (Days, Hours, ... , milliseconds)
        /// </summary>
        /// <param name="dateDiffInterval">Enumeration of the possible intervals (Days, Hours, Minutes.. MilliSeconds)</param>
        /// <param name="duration">If duration is a string, it will be parameterized; otherwise it will be a constant</param>
        /// <param name="startDate">If startDate is a string, it will be assumed to be a column name;
        /// if it is a dateEnumeration, then it can be either UTC, Local or default.</param>
        /// <returns>A code fragment which will perform the appropriate date add operation.</returns>
        public override string FormatDateMathSql(EnumDateDiffInterval dateDiffInterval
                                        , object startDate
                                        , object duration)
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

            // do the date math
            switch (dateDiffInterval)
            {
                case EnumDateDiffInterval.Hour:
                    return string.Format("({0} + ({1}/24))", startDate, durationParam);
                case EnumDateDiffInterval.Minute:
                    return string.Format("({0} + ({1}/1440))", startDate, durationParam);     //1440=24*60
                case EnumDateDiffInterval.Second:
                    return string.Format("({0} + ({1}/86400))", startDate, durationParam);    //=24*60*60
                case EnumDateDiffInterval.MilliSecond:
                    return string.Format("( to_timestamp( to_char({0}, 'dd-mm-yyyy hh24:mi:ss.') || "
                            + " to_char ( to_number( to_char( {0}, 'FF') ) + {1}), 'dd-mm-yyyy hh24:mi:ss.FF'))", startDate, durationParam);
                case EnumDateDiffInterval.Day:
                default:
                    return string.Format("({0} + {1})", startDate, durationParam);
            }
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
            string dbTime = dbDateType == EnumDateTimeLocale.Local ? "systimestamp" : "sys_extract_utc(systimestamp)";
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
            return string.Format("select {0} from dual", GetDbTimeAs(dbDateType, returnAsAlias));
        }

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
        public override object GetOutParamValue(DbCommand dbCommand, string paramName)
        {
            if (dbCommand.Parameters[BuildParameterName(paramName)].Value is Oracle.DataAccess.Types.OracleDecimal)
                return (Decimal)(Oracle.DataAccess.Types.OracleDecimal)dbCommand.Parameters[BuildParameterName(paramName)].Value;
            else return dbCommand.Parameters[BuildParameterName(paramName)].Value;
        }

        /// <summary>
        /// With Oracle ODP.NET, we must call built in function IsNull instead of comparing to DBNull.Value
        /// This method provides a consistent interface for testing out params for null
        /// </summary>
        /// <param name="dbCommand">DbCommand object</param>
        /// <param name="paramName">The name of the parameter to test</param>
        /// <returns>Boolean indicating if the parameter's value is null</returns>
        public override bool IsOutParamValueNull(DbCommand dbCommand, string paramName)
        {
            if (dbCommand.Parameters[BuildParameterName(paramName)].Value is Oracle.DataAccess.Types.OracleDecimal)
                return ((Oracle.DataAccess.Types.OracleDecimal)dbCommand.Parameters[BuildParameterName(paramName)].Value).IsNull;
            else return dbCommand.Parameters[BuildParameterName(paramName)].Value == DBNull.Value
                   || dbCommand.Parameters[BuildParameterName(paramName)].Value == null;
        }

        /// <summary>
        /// Return the Oracle specific statement for executing a stored procedure
        /// </summary>
        /// <param name="storedProcedure">Name of stored procedure.</param>
        /// <param name="dbParameters">OracleParameter collection</param>
        /// <returns>An Oracle compliant statement for executing the given stored procedure and parameters.</returns>
        public override string GenerateStoredProcedureCall(string storedProcedure
                                , DbParameterCollection dbParameters)
        {
            OracleParameterCollection oracleParameters = (OracleParameterCollection)dbParameters;
            StringBuilder commandText = new StringBuilder(storedProcedure);
            if (oracleParameters != null && oracleParameters.Count > 0)
            {
                bool firstParam = true;
                foreach (OracleParameter param in oracleParameters)
                {
                    if (param.Direction == ParameterDirection.ReturnValue)
                        commandText.Insert(0, param.ParameterName + " = ");
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
        /// Returns the .Net dataType for the given
        /// database native dataType.
        /// </summary>
        /// <param name="nativeDataType">String representation of the database native data type</param>
        /// <returns>String representation of the .Net data type</returns>
        public override string GetDotNetDataTypeFromNativeDataType(string nativeDataType)
        {
            nativeDataType = nativeDataType.ToLower();
            if (nativeDataType == Constants.DataTypeChar
                    || nativeDataType == Constants.DataTypeNChar
                    || nativeDataType == Constants.DataTypeClob
                    || nativeDataType == Constants.DataTypeXml
                    || nativeDataType == Constants.DataTypeXmlType
                    || nativeDataType == Constants.DataTypeVarChar2
                    || nativeDataType == Constants.DataTypeNVarChar2
                    || nativeDataType == Constants.DataTypeNClob)
                return IDataAccess.Constants.SystemString;
            else if (nativeDataType == Constants.DataTypeDate
                    || nativeDataType == Constants.DataTypeTimeStamp6
                    || nativeDataType == Constants.DataTypeTimeStamp
                    || nativeDataType == Constants.DataTypeTimeStampLTZ
                    || nativeDataType == Constants.DataTypeTimeStampTZ)
                return IDataAccess.Constants.SystemDateTime;
            else if (nativeDataType == Constants.DataTypeFloat
                    || nativeDataType == Constants.DataTypeDouble)
                return IDataAccess.Constants.SystemDouble;
            else if (nativeDataType == Constants.DataTypeNumber
                    || nativeDataType == Constants.DataTypeDecimal)
                return IDataAccess.Constants.SystemDecimal;
            else throw new ExceptionEvent(enumExceptionEventCodes.InvalidParameterValue
                    , string.Format("nativeDataType; {0} was not defined as a DotNetType.", nativeDataType));
        }

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
        public override DbType GetGenericDbTypeFromNativeDataType(string nativeDataType, Int16 size, Int16 scale)
        {
            if (nativeDataType.ToLower() == Constants.DataTypeNumber)
                if (size > 8)
                    return scale > 0 ? DbType.Decimal : DbType.Int64;
                else if (size > 4)
                    return scale > 0 ? DbType.Decimal : DbType.Int32;
                else if (size > 2)
                    return scale > 0 ? DbType.Decimal : DbType.Int16;
                else if (size > 0)
                    return scale > 0 ? DbType.Decimal : DbType.Byte;
                else return DbType.Decimal;
            throw new ExceptionEvent(enumExceptionEventCodes.InvalidParameterValue
                , string.Format("nativeDataType; {0} was not defined as a DotNetType.", nativeDataType));
        }

        /// <summary>
        /// Returns the .Net dataType for the given
        /// database native dataType.
        /// </summary>
        /// <param name="nativeDataType">String representation of the database native data type</param>
        /// <returns>String representation of the .Net data type</returns>
        public override DbType GetGenericDbTypeFromNativeDataType(string nativeDataType)
        {
            nativeDataType = nativeDataType.ToLower();
            if (nativeDataType == Constants.DataTypeChar
                    || nativeDataType == Constants.DataTypeNChar
                    || nativeDataType == Constants.DataTypeClob
                    || nativeDataType == Constants.DataTypeXml
                    || nativeDataType == Constants.DataTypeXmlType
                    || nativeDataType == Constants.DataTypeVarChar2
                    || nativeDataType == Constants.DataTypeNVarChar2
                    || nativeDataType == Constants.DataTypeNClob)
                return DbType.String;
            else if (nativeDataType == Constants.DataTypeDate
                    || nativeDataType == Constants.DataTypeTimeStamp6
                    || nativeDataType == Constants.DataTypeTimeStamp
                    || nativeDataType == Constants.DataTypeTimeStampLTZ
                    || nativeDataType == Constants.DataTypeTimeStampTZ)
                return DbType.DateTime;
            else if (nativeDataType == Constants.DataTypeFloat
                    || nativeDataType == Constants.DataTypeDouble)
                return DbType.Double;
            else if (nativeDataType == Constants.DataTypeNumber
                    || nativeDataType == Constants.DataTypeDecimal)
                return DbType.Decimal;
            else if (nativeDataType == Constants.DataTypeXml
                    || nativeDataType == Constants.DataTypeXmlType)
                return DbType.Xml;
            else if (nativeDataType == Constants.DataTypeRef_Cursor
                    || nativeDataType == Constants.DataTypeRefCursor)
                return DbType.Object;
            throw new ExceptionEvent(enumExceptionEventCodes.InvalidParameterValue
                , string.Format("nativeDataType; {0} was not defined as a DotNetType.", nativeDataType));
        }

        private OracleDbType GetOracleDataTypeFromNativeDataType(string nativeDataType, DbType paramType)
        {
            nativeDataType = nativeDataType.ToLower();
            if (nativeDataType == Constants.DataTypeChar)
                return OracleDbType.Char;
            if (nativeDataType == Constants.DataTypeNChar)
                return OracleDbType.NChar;
            if (nativeDataType == Constants.DataTypeClob)
                return OracleDbType.Clob;
            if (nativeDataType == Constants.DataTypeNClob)
                return OracleDbType.NClob;
            if (nativeDataType == Constants.DataTypeXml
                || nativeDataType == Constants.DataTypeXmlType)
                return OracleDbType.XmlType;
            if (nativeDataType == Constants.DataTypeVarChar2)
                return OracleDbType.Varchar2;
            if (nativeDataType == Constants.DataTypeNVarChar2)
                return OracleDbType.NVarchar2;
            if (nativeDataType == Constants.DataTypeDate)
                return OracleDbType.Date;
            if (nativeDataType == Constants.DataTypeTimeStamp6
                || nativeDataType == Constants.DataTypeTimeStamp
                || nativeDataType == Constants.DataTypeTimeStampLTZ
                || nativeDataType == Constants.DataTypeTimeStampTZ)
                return OracleDbType.TimeStamp;
            if (nativeDataType == Constants.DataTypeFloat
                || nativeDataType == Constants.DataTypeDouble) 
                return OracleDbType.Double;
            if (nativeDataType == Constants.DataTypeNumber)
            {
                if (paramType == DbType.Int16)
                    return OracleDbType.Int16;
                if (paramType == DbType.Int32)
                    return OracleDbType.Int32;
                if (paramType == DbType.Int64)
                    return OracleDbType.Int64;
                if (paramType == DbType.Decimal)
                    return OracleDbType.Decimal;
            }
            if (nativeDataType == Constants.DataTypeRefCursor
                || nativeDataType == Constants.DataTypeRef_Cursor)
                return OracleDbType.RefCursor;

            throw new ExceptionEvent(enumExceptionEventCodes.InvalidParameterValue
                        , string.Format("nativeDataType; {0} was not defined as a DotNetType.", nativeDataType));
        }


        /// <summary>
        /// Returns the .Net dataType for the given
        /// database numeric dataType.
        /// </summary>
        /// <param name="nativeDataType">.Net specific dataType</param>
        /// <param name="size">Numeric size of the dataType</param>
        /// <param name="scale">Numeric scale of the dataType</param>
        /// <returns>.Net DataType equivalent</returns>
        public override string GetDotNetDataTypeFromNativeDataType(string nativeDataType, Int16 size, Int16 scale)
        {
            if (nativeDataType.ToLower() == Constants.DataTypeNumber)
                if (size > 8)
                    return scale > 0 ? IDataAccess.Constants.SystemDouble : IDataAccess.Constants.SystemInt64;
                else if (size > 4)
                    return scale > 0 ? IDataAccess.Constants.SystemSingle : IDataAccess.Constants.SystemInt32;
                else if (size > 2)
                    return scale > 0 ? IDataAccess.Constants.SystemSingle : IDataAccess.Constants.SystemInt16;
                else if (size > 0)
                    return scale > 0 ? IDataAccess.Constants.SystemSingle : IDataAccess.Constants.SystemByte;
                else return IDataAccess.Constants.SystemDouble;

            throw new ExceptionEvent(enumExceptionEventCodes.InvalidParameterValue
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
            OracleParameter oracleParam1 = (OracleParameter)dbParam1;
            OracleParameter oracleParam2 = (OracleParameter)dbParam2;
            switch (oracleParam1.OracleDbType)
            {
                case OracleDbType.Int16:
                    return Convert.ToInt16(oracleParam1.Value) == Convert.ToInt16(oracleParam2.Value);
                case OracleDbType.Int32:
                    return Convert.ToInt32(oracleParam1.Value) == Convert.ToInt32(oracleParam2.Value);
                case OracleDbType.Int64:
                    return Convert.ToInt64(oracleParam1.Value) == Convert.ToInt64(oracleParam2.Value);
                case OracleDbType.Double:
                case OracleDbType.Decimal:
                    return Convert.ToDouble(oracleParam1.Value) == Convert.ToDouble(oracleParam2.Value);
                case OracleDbType.Char:
                case OracleDbType.NChar:
                case OracleDbType.Varchar2:
                case OracleDbType.NVarchar2:
                    return oracleParam1.Value.ToString().ToLower() == oracleParam2.Value.ToString().ToLower();
                case OracleDbType.Date:
                    return Convert.ToDateTime(oracleParam1.Value) == Convert.ToDateTime(oracleParam2.Value);
                default:
                    return oracleParam1.Value == oracleParam2.Value;
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
            , string nativeDbType
            , Int32 maxLength
            , ParameterDirection paramDirection
            , object paramValue)
        {
            OracleDbType? oracleDbType = null;
            if (!string.IsNullOrEmpty(nativeDbType))
                oracleDbType = GetOracleDataTypeFromNativeDataType(nativeDbType, paramType);
            else oracleDbType = OracleDbType.Varchar2;

            if (paramName.Length > Constants.ParamNameMaxLength)
                throw new ExceptionEvent(enumExceptionEventCodes.InvalidParameterValue
                            , string.Format("ParameterName; Length of {0} is {1}; {2} char max length"
                                , paramName
                                , paramName.Length
                                , Constants.ParamNameMaxLength));

            OracleParameter newParam = new OracleParameter();
            newParam.ParameterName = paramName;
            newParam.Value = paramValue;
            newParam.DbType = paramType;
            newParam.OracleDbType = oracleDbType.Value;
            newParam.Direction = paramDirection;
            return ValidateParam(newParam, maxLength, paramType, oracleDbType.Value);
        }

        private static OracleParameter ValidateParam(OracleParameter dbParam
            , Int32 size
            , DbType paramType
            , OracleDbType oracleDbType)
        {
            if (dbParam.Direction == ParameterDirection.InputOutput
                || dbParam.Direction == ParameterDirection.Output)
                dbParam.Size = size;

            if (dbParam.Value == null)
            {
                dbParam.Value = DBNull.Value;
                dbParam.DbType = paramType;
                dbParam.OracleDbType = oracleDbType;
            }
            if (dbParam.OracleDbType == OracleDbType.Clob)
                dbParam.DbType = DbType.String;
            return dbParam;
        }

        /// <summary>
        /// Returns a clone of the given parameter
        /// </summary>
        /// <param name="dbParam">The DbParameter to clone</param>
        /// <returns>A copy of the DbParameter</returns>
        public override DbParameter CloneParameter(DbParameter dbParam)
        {
            // DO NOT MOVE THE ORDER THAT THESE ATTRIBUTES ARE SET
            // SETTING ONE ATTRIBUTE WILL HAVE AN EFFECT ON ANOTHER
            //
            OracleParameter oracleParam = (OracleParameter)dbParam;
            OracleParameter cloneParam = new OracleParameter();
            cloneParam.ParameterName = dbParam.ParameterName;
            cloneParam.ArrayBindSize = oracleParam.ArrayBindSize;
            cloneParam.ArrayBindStatus = oracleParam.ArrayBindStatus;
            cloneParam.CollectionType = oracleParam.CollectionType;
            cloneParam.Offset = oracleParam.Offset;
            cloneParam.Direction = oracleParam.Direction;
            cloneParam.Value = oracleParam.Value;
            cloneParam.DbType = oracleParam.DbType;
            cloneParam.OracleDbType = oracleParam.OracleDbType;
            cloneParam.SourceColumn = oracleParam.SourceColumn;
            cloneParam.SourceColumnNullMapping = oracleParam.SourceColumnNullMapping;
            cloneParam.SourceVersion = oracleParam.SourceVersion;
            cloneParam.ParameterName = oracleParam.ParameterName;
            cloneParam.IsNullable = oracleParam.IsNullable;

            return ValidateParam(cloneParam, dbParam.Size, dbParam.DbType, oracleParam.OracleDbType);
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
            OracleParameterCollection oracleParameters = (OracleParameterCollection)dbParameters;
            OracleParameter oracleParam = (OracleParameter)dbParam;
            if (oracleParameters.Contains(oracleParam.ParameterName))
                throw new ExceptionEvent(enumExceptionEventCodes.DbParameterExistsInCollection
                        , string.Format("Parameter {0} already belongs to this collection; use Set to change value."
                                , oracleParam.ParameterName));

            oracleParameters.Add(CloneParameter(oracleParam));
            return oracleParameters[oracleParam.ParameterName];
        }

        /// <summary>
        /// Returns a clone of the given DbParameter collection.
        /// </summary>
        /// <param name="dbParameters">The collection to clone</param>
        /// <returns>A copy of the DbParameter collection</returns>
        public override DbParameterCollection CloneParameterCollection(DbParameterCollection srcDbCollection)
        {
            OracleParameterCollection srcOracleCollection = (OracleParameterCollection)srcDbCollection;
            OracleParameterCollection tgtOracleCollection = (OracleParameterCollection)
                    _database.GetSqlStringCommand(Constants.NoOpDbCommandText).Parameters;
            foreach (OracleParameter oracleParam in srcOracleCollection)
                CopyParameterToCollection(tgtOracleCollection, oracleParam);
            return tgtOracleCollection;
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
                OracleCommand cmd = (OracleCommand)((OracleCommand)dbCmd).Clone();
                StringBuilder sb = new StringBuilder();
                StringBuilder sbParamList = new StringBuilder();
                OracleParameterCollection cmdParams = cmd.Parameters;
                // since a command contains bind variables, we cannot execute those in a command window (like Toad)
                // we must replace the bindVariable indicator : with some other prefix to distinguish it from a column
                // for example: :Param ==> bv_Param
                // As a result we must make sure that the param name length is not violated
                // we do that by keeping the leftmost portion of the param name 
                Int32 newCount = cmdParams.Count;
                SortedDictionary<string, string> bindVariables = new SortedDictionary<string
                        , string>(StringComparer.CurrentCultureIgnoreCase);
                foreach (OracleParameter param in cmdParams)
                {
                    string bindName = "bv_" + param.ParameterName;

                    if ((cmdParams.Contains(bindName) || bindName.Length > Constants.ParamNameMaxLength)
                        || bindVariables.ContainsKey(bindName))
                    {
                        while ((cmdParams.Contains(bindName) || bindName.Length > Constants.ParamNameMaxLength)
                            || bindVariables.ContainsKey(bindName))
                        {
                            string bindPrefix = "bv_" + (newCount++).ToString();
                            bindName = bindPrefix + bindName.Substring(bindPrefix.Length, Constants.ParamNameMaxLength
                                - (bindPrefix.Length));
                        }
                    }
                    if (!bindVariables.ContainsKey(bindName))
                        bindVariables.Add(bindName, param.ParameterName);
                }
                foreach (string bindName in bindVariables.Keys)
                {
                    OracleParameter param = cmdParams[bindVariables[bindName]];
                    sb.AppendFormat("{0}{1} {2} := {3};{4}"
                        , sb.Length == 0 ? "declare " : ""
                        , bindName
                        , GetParamTypeDecl(param)
                        , GetParamValue(param)
                        , Environment.NewLine);
                    if (cmd.CommandType == CommandType.StoredProcedure)
                        sbParamList.AppendFormat("{0}{1}"
                            , bindName
                            , sbParamList.Length > 0 ? ", " : "");
                    cmd.CommandText = cmd.CommandText.ToLower().Replace(
                        BuildBindVariableName(param.ParameterName.ToLower()), bindName.ToLower());
                }

                cmd.CommandText = cmd.CommandText.Replace(";", string.Format(";{0}", Environment.NewLine));
                cmd.CommandText = cmd.CommandText.ToLower().Replace("begin ", string.Format("begin {0}"
                        , Environment.NewLine));
                if (cmd.CommandType == CommandType.StoredProcedure)
                {
                    sb.AppendFormat("begin{2}{0} ({1});{2}end;{2}"
                        , cmd.CommandText
                        , sbParamList.ToString()
                        , Environment.NewLine);
                }
                else sb.AppendFormat("begin{1}{0} {1}end;{1}", cmd.CommandText, Environment.NewLine);
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

        private static String GetParamTypeDecl(OracleParameter oracleParam)
        {
            if (oracleParam.OracleDbType == OracleDbType.Varchar2
                || oracleParam.OracleDbType == OracleDbType.NVarchar2)
                return string.Format("{0}({1})"
                    , oracleParam.OracleDbType == OracleDbType.Varchar2 ? "varchar2" : "nvarchar2"
                    , oracleParam.Value.ToString().Length > 0 ? oracleParam.Value.ToString().Length : 1);
            else if (oracleParam.OracleDbType == OracleDbType.Char
                || oracleParam.OracleDbType == OracleDbType.NChar)
                return string.Format("{0}({1})"
                    , oracleParam.OracleDbType == OracleDbType.Char ? "char" : "nchar"
                    , oracleParam.Value.ToString().Length > 0 ? oracleParam.Value.ToString().Length : 1);
            else if (oracleParam.OracleDbType == OracleDbType.Date)
                return "date";
            else if (oracleParam.OracleDbType == OracleDbType.Int16
                    || oracleParam.OracleDbType == OracleDbType.Int32
                    || oracleParam.OracleDbType == OracleDbType.Int64
                    || oracleParam.OracleDbType == OracleDbType.Byte
                    || oracleParam.OracleDbType == OracleDbType.Decimal)
                return "number";
            else if (oracleParam.OracleDbType == OracleDbType.Double)
                return "float";
            else if (oracleParam.OracleDbType == OracleDbType.RefCursor)
                return "sys_refCursor";
            else if (oracleParam.OracleDbType == OracleDbType.Clob)
                return "clob";
            else return string.Format("{0}"
                , oracleParam.OracleDbType);
        }


        private static string GetParamValue(OracleParameter oracleParam)
        {
            if (oracleParam.Direction != ParameterDirection.Output
                    && oracleParam.Value != null
                    && oracleParam.Value != DBNull.Value)
                if (oracleParam.OracleDbType == OracleDbType.Char
                            || oracleParam.OracleDbType == OracleDbType.NChar
                            || oracleParam.OracleDbType == OracleDbType.NVarchar2
                            || oracleParam.OracleDbType == OracleDbType.Varchar2
                            || oracleParam.OracleDbType == OracleDbType.Clob)
                    return string.Format("'{0}'", oracleParam.Value.ToString().Replace("'", "''"));
                else if (oracleParam.OracleDbType == OracleDbType.Date
                        || oracleParam.OracleDbType == OracleDbType.TimeStamp)
                    return string.Format("to_date('{0}', 'YYYY/MM/DD HH:MI:SS')"
                        , Convert.ToDateTime(oracleParam.Value).ToString("yyyy/MM/dd hh:mm:ss"));
                else return oracleParam.Value.ToString();
            else return "null";
        }
    }
}
