create or replace procedure B1.usp_CatalogReconcileColumns
    (
     ReconciledDate date,
     TraceOn number default 0
    )
as

lv_ReconciledDate date := ReconciledDate;
lv_TraceOn number(1):= TraceOn;

--
-- Name: Reconcile Columns
--
-- Proc which given a Date will compare the Database's system catalog
--        with internal DataSturcture to make sure that the columns are 
--        in sync.  If they are not, the proc will return those breaks.
--
--

 Cursor c1 is
        select s.SchemaName
        , a.tablename
        , b.column_name as name
        , a.ReconciledDate
        , a.Description
        , b.data_type as DataType
        , b.column_Id as OrdinalPosition
        , b.Data_Default as ColumnDefault
        , decode(b.nullable, 'Y', 1, 0) as IsNullable
        , 0 as IsRowGuIdCol
        , 0 as IsIdentity
        , 0 as IsComputed
        , b.char_length as CharacterMaximumLength
        , b.data_precision as NumericPrecision
        ,  Null as NumericPrecisionRadix --numeric_precision_radix
        , b.data_scale as NumericScale
        , b.data_length as DateTimePrecision --datetime_precision
        , b.data_length as DataLength
         from B1.catalogSchemas s
         , B1.catalogtables a
         , all_tab_columns b
        where s.SchemaName = a.SchemaName
        and a.tablename = b.table_name
        and B.TABLE_NAME = a.tableName
        and B.OWNER = s.SchemaName
        order by a.SchemaName, a.TableName, b.column_id;

Cursor c2 is
    select c.SchemaName, c.TableName, c.ColumnName , 'Column not found in database.' as dsc
    from B1.CatalogColumns c
    where c.ReconciledDate <> lv_ReconciledDate
          or c.ReconciledDate is null;

begin
  for c1_rec in c1 loop
            
    if lv_TraceOn = 1
    then
    dbms_output.put_line('Column: ' || c1_rec.name);
    end if;
  
    update B1.CatalogColumns 
    set ReconciledDate = lv_ReconciledDate
    , DataType = c1_rec.DataType
    , OrdinalPosition = c1_rec.OrdinalPosition
    , ColumnDefault = c1_rec.ColumnDefault
    , IsNullable = c1_rec.IsNullable
    , IsRowGuIdCol = c1_rec.IsRowGuIdCol
    , IsIdentity = c1_rec.IsIdentity
    , IsComputed = c1_rec.IsComputed
    , CharacterMaximumLength = c1_rec.CharacterMaximumLength
    , NumericPrecision = c1_rec.NumericPrecision
    , NumericPrecisionRadix = c1_rec.NumericPrecisionRadix 
    , NumericScale = c1_rec.NumericScale
    , DateTimePrecision = c1_rec.DateTimePrecision 
    
    where SchemaName = c1_rec.SchemaName
    and TableName = c1_rec.TableName
    and ColumnName = c1_rec.name;
    
    if sql%notfound then
    
        if TraceOn = 1
        then
            dbms_output.put_line('Schema ' || c1_rec.SchemaName || ' Table ' || c1_rec.TableName || ' Column ' || c1_rec.Name || ' ' ||  ' was created.');
        end if;
        
        insert into B1.CatalogColumns (SchemaName
                    , TableName
                    , ColumnName
                    , ReconciledDate
                    , DataType
                    , OrdinalPosition
                    , ColumnDefault
                    , IsNullable
                    , IsRowGuIdCol 
                    , IsIdentity
                    , IsComputed
                    , CharacterMaximumLength
                    , NumericPrecision
                    , NumericPrecisionRadix 
                    , NumericScale
                    , DateTimePrecision 
                    )
            values (c1_rec.SchemaName
                    , c1_rec.TableName
                    , c1_rec.name
                    , lv_ReconciledDate
                    , c1_rec.DataType
                    , c1_rec.OrdinalPosition
                    , c1_rec.ColumnDefault
                    , c1_rec.IsNullable
                    , c1_rec.IsRowGuIdCol
                    , c1_rec.IsIdentity
                    , c1_rec.IsComputed
                    , c1_rec.CharacterMaximumLength
                    , c1_rec.NumericPrecision
                    , c1_rec.NumericPrecisionRadix 
                    , c1_rec.NumericScale
                    , c1_rec.DateTimePrecision 
                    );
            
    end if;
        
  end loop;

 commit;

for c2_req in c2 loop
    -- update primary key definitions
    update B1.CatalogColumns 
    set ForeignKeyNumber = 0
    , ForeignKeySchemaName = null
    , ForeignKeyTableName = null
    , ForeignKeyColumnName = null
    where ForeignKeySchemaName = c2_req.SchemaName
    and ForeignKeyTableName = c2_req.TableName
    and ForeignKeyColumnName = c2_req.ColumnName;
    
    delete from B1.CatalogColumns 
    where SchemaName = c2_req.SchemaName
    and TableName = c2_req.TableName
    and ColumnName = c2_req.ColumnName;
    
    dbms_output.put_line('B1.usp_CatalogReconcileColumns:  SchemaName: ' || c2_req.SchemaName 
                            || '; TableName: ' || c2_req.TableName 
                            || '; ColumnName ' || c2_req.ColumnName 
                            || c2_req.dsc
                            || ' and has been deleted from Catalog table.');
end loop;
 
end;
/
grant execute on B1.usp_CatalogReconcileColumns to public
/