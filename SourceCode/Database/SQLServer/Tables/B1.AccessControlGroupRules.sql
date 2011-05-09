--
-- Access Control Group Rules
--
-- Maintains the AccessControlGroup's rules (Permissions)
-- for the given User Interface (UI) control code.
-- The values listed in this table override the default
-- access control rule defined at the group level
--
CREATE TABLE B1.AccessControlGroupRules
(
	AccessControlGroupCode  INT NOT NULL, -- Identifies the AccessGroup
	UIControlCode			INT NOT NULL, -- Identifies the UI Control
	AccessDenied			BIT NOT NULL DEFAULT 0, -- Indicates whether access is denied for the UI Control
	LastModifiedUserCode	INT,
	LastModifiedDateTime	DATETIME,
	CONSTRAINT PK_AccessControlGroupRules PRIMARY KEY (AccessControlGroupCode, UIControlCode) 
) ON B1Core

GO

ALTER TABLE B1.AccessControlGroupRules
ADD CONSTRAINT FK_AccessControlGroupRules_UIControls
FOREIGN KEY (UIControlCode)
REFERENCES B1.UIControls(UIControlCode)

GO

ALTER TABLE B1.AccessControlGroupRules
ADD CONSTRAINT FK_AccessControlGroupRules_UserMaster
FOREIGN KEY (LastModifiedUserCode)
REFERENCES B1.UserMaster(UserCode)

GO