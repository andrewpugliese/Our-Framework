CREATE TABLE B1.AppMaster
(
	AppCode					INT NOT NULL, -- Unique Numeric Code
	AppId					NVARCHAR(32) NOT NULL, -- Unique String Identifier
	AllowMultipleSessions	BIT NOT NULL DEFAULT(1), -- Indicates if the same appCode/appId
													-- can have multiple active app sessions
													-- TaskProcessingEngine Apps CANNOT have multiple sessions
	IsTaskProcessingHost	BIT NOT NULL DEFAULT(0), -- Indicates if the application will be used to host
													-- asynchronsou task processing 
	Remarks					NVARCHAR(512) NOT NULL, -- Description, comments
	LastModifiedUserCode	INT,	-- Last User to Modify the record
	LastModifiedDateTime	DATETIME, -- Date/Time of the last modification
	CONSTRAINT AppMaster_PK PRIMARY KEY (AppCode) 
) ON B1Core

GO

CREATE UNIQUE INDEX AppMaster_UX_AppId
ON B1.AppMaster(AppId)
ON B1CoreIdx

GO

ALTER TABLE B1.AppMaster
ADD CONSTRAINT AppMaster_FK_UserMaster
FOREIGN KEY (LastModifiedUserCode)
REFERENCES B1.UserMaster(UserCode)

GO
/*
DECLARE @name nvarchar(64), @level0type nvarchar(64), @level1type nvarchar(64), @level2type nvarchar(64)
, @level0name nvarchar(64), @level1name nvarchar(64), @level2name nvarchar(64), @value nvarchar(256)
set @name = 'Description'
set @level0type = 'SCHEMA'
set @level0name = 'B1'
set @level1type = 'TABLE'
set @level1name = 'AppMaster'
set @level2type = 'COLUMN'

set @value = 'Application Master.

Used to hold the Application Definition records.
Each record will have a unique AppCode and AppId

To be a Task Processing Engine, the application
must have the IsTaskProcessingEngine bit set true
AND it must NOT add a MultiSessionCode to the AppSessionTable'

EXEC sys.sp_addextendedproperty 
@name, 
@value,
@level0type,
@level0name,
@level1type,
@level1name;


set @level2name = 'AppCode'
set @value = 'Unique Numeric Code'

EXEC sys.sp_addextendedproperty 
@name, 
@value,
@level0type,
@level0name,
@level1type,
@level1name,
@level2type,
@level2name;

select * from 
fn_listextendedproperty (NULL, 'schema', 'B1', 'table', 'AppMaster', NULL, NULL);


select * from 
fn_listextendedproperty (NULL, 'schema', 'B1', 'table', 'AppMaster', 'column', NULL);



EXEC sys.sp_dropextendedproperty 
@name = N'Description', 
@level0type = N'SCHEMA', @level0name = B1, 
@level1type = N'TABLE',  @level1name = AppMaster;
*/