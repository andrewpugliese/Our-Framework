-- drop existing object if exists
--
if exists (select null from sys.procedures p 
						inner join sys.schemas s
						on p.schema_Id = s.schema_id
						and s.name = 'B1'
						where p.name = 'usp_CatalogGetColumns')
						drop proc B1.usp_CatalogGetColumns
go
--						
create proc B1.usp_CatalogGetColumns
(
@SchemaName varchar(24) = null,
@TableName varchar(64) = null
)
as
begin
--
-- Name: Catalog Get Columns
--
-- Proc will select content of the application catalog based on given Schema. 
-- Schema: all columns for all tables in schema
-- TableName: all columns for all tables with that name (across schemas)


	select s.name as SchemaName
		, t.name as TableName
		, c.name as ColumnName
		, data_type as DataType
		, column_id as OrdinalPosition
		, Column_Default as ColumnDefault
		, case when isc.is_nullable = 'yes' then 1
			else 0 
			end as IsNullable
		, is_rowguidcol as IsRowGuidCol
		, is_identity as IsIdentity
		, is_computed as IsComputed
		, character_maximum_length as CharacterMaximumLength
		, numeric_precision as NumericPrecision
		, numeric_precision_radix as NumericPrecisionRadix
		, numeric_scale as NumericScale
		, datetime_precision as DateTimePrecision
	from sys.tables t
	inner join sys.schemas s
		on s.schema_Id = t.schema_id
	inner join sys.columns c
		on c.object_Id = t.object_id
	inner join Information_Schema.Columns isc
		on isc.Table_Schema = s.name
		and isc.Table_Name = t.name
		and isc.Column_Name = c.Name
	where (@SchemaName is null or s.name = @SchemaName)
	and (@TableName is null or t.Name = @TableName)
	order by s.name, t.name, c.column_id

end
GO
grant execute on B1.usp_CatalogGetColumns to public
go