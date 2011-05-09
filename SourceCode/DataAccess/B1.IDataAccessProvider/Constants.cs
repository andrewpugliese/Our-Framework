using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace B1.IDataAccess
{
#pragma warning disable 1591 // disable the xmlComments warning
    public static class Constants
    {
        internal const string BindValuePrefix = "@";    // used for SqlServer and Db2
        internal const string DefaultTableAlias = "T";  // alias to use when joining tables
        internal const string NoOpDbCommandText = "--"; // comment for most databases
        internal const string ParameterPrefix = "@";    // used for SqlServer and Db2
        public const string RefCursor = "RefCursor";
        public const string SystemBoolean = "System.Boolean";
        public const string SystemByte = "System.Byte";
        public const string SystemByteArray = "System.Byte[]";
        public const string SystemDateTime = "System.DateTime";
        public const string SystemDecimal = "System.Decimal";
        public const string SystemDouble = "System.Double";
        public const string SystemGuid = "System.Guid";
        public const string SystemInt16 = "System.Int16";
        public const string SystemInt32 = "System.Int32";
        public const string SystemInt64 = "System.Int64";
        public const string SystemObject = "System.Object";
        public const string SystemSingle = "System.Single";
        public const string SystemString = "System.String";
        public const string SystemTimeSpan = "System.TimeSpan";
    }
#pragma warning restore 1591 
}
