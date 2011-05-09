--
-- User Master
--
-- Maintains the unique user records
--
CREATE TABLE B1.UserMaster
(
	UserCode				INT NOT NULL, -- unique numeric identifier
	UserId					VARCHAR(64) NOT NULL, -- unique string identifier
	FirstName				VARCHAR(40) NOT NULL,
	LastName				VARCHAR(60) NOT NULL,
	DefaultAccessGroupCode	INT NOT NULL, -- default access group code
	SignonRestricted		BIT NOT NULL DEFAULT (0), -- indicates if signon is restricted for this user
	MultipleSignonAllowed	BIT NOT NULL DEFAULT (0), -- indicates if multiple signons allowed for user
	ForcePasswordChange		BIT NOT NULL DEFAULT (1), -- indicates if user must change password
	FailedSignonAttempts	TINYINT NOT NULL DEFAULT (0), -- maintains the failed signon attempts
	UserPassword			VARCHAR(96) NOT NULL, 
	PasswordSalt			VARCHAR(96) NOT NULL, 
	EmailAddress			VARCHAR(96),
	LastSignonDateTime		DATETIME,
	NamePrefix				VARCHAR(5),
	MiddleName				VARCHAR(40),
	NameSuffix				VARCHAR(5),
	Remarks					VARCHAR(512),
	LastModifiedUserCode	INT,
	LastModifiedDateTime	DATETIME,
	CONSTRAINT PK_UserMaster_UserCode PRIMARY KEY (UserCode) 
) ON B1Core

GO

CREATE UNIQUE INDEX UX_UserMaster_UserId
ON B1.UserMaster(UserId)
ON B1CoreIdx

GO

CREATE UNIQUE INDEX UX_UserMaster_AccessGroupCode_Code
ON B1.UserMaster(DefaultAccessGroupCode, UserCode)
ON B1CoreIdx

GO

CREATE UNIQUE INDEX UX_UserMaster_LastName_FirstName_UserCode
ON B1.UserMaster(LastName, FirstName, UserCode)
ON B1CoreIdx

GO

ALTER TABLE B1.UserMaster
ADD CONSTRAINT FK_UserMaster_AccessControlGroups 
FOREIGN KEY (DefaultAccessGroupCode) 
REFERENCES B1.AccessControlGroups (AccessControlGroupCode)

GO

ALTER TABLE B1.UserMaster
ADD CONSTRAINT FK_UserMaster_UserMaster
FOREIGN KEY (LastModifiedUserCode)
REFERENCES B1.UserMaster(UserCode)

GO
