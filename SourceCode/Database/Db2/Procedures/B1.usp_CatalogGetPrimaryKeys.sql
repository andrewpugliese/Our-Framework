--#SET TERMINATOR /

CREATE OR REPLACE PROCEDURE B1.usp_CatalogGetPrimaryKeys
  (IN schemaName varchar(24), IN tableName varchar(64))
  RESULT SETS 1
  LANGUAGE SQL
  BEGIN
	DECLARE lv_schemaName varchar(64);
	DECLARE lv_tableName varchar(64);

	DECLARE returnCursor CURSOR WITH RETURN TO CLIENT FOR

	SELECT pk.table_schem as SchemaName
	, pk.table_name as TableName
	, pk.pk_name as PrimaryKeyName
	, pk.column_name as ColumnName
	, pk.key_seq as Ordinal
	FROM sysibm.sqlprimarykeys pk 
	WHERE (lv_schemaName is null or pk.table_schem = lv_schemaName)
	AND (lv_tableName is null or pk.table_name = lv_tableName)
	ORDER BY pk.table_schem , pk.table_name, pk.pk_name, pk.key_seq;

	SET lv_schemaName = schemaName;
	SET lv_tableName = tableName;

	OPEN returnCursor;
  END
/
GRANT EXECUTE ON PROCEDURE B1.usp_CatalogGetPrimaryKeys TO PUBLIC
/