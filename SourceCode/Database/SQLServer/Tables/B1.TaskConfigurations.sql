--
-- Task Configurations
--
-- Each record contains a different configuration description that
-- can be associated with a Task.  This forces a task to only be processed
-- by an Engine that is running in an environment that supports that configuration.
-- For example, it could describe specific hardware (for cryptography).
--
CREATE TABLE B1.TaskConfigurations
(
	ConfigId					VARCHAR(32) NOT NULL,	-- Unique string identifier for the configuration description
	ConfigurationDescription	VARCHAR(512) NOT NULL,	-- details specific to this configuration (e.g. memory, hardware, etc)
	LastModifiedUserCode		INT NULL,
	LastModifiedDateTime		DATETIME NULL,
	CONSTRAINT PK_TaskConfigurations_ConfigId PRIMARY KEY (ConfigId)
) ON B1Core

GO

ALTER TABLE B1.TaskConfigurations
ADD CONSTRAINT FK_TaskConfigurations_UserMaster_Code
FOREIGN KEY (LastModifiedUserCode)
REFERENCES B1.UserMaster(UserCode)

GO
