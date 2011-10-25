using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using B1.Core;

namespace B1.DataAccess
{
    /// <summary>
    /// Class of constants that are associated primarily with the database
    /// </summary>
    public class Constants
    {
        internal const string USP_UniqueIdsGetNextBlock = SCHEMA_CORE + "." + "usp_UniqueIdsGetNextBlock";
        internal const string USP_CatalogGetPrimaryKeys = SCHEMA_CORE + "." + "usp_CatalogGetPrimaryKeys";
        internal const string USP_CatalogGetIndexes = SCHEMA_CORE + "." + "usp_CatalogGetIndexes";
        internal const string USP_CatalogGetForeignKeys = SCHEMA_CORE + "." + "usp_CatalogGetFkeys";
        internal const string USP_CatalogGetColumns = SCHEMA_CORE + "." + "usp_CatalogGetColumns";
 
        internal const string SQL_Select_UniqueIds = "select " + UniqueIdKey + ", " + UniqueIdValue 
                                    + ", " + CacheBlockSize + " from " + SCHEMA_CORE + "." + TABLE_UniqueIds;

        internal const string SQL_Update_UniqueIds_SetUniqueId = "update " + SCHEMA_CORE + "." + TABLE_UniqueIds 
                                    + " set " + UniqueIdValue + " = {0} where " + UniqueIdKey 
                                    +  " = {1} and " + UniqueIdValue + " = {2}";

        internal const string SQL_Update_UniqueIds_SetMaxAndRolloverId = "update " + SCHEMA_CORE + "." + TABLE_UniqueIds
                                    + " set " + MaxIdValue + " = {0} "
                                    + ", " + RolloverIdValue + " = {1} where " + UniqueIdKey
                                    + " = {2}";

        /// <summary>
        /// Returns the schema string which owns the core objects
        /// </summary>
        public const string SCHEMA_CORE = "B1";
        /// <summary>
        /// Returns the connection key string
        /// </summary>
        public const string ConnectionKey = "ConnectionKey";

        internal const string TABLE_UniqueIds = "UniqueIds";
        /// <summary>
        /// Database table used to demo functionality
        /// </summary>
        public const string TABLE_TestSequence = "TestSequence";
        /// <summary>
        /// Database table used to hold the database catalog metadata about columns
        /// </summary>
        public const string TABLE_CatalogColumns = "CatalogColumns";
        /// <summary>
        /// Database table used to hold the database catalog metadata about indexes
        /// </summary>
        public const string TABLE_CatalogIndexes = "CatalogIndexes";
        /// <summary>
        /// Database table used to hold the database catalog metadata about tables
        /// </summary>
        public const string TABLE_CatalogTables = "CatalogTables";
        /// <summary>
        /// Database table used to hold the application configuration settings
        /// </summary>
        public const string TABLE_AppConfigSettings = "AppConfigSettings";

        internal const string All_Constraints = "All_Constraints";
        internal const string All_Cons_Columns = "All_Cons_Columns";
        internal const string All_Ind_Columns = "All_Ind_Columns";
        internal const string All_Ind_Expressions = "All_Ind_Expressions";
        internal const string All_Indexes = "All_Indexes";
        internal const string All_Tab_Columns = "All_Tab_Columns";
        internal const string AppConfigSetName = "AppConfigSetName";
        internal const string BlockAmount = "BlockAmount";
        internal const string CacheBlockSize = "CacheBlockSize";
        internal const string Char_Length = "char_length";
        internal const string Character_Mximum_Length = "character_maximum_length";
        internal const string CharacterMaximumLength = "CharacterMaximumLength";
        internal const string ColumnFunction = "ColumnFunction";
        internal const string ColId = "ColId";
        internal const string ColName = "ColName";
        internal const string ColOrder = "ColOrder";
        internal const string ColSeq = "ColSeq";
        internal const string Column_Expression = "Column_Expression";
        internal const string Column_Name = "Column_Name";
        internal const string Column_Position = "Column_Position";
        internal const string ColumnName = "ColumnName";
        internal const string Column_Default = "Column_Default";
        internal const string ColumnDefault = "ColumnDefault";
        internal const string Column_Id = "Column_Id";
        internal const string Columns = "Columns";
        internal const string ConstraintName = "ConstraintName";
        internal const string Constraint_Catalog = "Constraint_Catalog";
        internal const string Constraint_Name = "Constraint_Name";
        internal const string Constraint_Object_Id = "Constraint_Object_Id";
        internal const string Constraint_Schema = "Constraint_Schema";
        internal const string Constraint_Type = "Constraint_Type";
        internal const string Data_Type = "Data_Type";
        internal const string Data_Default = "Data_Default";
        internal const string Data_Length = "Data_Length";
        internal const string Data_Precision = "Data_Precision";
        internal const string Data_Scale = "Data_Scale";
        internal const string DataType = "DataType";
        internal const string DataTime_Precision = "DateTime_Precision";
        internal const string DataTimePrecision = "DateTimePrecision";
        internal const string DbProviderFactoryDB2 = "IBM.Data.DB2.DB2Factory";
        internal const string DbProviderFactoryOracle = "Oracle.DataAccess.Client.OracleClientFactory";
        internal const string Descend = "Descend";
        internal const string FKColumn_Name = "FKColumn_Name";
        internal const string FKTable_Schem = "FKTable_Schem";
        internal const string FKTable_Name = "FKTable_Name";
        internal const string FK_Name = "FK_Name";
        internal const string ForeignKey = "ForeignKey";
        internal const string ForeignKeyName = "ForeignKeyName";
        internal const string ForeignKeys = "ForeignKeys";
        internal const string Foreign_Key_Columns = "Foreign_Key_Columns";
        internal const string Foreign_Keys = "Foreign_Keys";
        internal const string GlobalConfigSetName = "GlobalConfigSetName";
        internal const string HelperAssembly = "HelperAssembly";
        internal const string HelperClass = "HelperClass";
        internal const string HelperPath = "HelperPath";
        internal const string HelperAssemblyDB2Default = "B1.DataAccess.Db2";
        internal const string HelperAssemblyOracleDefault = "B1.DataAccess.Oracle";
        internal const string HelperAssemblySqlServerDefault = "B1.DataAccess.SqlServer";
        internal const string HelperClassDB2Default = "B1.DataAccess.Db2.DB2Helper";
        internal const string HelperClassOracleDefault = "B1.DataAccess.OracleDb.OracleHelper";
        internal const string HelperClassSqlServerDefault = "B1.DataAccess.SqlServer.SqlServerHelper";
        internal const string HelperPathDB2Default = "";
        internal const string HelperPathOracleDefault = "";
        internal const string HelperPathSqlServerDefault = "";
        internal const string Id = "Id";
        internal const string IDataAccessHelperDB2 = IDataAccessProviderDB2 + "." + "DB2Helper";
        internal const string IDataAccessProviderDB2 = SCHEMA_CORE + "." + "DataAccess.DB2";
        internal const string IDataAccessHelperOracle = IDataAccessProviderOracle + "Db." + "OracleHelper";
        internal const string IDataAccessProviderOracle = SCHEMA_CORE + "." + "DataAccess.Oracle";
        internal const string IDataAccessHelperSqlServer = IDataAccessProviderSqlServer + "." + "SqlServerHelper";
        internal const string IDataAccessProviderSqlServer = SCHEMA_CORE + "." + "DataAccess.SqlServer";
        internal const string IndSchema = "IndSchema";
        internal const string Index_Name = "Index_Name";
        internal const string Index_Owner = "Index_Owner";
        internal const string IndexName = "IndexName";
        internal const string IndName = "IndName";
        internal const string Index_Columns = "Index_Columns";
        internal const string Index_Id = "Index_Id";
        internal const string Indexes = "Indexes";
        internal const string IndexColUse = "IndexColUse";
        internal const string IndexType = "IndexType";
        internal const string Information_Schema = "Information_Schema";
        internal const string Is_Computed = "Is_Computed";
        internal const string Is_Descending_Key = "Is_Descending_Key";
        internal const string IsComputed = "IsComputed";
        internal const string IsDescend = "IsDescend";
        internal const string Is_Identity = "Is_Identity";
        internal const string IsIdentity = "IsIdentity";
        internal const string Is_Nullable = "Is_Nullable";
        internal const string IsNullable = "IsNullable";
        internal const string IsPrimaryKey = "IsPrimaryKey";
        internal const string IsRowGuidCol = "IsRowGuidCol";
        internal const string Is_RowGuidCol = "Is_RowGuidCol";
        internal const string Key_Column_Usage = "Key_Column_Usage";
        internal const string Key_Ordinal = "Key_Ordinal";
        internal const string KeyColUse = "KeyColUse";
        internal const string Key_Seq = "Key_Seq";
        internal const string Is_Primary_Key = "Is_Primary_Key";
        internal const string Is_Unique = "Is_Unique";
        internal const string IsUnique = "IsUnique";
        internal const string MaxIdValue = "MaxIdValue";
        internal const string Name = "name";
        internal const string Numeric_Precision = "Numeric_Precision";
        internal const string NumericPrecision = "NumericPrecision";
        internal const string Numeric_Precision_Radix = "Numeric_Precision_Radix";
        internal const string NumericPrecisionRadix = "NumericPrecisionRadix";
        internal const string Numeric_Scale = "Numeric_Scale";
        internal const string NumericScale = "NumericScale";
        internal const string Nullable = "Nullable";
        internal const string Objects = "Objects";
        internal const string Object_Id = "Object_Id";
        internal const string Ordinal = "Ordinal";
        internal const string Ordinal_Position = "Ordinal_Position";
        internal const string OrdinalPosition = "OrdinalPosition";
        internal const string Owner = "Owner";
        /// <summary>
        /// Default Parameter Name for Paging 'Page Size'
        /// </summary>
        public const string PageSize = "PageSize";
        internal const string ParamNewId = "NewId";
        internal const string ParamDelimiters = ", {0}()[];+-/*.<>=!";
        internal const string Parent_Column_Id = "Parent_Column_Id";
        internal const string Parent_Object_Id = "Parent_Object_Id";
        internal const string Position = "Position";
        internal const string PKColumn_Name = "PKColumn_Name";
        internal const string PKTable_Schem = "PKTable_Schem";
        internal const string PKTable_Name = "PKTable_Name";
        internal const string PK_Name = "PK_Name";
        internal const string PrimaryKeyName = "PrimaryKeyName";
        internal const string PrimaryKey = "PrimaryKey";
        internal const string PrimaryKeys = "PrimaryKeys";
        internal const string Pseudo_Column = "pseudo_column";
        internal const string R_Constraint_Name = "R_Constraint_Name";
        internal const string R_Owner = "R_Owner";
        internal const string RefSchema = "RefSchema";
        internal const string RefTable = "RefTable";
        internal const string RefColumn = "RefColumn";
        internal const string Referenced_Column_Id = "Referenced_Column_Id";
        internal const string Referenced_Object_Id = "Referenced_Object_Id";
        internal const string Return_Value = "Return_Value";
        internal const string RolloverIdValue = "RolloverIdValue";
        internal const string Schema_Id = "Schema_Id";
        internal const string SchemaName = "SchemaName";
        internal const string Schemas = "Schemas";
        internal const string SQLColumns = "SQLColumns";
        internal const string SQLForeignKeys = "SQLForeignKeys";
        internal const string SQLPrimaryKeys = "SQLPrimaryKeys";
        internal const string Sys = "sys";
        internal const string SysCat = "syscat";
        internal const string SysColumns = "SysColumns";
        internal const string SysIbm = "sysibm";
        internal const string TabName = "TabName";
        internal const string Table_Constraints = "Table_Constraints";
        internal const string Table_Name = "Table_Name";
        internal const string Table_Schem = "Table_Schem";
        internal const string Table_Schema = "Table_Schema";
        internal const string TableName = "TableName";
        internal const string Tables = "Tables";
        internal const string Type = "Type";
        internal const string Type_Desc = "Type_Desc";
        internal const string TypeDescription = "TypeDescription";
        internal const string Type_Name = "Type_Name";
        internal const string UniqueIdValue = "UniqueIdValue";
        internal const string UniqueIdKey = "UniqueIdKey";
        internal const string UniqueRule = "UniqueRule";
        internal const string Uniqueness = "Uniqueness";

        internal const string ParamSetValueSuffix = "_sv";
        internal const byte ParamNameOracleMaxLength = 30; // maximum size of a parameter name required for Oracle
        internal const string ParamAliasSuffix = "_a";
        
        // The maximum size, in bytes, of the data within the column. The default value is 0 
        // (to be used when you do not want to specify the maximum size of the value).
        internal const Int32 ParamSizeDefault = 0;     
    }
}
