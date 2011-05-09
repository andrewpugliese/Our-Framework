--
-- This table is used to hold the database catalog meta data about schemas
-- The DataAccessMgr class will load this meta data into a local cache
-- in order to build dynamic sql statements.
-- The ReconciledDate reflects the last time the schema was reconciled 
-- with the database meta data.
--
CREATE TABLE  B1.CatalogSchemas(
  SchemaName Varchar(24) NOT NULL, -- unique name of schema
  ReconciledDate DateTime default (Getdate()), -- time this index was last reconciled with db
  Description Varchar(256) NULL, -- user defined comments about index
 CONSTRAINT PK_Tables_SchemaName PRIMARY KEY(SchemaName)
) ON B1Core
go
grant references on B1.CatalogSchemas to public
go
grant insert, select, update, delete on B1.CatalogSchemas to public
go