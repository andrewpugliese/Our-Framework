--#SET TERMINATOR /
CREATE OR REPLACE PROCEDURE B1.usp_CatalogGetColumns
  (IN schemaName varchar(24), IN tableName varchar(64))
  RESULT SETS 1
  LANGUAGE SQL
  BEGIN
	DECLARE lv_schemaName varchar(64);
	DECLARE lv_tableName varchar(64);

	DECLARE returnCursor CURSOR WITH RETURN TO CLIENT FOR
	SELECT table_schem as SchemaName 
		, c.table_name as TableName
		, c.column_name as ColumnName
		, type_name as DataType
		, c.ordinal_position as OrdinalPosition
		, Column_Default as ColumnDefault
		, nullable as IsNullable
		, 0 as IsRowGuidCol
		, case when pseudo_column = 2 then 1
			else 0 
			end as IsIdentity
		, 0 as IsComputed
		, character_maximum_length as CharacterMaximumLength
		, numeric_precision as NumericPrecision
		, numeric_precision_radix as NumericPrecisionRadix
		, numeric_scale as NumericScale
		, datetime_precision as DateTimePrecision
	FROM sysibm.columns c
	inner join sysibm.sqlcolumns sc
	 	ON table_schema = table_schem 
	 	AND (c.table_name = sc.table_name)
		AND c.column_name = sc.column_name
	WHERE (lv_schemaName is null or table_schema = lv_schemaName)
	AND (lv_tableName is null or c.table_Name = lv_tableName)
	ORDER BY table_schema, c.table_name, c.ordinal_position;

	SET lv_schemaName = schemaName;
	SET lv_tableName = tableName;

	OPEN returnCursor;
  END
/