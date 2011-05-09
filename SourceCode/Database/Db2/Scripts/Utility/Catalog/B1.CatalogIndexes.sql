--
-- This table is used to hold the database catalog meta data about indexes
-- The DataAccessMgr class will load this meta data into a local cache
-- in order to build dynamic sql statements that require performance
-- optimizations and index verification.
-- The ReconciledDate reflects the last time the index was reconciled 
-- with the database meta data.
--
CREATE TABLE B1.CatalogIndexes(
  SchemaName Varchar(30) NOT NULL, -- unique name of the schema index belongs
  SchemaNameUPPER generated always as (Upper(SchemaName)), -- used for case insensitive uniqueness
  TableName Varchar(30) NOT NULL, -- unique name of the table index belongs
  TableNameUPPER generated always as (Upper(TableName)), -- used for case insensitive uniqueness
  IndexName Varchar(30) NOT NULL, -- unique name of the index
  IndexNameUPPER generated always as (Upper(IndexName)), -- used for case insensitive uniqueness
  IsPrimaryKey smallint not null  check (IsPrimaryKey between 0 and 1),
  IsUnique smallint not null  check (IsUnique between 0 and 1),
  IsDescend smallint not null  check (IsDescend between 0 and 1), -- is this a descending index column
  KeyNum smallint not null, -- what position is this column in the index 
  ColumnName varchar(30) NOT NULL, -- name of the column that is part of the index
  ColumnNameUPPER generated always as (Upper(ColumnName)), -- used for case insensitive uniqueness
  ColumnFunction varchar(128), -- the function body for function-based columns
  ReconciledDate timestamp with DEFAULT current timestamp, -- time this index was last reconciled with db
  Description Varchar(256) NULL, -- user defined comments about index
 CONSTRAINT PK_Indexes PRIMARY KEY(SchemaName, TableName, IndexName, ColumnName)
) in B1Core index in B1CoreIdx
/
-- sample case insensitive index on indexName
CREATE UNIQUE INDEX B1.UX_CatalogIndexes ON B1.CatalogIndexes 
(IndexNameUpper
, ColumnNameUpper
, TableNameUpper
, SchemaNameUpper)
/
ALTER TABLE B1.CatalogIndexes ADD CONSTRAINT FK_Indexes_Columns 
FOREIGN KEY(SchemaName, TableName, ColumnName)
REFERENCES B1.CatalogColumns (SchemaName, TableName, ColumnName)
/
grant references on B1.CatalogIndexes to public
/
grant insert, update, select, delete on B1.CatalogIndexes to public
/