--
-- Application Master
--
-- Used to hold the Application Definition records.
-- Each record will have a unique AppCode and AppId
--
-- To be a Task Processing Engine, the application
-- must have the IsTaskProcessingEngine bit set true
-- AND it must NOT add a MultiSessionCode to the AppSessionTable
--
CREATE TABLE B1.AppMaster
(
	AppCode					INT NOT NULL, -- Unique Numeric Code
	AppId					VARCHAR(32) NOT NULL, -- Unique String Identifier
	Remarks					VARCHAR(512) NOT NULL, -- Description, comments
	IsTaskProcessingEngine	BIT NOT NULL DEFAULT(0), -- Indicates if the application
													--  will act as a TaskProcessing Engine
	LastModifiedUserCode	INT,	-- Last User to Modify the record
	LastModifiedDateTime	DATETIME, -- Date/Time of the last modification
	CONSTRAINT PK_AppMaster PRIMARY KEY (AppCode) 
) ON B1Core

GO

CREATE UNIQUE INDEX UX_AppMaster_AppId
ON B1.AppMaster(AppId)
ON B1CoreIdx

GO

ALTER TABLE B1.AppMaster
ADD CONSTRAINT FK_AppMaster_UserMaster
FOREIGN KEY (LastModifiedUserCode)
REFERENCES B1.UserMaster(UserCode)

GO