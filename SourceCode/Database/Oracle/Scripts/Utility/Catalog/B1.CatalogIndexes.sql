--
-- This table is used to hold the database catalog meta data about indexes
-- The DataAccessMgr class will load this meta data into a local cache
-- in order to build dynamic sql statements that require performance
-- optimizations and index verification.
-- The ReconciledDate reflects the last time the index was reconciled 
-- with the database meta data.
--
CREATE TABLE B1.CatalogIndexes(
  SchemaName Varchar2(30) NOT NULL, -- unique name of the schema index belongs
  TableName Varchar2(30) NOT NULL, -- unique name of the table index belongs
  IndexName Varchar2(30) NOT NULL, -- unique name of the index
  IsPrimaryKey number(1) not null,
  IsUnique number(1) not null,
  IsDescend number(1) not null, -- is this a descending index column
  KeyNum number(10) not null, -- what position is this column in the index 
  ColumnName varchar2(30) NOT NULL, -- name of the column that is part of the index
  ColumnFunction varchar2(128), -- the function body for function-based columns
  ReconciledDate Date default (sysdate), -- time this index was last reconciled with db
  Description Varchar2(256) NULL, -- user defined comments about index
 CONSTRAINT PK_Indexes PRIMARY KEY(SchemaName, TableName, IndexName, ColumnName)
) tablespace B1Core
/
-- sample case insensitive index on indexName
CREATE UNIQUE INDEX B1.UX_CatalogIndexes ON B1.CatalogIndexes 
(NLSSORT(IndexName, 'NLS_SORT=BINARY_CI' )
,  NLSSORT(ColumnName, 'NLS_SORT=BINARY_CI' )
, NLSSORT(TableName, 'NLS_SORT=BINARY_CI' )
, NLSSORT(SchemaName, 'NLS_SORT=BINARY_CI' )) tablespace B1CoreIdx
/
ALTER TABLE B1.CatalogIndexes ADD CONSTRAINT FK_Indexes_Columns 
FOREIGN KEY(SchemaName, TableName, ColumnName)
REFERENCES B1.CatalogColumns (SchemaName, TableName, ColumnName)
/
grant references on B1.CatalogIndexes to public
/
grant insert, update, select, delete on B1.CatalogIndexes to public
-- NOTE: YOU MUST END THIS FILE WITH THE SLASH OR COMMAND LINE SQLPLUS WILL HANG WAITING FOR COMMAND INPUT
/