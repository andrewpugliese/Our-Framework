--
-- Access Control Groups
--
-- It is used to define access control groups
-- Gropus will then be assigned specific 
-- rules (permissions) to different User Interface objects
-- In addition, 

CREATE TABLE B1.AccessControlGroups
(
	AccessControlGroupCode  INT NOT NULL,	-- unique numeric identifier
	AccessControlGroupName  VARCHAR(64) NOT NULL, -- unique string identifier
	DefaultAccessDenied		BIT DEFAULT (0), -- indicates whether the default access is denied or allowed
	Remarks					VARCHAR(512),
	CONSTRAINT PK_AccessControlGroups PRIMARY KEY (AccessControlGroupCode) 
) ON B1Core

GO

CREATE UNIQUE INDEX UX_AccessControlGroups_GroupName
ON B1.AccessControlGroups(AccessControlGroupName) 
ON B1CoreIdx


GO