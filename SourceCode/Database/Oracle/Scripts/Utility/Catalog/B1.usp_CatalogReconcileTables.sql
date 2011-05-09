create or replace procedure B1.usp_CatalogReconcileTables
    (
     ReconciledDate date,
     TraceOn number default 0
    )
as

lv_ReconciledDate date := ReconciledDate;
lv_TraceOn number(1) := TraceOn;

Cursor c1 is
    select a.object_name as table_name, a.owner as name
    from all_objects a, B1.CATALOGSCHEMAS b, all_tables t
    where a.OWNER = b.SchemaName
    and a.object_type = 'TABLE'
    and a.object_name not like '%$_%'
    and t.table_name = a.object_name
    and a.owner = t.owner
    order by object_Id;

cursor c2 is
    select TableName, s.SchemaName
    from B1.CatalogTables t
    inner join B1.CatalogSchemas s
        on t.SchemaName = s.SchemaName
    where t.ReconciledDate <> lv_ReconciledDate
        or t.ReconciledDate is null;
    
begin
--
-- Name: Reconcile Tables
--
-- Proc which given a Date will compare the Database's system catalog
--        with applications's data dictionary to make sure that the tables are 
--        in sync.  If they are not, the proc will return those breaks.
--        In will also update the referential integrity hierarchy for each table.
--


-- declare temporary table to hold tables and hierarchys
 for c1_rec in c1 loop
    
    if lv_TraceOn = 1
    then
        dbms_output.put_line('Schema '||c1_rec.Name||'; Table ' ||c1_rec.Table_Name || ' listed.');
    end if;
 
    update B1.CatalogTables 
    set ReconciledDate = lv_ReconciledDate
    where TableName = c1_rec.Table_Name
    and SchemaName = c1_rec.name;
   
    if sql%rowcount = 0 then
    
        if lv_TraceOn = 1
        then
            dbms_output.put_line('Schema '|| c1_rec.Name 
                                    || '; Table ' || c1_rec.Table_Name 
                                    || ' created.');
        end if;
        
        insert into B1.CatalogTables (SchemaName, TableName, ReconciledDate)
        values (c1_rec.Name, c1_rec.Table_Name, lv_ReconciledDate);
        
   end if;
 end loop;
 


for c2_req in c2 loop

    -- update primary key definitions
    update B1.CatalogColumns 
    set ForeignKeyNumber = 0
    , ForeignKeySchemaName = null
    , ForeignKeyTableName = null
    , ForeignKeyColumnName = null
    where ForeignKeyTableName = c2_req.TableName
    and ForeignKeySchemaName = c2_req.SchemaName;

    delete from B1.CatalogColumns 
    where SchemaName = c2_req.SchemaName and TableName = c2_req.TableName;
    
    delete from B1.CatalogTables 
    where c2_req.SchemaName = c2_req.SchemaName and TableName = c2_req.TableName;
    
    dbms_output.put_line('Table Reconciliation: SchemaName: ' || c2_req.SchemaName 
                            || '; TableName: ' || c2_req.TableName 
                            || ' and has been deleted from Catalog and all its columns.');
end loop; 

commit;


end;
/
grant execute on B1.usp_CatalogReconcileTables to public;
/