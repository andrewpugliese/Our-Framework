--
-- Signon Control Table
--
-- Used for controlling Signon functionality
-- 
-- The table is designed to only contain 1 row
-- this row contains the switches for restricting
-- new user signons as well as forcing off currently
-- signed on users and the messages to be displayed.
-- 
-- In addition, it contains constants that control
-- the signon and status updates.
--
CREATE TABLE B1.SignonControl
(
	ControlCode					TINYINT NOT NULL DEFAULT(1) CONSTRAINT SignonControl_CC_Code CHECK (ControlCode = 1),
	RestrictSignon				BIT NOT NULL DEFAULT(0), -- Prevents new signons
	ForceSignoff				BIT NOT NULL DEFAULT(0), -- Forces current users off
	StatusSeconds				SMALLINT NOT NULL DEFAULT(60), -- Frequency of application status updates
	TimeoutSeconds				SMALLINT NOT NULL DEFAULT(120), -- Time allowed for user to be considered active
	FailedAttemptLimit			TINYINT NOT NULL DEFAULT(5), -- Attempts allowed for signon before being locked out
	RestrictSignonMsg			NVARCHAR(256), -- Message to display when preventing new signon
	SignoffWarningMsg			NVARCHAR(256), -- Message to display before signing users off
	LastModifiedUserCode		INT,
	LastModifiedDateTime		DATETIME,
	CONSTRAINT SignonControl_PK PRIMARY KEY (ControlCode) 
) ON B1Core
GO

ALTER TABLE B1.SignonControl
ADD CONSTRAINT SignonControl_FK_UserMaster
FOREIGN KEY (LastModifiedUserCode)
REFERENCES B1.UserMaster(UserCode)

GO