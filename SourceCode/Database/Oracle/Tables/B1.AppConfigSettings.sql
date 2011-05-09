CREATE TABLE B1.AppConfigSettings(
  ConfigSetName varchar2(48) not null,
  ConfigKey varchar2(48) not null,
  ConfigValue varchar2(256) not null,
  ConfigDescription varchar(256) null,
 CONSTRAINT PK_AppConfigSettings PRIMARY KEY(ConfigSetName, ConfigKey, ConfigValue)
)  tablespace B1Core
/
CREATE UNIQUE  INDEX B1.UX_AppConfigSettings ON B1.AppConfigSettings
(nlssort( ConfigSetName , 'NLS_SORT=BINARY_CI' )
, nlssort( ConfigKey , 'NLS_SORT=BINARY_CI' )
, nlssort( ConfigValue , 'NLS_SORT=BINARY_CI' ))  tablespace B1CoreIdx
/
grant references on B1.AppConfigSettings to public
/
grant insert, select, update, delete on B1.AppConfigSettings to public
-- NOTE: YOU MUST END THIS FILE WITH THE SLASH OR COMMAND LINE SQLPLUS WILL HANG WAITING FOR COMMAND INPUT
/