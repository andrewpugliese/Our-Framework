-- Load database initial data
--

-- Setup the application configuration parameters
insert into B1.AppConfigParameters (ParameterName, ParameterValue)
values ('QID', 'E1850QUEUE')

-- Setup the application configuration settings
insert into B1.AppConfigSettings (ConfigSetName, ConfigKey, ConfigValue, ConfigDescription)
values ('ALL', 'ChromeQueue', '<<QID>>/Chrome', 'Chrome queue')

insert into B1.AppConfigSettings (ConfigSetName, ConfigKey, ConfigValue, ConfigDescription)
values ('ALL', 'ExperianTimeout', '300', 'Experian server timeout')

insert into B1.AppConfigSettings (ConfigSetName, ConfigKey, ConfigValue, ConfigDescription)
values ('eContracting', 'ExperianTimeout', '200', 'Experian server timeout')

insert into B1.AppConfigSettings (ConfigSetName, ConfigKey, ConfigValue, ConfigDescription)
values ('eContracting', 'CreditBureaus', 'TransUnion', 'TransUnion is supported')

insert into B1.AppConfigSettings (ConfigSetName, ConfigKey, ConfigValue, ConfigDescription)
values ('eContracting', 'CreditBureaus', 'Experian', 'Experian is supported')

-- Add an entry for generating sequence ids for AppSequenceId which
-- will be used for demo/testing utility
insert into B1.UniqueIds (UniqueIdKey, MaxIdValue, RolloverIdValue)
values ('AppSequenceId', 99999, 1)

-- Add an entry for generating sequence ids for AppSequenceId which
-- will be used for demo/testing utility
insert into B1.UniqueIds (UniqueIdKey, MaxIdValue, RolloverIdValue)
values ('MultipleSessionCode', 99999, 1)

--------------------------------------------------------------------------------
-- USER RELATED DATA

DECLARE @uagc INT, @uagcAdmin INT, @uagcGuest INT

EXEC B1.usp_UniqueIdsGetNextBlock 'AccessControlGroupCode', 1, @uagc out

INSERT INTO B1.AccessControlGroups (AccessControlGroupCode, AccessControlGroupName, DefaultAccessDenied, Remarks)
VALUES
(
@uagc, 'Administrators', 0, 'Application Administrators with no restrictions.'
)
SET @uagcAdmin = @uagc

EXEC B1.usp_UniqueIdsGetNextBlock 'AccessControlGroupCode', 1, @uagc out

INSERT INTO B1.AccessControlGroups (AccessControlGroupCode, AccessControlGroupName, DefaultAccessDenied, Remarks)
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
	, PasswordSalt
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
, '4ejxiLVxr/CVUsjPjbks3QGxlamYrtJGKvK2lEbSP6YT5g31B9FXnzv7DV41OnXPbuLk+aahdxCbAh0bqYfp/w=='
, 'fpi8580t7r2FpeouIiPOKRGGfcLeOyIeg+qX03MymqJm0SoDAeMGWOovESyyaPTLAyvU'
, @uagcAdmin
, 0
, 0
, 0
, 0
, null
, 'admin'
, null
, 'user'
, null
, 'The default system administrator.  There can only be 1 administrator in the system'
)


EXEC B1.usp_UniqueIdsGetNextBlock 'UserCode', 1, @uc out

INSERT INTO B1.UserMaster
(
	UserCode
	, UserId
	, UserPassword
	, PasswordSalt
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
, '4ejxiLVxr/CVUsjPjbks3QGxlamYrtJGKvK2lEbSP6YT5g31B9FXnzv7DV41OnXPbuLk+aahdxCbAh0bqYfp/w=='
, 'fpi8580t7r2FpeouIiPOKRGGfcLeOyIeg+qX03MymqJm0SoDAeMGWOovESyyaPTLAyvU'
, @uagcAdmin
, 0
, 1
, 0
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
	, PasswordSalt
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
, 'demo'
, '4ejxiLVxr/CVUsjPjbks3QGxlamYrtJGKvK2lEbSP6YT5g31B9FXnzv7DV41OnXPbuLk+aahdxCbAh0bqYfp/w=='
, 'fpi8580t7r2FpeouIiPOKRGGfcLeOyIeg+qX03MymqJm0SoDAeMGWOovESyyaPTLAyvU'
, @uagcGuest
, 0
, 0
, 0
, 0
, null
, 'Demo'
, null
, 'User'
, null
, 'A demo user with limited or no access privileges.'
)


EXEC B1.usp_UniqueIdsGetNextBlock 'UserCode', 1, @uc out

INSERT INTO B1.UserMaster
(
	UserCode
	, UserId
	, UserPassword
	, PasswordSalt
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
, 'mperrotta'
, '4ejxiLVxr/CVUsjPjbks3QGxlamYrtJGKvK2lEbSP6YT5g31B9FXnzv7DV41OnXPbuLk+aahdxCbAh0bqYfp/w=='
, 'fpi8580t7r2FpeouIiPOKRGGfcLeOyIeg+qX03MymqJm0SoDAeMGWOovESyyaPTLAyvU'
, @uagcAdmin
, 0
, 1
, 0
, 0
, 'Mr'
, 'Mark'
, null
, 'Perrotta'
, null
, 'A member of the administrators group.'
)


EXEC B1.usp_UniqueIdsGetNextBlock 'UserCode', 1, @uc out

INSERT INTO B1.UserMaster
(
	UserCode
	, UserId
	, UserPassword
	, PasswordSalt
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
, 'gpironi'
, '4ejxiLVxr/CVUsjPjbks3QGxlamYrtJGKvK2lEbSP6YT5g31B9FXnzv7DV41OnXPbuLk+aahdxCbAh0bqYfp/w=='
, 'fpi8580t7r2FpeouIiPOKRGGfcLeOyIeg+qX03MymqJm0SoDAeMGWOovESyyaPTLAyvU'
, @uagcAdmin
, 0
, 1
, 0
, 0
, 'Mr'
, 'Giorgio'
, null
, 'Pironi'
, null
, 'A member of the administrators group.'
)


EXEC B1.usp_UniqueIdsGetNextBlock 'UserCode', 1, @uc out

INSERT INTO B1.UserMaster
(
	UserCode
	, UserId
	, UserPassword
	, PasswordSalt
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
, 'sasherman'
, '4ejxiLVxr/CVUsjPjbks3QGxlamYrtJGKvK2lEbSP6YT5g31B9FXnzv7DV41OnXPbuLk+aahdxCbAh0bqYfp/w=='
, 'fpi8580t7r2FpeouIiPOKRGGfcLeOyIeg+qX03MymqJm0SoDAeMGWOovESyyaPTLAyvU'
, @uagcAdmin
, 0
, 1
, 0
, 0
, 'Mr'
, 'Steven'
, null
, 'Asherman'
, null
, 'A member of the administrators group.'
)


EXEC B1.usp_UniqueIdsGetNextBlock 'UserCode', 1, @uc out

INSERT INTO B1.UserMaster
(
	UserCode
	, UserId
	, UserPassword
	, PasswordSalt
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
, 'nsinghal'
, '4ejxiLVxr/CVUsjPjbks3QGxlamYrtJGKvK2lEbSP6YT5g31B9FXnzv7DV41OnXPbuLk+aahdxCbAh0bqYfp/w=='
, 'fpi8580t7r2FpeouIiPOKRGGfcLeOyIeg+qX03MymqJm0SoDAeMGWOovESyyaPTLAyvU'
, @uagcAdmin
, 0
, 1
, 0
, 0
, 'Mr'
, 'Narayan'
, null
, 'Singhal'
, null
, 'A member of the administrators group.'
)


EXEC B1.usp_UniqueIdsGetNextBlock 'UserCode', 1, @uc out

INSERT INTO B1.UserMaster
(
	UserCode
	, UserId
	, UserPassword
	, PasswordSalt
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
, 'akumar'
, '4ejxiLVxr/CVUsjPjbks3QGxlamYrtJGKvK2lEbSP6YT5g31B9FXnzv7DV41OnXPbuLk+aahdxCbAh0bqYfp/w=='
, 'fpi8580t7r2FpeouIiPOKRGGfcLeOyIeg+qX03MymqJm0SoDAeMGWOovESyyaPTLAyvU'
, @uagcAdmin
, 0
, 1
, 0
, 0
, 'Mr'
, 'Arun'
, null
, 'Kumar'
, null
, 'A member of the administrators group.'
)


EXEC B1.usp_UniqueIdsGetNextBlock 'UserCode', 1, @uc out

INSERT INTO B1.UserMaster
(
	UserCode
	, UserId
	, UserPassword
	, PasswordSalt
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
, 'mjain'
, '4ejxiLVxr/CVUsjPjbks3QGxlamYrtJGKvK2lEbSP6YT5g31B9FXnzv7DV41OnXPbuLk+aahdxCbAh0bqYfp/w=='
, 'fpi8580t7r2FpeouIiPOKRGGfcLeOyIeg+qX03MymqJm0SoDAeMGWOovESyyaPTLAyvU'
, @uagcAdmin
, 0
, 1
, 0
, 0
, 'Mr'
, 'Mayank'
, null
, 'Jain'
, null
, 'A member of the administrators group.'
)


EXEC B1.usp_UniqueIdsGetNextBlock 'UserCode', 1, @uc out

INSERT INTO B1.UserMaster
(
	UserCode
	, UserId
	, UserPassword
	, PasswordSalt
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
, 'masherman'
, '4ejxiLVxr/CVUsjPjbks3QGxlamYrtJGKvK2lEbSP6YT5g31B9FXnzv7DV41OnXPbuLk+aahdxCbAh0bqYfp/w=='
, 'fpi8580t7r2FpeouIiPOKRGGfcLeOyIeg+qX03MymqJm0SoDAeMGWOovESyyaPTLAyvU'
, @uagcAdmin
, 0
, 1
, 0
, 0
, 'Mr'
, 'Michael'
, null
, 'Asherman'
, null
, 'A member of the administrators group.'
)


-- Default Signon Control Values
INSERT INTO B1.SignonControl DEFAULT VALUES

-- Default DbSetup Application

DECLARE @AppCode INT

EXEC B1.usp_UniqueIdsGetNextBlock 'AppCode', 1, @AppCode out

INSERT INTO B1.AppMaster
(
AppCode
, AppId
, Remarks
)
VALUES
(
@AppCode
, 'DbSetup1'
, 'Database Setup and Unit Test Application'
)

GO

DECLARE @AppCode INT

EXEC B1.usp_UniqueIdsGetNextBlock 'AppCode', 1, @AppCode out

INSERT INTO B1.AppMaster
(
AppCode
, AppId
, Remarks
)
VALUES
(
@AppCode
, 'AdminWeb1'
, 'Administrative Website Application'
)

GO 

DECLARE @AppCode INT

EXEC B1.usp_UniqueIdsGetNextBlock 'AppCode', 1, @AppCode out

INSERT INTO B1.AppMaster
(
AppCode
, AppId
, IsTaskProcessingEngine
, Remarks
)
VALUES
(
@AppCode
, 'TPE1'
, 1
, 'Task Processing Engine'
)

GO 

--------------------------------------------------------------------------------
-- UI CONTROLS RELATED DATA


DECLARE @UIControlCode INT

EXEC B1.usp_UniqueIdsGetNextBlock 'UIControlCode', 1, @UIControlCode out

INSERT INTO B1.UIControls
(
UIControlCode
, UIControlURI
, Description
)
VALUES
(
@UIControlCode
, 'B1.DbSetup.TestUserSession.CleanupInActiveSessions'
, 'Used to cleanup inactive user sessions'
)

GO

DECLARE @UIControlCode INT

EXEC B1.usp_UniqueIdsGetNextBlock 'UIControlCode', 1, @UIControlCode out

INSERT INTO B1.UIControls
(
UIControlCode
, UIControlURI
, Description
)
VALUES
(
@UIControlCode
, 'B1.DbSetup.TestAppSession.CleanupInActiveSessions'
, 'Used to cleanup inactive app sessions'
)

GO

DECLARE @UIControlCode INT

EXEC B1.usp_UniqueIdsGetNextBlock 'UIControlCode', 1, @UIControlCode out

INSERT INTO B1.UIControls
(
UIControlCode
, UIControlURI
, Description
)
VALUES
(
@UIControlCode
, 'B1.DbSetup.TestAppSession.ChangeSignonControl'
, 'Used to change the signon control settings'
)

GO

DECLARE @UIControlCode INT, @guests INT

EXEC B1.usp_UniqueIdsGetNextBlock 'UIControlCode', 1, @UIControlCode out

INSERT INTO B1.UIControls
(
UIControlCode
, UIControlURI
, Description
)
VALUES
(
@UIControlCode
, 'B1.DbSetup.TestUserSession.SignOff'
, 'Used to signoff'
)

SELECT @guests = AccessControlGroupCode
FROM B1.AccessControlGroups
WHERE AccessControlGroupName = 'Guests'

INSERT INTO B1.AccessControlGroupRules
(
AccessControlGroupCode
, UIControlCode
, AccessDenied
)
VALUES
(
@guests
, @UIControlCode
, 0
)

GO

INSERT INTO B1.TaskStatusCodes (StatusCode, StatusName) VALUES (0, 'NotQueued')

INSERT INTO B1.TaskStatusCodes (StatusCode, StatusName) VALUES (32, 'Queued')

INSERT INTO B1.TaskStatusCodes (StatusCode, StatusName) VALUES (64, 'InProcess')

INSERT INTO B1.TaskStatusCodes (StatusCode, StatusName) VALUES (128, 'Failed')

INSERT INTO B1.TaskStatusCodes (StatusCode, StatusName) VALUES (255, 'Succeeded')

GO