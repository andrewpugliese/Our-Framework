-- drop existing object if exists
--
if exists (select null from sys.procedures p 
						inner join sys.schemas s
						on p.schema_Id = s.schema_id
						and s.name = 'B1'
						where p.name = 'usp_CatalogGetIndexes')
						drop proc B1.usp_CatalogGetIndexes
go
--						
CREATE PROCEDURE B1.usp_CatalogGetIndexes
(
	@SchemaName varchar(24) = null,
	@TableName varchar(64) = null
)
AS
BEGIN
--
-- Name: Catalog Get Indexes
--
-- Proc which will return the Index Columns of all Indexes
-- Schema: all tables in schema
-- TableName: all tables with that name (across schemas)
--


		select s.name as SchemaName
		, o.name as TableName
		, i.name as IndexName
		, c.name as ColumnName
		, is_unique as IsUnique
		, i.type_desc as TypeDescription
		, is_descending_key as IsDescend
		, null as columnFunction
		, key_ordinal as Ordinal
		, is_primary_key as IsPrimaryKey
		from sys.indexes i
		inner join sys.index_columns ik
			on i.object_id = ik.object_id
			and ik.index_id = i.index_id
		inner join syscolumns c
			on c.id = i.object_id
			and c.colid = ik.column_Id
		inner join sys.objects o
			on o.object_id = i.object_id
			and o.type = 'U'
		inner join sys.schemas s
		on s.schema_id = o.schema_id
		where (@SchemaName is null or s.name = @SchemaName)
		and (@TableName is null or o.name = @TableName)
		order by SchemaName, TableName, IndexName, key_ordinal
				
END
GO
grant execute on B1.usp_CatalogGetIndexes to public
go