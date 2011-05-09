-- drop existing object if exists
--
if exists (select null from sys.procedures p 
						inner join sys.schemas s
						on p.schema_Id = s.schema_id
						and s.name = 'B1'
						where p.name = 'usp_CatalogReconcileFKeys')
						drop proc B1.usp_CatalogReconcileFKeys
go
--		
create proc B1.usp_CatalogReconcileFKeys
(
@ReconciledDate datetime,
@TraceOn bit = 0
)
as
begin
--
--
-- Name: Reconcile Foreign Keys
--
-- Proc which given a Date will compare the Database's system catalog
--        with applications catalog tables to make sure that the columns of 
--        the foreign key are in sync.  
--
--
	declare @FKConstraint varchar(128)
						, @FKSchema varchar(64)
						, @FKTable varchar(64)
						, @FKColumnName varchar(64)
						, @FKNumber smallint
						, @RKSchema varchar(64)
						, @RKTable varchar(64)
						, @RKColumnName varchar(64)
						, @RKNumber smallint
	begin tran
	set xact_abort on
		-- first erase all current foreign key settings
		update B1.CatalogColumns
		set ForeignKeyNumber = 0
		, ForeignKeySchemaName = null
		, ForeignKeyTableName = null
		, ForeignKeyColumnName = null
		where ForeignKeyNumber > 0
		or ForeignKeySchemaName is null
		or ForeignKeyTableName is null
		or ForeignKeyColumnName is null

		declare c cursor for
		select FKCU.Constraint_Name as ConstraintName
			, FKCU.Table_Schema as FKSchema
			, FKCU.Table_Name as FKTable
			, FKCU.Column_Name as FKColumn
			, FKCU.Ordinal_Position  as FKNumber

			, RKCU.Table_Schema as RKSchema
			, RKCU.Table_Name as RKTable
			, RKCU.Column_Name as RKColumn
			, RKCU.Ordinal_Position  as RKKeyNumber
		from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS c
		inner join INFORMATION_SCHEMA.KEY_COLUMN_USAGE FKCU
			on FKCU.CONSTRAINT_NAME = c.CONSTRAINT_NAME
		inner join B1.CatalogSchemas FS
			on FS.SchemaName = FKCU.Table_Schema
		inner join B1.CatalogTables FT
			on FS.SchemaName = FT.SchemaName
			and FT.TableName = FKCU.Table_Name
		inner join B1.CatalogColumns FC
			on FC.SchemaName = FT.SchemaName
			and FC.TableName = FT.TableName
			and FC.ColumnName = FKCU.Column_Name

		inner join INFORMATION_SCHEMA.KEY_COLUMN_USAGE RKCU
			on RKCU.CONSTRAINT_NAME = C.UNIQUE_CONSTRAINT_NAME
			and RKCU.Ordinal_Position = FKCU.Ordinal_Position
		inner join B1.CatalogSchemas RS
			on RS.SchemaName = RKCU.Table_Schema
		inner join B1.CatalogTables RT
			on RS.SchemaName = RT.SchemaName
			and RT.TableName = RKCU.Table_Name
		inner join B1.CatalogColumns RC
			on RC.SchemaName = RT.SchemaName
			and RC.TableName = RT.TableName
			and RC.ColumnName = RKCU.Column_Name
		order by C.CONSTRAINT_NAME, RKCU.Ordinal_Position
		open c
		fetch c into  @FKConstraint
						, @FKSchema
						, @FKTable
						, @FKColumnName
						, @FKNumber
						, @RKSchema
						, @RKTable
						, @RKColumnName
						, @RKNumber

		while @@fetch_status = 0
		begin

			if @TraceOn = 1
				raiserror('FKConstraint: %s,  FKSchema: %s, FKTable: %s, FKColumn: %s, FKNumber: %d', 5, 1
						, @FKConstraint
						, @FKSchema
						, @FKTable 
						, @FKColumnName
						, @FKNumber)

			update B1.CatalogColumns 
			set ForeignKeyNumber = @RKNumber
				, ForeignKeySchemaName = @RKSchema
				, ForeignKeyTableName = @RKTable
				, ForeignKeyColumnName = @RKColumnName
			where SchemaName = @FKSchema
			and TableName = @FKTable
			and ColumnName = @FKColumnName

			if @@ROWCOUNT = 0
				raiserror('Error: Table: %s,  Column: %s, PrimaryKeyNumber: %d was missing from the application catalog', 5, 1, 
					@FKTable, @FKColumnName, @RKNumber)

		fetch c into  @FKConstraint
						, @FKSchema
						, @FKTable
						, @FKColumnName
						, @FKNumber
						, @RKSchema
						, @RKTable
						, @RKColumnName
						, @RKNumber
		end

		close c
		deallocate c

	commit tran;
end
GO
grant execute on B1.usp_CatalogReconcileFKeys to public
go