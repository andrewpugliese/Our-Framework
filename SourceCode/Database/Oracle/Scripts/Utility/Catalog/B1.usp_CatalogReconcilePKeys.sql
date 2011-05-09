create or replace procedure B1.usp_CatalogReconcilePKeys
    (
     ReconciledDate date,
     TraceOn number default 0
    )
as
--
-- Name: Reconcile Primary Keys
--
-- Proc which given a Date will compare the Database's system catalog
--        with Internal Data Structure to make sure that the columns of 
--        the primary key are in sync.  If they are not, the proc will return those breaks.
--

lv_ReconciledDate date := ReconciledDate;
lv_TraceOn number(1):= TraceOn;

Cursor c1 is
    select t.SchemaName, t.TableName, c.ColumnName, b.Position
    from all_constraints a
    , all_cons_columns b
    , B1.CatalogTables t
    , B1.CatalogColumns c
    , B1.CatalogSchemas s
    where a.constraint_type = 'P' --primary key
      and a.owner = b.owner
      and a.table_name = b.table_name
      and a.constraint_name = b.constraint_name
      and a.owner = b.owner
      and t.TableName = a.Table_Name
      and c.TableName = t.TableName
      and c.SchemaName = t.SchemaName
      and c.ColumnName = b.Column_Name
      and s.SchemaName = a.owner
      and t.SchemaName = s.SchemaName
    order by t.SchemaName, t.TableName, b.Position;

begin
    -- First erase all old PrimaryKey Settings
    update B1.CatalogColumns
    set PrimaryKeyNumber = 0
    where PrimaryKeyNumber > 0;
    
    for c1_rec in c1 loop
        -- Now set the PrimaryKey Number
        update B1.CatalogColumns
        set PrimaryKeyNumber = c1_rec.Position
        where SchemaName = c1_rec.SchemaName
        and TableName = c1_rec.TableName
        and ColumnName = c1_rec.ColumnName;
        
        if sql%rowcount = 0
        then
            raise_application_error(-20001, 'usp_CatalogReconcilePKeys: Unable to find Column ' 
              || c1_rec.SchemaName || '.' || c1_rec.TableName || '.' || c1_rec.ColumnName );
            rollback;
            return;
        end if;
        
    end loop;

    commit;
end;
/
grant execute on B1.usp_CatalogReconcilePKeys to public
/