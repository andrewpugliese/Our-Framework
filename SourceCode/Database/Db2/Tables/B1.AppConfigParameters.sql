--#SET TERMINATOR /
--
-- This table is used for testing and demoing the functionality
-- of the DataAccessMgr class and the UniqueIds table and stored procedure.
--  
CREATE TABLE B1.AppConfigParameters(
  ParameterName Varchar(48) not null,
  ParameterValue Varchar(256) not null,
  ParameterNameUPPER generated always as (Upper(ParameterName)), -- used for case insensitive uniqueness
 CONSTRAINT PK_AppConfigParameters PRIMARY KEY(ParameterName)
)  in B1Core index in B1CoreIdx
/
-- sample case insensitve index on uniqueidkey
create unique index B1.UX_AppConfigParameters_ParamName on
B1.AppConfigParameters (ParameterNameUPPER)
/
GRANT REFERENCES ON B1.AppConfigParameters TO PUBLIC
/
GRANT ALL ON B1.AppConfigParameters TO PUBLIC 
/
