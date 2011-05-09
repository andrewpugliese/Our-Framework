-- drop existing object if exists
--
if exists (select null from sys.procedures p 
						inner join sys.schemas s
						on p.schema_Id = s.schema_Id
						and s.name = 'B1'
						where p.name = 'usp_CatalogReconcileTables')
						drop proc B1.usp_CatalogReconcileTables
go
--						
create proc B1.usp_CatalogReconcileTables
(
@ReconciledDate datetime = null,
@TraceOn bit = 0
)
as
begin
--
-- Name: Catalog Reconcile Tables
--
-- Proc which given a Date will compare the Database's system catalog
--		with application's catalog tables to make sure that they are 
--		in sync.  If they are not, the proc will return those breaks.
--		In will also update the referential integrity hierarchy for each table.
--

if @ReconciledDate is null
	set @ReconciledDate = getdate()
	

declare @TableName varchar(64), @SchemaName varchar(64)
			
declare c cursor for
		select s.name, t.name
		from sys.tables t
		inner join sys.schemas s
		on t.schema_Id = s.schema_id
		order by s.name, t.name
open c
fetch c into @SchemaName, @TableName

begin tran
set xact_abort on

	while @@fetch_status = 0
	begin
		if @TraceOn = 1
		begin
			declare @s varchar(20)
			raiserror('TEMP TABLE LIST: Table: %s,  Schema: %s', 5, 1, @TableName, @SchemaName)
		end	

		update B1.CatalogTables 
		set ReconciledDate = @ReconciledDate
		where TableName = @TableName
		and SchemaName = @SchemaName

		if @@rowcount <> 1
		begin
					
			insert into B1.CatalogTables (SchemaName, TableName, ReconciledDate)
			values (@SchemaName, @TableName, @ReconciledDate)
		end		

		fetch c into @SchemaName, @TableName
	end

commit tran

close c
deallocate c

declare c cursor for
	select SchemaName, TableName
	from B1.CatalogTables t
	where t.ReconciledDate is null
		  or t.ReconciledDate <> @ReconciledDate
open c
fetch c into @SchemaName, @TableName

begin tran
set xact_abort on

	while @@fetch_status = 0
	begin
		if @TraceOn = 1
		begin
			raiserror('Removing Table: %s,  Schema: %s from application catalog', 5, 1
					, @TableName, @SchemaName)
		end	

		-- update primary key definitions
		update B1.CatalogColumns 
		set ForeignKeyNumber = 0
		, ForeignKeySchemaName = null
		, ForeignKeyTableName = null
		, ForeignKeyColumnName = null
		where ForeignKeySchemaName = @SchemaName
		and ForeignKeyTableName = @TableName

		delete from B1.CatalogColumns 
		where SchemaName = @SchemaName
		and TableName = @TableName

		delete from B1.CatalogTables
		where SchemaName = @SchemaName
		and TableName = @TableName

		fetch c into @SchemaName, @TableName
	end

commit tran

end

close c
deallocate c

GO
grant execute on B1.usp_CatalogReconcileTables to public
go