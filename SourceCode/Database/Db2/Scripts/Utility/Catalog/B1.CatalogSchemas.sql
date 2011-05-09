--#SET TERMINATOR /
--
-- This table is used to hold the database catalog meta data about schemas
-- The DataAccessMgr class will load this meta data into a local cache
-- in order to build dynamic sql statements.
-- The ReconciledDate reflects the last time the schema was reconciled 
-- with the database meta data.
--
CREATE TABLE B1.CatalogSchemas(
    SchemaName varchar(24) NOT NULL, -- unique name of schema
    SchemaNameUPPER generated always as (Upper(SchemaName)), -- used for case insensitive uniqueness
    ReconciledDate timestamp with DEFAULT current timestamp, -- time this index was last reconciled with db
    Description varchar(256) NULL,-- user defined comments about index
 CONSTRAINT PK_Tables_Schema PRIMARY KEY(SchemaName)
) in B1Core index in B1CoreIdx
/
-- sample case insensitive index on schema name
create unique index B1.UX_CatalogSchemas_SchemaName on
B1.CatalogSchemas(SchemaNameUPPER )
/
GRANT REFERENCES ON B1.CatalogSchemas TO PUBLIC
/
GRANT ALL ON B1.CatalogSchemas TO PUBLIC 
/
