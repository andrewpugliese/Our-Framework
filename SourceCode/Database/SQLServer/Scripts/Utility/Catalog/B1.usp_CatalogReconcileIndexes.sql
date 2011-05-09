-- drop existing object if exists
--
if exists (select null from sys.procedures p 
						inner join sys.schemas s
						on p.schema_Id = s.schema_id
						and s.name = 'B1'
						where p.name = 'usp_CatalogReconcileIndexes')
						drop proc B1.usp_CatalogReconcileIndexes
go
--		
create proc B1.usp_CatalogReconcileIndexes
(
@ReconciledDate datetime,
@TraceOn bit = 0
)
as
begin
--
--
-- Name: Reconcile Indexes
--
-- Proc which given a Date will compare the Database's system catalog
--        with applications catalog tables to make sure that the columns of 
--        the indexes are in sync.  If they are not, the proc will return those breaks.
--
--
	declare @SchemaName varchar(64), @IndexName varchar(64), @IndexId bigint
			, @TableId smallint, @TableName varchar(64)
			, @ColumnId smallint, @ColumnName varchar(64), @ColumnFunction varchar(128)
			, @PrimaryKeyNumber smallint, @IsUnique bit, @TypeDesc varchar(128)
			, @ColId smallint, @KeyNum smallint, @IsPrimaryKey bit, @IsDescend bit
			
	begin tran
	set xact_abort on

		declare c cursor for


		select s.name as SchemaName
		, o.name as TableName
		, i.name as IndexName
		, cc.ColumnName
		, is_unique
		, i.type_desc
		, is_descending_key
		, null as columnFunction
		, key_ordinal
		, is_primary_key
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
			on o.schema_id = s.schema_id
		inner join b1.CatalogTables t
			on o.name = t.TableName
			and s.name = t.SchemaName
		inner join b1.CatalogSchemas cs
			on cs.SchemaName = s.name
		inner join b1.CatalogColumns cc
			on c.name = cc.ColumnName
			and cc.SchemaName = t.SchemaName
			and cc.TableName = t.TableName
		order by o.name, i.name, Key_ordinal
	
		open c
		fetch c into  @SchemaName, @TableName,  @IndexName, @ColumnName
					, @IsUnique, @TypeDesc, @IsDescend, @ColumnFunction, @KeyNum, @IsPrimaryKey

		while @@fetch_status = 0
		begin

			if @TraceOn = 1
				raiserror('Table: %s,  Index: %s, Column: %s', 5, 1, 
					@TableName, @IndexName, @ColumnName)

			update B1.CatalogIndexes 
			set IndexName = @IndexName
			, IsUnique = @IsUnique
			, IsPrimaryKey = @IsPrimaryKey
			, KeyNum = @KeyNum
			, ReconciledDate = @ReconciledDate
			, ColumnFunction = @ColumnFunction
			, IsDescend = @IsDescend
			where IndexName = @IndexName
			and TableName = @TableName
			and SchemaName = @SchemaName

			if @@ROWCOUNT = 0
			begin
				Exec B1.usp_UniqueIdsGetNextBlock 'IndexId', 1, @IndexId out
				
				insert into B1.CatalogIndexes(SchemaName
					, TableName
					, IndexName
					, ColumnName
					, IsUnique
					, IsPrimaryKey
					, KeyNum
					, IsDescend
					, ColumnFunction)
				values (@SchemaName
					, @TableName
					, @IndexName
					, @ColumnName
					, @IsUnique
					, @IsPrimaryKey
					, @KeyNum
					, @IsDescend
					, @ColumnFunction)
			end
			
			fetch c into  @SchemaName, @TableName,  @IndexName, @ColumnName
						, @IsUnique, @TypeDesc, @IsDescend, @ColumnFunction, @KeyNum, @IsPrimaryKey
				
		end

		close c
		deallocate c

	commit tran;
	

declare c cursor for
	select SchemaName, TableName, IndexName
	from B1.CatalogIndexes i
	where i.ReconciledDate is null
		  or i.ReconciledDate <> @ReconciledDate
open c
fetch c into @SchemaName, @TableName, @IndexName

begin tran
set xact_abort on

	while @@fetch_status = 0
	begin
		if @TraceOn = 1
		begin
			raiserror('Removing Index: %s,  Table: %s, Schema: %s from application catalog', 5, 1
					, @IndexName, @TableName, @SchemaName)
		end	

		-- update primary key definitions
		delete from B1.CatalogIndexes
		where SchemaName = @SchemaName
		and TableName = @TableName
		and IndexName = @IndexName

		fetch c into @SchemaName, @TableName, @IndexName
	end

commit tran	
close c
deallocate c

end
GO
grant execute on B1.usp_CatalogReconcileIndexes to public