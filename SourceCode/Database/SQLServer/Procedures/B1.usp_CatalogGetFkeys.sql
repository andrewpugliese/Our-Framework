-- drop existing object if exists
--
if exists (select null from sys.procedures p 
						inner join sys.schemas s
						on p.schema_Id = s.schema_id
						and s.name = 'B1'
						where p.name = 'usp_CatalogGetFKeys')
						drop proc B1.usp_CatalogGetFKeys
go
--						
CREATE PROCEDURE B1.usp_CatalogGetFKeys
(
	@SchemaName varchar(24) = null,
	@TableName varchar(64) = null
)
AS
BEGIN
--
-- Name: Catalog Get Foreign Keys
--
-- Proc which will return the Foreign Columns of all Foreign Keys for given Schema, Table
-- Schema: all tables in schema
-- TableName: all foreign keys of that table (across schemas)
--

select s.name as SchemaName
	, t.name as TableName
	, fk.name as ForeignKey
	, cc.name as ColumnName
	, sRef.name as RefSchema
	, tRef.name as RefTable
	, ccRef.name as RefColumn
	, fkcu.ORDINAL_POSITION as Ordinal
	from sys.tables t
	inner join sys.schemas s
		on s.schema_Id = t.schema_id
	inner join sys.foreign_keys fk
		on t.object_id = fk.parent_object_id
	inner join sys.foreign_key_columns fkc
		on fk.object_id = fkc.constraint_object_id
	inner join sys.columns cc
		on cc.object_id = t.object_id
		and fkc.parent_column_id = cc.column_id
	inner join sys.tables tRef
		on tRef.object_id = fk.referenced_object_id
	inner join sys.schemas sRef
		on sRef.schema_id = tRef.schema_id
	inner join sys.columns ccRef
		on ccRef.object_id = fk.referenced_object_id
		and fkc.referenced_column_id = ccRef.column_id
	inner join INFORMATION_SCHEMA.KEY_COLUMN_USAGE fkcu
		on fkcu.TABLE_SCHEMA = s.name
		and fkcu.TABLE_NAME = t.name
		and fkcu.COLUMN_NAME = cc.name
	where (@SchemaName is null or s.name = @SchemaName)
	and (@TableName is null or t.name = @TableName)
	order by s.name, t.name, fk.name, fkcu.ORDINAL_POSITION
				
END
GO
grant execute on B1.usp_CatalogGetFKeys to public
go