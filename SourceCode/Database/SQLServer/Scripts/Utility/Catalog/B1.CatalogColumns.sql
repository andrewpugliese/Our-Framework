--
-- This table is used to hold the database catalog meta data about columns
-- The DataAccessMgr class will load this meta data into a local cache
-- in order to build dynamic sql statements
-- The ReconciledDate reflects the last time the column was reconciled 
-- with the database meta data.
--  
CREATE TABLE B1.CatalogColumns(
  SchemaName varchar(24) NOT NULL -- unique name for the schema of the table this column belongs to
  , TableName varchar(64) NOT NULL -- unique name for the table this column belongs to
  , ColumnName Varchar(64) NOT NULL -- unique name (within table) for the column
  , DataType Varchar(64) not null -- data type of the column as defined by database
  , OrdinalPosition Tinyint not null -- position of column in table
  , ColumnDefault Varchar(128) null -- default value or function of column
  , IsNullable Bit default 1 -- is null allowed
  , IsRowGuidCol Bit default 0 check (IsRowGuidCol between 0 and 1) -- is it a guid datatype
  , IsIdentity Bit default 0 check (IsIdentity between 0 and 1) -- is it an identity
  , IsComputed Bit default 0 check (IsComputed between 0 and 1) -- is it a computed column
  , CharacterMaximumlength Smallint null -- maximum legth of data type.
  , NumericPrecision Smallint null
  , NumericPrecisionRadix Smallint null
  , NumericScale Smallint null
  , DateTimePrecision Smallint null
  , PrimaryKeyNumber Tinyint default (0)
  , ForeignKeyNumber Tinyint default (0)
  , ForeignKeySchemaName varchar(24) null
  , ForeignKeyTableName varchar(64) null
  , ForeignKeyColumnName varchar(64) null
  , ReconciledDate DateTime default (Getdate()) -- time this index was last reconciled with db
  , Description Varchar(256) NULL -- user defined comments about the column
 ,CONSTRAINT PK_Columns_ColumnName PRIMARY KEY (SchemaName, TableName, ColumnName)
) ON B1Core
go
CREATE UNIQUE INDEX UX_Columns_ColumnName ON B1.CatalogColumns 
(ColumnName, TableName, SchemaName ) ON B1CoreIdx
go
ALTER TABLE B1.CatalogColumns add CONSTRAINT FK_Columns_Tables_1 FOREIGN KEY(SchemaName, TableName)
REFERENCES B1.CatalogTables (SchemaName, TableName)
go
ALTER TABLE B1.CatalogColumns add CONSTRAINT FK_Columns_Columns
	FOREIGN KEY(ForeignKeySchemaName, ForeignKeyTableName, ForeignKeyColumnName )
REFERENCES B1.CatalogColumns (SchemaName, TableName, ColumnName)
go
grant references on B1.CatalogColumns to public
go
grant insert, select, update, delete on B1.CatalogColumns to public
go