create or replace procedure B1.usp_CatalogReconcileIndexes
    (
     ReconciledDate date,
     TraceOn number default 0
    )
is

lv_ReconciledDate date := ReconciledDate;
lv_TraceOn number(1):= TraceOn;
lv_IsUnique number(1) := 0;
lv_IsPrimaryKey number(1) := 0;
lv_IsDescend number(1) := 0;
lv_SchemaName B1.CatalogIndexes.SchemaName%type := null;
lv_TableName B1.CatalogIndexes.TableName%type := null;
lv_ColumnName B1.CatalogIndexes.ColumnName%type := null;

--
-- Name: Reconcile Indexes 
--
-- Proc which given a Date will compare the Database's system catalog
--        with internal DataSturcture to make sure that the columns are 
--        in sync.  If they are not, the proc will return those breaks.
--
--

 Cursor c1 is
    select i.owner
    , i.index_name
    , ic.table_name
    , ic.column_name
    , column_expression
    , ic.column_position
    , uniqueness
    , constraint_type
    , descend
    from all_indexes i
    inner join all_ind_columns ic
      on i.owner   = ic.index_owner
      and i.index_name = ic.index_name
    inner join B1.CatalogSchemas s
      on i.owner = s.SchemaName
    inner join B1.CatalogTables t
      on t.TableName = i.table_name
      and s.SchemaName = i.owner
    left outer join B1.CatalogColumns c
      on c.ColumnName = ic.Column_Name
    left outer join all_ind_expressions ie
      on i.owner   = ie.index_owner
      and i.index_name = ie.index_name
      and ie.column_position = ic.column_position
    left outer join all_constraints pk
      on pk.owner = i.owner
      and pk.constraint_name = i.index_name
    order by i.owner, Table_Name, Index_Name, column_Position;    
    
    Cursor c2 is
       select SchemaName
        , TableName
        , IndexName
        from B1.CatalogIndexes i
        where i.ReconciledDate <> lv_ReconciledDate
              or i.ReconciledDate is null;
 
    Cursor c3 is
        select ColumnName
        from B1.CatalogColumns c
        inner join B1.CatalogTables t
          on c.TableName = t.TableName
          and c.SchemaName = t.SchemaName
        where t.TableName = lv_TableName
        and t.SchemaName = lv_SchemaName;
        
begin
  for c1_rec in c1 loop
            
      if lv_TraceOn = 1
      then
        dbms_output.put_line('Index: '  || c1_rec.owner || '.' || c1_rec.table_name || '.' || c1_rec.index_name || '.' || c1_rec.column_name);
      end if;
  
      if c1_rec.uniqueness = 'UNIQUE' 
      then
        lv_isUnique := 1;
      else lv_isUnique := 0;
      end if;
      
      if c1_rec.constraint_type = 'P' 
      then
        lv_isPrimaryKey := 1;
      else lv_isPrimaryKey := 0;
      end if;
    
      if c1_rec.descend = 'ASC' 
      then
        lv_isDescend := 0;
      else lv_isDescend := 1;
      end if;
      
      -- if the column_expression is not null, then this column
      -- is a function;  we must parse the column_name from the expression.
      lv_ColumnName := c1_rec.Column_Name;
      if c1_rec.column_expression is not null
      then
        lv_ColumnName := null;
        lv_SchemaName := c1_rec.owner;
        lv_TableName := c1_rec.table_name;
        for c3_rec in c3 loop
          if lv_TraceOn = 1
          then
            dbms_output.put_line('column to search: ' || c3_rec.ColumnName || ' in expression: ' || c1_rec.column_expression);              
          end if;
          if instr(c1_rec.column_expression, c3_rec.columnName, 1) > 0 -- we have the column
          then
            lv_ColumnName := c3_rec.columnName; -- we found the column in the expression
            exit;                           -- we can exit the inner loop
            if lv_TraceOn = 1
            then
              dbms_output.put_line('columnFound: ' || c3_rec.ColumnName || ' Expression: ' || c1_rec.column_expression);              
            end if;
            
          end if;
        end loop;
        
        if lv_ColumnName is  null
        then
          RAISE_APPLICATION_ERROR(-20010, 'Column could not be parsed from column expression for index '
              || c1_rec.Owner || '.' || c1_rec.Table_Name || '.' || c1_rec.Index_Name || '; expression: ' || c1_rec.column_expression);        
        end if;
      end if;
      
 			update B1.CatalogIndexes 
			set IsUnique = lv_isUnique
			, IsPrimaryKey = lv_isPrimaryKey
			, KeyNum = c1_rec.column_position
      , columnfunction = c1_rec.column_expression
      , isdescend = lv_isDescend
			, ReconciledDate = lv_ReconciledDate
			where IndexName = c1_rec.index_name
			and TableName = c1_rec.table_name
      and SchemaName = c1_rec.owner
      and ColumnName = lv_ColumnName;
    
    
    if sql%notfound 
    then
           
        if lv_TraceOn = 1
        then
          dbms_output.put_line('Insert index: '  || c1_rec.owner || '.' || c1_rec.table_name || '.' || c1_rec.index_name 
              || '.' || c1_rec.column_name || ' ColumnExpression: ' || c1_rec.column_expression || ' columnParsed: ' || lv_columnName);
        end if;
                    
				insert into B1.CatalogIndexes(SchemaName
          , TableName
					, IndexName
          , ColumnName
					, IsUnique
					, IsPrimaryKey
					, KeyNum
					, IsDescend
					, ColumnFunction)
				values (c1_rec.owner
          , c1_rec.table_name
					, c1_rec.index_name
          , lv_ColumnName
					, lv_IsUnique
					, lv_IsPrimaryKey
					, c1_rec.column_position
					, lv_IsDescend
					, c1_rec.column_expression);            
    end if;
        
  end loop;

commit;

  for c2_req in c2 loop

    delete from B1.CatalogIndexes 
    where SchemaName = c2_req.SchemaName
    and TableName = c2_req.TableName
    and IndexName = c2_req.IndexName;
    
    dbms_output.put_line('B1.usp_CatalogReconcileIndexes:  IndexName: ' 
                            || c2_req.SchemaName || '.'
                            || c2_req.TableName || '.'
                            || c2_req.IndexName 
                            || ' and has been deleted from Catalog table.');
  end loop;

end;
/
grant execute on B1.usp_CatalogReconcileIndexes to public
/