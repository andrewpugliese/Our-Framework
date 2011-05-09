-- Load database initial data
--
-- Add an entry for generating sequence ids for AppSequenceId which
-- will be used for demo/testing utility
insert into B1.UniqueIds (UniqueIdKey, MaxIdValue, RolloverIdValue)
values ('AppSequenceId', 99999, 1)
/
-- Setup the application configuration parameters
insert into B1.AppConfigParameters (ParameterName, ParameterValue)
values ('QID', 'E1850QUEUE')
/
-- Setup the application configuration settings
insert into B1.AppConfigSettings (ConfigSetName, ConfigKey, ConfigValue, ConfigDescription)
values ('ALL', 'ChromeQueue', '<<QID>>/Chrome', 'Chrome queue')
/
insert into B1.AppConfigSettings (ConfigSetName, ConfigKey, ConfigValue, ConfigDescription)
values ('ALL', 'ExperianTimeout', '300', 'Experian server timeout')
/
insert into B1.AppConfigSettings (ConfigSetName, ConfigKey, ConfigValue, ConfigDescription)
values ('eContracting', 'ExperianTimeout', '200', 'Experian server timeout')
/
insert into B1.AppConfigSettings (ConfigSetName, ConfigKey, ConfigValue, ConfigDescription)
values ('eContracting', 'CreditBureaus', 'TransUnion', 'TransUnion is supported')
/
insert into B1.AppConfigSettings (ConfigSetName, ConfigKey, ConfigValue, ConfigDescription)
values ('eContracting', 'CreditBureaus', 'Experian', 'Experian is supported')
-- NOTE: YOU MUST END THIS FILE WITH THE SLASH OR COMMAND LINE SQLPLUS WILL HANG WAITING FOR COMMAND INPUT
/