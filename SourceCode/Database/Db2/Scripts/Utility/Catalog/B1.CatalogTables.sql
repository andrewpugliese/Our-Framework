--
-- This table is used to hold the database catalog meta data about tables
-- The DataAccessMgr class will load this meta data into a local cache
-- in order to build dynamic sql statements.
-- The ReconciledDate reflects the last time the table was reconciled 
-- with the database meta data.
--
CREATE TABLE B1.CatalogTables(
    SchemaName varchar(24) NOT NULL, -- unique schema owning the table
    SchemaNameUPPER generated always as (Upper(SchemaName)), -- used for case insensitive uniqueness
    TableName varchar(30) NOT NULL, -- unique name of the table
    TableNameUPPER generated always as (Upper(TableName)), -- used for case insensitive uniqueness
    ReconciledDate timestamp  with DEFAULT current timestamp, -- time this index was last reconciled with db
    Description varchar(256) NULL, -- user defined comments about index
 CONSTRAINT PK_Tables PRIMARY KEY(SchemaName, TableName)
) in B1Core index in B1CoreIdx
/
-- sample case insensitive index on table name
CREATE UNIQUE  INDEX B1.UX_Tables_TableName_Schema ON B1.CatalogTables 
(TableNameUpper, SchemaNameUpper) 
/
ALTER TABLE B1.CatalogTables ADD  CONSTRAINT FK_Tables_Schemas FOREIGN KEY(SchemaName)
REFERENCES B1.CatalogSchemas (SchemaName)
/
GRANT REFERENCES ON B1.CatalogTables TO PUBLIC
/
GRANT ALL ON B1.CatalogTables TO PUBLIC 
/
-- NOTE: YOU MUST END THIS FILE WITH THE SLASH OR COMMAND LINE SQLPLUS WILL HANG WAITING FOR COMMAND INPUT
/
