--
-- This table is used to hold the database catalog meta data about columns
-- The DataAccessMgr class will load this meta data into a local cache
-- in order to build dynamic sql statements
-- The ReconciledDate reflects the last time the column was reconciled 
-- with the database meta data.
--  
CREATE TABLE B1.CatalogColumns(
    SchemaName varchar2(24) NOT NULL -- unique schema for the table this column belongs to
    , TableName varchar2(30) NOT NULL -- unique id for the table this column belongs to
    , ColumnName varchar2(30) NOT NULL -- unique name (within table) for the column
    , DataType varchar2(64) not null -- data type of the column as defined by database
    , OrdinalPosition number(3) not null -- position of column in table
    , ColumnDefault varchar2(128) null -- default value or function of column
    , IsNullable number(1) default 1 -- is null allowed
    , IsRowGuidCol number(1) default 0 check (IsRowGuidCol between 0 and 1) -- Sqlserver specific
    , IsIdentity number(1) default 0 check (IsIdentity between 0 and 1) -- Sqlserver specific
    , IsComputed number(1) default 0 check (IsComputed between 0 and 1) -- Sqlserver specific
    , CharacterMaximumLength number(5) null -- maximum length of column
    , NumericPrecision number(5) null
    , NumericPrecisionRadix number(5) null
    , NumericScale number(5) null
    , DateTimePrecision number(5) null
    , PrimaryKeyNumber number(3) default (0)
    , ForeignKeyNumber number(3) default (0)
    , ForeignKeySchemaName varchar2(24) null
    , ForeignKeyTableName varchar2(30) null
    , ForeignKeyColumnName varchar2(30) null
    , ReconciledDate date default (sysdate) -- time this index was last reconciled with db
    , Description varchar2(256) NULL -- user defined comments about the column
 ,CONSTRAINT PK_Columns PRIMARY KEY (SchemaName, TableName, ColumnName)
) tablespace B1Core
/
-- sample of a case insenstive index on the character column
CREATE UNIQUE  INDEX B1.UX_Columns ON B1.CatalogColumns 
(nlssort( ColumnName, 'NLS_SORT=BINARY_CI' )
, nlssort( TableName, 'NLS_SORT=BINARY_CI' )
,  nlssort( SchemaName, 'NLS_SORT=BINARY_CI' )) tablespace B1CoreIdx
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
-- NOTE: YOU MUST END THIS FILE WITH THE SLASH OR COMMAND LINE SQLPLUS WILL HANG WAITING FOR COMMAND INPUT
/