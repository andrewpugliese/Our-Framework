--
-- User Sessions
-- 
-- Used to hold the user session records.
-- A user can have multiple session records (each with a unique SessionCode)
-- provided the AllowMultipleSessions attribute is true in the UserMaster table.
-- This table will record the date/time (UTC) that a user signed on as well as
-- information about the application by which the user signed on.
-- The SessionDateTime can be used to track active users and the ForceSignoff
-- can be used to have a user session removed at the next AppSession status event.
--
CREATE TABLE B1.UserSessions
(
	SessionCode				BIGINT NOT NULL, -- Unqiue Code for this user's session
	UserCode				INT NOT NULL, -- Unique numeric identifier for this user
	UserId					VARCHAR(64) NOT NULL, -- Unique string identifier for this user
	SignonDateTime			DATETIME NOT NULL DEFAULT (GETUTCDATE()), -- Time User signed onto system
	SessionDateTime			DATETIME NOT NULL DEFAULT (GETUTCDATE()), -- Time User last updated the session record
	ForceSignoff			BIT NOT NULL DEFAULT(0), -- Indicates that this session must be forced off
	AppCode					INT NOT NULL, -- unique code of the application User was signed on
	AppId					VARCHAR(32) NOT NULL, -- unique identifier of the application User was signed on
	AppVersion				VARCHAR(24) NOT NULL, -- version of the application User was signed on
	AppMachine				VARCHAR(64)	NOT NULL, -- Machine name where the application User was signed on
	RemoteAddress			VARCHAR(64), -- IP address where a user's browser was located
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
