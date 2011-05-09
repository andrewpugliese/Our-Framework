create or replace procedure B1.usp_CatalogReconcileFKeys
    (
     ReconciledDate date,
     TraceOn number default 0
    )
as
--
-- Name: Reconcile Foreign Keys
--
-- Proc which given a Date will compare the Database's system catalog
--        with Internal Data Structure to make sure that the columns of 
--        the foriegn key are in sync.  
--

lv_ReconciledDate date := ReconciledDate;
lv_TraceOn number(1):= TraceOn;

Cursor c1 is
    select ca.Constraint_name
            , ca.Owner as FKOwner
            , ca.Table_Name as FKTable
            , ca.Column_Name as FKColumn 
            , ca.Position as FKPosition
            , car.Owner as RKOwner
            , car.Table_Name as RKTable
            , car.Column_Name as RKColumn 
            , car.Position as RKPosition
            
    from all_constraints c
    inner join all_cons_columns ca
        on C.CONSTRAINT_NAME = ca.constraint_name
    inner join B1.CatalogSchemas s
        on s.SchemaName = ca.Owner
    inner join B1.CatalogTables t
        on s.SchemaName = t.SchemaName
        and ca.Table_Name = t.TableName
    inner join B1.CatalogColumns fc
        on t.tableName = fc.tableName
        and t.SchemaName = fc.SchemaName
        and fc.columnName = ca.Column_name

    inner join all_cons_columns car
        on C.R_CONSTRAINT_NAME = car.constraint_name
        and car.Position = ca.Position
    inner join B1.CatalogSchemas rs
        on rs.SchemaName = car.Owner
    inner join B1.CatalogTables rt
        on rs.SchemaName = rt.SchemaName
        and car.Table_Name = rt.TableName
    inner join B1.CatalogColumns rc
        on rt.SchemaName = rc.SchemaName
        and rt.TableName = rc.TableName
       and rc.columnName = car.Column_name
     order by c.constraint_name, ca.Position;

begin
    -- First erase all old Foreign Key Settings
    update B1.CatalogColumns
    set ForeignKeyNumber = 0
    , ForeignKeySchemaName = null
    , ForeignKeyTableName = null
    , ForeignKeyColumnName = null
    where ForeignKeyNumber > 0
    or ForeignKeySchemaName is null
    or ForeignKeyTableName is null
    or ForeignKeyColumnName is null;

    
    for c1_rec in c1 loop
        -- Now set the Foreign Key Number and Ids
        update B1.CatalogColumns
        set ForeignKeyNumber = c1_rec.RKPosition
        , ForeignKeySchemaName = c1_rec.RKOwner
        , ForeignKeyTableName = c1_rec.RKTable
        , ForeignKeyColumnName = c1_rec.RKColumn
        where SchemaName = c1_rec.FKOwner
        and TableName = c1_rec.FKTable
        and ColumnName = c1_rec.FKColumn;
        
        if sql%rowcount = 0
        then
            raise_application_error(-20001, 'usp_CatalogReconcileFKeys: Unable to find Column ' 
              || c1_rec.FKOwner || '.' || c1_rec.FKTable || '.' || c1_rec.FKColumn );
            rollback;
            return;
        end if;
        
    end loop;

    commit;
end;
/
grant execute on B1.usp_CatalogReconcileFKeys to public
/