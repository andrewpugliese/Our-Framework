CREATE TABLE B1.AppConfigSettings(
  ConfigSetName NVARCHAR(48) not null,
  ConfigKey NVARCHAR(48) not null,
  ConfigValue NVARCHAR(256) not null,
  ConfigDescription NVARCHAR(256) null,
 CONSTRAINT AppConfigSettings_PK PRIMARY KEY(ConfigSetName, ConfigKey, ConfigValue)
) ON B1Core
GO
