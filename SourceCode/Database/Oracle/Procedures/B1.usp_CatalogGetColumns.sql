CREATE OR REPLACE PROCEDURE B1.usp_CatalogGetColumns 
(
    SchemaName varchar2 default null,
    TableName varchar2 default null,
    o_Ref_Cursor out sys_refcursor 
)
as
--
-- Name: Catalog Get Columns
--
-- Proc will select content of CatalogColumns based on given Schema. 
-- Schema: all columns for all tables in schema
-- TableName: all columns for all tables with that name (across schemas)
    lv_SchemaName all_constraints.owner%type := SchemaName ;
    lv_TableName all_constraints.table_name%type := TableName;
    
Begin


  open o_Ref_Cursor for

        select c.owner as SchemaName
        , c.table_name as Tablename
        , c.column_name as ColumnName
        , c.data_type as DataType
        , c.column_Id as OrdinalPosition
        , c.Data_Default as ColumnDefault
        , decode(c.nullable, 'Y', 1, 0) as IsNullable
        , 0 as IsRowGuIdCol
        , 0 as IsIdentity
        , 0 as IsComputed
        , c.char_length as CharacterMaximumLength
        , c.data_precision as NumericPrecision
        ,  Null as NumericPrecisionRadix --numeric_precision_radix
        , c.data_scale as NumericScale
        , c.data_length as DateTimePrecision --datetime_precision
        , c.data_length as DataLength
         from all_tab_columns c
        where (lv_SchemaName is null or c.owner = lv_SchemaName)
        and (lv_TableName is null or c.table_name = lv_TableName)
        order by c.owner, c.table_name, c.column_id;

end;
/
grant execute on B1.usp_CatalogGetColumns to public;
/

