--#SET TERMINATOR /

CREATE OR REPLACE PROCEDURE B1.usp_CatalogGetFKeys
  (IN schemaName varchar(24), IN tableName varchar(64))
  RESULT SETS 1
  LANGUAGE SQL
  BEGIN
	DECLARE lv_schemaName varchar(64);
	DECLARE lv_tableName varchar(64);

	DECLARE returnCursor CURSOR WITH RETURN TO CLIENT FOR

	SELECT fk.fktable_schem as SchemaName
	, fk.fktable_name as TableName
	, fk.fk_name as ForeignKeyName
	, fk.fkcolumn_name as ColumnName
	, fk.key_seq as Ordinal
	, fk.pktable_schem as RefSchema
	, fk.pktable_name as RefTable
	, fk.pkcolumn_name as RefColumn
	FROM sysibm.sqlforeignkeys fk 
	WHERE (lv_schemaName is null or fk.fktable_schem = lv_schemaName)
	AND (lv_tableName is null or fk.fktable_name = lv_tableName)
	ORDER BY fk.fktable_schem , fk.fktable_name, fk.fk_name, fk.key_seq;


	SET lv_schemaName = schemaName;
	SET lv_tableName = tableName;

	OPEN returnCursor;
  END
/
GRANT EXECUTE ON PROCEDURE B1.usp_CatalogGetFKeys TO PUBLIC
/
