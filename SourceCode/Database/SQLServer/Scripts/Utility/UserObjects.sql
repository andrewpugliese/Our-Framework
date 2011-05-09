DROP TABLE B1.AppSessionControl
DROP TABLE B1.UserSessions
DROP TABLE B1.UserAccessGroupPermissions
DROP TABLE B1.UIControls

GO

ALTER TABLE B1.UserAccessGroups
DROP CONSTRAINT FK_UserAccessGroups_UserMaster

GO

DROP TABLE B1.UserMaster
DROP TABLE B1.UserAccessGroups

GO

CREATE TABLE B1.UserAccessGroups
(
	UserAccessGroupCode     INT NOT NULL,
	UserAccessGroupName     VARCHAR(64) NOT NULL,
	DefaultAccessDenied		BIT DEFAULT (0),
	Remarks					VARCHAR(512),
	LastModifiedUserCode	INT,
	LastModifiedDateTime	DATETIME,
	CONSTRAINT PK_UserAccessGroups PRIMARY KEY (UserAccessGroupCode) 
) ON B1Core

GO

CREATE UNIQUE INDEX UX_UserAccessGroups_GroupName
ON B1.UserAccessGroups(UserAccessGroupName) 
ON B1CoreIdx


GO

CREATE TABLE B1.UserMaster
(
	UserCode				INT NOT NULL,
	UserId					VARCHAR(64) NOT NULL,
	UserPassword			VARCHAR(172) NOT NULL,
	DefaultAccessGroupCode	INT NOT NULL,
	SignonRestricted		BIT NOT NULL DEFAULT (0),
	MultipleSignonAllowed	BIT NOT NULL DEFAULT (0),
	ForcePasswordChange		BIT NOT NULL DEFAULT (1),
	FailedSignonAttempts	TINYINT NOT NULL DEFAULT (0),
	LastSignonDateTime		DATETIME,
	NamePrefix				VARCHAR(5),
	FirstName				VARCHAR(40),
	MiddleName				VARCHAR(40),
	LastName				VARCHAR(60),
	NameSuffix				VARCHAR(5),
	Remarks					VARCHAR(512),
	LastModifiedUserCode	INT,
	LastModifiedDateTime	DATETIME,
	CONSTRAINT PK_UserMaster_UserCode PRIMARY KEY (UserCode) 
) ON B1Core

GO

CREATE UNIQUE INDEX UX_UserAccessGroups_UserId
ON B1.UserMaster(UserId)
ON B1CoreIdx

GO

CREATE UNIQUE INDEX UX_UserAccessGroups_AccessGroupCode_Code
ON B1.UserMaster(DefaultAccessGroupCode, UserCode)
ON B1CoreIdx

GO

ALTER TABLE B1.UserMaster
ADD CONSTRAINT FK_UserMaster_UserAccessGroups 
FOREIGN KEY (DefaultAccessGroupCode) 
REFERENCES B1.UserAccessGroups (UserAccessGroupCode)

GO

ALTER TABLE B1.UserMaster
ADD CONSTRAINT FK_UserMaster_UserMaster
FOREIGN KEY (LastModifiedUserCode)
REFERENCES B1.UserMaster(UserCode)

GO

ALTER TABLE B1.UserAccessGroups
ADD CONSTRAINT FK_UserAccessGroups_UserMaster
FOREIGN KEY (LastModifiedUserCode)
REFERENCES B1.UserMaster(UserCode)

GO

CREATE TABLE B1.UIControls
(
	UIControlCode			INT NOT NULL,
	UIControlURI			VARCHAR(128) NOT NULL,
	Description				VARCHAR(512),
	LastModifiedUserCode	INT,
	LastModifiedDateTime	DATETIME,
	CONSTRAINT PK_UIControls_UIControlCode PRIMARY KEY (UIControlCode)
)
ON B1Core

GO

CREATE UNIQUE INDEX UX_UIControls_UIControlURI 
ON B1.UIControls (UIControlURI)
ON B1CoreIdx

GO

ALTER TABLE B1.UIControls
ADD CONSTRAINT FK_UIControls_UserMaster
FOREIGN KEY (LastModifiedUserCode)
REFERENCES B1.UserMaster(UserCode)

GO

CREATE TABLE B1.UserAccessGroupPermissions
(
	UserAccessGroupCode     INT NOT NULL,
	UIControlCode			INT NOT NULL,
	AccessDenied			BIT NOT NULL DEFAULT 0,
	LastModifiedUserCode	INT,
	LastModifiedDateTime	DATETIME,
	CONSTRAINT PK_UserAccessGroupPermissions PRIMARY KEY (UserAccessGroupCode, UIControlCode) 
) ON B1Core

GO

ALTER TABLE B1.UserAccessGroupPermissions
ADD CONSTRAINT FK_UserAccessGroupPermissions_UIControls
FOREIGN KEY (UIControlCode)
REFERENCES B1.UIControls(UIControlCode)

GO

ALTER TABLE B1.UserAccessGroupPermissions
ADD CONSTRAINT FK_UserAccessGroupPermissions_UserMaster
FOREIGN KEY (LastModifiedUserCode)
REFERENCES B1.UserMaster(UserCode)

GO

CREATE TABLE B1.UserSessions
(
	UserCode				INT NOT NULL,
	SessionCode				BIGINT NOT NULL,
	UserId					VARCHAR(64) NOT NULL,
	SignonDateTime			DATETIME NOT NULL DEFAULT (GETUTCDATE()),
	LastCheckinDateTime		DATETIME NOT NULL DEFAULT (GETUTCDATE()),
	ApplicationInstance		VARCHAR(64) NOT NULL,
	ApplicationVersion		VARCHAR(24) NOT NULL,
	ForceSignoff			BIT NOT NULL DEFAULT(0),
	HostAddress				VARCHAR(64),
	HostPort				INT,
	CONSTRAINT PK_UserSessions PRIMARY KEY (SessionCode, UserCode)
) ON B1Core

GO

CREATE UNIQUE INDEX UX_UserSessions_UserCode
ON B1.UserSessions(UserCode, SessionCode)

GO

CREATE UNIQUE INDEX UX_UserSessions_UserId
ON B1.UserSessions(UserId, SessionCode)

GO

ALTER TABLE B1.UserSessions
ADD CONSTRAINT FK_UserSessions_UserMaster_UserCode
FOREIGN KEY (UserCode)
REFERENCES B1.UserMaster(UserCode)

GO

ALTER TABLE B1.UserSessions
ADD CONSTRAINT FK_UserSessions_UserMaster_UserId
FOREIGN KEY (UserId)
REFERENCES B1.UserMaster(UserId)

GO

CREATE TABLE B1.AppSessionControl
(
	SessionControlCode			TINYINT NOT NULL DEFAULT(1) CONSTRAINT CC_AppSessionControl_Code CHECK (SessionControlCode = 1),
	RestrictSignon				BIT NOT NULL DEFAULT(0),
	ForceSignoff				BIT NOT NULL DEFAULT(0),
	CheckinSeconds				SMALLINT NOT NULL DEFAULT(60),
	SessionTimeoutSeconds		SMALLINT NOT NULL DEFAULT(120),
	SignonFailureLimit			TINYINT NOT NULL DEFAULT(5),
	RestrictSignonMsg			VARCHAR(256),
	SignoffWarningMsg			VARCHAR(256),
	LastModifiedUserCode		INT,
	LastModifiedDateTime		DATETIME,
	CONSTRAINT PK_AppSessionControl PRIMARY KEY (SessionControlCode) 
) ON B1Core

GO

ALTER TABLE B1.AppSessionControl
ADD CONSTRAINT FK_AppSessionControl_UserMaster
FOREIGN KEY (LastModifiedUserCode)
REFERENCES B1.UserMaster(UserCode)

GO


CREATE TABLE B1.AppSessions
(
	ApplicationInstance		VARCHAR(64) NOT NULL,
	ApplicationVersion		VARCHAR(24) NOT NULL,
	SessionStartDateTime	DATETIME NOT NULL DEFAULT (GETUTCDATE()),
	SessionCheckinDateTime	DATETIME NOT NULL DEFAULT (GETUTCDATE()),
	HostAddress				VARCHAR(64) NOT NULL,
	StatusMessage			VARCHAR(512) NOT NULL,
	CONSTRAINT PK_AppSessions PRIMARY KEY (ApplicationInstance) 
) ON B1Core

GO

DECLARE @uagc INT, @uagcAdmin INT, @uagcGuest INT

EXEC B1.usp_UniqueIdsGetNextBlock 'UserAccessGroupCode', 1, @uagc out

INSERT INTO B1.UserAccessGroups (UserAccessGroupCode, UserAccessGroupName, DefaultAccessDenied, Remarks)
VALUES
(
@uagc, 'Administrators', 0, 'Application Administrators with no restrictions.'
)
SET @uagcAdmin = @uagc

EXEC B1.usp_UniqueIdsGetNextBlock 'UserAccessGroupCode', 1, @uagc out

INSERT INTO B1.UserAccessGroups (UserAccessGroupCode, UserAccessGroupName, DefaultAccessDenied, Remarks)
VALUES
(
@uagc, 'Guests', 1, 'Guest users with all restrictions by default' 
)
SET @uagcGuest = @uagc


DECLARE @uc INT

EXEC B1.usp_UniqueIdsGetNextBlock 'UserCode', 1, @uc out

INSERT INTO B1.UserMaster
(
	UserCode
	, UserId
	, UserPassword
	, DefaultAccessGroupCode
	, SignonRestricted
	, MultipleSignonAllowed
	, ForcePasswordChange
	, FailedSignonAttempts
	, NamePrefix
	, FirstName
	, MiddleName
	, LastName
	, NameSuffix
	, Remarks
)
VALUES
(
@uc
, 'administrator'
, 'administrator'
, @uagcAdmin
, 0
, 0
, 1
, 0
, null
, 'administrator'
, null
, 'administrator'
, null
, 'The default system administrator.  There can only be 1 administrator in the system'
)


EXEC B1.usp_UniqueIdsGetNextBlock 'UserCode', 1, @uc out

INSERT INTO B1.UserMaster
(
	UserCode
	, UserId
	, UserPassword
	, DefaultAccessGroupCode
	, SignonRestricted
	, MultipleSignonAllowed
	, ForcePasswordChange
	, FailedSignonAttempts
	, NamePrefix
	, FirstName
	, MiddleName
	, LastName
	, NameSuffix
	, Remarks
)
VALUES
(
@uc
, 'apugliese'
, 'apugliese'
, @uagcAdmin
, 0
, 1
, 1
, 0
, 'Mr'
, 'Andrew'
, null
, 'Pugliese'
, null
, 'A member of the administrators group.'
)


EXEC B1.usp_UniqueIdsGetNextBlock 'UserCode', 1, @uc out

INSERT INTO B1.UserMaster
(
	UserCode
	, UserId
	, UserPassword
	, DefaultAccessGroupCode
	, SignonRestricted
	, MultipleSignonAllowed
	, ForcePasswordChange
	, FailedSignonAttempts
	, NamePrefix
	, FirstName
	, MiddleName
	, LastName
	, NameSuffix
	, Remarks
)
VALUES
(
@uc
, 'noaccessuser'
, 'noaccessuser'
, @uagcGuest
, 0
, 0
, 0
, 0
, 'Mr'
, 'NoAccess'
, null
, 'User'
, null
, 'A demo use with limited or no access privileges.'
)

-- Default Application Session Control Values
INSERT INTO B1.AppSessionControl DEFAULT VALUES

