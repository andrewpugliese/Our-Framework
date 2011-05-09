-- drop existing object if exists
--
if exists (select null from sys.procedures p 
						inner join sys.schemas s
						on p.schema_Id = s.schema_id
						and s.name = 'B1'
						where p.name = 'usp_CatalogReconcileColumns')
						drop proc B1.usp_CatalogReconcileColumns
go
--		
create proc B1.usp_CatalogReconcileColumns
(
@ReconciledDate datetime,
@TraceOn bit = 0
)
as
begin
--
-- Name: Reconcile Columns
--
-- Proc which given a Date will compare the Database's system catalog
--		with  applications catalog tables to make sure that the columns are 
--		in sync.  If they are not, the proc will return those breaks.
--
--
declare @TableName varchar(64), @TableId smallint , @ColumnName varchar(64)
		, @DataType varchar(64), @ColumnDefault varchar(128), @OrdinalPosition tinyint
		, @IsNullable bit, @IsRowGuidCol bit, @IsIdentity bit, @IsComputed bit
		, @CharacterMaximumLength bigint, @NumericPrecision smallint
		, @NumericPrecisionRadix smallint, @NumericScale smallint, @DateTimePrecision smallint
		, @ColumnId BigInt, @SchemaName varchar(64)

declare c cursor for
select s.name as SchemaName
		, dt.TableName
		, c.name as ColumnName
		, data_type as DataType
		, ordinal_position as OrdinalPosition
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
inner join B1.CatalogSchemas ds
	on ds.SchemaName = s.name
inner join B1.CatalogTables dt
	on ds.schemaName = dt.SchemaName
	and dt.TableName = t.name
inner join Information_Schema.Columns isc
	on isc.Table_Schema = ds.SchemaName
	and isc.Table_Name = dt.TableName
	and isc.Column_Name = c.Name
order by s.name, dt.TableName, ordinal_position
open c
fetch c into @SchemaName
		, @TableName
		, @ColumnName
		, @DataType
		, @OrdinalPosition
		, @ColumnDefault
		, @IsNullable
		, @IsRowGuidCol
		, @IsIdentity
		, @IsComputed
		, @CharacterMaximumLength
		, @NumericPrecision
		, @NumericPrecisionRadix
		, @NumericScale
		, @DateTimePrecision

while @@fetch_status = 0
begin
	if @TraceOn = 1
		raiserror('SchemaName: %s, Table: %s,  Column: %s', 5, 1
			, @SchemaName,@TableName, @ColumnName)

		update B1.CatalogColumns 
		set ReconciledDate = @ReconciledDate
			, DataType = @DataType
			, OrdinalPosition = @OrdinalPosition
			, ColumnDefault = @ColumnDefault
			, IsNullable = @IsNullable
			, IsRowGuidCol = @IsRowGuidCol
			, IsIdentity = @IsIdentity
			, IsComputed = @IsComputed
			, CharacterMaximumLength = @CharacterMaximumLength
			, NumericPrecision = @NumericPrecision
			, NumericPrecisionRadix = @NumericPrecisionRadix
			, NumericScale = @NumericScale
			, DateTimePrecision = @DateTimePrecision
		where ColumnName = @ColumnName
		and SchemaName = @SchemaName 
		and TableName = @TableName

	if @@rowcount <> 1
	begin
			
		insert into B1.CatalogColumns (SchemaName
							, TableName
							, ColumnName
							, DataType
							, OrdinalPosition
							, ColumnDefault
							, IsNullable
							, IsRowGuidCol
							, IsIdentity
							, IsComputed
							, CharacterMaximumLength
							, NumericPrecision
							, NumericPrecisionRadix
							, NumericScale
							, DateTimePrecision
							, ReconciledDate)
		values (@SchemaName
				, @TableName
				, @ColumnName
				, @DataType
				, @OrdinalPosition
				, @ColumnDefault
				, @IsNullable
				, @IsRowGuidCol
				, @IsIdentity
				, @IsComputed
				, @CharacterMaximumLength
				, @NumericPrecision
				, @NumericPrecisionRadix
				, @NumericScale
				, @DateTimePrecision
				, @ReconciledDate)
	end
	
	fetch c into @SchemaName
				, @TableName
				, @ColumnName
				, @DataType
				, @OrdinalPosition
				, @ColumnDefault
				, @IsNullable
				, @IsRowGuidCol
				, @IsIdentity
				, @IsComputed
				, @CharacterMaximumLength
				, @NumericPrecision
				, @NumericPrecisionRadix
				, @NumericScale
				, @DateTimePrecision
end

close c
deallocate c


declare c cursor for
	select c.SchemaName, c.TableName, c.ColumnName
	from B1.CatalogColumns c
	where c.ReconciledDate is null
			or c.ReconciledDate <> @ReconciledDate
open c
fetch c into @SchemaName, @TableName, @ColumnName

begin tran
set xact_abort on


	while @@fetch_status = 0
	begin
		if @TraceOn = 1
		begin
			raiserror('Removing Schema: %s, Table: %s,  Column: %s from application catalog', 5, 1
					, @SchemaName, @TableName, @ColumnName)
		end	

		-- update primary key definitions
		update B1.CatalogColumns 
		set ForeignKeyNumber = 0
		, ForeignKeySchemaName = null
		, ForeignKeyTableName = null
		, ForeignKeyColumnName = null
		where ForeignKeySchemaName = @SchemaName
		and ForeignKeyTableName = @TableName
		and ForeignKeyColumnName  = @ColumnName

		delete from B1.CatalogColumns 
		where SchemaName = @SchemaName
		and TableName = @TableName
		and ColumnName = @ColumnName

		fetch c into @SchemaName, @TableName, @ColumnName
	end
	
commit tran


close c
deallocate c

end


GO
grant execute on  B1.usp_CatalogReconcileColumns to public
go