create or replace procedure B1.usp_CatalogReconcileAll
(
ReconciledDate date default sysdate
, TraceOn number default 0
)
as

-- 
-- Catalog Reconcile All
--  Procedure calls all catalog reconciliation procedures
--
-- Created 08/27/09 apugliese
--

lv_ReconcileDate date := ReconciledDate;
lv_TraceOn number(1) := TraceOn;
lv_TableCount number(5) := 1;

begin

    -- update the data dictionary meta data 
    -- reconcile the DataDictionary Tables
    B1.usp_CatalogReconcileSchemas (lv_ReconcileDate, lv_TraceOn);

    -- reconcile the DataDictionary Tables
    B1.usp_CatalogReconcileTables (lv_ReconcileDate, lv_TraceOn);

    -- reconcile the DataDictionary Columns
    B1.usp_CatalogReconcileColumns (lv_ReconcileDate, lv_TraceOn);

    -- reconcile the DataDictionary Primary Keys
    B1.usp_CatalogReconcilePKeys (lv_ReconcileDate, lv_TraceOn);

    -- reconcile the DataDictionary Foreign Keys
    B1.usp_CatalogReconcileFKeys (lv_ReconcileDate, lv_TraceOn);
    
    -- reconcilde the indexes
    B1.usp_CatalogReconcileIndexes (lv_ReconcileDate, lv_TraceOn);

end;
/
grant execute on B1.usp_CatalogReconcileAll to public
/