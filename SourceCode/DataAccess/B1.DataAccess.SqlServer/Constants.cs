using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace B1.DataAccess.SqlServer
{
    internal class Constants
    {
        internal const Int32 DBError_CommandTimeOut = -2;
        internal const Int32 DBError_LockTimeOut = 1222;
        internal const Int32 DBError_UniqueConstraintViolation = 2601;
        internal const Int32 DBError_ForeignKeyViolation = 547;
        internal const Int32 DBError_UniqueKeyViolation = 2627;
        internal const Int32 DBError_QueryTimeOut = 8649;
        internal const Int32 DBError_NullConstraintViolation = 515;
        internal const string NoOpDbCommandText = "--";
        internal const string DefaultTableAlias = "T";
        internal const string ParameterPrefix = "@";
        internal const string BindValuePrefix = "@";
        public static readonly string DataTypeChar = SqlDbType.Char.ToString().ToLower();
        public static readonly string DataTypeNChar = SqlDbType.NChar.ToString().ToLower();
        public static readonly string DataTypeVarChar = SqlDbType.VarChar.ToString().ToLower();
        public static readonly string DataTypeNVarChar = SqlDbType.NVarChar.ToString().ToLower();
        public static readonly string DataTypeXml = SqlDbType.Xml.ToString().ToLower();
        public static readonly string DataTypeDate = SqlDbType.Date.ToString().ToLower();
        public static readonly string DataTypeDateTime = SqlDbType.DateTime.ToString().ToLower();
        public static readonly string DataTypeSmallDateTime = SqlDbType.SmallDateTime.ToString().ToLower();
        public static readonly string DataTypeDateTime2 = SqlDbType.DateTime2.ToString().ToLower();
        public static readonly string DataTypeTimeStamp = SqlDbType.Timestamp.ToString().ToLower();
        public static readonly string DataTypeTime = SqlDbType.Time.ToString().ToLower();
        public static readonly string DataTypeInt = SqlDbType.Int.ToString().ToLower();
        public static readonly string DataTypeSmallInt = SqlDbType.SmallInt.ToString().ToLower();
        public static readonly string DataTypeBigInt = SqlDbType.BigInt.ToString().ToLower();
        public static readonly string DataTypeMoney = SqlDbType.Money.ToString().ToLower();
        public static readonly string DataTypeSmallMoney = SqlDbType.SmallMoney.ToString().ToLower();
        public static readonly string DataTypeDecimal = SqlDbType.Decimal.ToString().ToLower();
        public static readonly string DataTypeReal = SqlDbType.Real.ToString().ToLower();
        public static readonly string DataTypeFloat = SqlDbType.Float.ToString().ToLower();
        public static readonly string DataTypeBinary = SqlDbType.Binary.ToString().ToLower();
        public static readonly string DataTypeText = SqlDbType.Text.ToString().ToLower();
        public static readonly string DataTypeNText = SqlDbType.NText.ToString().ToLower();
        public static readonly string DataTypeTinyint = SqlDbType.TinyInt.ToString().ToLower();
        public static readonly string DataTypeBit = SqlDbType.Bit.ToString().ToLower();
        public static readonly string DataTypeUniqueId = SqlDbType.UniqueIdentifier.ToString().ToLower();
    }
}
