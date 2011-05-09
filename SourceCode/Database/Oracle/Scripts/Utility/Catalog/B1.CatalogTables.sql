--
-- This table is used to hold the database catalog meta data about tables
-- The DataAccessMgr class will load this meta data into a local cache
-- in order to build dynamic sql statements.
-- The ReconciledDate reflects the last time the table was reconciled 
-- with the database meta data.
--
CREATE TABLE B1.CatalogTables(
    SchemaName varchar2(24) NOT NULL, -- unique schema owning the table
    TableName varchar2(30) NOT NULL, -- unique name of the table
    ReconciledDate date default (sysdate), -- time this index was last reconciled with db
    Description varchar2(256) NULL, -- user defined comments about index
 CONSTRAINT PK_Tables PRIMARY KEY(SchemaName, TableName)
) tablespace B1Core
/
-- sample case insensitive index on table name
CREATE UNIQUE  INDEX B1.UX_Tables_TableName_Schema ON B1.CatalogTables 
(nlssort( TableName, 'NLS_SORT=BINARY_CI' ), nlssort( SchemaName, 'NLS_SORT=BINARY_CI' )) tablespace B1CoreIdx
/
ALTER TABLE B1.CatalogTables ADD  CONSTRAINT FK_Tables_Schemas FOREIGN KEY(SchemaName)
REFERENCES B1.CatalogSchemas (SchemaName)
/
grant references on B1.CatalogTables to public
/
grant insert, update, select, delete on B1.CatalogTables to public
-- NOTE: YOU MUST END THIS FILE WITH THE SLASH OR COMMAND LINE SQLPLUS WILL HANG WAITING FOR COMMAND INPUT
/