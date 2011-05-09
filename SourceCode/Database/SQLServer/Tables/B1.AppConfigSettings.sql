CREATE TABLE B1.AppConfigSettings(
  ConfigSetName varchar(48) not null,
  ConfigKey varchar(48) not null,
  ConfigValue varchar(256) not null,
  ConfigDescription varchar(256) null,
 CONSTRAINT PK_AppConfigSettings PRIMARY KEY(ConfigSetName, ConfigKey, ConfigValue)
) ON B1Core
go
grant references on B1.AppConfigSettings to public
go
grant insert, select, update, delete on B1.AppConfigSettings to public
go