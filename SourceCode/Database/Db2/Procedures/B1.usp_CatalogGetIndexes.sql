--#SET TERMINATOR /

CREATE OR REPLACE PROCEDURE B1.usp_CatalogGetIndexes
  (IN schemaName varchar(24), IN tableName varchar(64))
  RESULT SETS 1
  LANGUAGE SQL
  BEGIN
	DECLARE lv_schemaName varchar(64);
	DECLARE lv_tableName varchar(64);

	DECLARE returnCursor CURSOR WITH RETURN TO CLIENT FOR
	SELECT i.indschema as SchemaName
	, i.tabname as TableName
	, i.indname as IndexName
	, ic.colname as ColumnName
	, case when uniquerule = 'P' then 1
		when uniquerule = 'U' then 1
		else 0 
		end as IsUnique
	, indextype as TypeDescription
	, case when colorder = 'A' then 0
		else 1 
		end as IsDescend
	, null as columnFunction
	, ic.colseq as Ordinal
	, case when uniquerule = 'P' then 1
		else 0 
		end as IsPrimaryKey
	FROM syscat.indexes i 
	INNER JOIN syscat.indexcoluse ic 
		ON i.indschema = ic.indschema
		and i.indname = ic.indname
	WHERE (lv_schemaName is null or i.indschema = lv_schemaName)
	AND (lv_tableName is null or i.tabname = lv_tableName)
	ORDER BY i.indschema, i.tabname, i.indname, ic.colseq;


	SET lv_schemaName = schemaName;
	SET lv_tableName = tableName;

	OPEN returnCursor;
  END
/
GRANT EXECUTE ON PROCEDURE B1.usp_CatalogGetIndexes TO PUBLIC
/