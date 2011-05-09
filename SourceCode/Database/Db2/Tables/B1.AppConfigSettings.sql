--#SET TERMINATOR /
--
-- This table is used for testing and demoing the functionality
-- of the DataAccessMgr class and the UniqueIds table and stored procedure.
--  
CREATE TABLE B1.AppConfigSettings(
  ConfigSetName varchar(48) not null,
  ConfigKey varchar(48) not null,
  ConfigValue varchar(256) not null,
  ConfigDescription varchar(256) null,
 CONSTRAINT PK_AppConfigSettings PRIMARY KEY(ConfigSetName, ConfigKey, ConfigValue)
)  in B1Core index in B1CoreIdx
/
GRANT REFERENCES ON B1.AppConfigSettings TO PUBLIC
/
GRANT ALL ON B1.AppConfigSettings TO PUBLIC 
/
