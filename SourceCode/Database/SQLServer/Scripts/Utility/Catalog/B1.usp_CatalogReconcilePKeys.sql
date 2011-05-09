-- drop existing object if exists
--
if exists (select null from sys.procedures p 
						inner join sys.schemas s
						on p.schema_Id = s.schema_id
						and s.name = 'B1'
						where p.name = 'usp_CatalogReconcilePKeys')
						drop proc B1.usp_CatalogReconcilePKeys
go
--		
create proc B1.usp_CatalogReconcilePKeys
(
@ReconciledDate datetime,
@TraceOn bit = 0
)
as
begin
--
--
-- Name: Reconcile Primary Keys
--
-- Proc which given a Date will compare the Database's system catalog
--        with applications catalog tables to make sure that the columns of 
--        the primary key are in sync.  If they are not, the proc will return those breaks.
--
--
	declare @SchemaName varchar(24), @TableName varchar(64)
			, @ColumnName varchar(64)
			, @PrimaryKeyNumber smallint
	begin tran
	set xact_abort on
		-- first erase all current primary key settings
		update B1.CatalogColumns
		set PrimaryKeyNumber = 0
		where PrimaryKeyNumber > 0

		declare c cursor for
		select t.SchemaName, t.TableName, c.ColumnName, pkcols.Ordinal_Position as PrimaryKeyNumber
		from INFORMATION_SCHEMA.TABLE_CONSTRAINTS tableConstraints
		inner join INFORMATION_SCHEMA.KEY_COLUMN_USAGE pkcols
		on pkcols.Constraint_Name = tableConstraints.Constraint_Name
			and pkcols.Table_Schema = tableConstraints.Constraint_Schema
		inner join B1.CatalogSchemas s
			on s.SchemaName = tableConstraints.Constraint_Schema
		inner join B1.CatalogTables t		
			on t.TableName = tableConstraints.Table_Name
			and t.SchemaName = s.SchemaName
		inner join B1.CatalogColumns c
			on c.SchemaName = t.SchemaName
			and c.TableName = t.TableName
			and c.ColumnName = pkcols.Column_Name
		where tableConstraints.Constraint_Type='Primary Key'
		order by t.SchemaName, t.TableName, pkcols.Ordinal_Position
		open c
		fetch c into  @SchemaName, @TableName, @ColumnName, @PrimaryKeyNumber

		while @@fetch_status = 0
		begin

			if @TraceOn = 1
				raiserror('Schema: %s, Table: %s,  Column: %s, PrimaryKeyNumber: %d', 5, 1, 
					@SchemaName, @TableName, @ColumnName, @PrimaryKeyNumber)

			update B1.CatalogColumns 
			set PrimaryKeyNumber = @PrimaryKeyNumber
			where SchemaName = @SchemaName
			and TableName = @TableName
			and ColumnName = @ColumnName
			
			if @@ROWCOUNT = 0
				raiserror('Error: Schema: %s Table: %s,  Column: %s, PrimaryKeyNumber: %d was missing from the application catalog', 5, 1, 
					@SchemaName, @TableName, @ColumnName, @PrimaryKeyNumber)
				
			
			fetch c into  @SchemaName, @TableName, @ColumnName, @PrimaryKeyNumber
		end

		close c
		deallocate c

	commit tran;
end
GO
grant execute on B1.usp_CatalogReconcilePKeys to public