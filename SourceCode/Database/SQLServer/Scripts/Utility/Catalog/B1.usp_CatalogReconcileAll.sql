-- drop existing object if exists
--
if exists (select null from sys.procedures p 
						inner join sys.schemas s
						on p.schema_Id = s.schema_id
						and s.name = 'B1'
						where p.name = 'usp_CatalogReconcileAll')
						drop proc B1.usp_CatalogReconcileAll
go
--		
create Procedure B1.usp_CatalogReconcileAll
(
@ReconciledDate datetime = null,
@TraceOn bit = 0
)
as 
begin

if @ReconciledDate is null
	set @ReconciledDate = GETDATE();
	
exec B1.usp_CatalogReconcileSchemas @ReconciledDate, @TraceOn
exec B1.usp_CatalogReconcileTables @ReconciledDate, @TraceOn
exec B1.usp_CatalogReconcileColumns @ReconciledDate, @TraceOn
exec B1.usp_CatalogReconcilePKeys @ReconciledDate, @TraceOn
exec B1.usp_CatalogReconcileFKeys @ReconciledDate, @TraceOn
exec B1.usp_CatalogReconcileIndexes @ReconciledDate, @TraceOn

end
go
grant execute on B1.usp_CatalogReconcileAll to public
go