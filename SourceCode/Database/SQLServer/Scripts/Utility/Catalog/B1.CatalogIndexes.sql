--
-- This table is used to hold the database catalog meta data about indexes
-- The DataAccessMgr class will load this meta data into a local cache
-- in order to build dynamic sql statements that require performance
-- optimizations and index verification.
-- The ReconciledDate reflects the last time the index was reconciled 
-- with the database meta data.
--
CREATE TABLE B1.CatalogIndexes(
  IndexName Varchar(64) NOT NULL, -- unique name of the index
  SchemaName varchar(24) NOT NULL, -- name of the schema of the table that index belongs
  TableName varchar(64) NOT NULL, -- name of the table that index belongs
  IsPrimaryKey bit not null,
  IsUnique bit not null,
  IsDescend bit not null, -- is this a descending index column
  KeyNum smallint not null, -- what position is this column in the index 
  ColumnName varchar(64) NOT NULL, -- name of the column that is part of the index
  ColumnFunction Varchar(1024) NULL, -- the function body for function-based columns
  ReconciledDate DateTime default (Getdate()), -- time this index was last reconciled with db
  Description Varchar(256) NULL, -- user defined comments about index
 CONSTRAINT PK_Indexes_IndexName_ColumnName PRIMARY KEY(IndexName, ColumnName)
) ON B1Core
go
ALTER TABLE B1.CatalogColumns add CONSTRAINT FK_Indexes_Tables FOREIGN KEY(SchemaName, TableName)
REFERENCES B1.CatalogTables (SchemaName, TableName)
go
grant references on B1.CatalogIndexes to public
go
grant insert, update, select, delete on B1.CatalogIndexes to public
go
