using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Oracle.DataAccess.Client;

namespace B1.DataAccess.OracleDb
{
    internal class Constants
    {
        internal const Int32 DBError_UniqueConstraintViolation = 1;
        internal const Int32 DBError_ForeignKeyViolation = 2291;
        internal const Int32 DBError_NullConstraintViolation = 1400;
        internal const string NoOpDbCommandText = "dbms_output.put_line('--');";
        internal const string DefaultTableAlias = "T";
        internal const byte ParamNameMaxLength = 30;
        internal const string ParameterPrefix = "";
        internal const string BindValuePrefix = ":";
        internal static readonly string DataTypeChar = OracleDbType.Char.ToString().ToLower();
        internal static readonly string DataTypeNChar = OracleDbType.NChar.ToString().ToLower();
        internal static readonly string DataTypeVarChar2 = OracleDbType.Varchar2.ToString().ToLower();
        internal static readonly string DataTypeNVarChar2 = OracleDbType.NVarchar2.ToString().ToLower();
        internal static readonly string DataTypeClob = OracleDbType.Clob.ToString().ToLower();
        internal static readonly string DataTypeNClob = OracleDbType.NClob.ToString().ToLower();
        internal static readonly string DataTypeXml = "xml";
        internal static readonly string DataTypeXmlType = OracleDbType.XmlType.ToString().ToLower();
        internal static readonly string DataTypeDate = OracleDbType.Date.ToString().ToLower();
        internal static readonly string DataTypeTimeStamp = OracleDbType.TimeStamp.ToString().ToLower();
        internal static readonly string DataTypeTimeStamp6 = "timestamp(6)";
        internal static readonly string DataTypeTimeStampLTZ = OracleDbType.TimeStampLTZ.ToString().ToLower();
        internal static readonly string DataTypeTimeStampTZ = OracleDbType.TimeStampTZ.ToString().ToLower();
        internal static readonly string DataTypeByte = OracleDbType.Byte.ToString().ToLower();
        internal static readonly string DataTypeDecimal = OracleDbType.Decimal.ToString().ToLower();
        internal static readonly string DataTypeFloat = "float";
        internal static readonly string DataTypeNumber = "number";
        internal static readonly string DataTypeDouble = OracleDbType.Double.ToString().ToLower();
        internal static readonly string DataTypeInt16 = OracleDbType.Int16.ToString().ToLower();
        internal static readonly string DataTypeInt32 = OracleDbType.Int32.ToString().ToLower();
        internal static readonly string DataTypeInt64 = OracleDbType.Int64.ToString().ToLower();
        internal static readonly string DataTypeRefCursor = OracleDbType.RefCursor.ToString().ToLower();
        internal static readonly string DataTypeRef_Cursor = "ref cursor";
    }
}
