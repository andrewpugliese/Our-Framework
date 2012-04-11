--
-- B1.LoadSampleData
--

-- Setup the application configuration parameters
insert into B1.AppConfigParameters (ParameterName, ParameterValue)
values ('QID', 'E1850QUEUE')

-- Setup the application configuration settings
insert into B1.AppConfigSettings (ConfigSetName, ConfigKey, ConfigValue, ConfigDescription)
values ('ALL', 'ChromeQueue', '<<QID>>/Chrome', 'Chrome queue')

insert into B1.AppConfigSettings (ConfigSetName, ConfigKey, ConfigValue, ConfigDescription)
values ('ALL', 'ExperianTimeout', '300', 'Experian server timeout')

insert into B1.AppConfigSettings (ConfigSetName, ConfigKey, ConfigValue, ConfigDescription)
values ('eContracting', 'ExperianTimeout', '200', 'Experian server timeout')

insert into B1.AppConfigSettings (ConfigSetName, ConfigKey, ConfigValue, ConfigDescription)
values ('eContracting', 'CreditBureaus', 'TransUnion', 'TransUnion is supported')

insert into B1.AppConfigSettings (ConfigSetName, ConfigKey, ConfigValue, ConfigDescription)
values ('eContracting', 'CreditBureaus', 'Experian', 'Experian is supported')

GO