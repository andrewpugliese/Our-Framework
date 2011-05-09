-- drop existing object if exists
--
if exists (select null from sys.procedures p 
						inner join sys.schemas s
						on p.schema_Id = s.schema_id
						and s.name = 'B1'
						where p.name = 'usp_CatalogGetPrimaryKeys')
						drop proc B1.usp_CatalogGetPrimaryKeys
go
--						
CREATE PROCEDURE B1.usp_CatalogGetPrimaryKeys
(
	@SchemaName varchar(24) = null,
	@TableName varchar(64) = null
)
AS
BEGIN
--
-- Name: Catalog Get PrimaryKeys
--
-- Proc which will return the Primary Key Columns of all Tables
-- Schema: all tables in schema
-- TableName: all tables with that name (across schemas)
-- UseSystemViews: Indicates if metadata should be retrieved directly
--	database system tables
--

	
	SELECT T.TABLE_SCHEMA as SchemaName
		, T.TABLE_NAME as TableName
		, T.CONSTRAINT_NAME as PrimaryKeyName
		, PK.COLUMN_NAME as ColumnName
		, Pk.ORDINAL_POSITION Ordinal
	FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS T
		INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE PK
			ON PK.Constraint_Name = T.Constraint_Name
				AND PK.Table_Schema = T.Constraint_Schema
	WHERE T.CONSTRAINT_TYPE = 'PRIMARY KEY'
		AND T.CONSTRAINT_CATALOG = DB_NAME()
		AND (@SchemaName IS NULL OR T.TABLE_SCHEMA = @SchemaName)
		AND (@TableName IS NULL OR T.TABLE_NAME = @TableName)
	ORDER BY T.CONSTRAINT_SCHEMA, T.TABLE_NAME, T.CONSTRAINT_NAME, PK.ORDINAL_POSITION
	
	
END
GO
grant execute on B1.usp_CatalogGetPrimaryKeys to public
go