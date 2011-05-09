--
-- This table is used to hold the database catalog meta data about schemas
-- The DataAccessMgr class will load this meta data into a local cache
-- in order to build dynamic sql statements.
-- The ReconciledDate reflects the last time the schema was reconciled 
-- with the database meta data.
--
CREATE TABLE B1.CatalogSchemas(
    SchemaName varchar2(24) NOT NULL, -- unique name of schema
    ReconciledDate date default (sysdate), -- time this index was last reconciled with db
    Description varchar2(256) NULL,-- user defined comments about index
 CONSTRAINT PK_Tables_Schema PRIMARY KEY(SchemaName)
) tablespace B1Core
/
-- sample case insensitive index on schema name
create unique index B1.UX_CatalogSchemas_SchemaName on
B1.CatalogSchemas( nlssort( SchemaName, 'NLS_SORT=BINARY_CI' ) ) tablespace B1CoreIdx
/
grant references on B1.CatalogSchemas to public
/
grant insert, update, select, delete on B1.CatalogSchemas to public
-- NOTE: YOU MUST END THIS FILE WITH THE SLASH OR COMMAND LINE SQLPLUS WILL HANG WAITING FOR COMMAND INPUT
/