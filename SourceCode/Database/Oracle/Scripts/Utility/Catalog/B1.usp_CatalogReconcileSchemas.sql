CREATE OR REPLACE procedure B1.usp_CatalogReconcileSchemas
    (
     ReconciledDate date,
     TraceOn number default 0
    )
as

--
-- Name: Reconcile Schemas
--
-- Proc which given a Date will compare the Database's system catalog
--        with application's data dictionary to make sure that the schemas are 
--        in sync.  If they are not, the proc will return those breaks.
--
--

lv_ReconciledDate date := ReconciledDate;
lv_TraceOn number := TraceOn;

lv_name B1.CATALOGSCHEMAS.SCHEMANAME%type;

 cursor C1 is
  select UserName
    from All_Users a
   where UserName in ('B1');

cursor c2 is
    select SchemaName, 'Schema not found in database catalog.' as Dsc
      from B1.CatalogSchemas
     where ReconciledDate is null
  UNION ALL
    select SchemaName, 'Schema not found in data dictionary.' as Dsc
      from B1.CatalogSchemas
     where ReconciledDate <> lv_ReconciledDate;

Begin
  For C1_rec in C1 loop
    update B1.CatalogSchemas 
    set ReconciledDate = lv_ReconciledDate
    where SchemaName = c1_rec.UserName;
    
    if SQL%rowcount = 0 then --no data
    
        if lv_TraceOn = 1
        then
            dbms_output.put_line('Schema ' || c1_rec.UserName || ' created.');
        end if;
        
        insert into B1.CatalogSchemas (SchemaName, ReconciledDate)
            values (c1_rec.UserName, lv_ReconciledDate);
    end if;
      
  
  End loop;
  
  commit;
  

for c2_req in c2 loop
    dbms_output.put_line('B1.usp_CatalogReconcileSchemas: Error ! SchemaName: ' || c2_req.SchemaName 
                            || c2_req.dsc);
end loop;
  
end;
/
grant execute on B1.usp_CatalogReconcileSchemas to public;
/