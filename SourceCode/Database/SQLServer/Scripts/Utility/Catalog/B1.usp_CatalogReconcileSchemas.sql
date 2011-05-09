-- drop existing object if exists
--
if exists (select null from sys.procedures p 
						inner join sys.schemas s
						on p.schema_Id = s.schema_id
						and s.name = 'B1'
						where p.name = 'usp_CatalogReconcileSchemas')
						drop proc B1.usp_CatalogReconcileSchemas
go
--						
create proc B1.usp_CatalogReconcileSchemas
(
@ReconciledDate datetime,
@TraceOn bit = 0
)
as
begin
--
-- Name: Reconcile Schemas
--
-- Proc which given a Date will compare the Databases system catalog
--		with the applications catalog tables to make sure that the schemas are 
--		in sync.  If they are not, the proc will return those breaks.
--
declare @SchemaName varchar(64)

declare c cursor for
select schema_name
from Information_Schema.Schemata iss
inner join sys.schemas s
on s.name = iss.schema_name
where Schema_Owner = 'dbo'
open c
fetch c into @SchemaName

while @@fetch_status = 0
begin
	if @TraceOn = 1
		raiserror('Schema: %s ', 5, 1, @SchemaName)
		
	update B1.CatalogSchemas 
	set ReconciledDate = @ReconciledDate
	where SchemaName = @SchemaName
			
	if @@rowcount <> 1
	begin
		insert into B1.CatalogSchemas (SchemaName, ReconciledDate)
		values (@SchemaName, @ReconciledDate)
	end

	fetch c into @SchemaName
end

close c
deallocate c

declare c cursor for
	select SchemaName
	from B1.CatalogSchemas
	where ReconciledDate <> @ReconciledDate
		or ReconciledDate is null

open c
fetch c into @SchemaName

begin tran
set xact_abort on

	while @@fetch_status = 0
	begin
		if @TraceOn = 1
		begin
			raiserror('Removing Schema: %s from application catalog', 5, 1, @SchemaName)
		end	

		-- update primary key definitions
		delete from B1.CatalogSchemas
		where SchemaName = @SchemaName

		fetch c into @SchemaName
	end

commit tran	
close c
deallocate c

end
GO
grant execute on B1.usp_CatalogReconcileSchemas to public
go