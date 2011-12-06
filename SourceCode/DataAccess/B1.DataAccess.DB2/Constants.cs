using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using IBM.Data.DB2;

namespace B1.DataAccess.DB2
{
    internal class Constants
    {
        internal const Int32 DBError_UniqueConstraintViolation = 1;
        internal const string NoOpDbCommandText = "--";
        internal const string DefaultTableAlias = "T";
        internal const string ParameterPrefix = "@";
        internal const string BindValuePrefix = "@";
        internal static readonly string DataTypeChar = DB2Type.Char.ToString().ToLower();
        internal static readonly string DataTypeClob = DB2Type.Clob.ToString().ToLower();
        internal static readonly string DataTypeLongVarChar = DB2Type.LongVarChar.ToString().ToLower();
        internal static readonly string DataTypeGraphic = DB2Type.Graphic.ToString().ToLower();
        internal static readonly string DataTypeVarChar = DB2Type.VarChar.ToString().ToLower();
        internal static readonly string DataTypeVarGraphic = DB2Type.VarGraphic.ToString().ToLower();
        internal static readonly string DataTypeLongVarGraphic = DB2Type.LongVarGraphic.ToString().ToLower();
        internal static readonly string DataTypeXml = DB2Type.Xml.ToString().ToLower();
        internal static readonly string DataTypeRowId = DB2Type.RowId.ToString().ToLower();
        internal static readonly string DataTypeDbClob = DB2Type.DbClob.ToString().ToLower();
        internal static readonly string DataTypeDate = DB2Type.Date.ToString().ToLower();
        internal static readonly string DataTypeTimeStamp = DB2Type.Timestamp.ToString().ToLower();
        internal static readonly string DataTypeTimeStampTZ = DB2Type.TimeStampWithTimeZone.ToString().ToLower();
        internal static readonly string DataTypeTime = DB2Type.Time.ToString().ToLower();

        internal static readonly string DataTypeSmallInt = DB2Type.SmallInt.ToString().ToLower();

        internal static readonly string DataTypeInt = DB2Type.Integer.ToString().ToLower();

        internal static readonly string DataTypeBigInt = DB2Type.BigInt.ToString().ToLower();

        internal static readonly string DataTypeNumeric = DB2Type.Numeric.ToString().ToLower();
        internal static readonly string DataTypeDecimal = DB2Type.Decimal.ToString().ToLower();

        internal static readonly string DataTypeReal = DB2Type.Real.ToString().ToLower();
        internal static readonly string DataTypeReal370 = DB2Type.Real370.ToString().ToLower();

        internal static readonly string DataTypeDouble = DB2Type.Double.ToString().ToLower();
        internal static readonly string DataTypeDecimalFloat = DB2Type.DecimalFloat.ToString().ToLower();
        internal static readonly string DataTypeFloat = DB2Type.Float.ToString().ToLower();

        internal static readonly string DataTypeBinary = DB2Type.Binary.ToString().ToLower();
        internal static readonly string DataTypeBinaryXml = DB2Type.BinaryXml.ToString().ToLower();
        internal static readonly string DataTypeLongVarBinary = DB2Type.LongVarBinary.ToString().ToLower();
        internal static readonly string DataTypeBlob = DB2Type.Blob.ToString().ToLower();
    }
}
