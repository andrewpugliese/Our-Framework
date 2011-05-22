--
-- Task Registrations
--
-- Each record refers to a C# class referred to by the fully qualfied class name
-- located in the Assembly file.
-- Each class implements a defined interface for the Framework's Task Processing
-- The TaskId will default to the FullyQualifiedClassName and Assembly Version
-- Tasks MUST be registered before they can be exected by the Task Processing Engine
--
CREATE TABLE B1.TaskRegistrations
(
	TaskId					VARCHAR(64) NOT NULL, -- Will default to class name and version on registration
	ClassName				VARCHAR(64) NOT NULL, -- Fully qualified name of task process derived class
	AssemblyName			VARCHAR(64) NOT NULL, -- Name of the assembly to containing the class
	AssemblyVersion			VARCHAR(16) NOT NULL, -- version of the assembly loaded
	LastRegisteredDate		DATETIME DEFAULT GETDATE() NOT NULL,
	TaskDescription			VARCHAR(512) NOT NULL,
	AssemblyFile			VARCHAR(128),	-- Full path of the assembly location; null indicates current dir
	LastModifiedUserCode	INT NULL,
	LastModifiedDateTime	DATETIME NULL,
CONSTRAINT PK_TaskRegistrations_TaskId PRIMARY KEY (TaskId)
) ON B1Core
  
GO

ALTER TABLE B1.TaskRegistrations
ADD CONSTRAINT FK_TaskRegistrations_UserMaster_Code
FOREIGN KEY (LastModifiedUserCode)
REFERENCES B1.UserMaster(UserCode)

GO

CREATE UNIQUE INDEX UI_TaskRegistrations_ClassName
ON B1.TaskRegistrations
(
	ClassName,
	AssemblyName,
	AssemblyVersion
) ON B1CoreIdx

GO
