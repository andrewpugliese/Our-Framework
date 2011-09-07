--
-- Task Registrations
--
-- Each record refers to a C# class referred to by the fully qualfied class name
-- located in the Assembly file.
-- Each class implements a defined interface for the Framework's Task Processing
-- The TaskId is the Fully Qualified ClassName
-- Tasks MUST be registered before they can be exected by the Task Processing Engine
--
CREATE TABLE B1.TaskRegistrations
(
	TaskId					NVARCHAR(64) NOT NULL, -- Fully Qualified Class Name
	AssemblyName			NVARCHAR(64) NOT NULL, -- Name of the assembly to containing the class
	LastRegisteredDate		DATETIME DEFAULT GETDATE() NOT NULL,
	TaskDescription			NVARCHAR(512) NOT NULL,
	LastModifiedUserCode	INT NULL,
	LastModifiedDateTime	DATETIME NULL,
CONSTRAINT TaskRegistrations_PK_TaskId PRIMARY KEY (TaskId)
) ON B1Core
  
GO

ALTER TABLE B1.TaskRegistrations
ADD CONSTRAINT TaskRegistrations_FK_UserMaster_Code
FOREIGN KEY (LastModifiedUserCode)
REFERENCES B1.UserMaster(UserCode)

GO

CREATE UNIQUE INDEX TaskRegistrations_UX_AssemblyName_TaskId
ON B1.TaskRegistrations
(
	AssemblyName,
	TaskId
) ON B1CoreIdx

GO
