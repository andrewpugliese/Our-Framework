--#SET TERMINATOR /
BEGIN
DECLARE lv_SchemaTools varchar(128);
DECLARE lv_Schema varchar(128);
DECLARE lv_Table varchar(128);
DECLARE lv_Exists int;

SET lv_Schema = 'B1';
SET lv_SchemaTools = 'SYSTOOLS';
SET lv_Table = 'DROPSCHEMAERRORS';
SET lv_Exists = 0;

SELECT 1 into lv_Exists FROM sysibm.dual
WHERE EXISTS (SELECT NULL FROM sysibm.sqlschemas WHERE table_schem = lv_Schema);
IF lv_Exists = 1
THEN
	CALL ADMIN_DROP_SCHEMA(lv_Schema, NULL, lv_SchemaTools, lv_Table);
END IF;
END
/
-- create the B1 schema for the database objects
CREATE SCHEMA B1 AUTHORIZATION ADMINISTRATOR
/
