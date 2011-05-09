--
-- Application Sessions
--
-- Used to maintain the status of the application sessions.
-- An application can ONLY have 1 session at a time for a
-- single unique AppCode or AppId if it is going to be part
-- of the Task Processing Grid.
--
-- An application can have multiple instances running, the 
-- only caveat is that each instance be assigned a unique
-- code and id that is defined in the AppMaster table
-- and placed in the individual configuration file.
-- 
CREATE TABLE B1.AppSessions
(
	AppCode					INT NOT NULL, -- Unique App Numeric Code
	AppId					VARCHAR(32) NOT NULL, -- Unique App String Id
	AppProduct				VARCHAR(64) NOT NULL, -- Found in Assembly.cs
	AppVersion				VARCHAR(24) NOT NULL, -- Found in Assembly.cs
	MultipleSessionCode		BIGINT NOT NULL DEFAULT(0), -- A unique code when application can have multiple instances
	MachineName				VARCHAR(64) NOT NULL, -- Name of server app resides
	ProcessId				BIGINT NOT NULL, -- OS Process Id
	StartDateTime			DATETIME NOT NULL DEFAULT (GETUTCDATE()), -- Time app started
	StatusDateTime			DATETIME NOT NULL DEFAULT (GETUTCDATE()), -- Time of last status update
	EnvironmentSettings		VARCHAR(512) NOT NULL, -- Various environment settings (ie. OS version, etc)
	ConfigSettings			VARCHAR(512) NOT NULL, -- Various configuration settings of the app.
	StatusMessage			VARCHAR(512) NOT NULL, 
	CONSTRAINT PK_AppSessions PRIMARY KEY (AppCode, MultipleSessionCode) 
) ON B1Core

GO

CREATE UNIQUE INDEX UX_AppSessions_AppId
ON B1.AppSessions(AppId, MultipleSessionCode)
ON B1CoreIdx

GO

ALTER TABLE B1.AppSessions
ADD CONSTRAINT FK_AppSessions_AppMaster_Code
FOREIGN KEY (AppCode)
REFERENCES B1.AppMaster(AppCode)

GO

ALTER TABLE B1.AppSessions
ADD CONSTRAINT FK_AppSessions_AppMaster_Id
FOREIGN KEY (AppId)
REFERENCES B1.AppMaster(AppId)

GO