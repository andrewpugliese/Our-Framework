--
-- User Master
--
-- Maintains the unique user records
--
CREATE TABLE B1.UserMaster
(
	UserCode				INT NOT NULL, -- unique numeric identifier
	UserId					NVARCHAR(64) NOT NULL, -- unique string identifier
	FirstName				NVARCHAR(40) NOT NULL,
	LastName				NVARCHAR(60) NOT NULL,
	DefaultAccessGroupCode	INT NOT NULL, -- default access group code
	SignonRestricted		BIT NOT NULL DEFAULT (0), -- indicates if signon is restricted for this user
	MultipleSignonAllowed	BIT NOT NULL DEFAULT (0), -- indicates if multiple signons allowed for user
	ForcePasswordChange		BIT NOT NULL DEFAULT (1), -- indicates if user must change password
	FailedSignonAttempts	TINYINT NOT NULL DEFAULT (0), -- maintains the failed signon attempts
	UserPassword			NVARCHAR(96) NOT NULL, 
	PasswordSalt			NVARCHAR(96) NOT NULL, 
	EmailAddress			NVARCHAR(96),
	LastSignonDateTime		DATETIME,
	NamePrefix				NVARCHAR(5),
	MiddleName				NVARCHAR(40),
	NameSuffix				NVARCHAR(5),
	Remarks					NVARCHAR(512),
	LastModifiedUserCode	INT,
	LastModifiedDateTime	DATETIME,
	CONSTRAINT UserMaster_PK_UserCode PRIMARY KEY (UserCode) 
) ON B1Core

GO

CREATE UNIQUE INDEX UserMaster_UX_UserId
ON B1.UserMaster(UserId)
ON B1CoreIdx

GO

CREATE UNIQUE INDEX UserMaster_UX_AccessGroupCode_Code
ON B1.UserMaster(DefaultAccessGroupCode, UserCode)
ON B1CoreIdx

GO

CREATE UNIQUE INDEX UserMaster_UX_LastName_FirstName_UserCode
ON B1.UserMaster(LastName, FirstName, UserCode)
ON B1CoreIdx

GO

ALTER TABLE B1.UserMaster
ADD CONSTRAINT UserMaster_FK_AccessControlGroups 
FOREIGN KEY (DefaultAccessGroupCode) 
REFERENCES B1.AccessControlGroups (AccessControlGroupCode)

GO

ALTER TABLE B1.UserMaster
ADD CONSTRAINT UserMaster_FK_UserMaster
FOREIGN KEY (LastModifiedUserCode)
REFERENCES B1.UserMaster(UserCode)

GO
