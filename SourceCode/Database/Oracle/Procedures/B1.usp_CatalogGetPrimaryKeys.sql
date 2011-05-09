CREATE OR REPLACE procedure B1.usp_CatalogGetPrimaryKeys
    (
    SchemaName varchar2 default null ,
    TableName varchar2 default null,
    o_Ref_Cursor out sys_refcursor
    )
as
--
-- Name: Catalog Get PrimaryKeys
--
-- Proc which will return the Primary Key Columns of all Tables
-- Schema: all tables in schema
-- TableName: all tables with that name (across schemas)
--
    lv_SchemaName all_constraints.owner%type := SchemaName ;
    lv_TableName all_constraints.table_name%type := TableName;

Begin

  open o_Ref_Cursor for
    select a.owner as SchemaName
    , a.table_name as TableName
    , column_name as ColumnName
    , b.Position as Ordinal
    from all_constraints a
    , all_cons_columns b
 		where (lv_SchemaName is null or a.owner = lv_SchemaName)
      and (lv_TableName is null or a.Table_Name = lv_TableName)
      and a.constraint_type = 'P' --primary key
      and a.owner = b.owner
      and a.table_name = b.table_name
      and a.constraint_name = b.constraint_name
      and a.owner = b.owner
    order by a.owner, a.table_name, a.constraint_name, b.Position;
end;
/
grant execute on B1.usp_CatalogGetPrimaryKeys to public;
/

