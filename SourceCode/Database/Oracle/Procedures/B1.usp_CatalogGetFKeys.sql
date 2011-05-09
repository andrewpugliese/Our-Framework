CREATE OR REPLACE procedure B1.usp_CatalogGetFkeys
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
	    select ca.Owner as SchemaName
            , ca.Table_Name as TableName
            , ca.Constraint_name as ForeignKey
            , ca.Column_Name as ColumnName
            , car.Owner as RefSchema
            , car.Table_Name as RefTable
            , car.Column_Name as RefColumn 
            , ca.Position as Ordinal
    from all_constraints c
    inner join all_cons_columns ca
        on C.CONSTRAINT_NAME = ca.constraint_name
    inner join all_cons_columns car
        on C.R_CONSTRAINT_NAME = car.constraint_name
        and car.Position = ca.Position
 		where (lv_SchemaName is null or ca.owner = lv_SchemaName)
    and (lv_TableName is null or ca.Table_Name = lv_TableName)
    order by ca.owner, ca.table_name, c.constraint_name, ca.Position;


end;
/
grant execute on B1.usp_CatalogGetFkeys to public;
/
