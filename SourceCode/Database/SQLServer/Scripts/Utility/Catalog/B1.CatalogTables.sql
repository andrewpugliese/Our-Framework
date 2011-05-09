--
-- This table is used to hold the database catalog meta data about tables
-- The DataAccessMgr class will load this meta data into a local cache
-- in order to build dynamic sql statements.
-- The ReconciledDate reflects the last time the table was reconciled 
-- with the database meta data.
--
CREATE TABLE B1.CatalogTables(
  SchemaName varchar(24) NOT NULL, -- unique name of the schema owning the table
  TableName Varchar(64) NOT NULL, -- unique name of the table
  ReconciledDate DateTime default (Getdate()), -- time this index was last reconciled with db
  Description Varchar(256) NULL, -- user defined comments about index
 CONSTRAINT PK_Tables_TableId PRIMARY KEY(SchemaName, TableName)
) ON B1Core
go
CREATE UNIQUE INDEX UX_CatalogTables_TableName_SchemaName ON B1.CatalogTables 
(TableName, SchemaName ) ON B1CoreIdx
go
ALTER TABLE B1.CatalogTables ADD CONSTRAINT FK_Tables_Schemas FOREIGN KEY(SchemaName)
REFERENCES B1.CatalogSchemas (SchemaName)
go
grant references on B1.CatalogTables to public
go
grant insert, update, select, delete on B1.CatalogTables to public
go