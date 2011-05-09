--#SET TERMINATOR /
--
-- This table is used to hold the database catalog meta data about columns
-- The DataAccessMgr class will load this meta data into a local cache
-- in order to build dynamic sql statements
-- The ReconciledDate reflects the last time the column was reconciled 
-- with the database meta data.
--  
CREATE TABLE B1.CatalogColumns(
    SchemaName varchar(24) NOT NULL -- unique schema for the table this column belongs to
    , SchemaNameUPPER generated always as (Upper(SchemaName)) -- used for case insensitive uniqueness
    , TableName varchar(30) NOT NULL -- unique id for the table this column belongs to
    , TableNameUPPER generated always as (Upper(TableName)) -- used for case insensitive uniqueness
    , ColumnName varchar(30) NOT NULL -- unique name (within table) for the column
    , ColumnNameUPPER generated always as (Upper(ColumnName)) -- used for case insensitive uniqueness
    , DataType varchar(64) not null -- data type of the column as defined by database
    , OrdinalPosition smallint not null -- position of column in table
    , ColumnDefault varchar(128) null -- default value or function of column
    , IsNullable smallint with default 1 -- is null allowed
    , IsRowGuidCol smallint with default 0 check (IsRowGuidCol between 0 and 1) -- Sqlserver specific
    , IsIdentity smallint with default 0 check (IsIdentity between 0 and 1) -- Sqlserver specific
    , IsComputed smallint with default 0 check (IsComputed between 0 and 1) -- Sqlserver specific
    , CharacterMaximumLength smallint null -- maximum length of column
    , NumericPrecision smallint null
    , NumericPrecisionRadix smallint null
    , NumericScale smallint null
    , DateTimePrecision smallint null
    , PrimaryKeyNumber smallint with default 0
    , ForeignKeyNumber smallint with default 0
    , ForeignKeySchemaName varchar(24) null
    , ForeignKeyTableName varchar(30) null
    , ForeignKeyColumnName varchar(30) null
    , ReconciledDate timestamp with DEFAULT current timestamp -- time this index was last reconciled with db
    , Description varchar(256) NULL -- user defined comments about the column
 , CONSTRAINT PK_Columns PRIMARY KEY (SchemaName, TableName, ColumnName)
) in B1Core index in B1CoreIdx
/
-- sample of a case insenstive index on the character column
CREATE UNIQUE  INDEX B1.UX_Columns ON B1.CatalogColumns 
(ColumnNameUpper
, TableNameUpper
,  SchemaNameUpper) 
/
ALTER TABLE B1.CatalogColumns  add  CONSTRAINT FK_Columns_Tables FOREIGN KEY(SchemaName, TableName)
REFERENCES B1.CatalogTables (SchemaName, TableName)
/
ALTER TABLE B1.CatalogColumns  add  CONSTRAINT FK_Columns_Columns 
FOREIGN KEY(ForeignKeySchemaName, ForeignKeyTableName, ForeignKeyColumnName)
REFERENCES B1.CatalogColumns (SchemaName, TableName, ColumnName)
/
grant references on B1.CatalogColumns to public
/
grant all on B1.CatalogColumns to public
/