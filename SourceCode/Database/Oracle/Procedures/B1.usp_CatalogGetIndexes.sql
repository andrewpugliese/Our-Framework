CREATE OR REPLACE procedure B1.usp_CatalogGetIndexes
    (
    SchemaName varchar2 default null ,
    TableName varchar2 default null,
    o_Ref_Cursor out sys_refcursor
    )
as
--
-- Name: Catalog Get PrimaryKeys
--
-- Proc which will return the Index Columns of all Tables
-- Schema: all tables in schema
-- TableName: all tables with that name (across schemas)
--
    lv_SchemaName all_constraints.owner%type := SchemaName ;
    lv_TableName all_constraints.table_name%type := TableName;
    
Begin
  
  open o_Ref_Cursor for
	   select i.owner as SchemaName
      , i.index_name as IndexName
      , ic.table_name as TableName
      , ic.column_name as ColumnName
      , column_expression as ColumnFunction
      , ic.column_position as Ordinal
      , decode(uniqueness, 'UNIQUE', 1, 0) as IsUnique
      , decode(constraint_type, 'P', 1, 0) as IsPrimaryKey
      , decode(descend, 'DESC', 1, 0) as IsDescend
    from all_indexes i
    inner join all_ind_columns ic
      on i.owner   = ic.index_owner
      and i.index_name = ic.index_name
     left outer join all_ind_expressions ie
      on i.owner   = ie.index_owner
      and i.index_name = ie.index_name
      and ie.column_position = ic.column_position
    left outer join all_constraints pk
      on pk.owner = i.owner
      and pk.constraint_name = i.index_name
		where (lv_SchemaName is null or i.owner = lv_SchemaName)
    and (lv_TableName is null or ic.Table_Name = lv_TableName)
    order by i.Owner, i.Table_Name, i.Index_Name, ic.column_Position;     

end;
/
grant execute on B1.usp_CatalogGetIndexes to public;
/
