-- 
-- Task Status Codes
--
-- Contains the various task processing status code definations
-- -- 0: NotQueued; 32: Queued; 64: InProcess; 128: Failed; 255: Succeeded
--
CREATE TABLE B1.TaskStatusCodes
(
	StatusCode					TINYINT NOT NULL,		-- unique numeric identifier
	StatusName					NVARCHAR(48) NOT NULL,	-- unique string identifier
	LastModifiedUserCode		INT NULL,
	LastModifiedDateTime		DATETIME NULL,
	CONSTRAINT TaskStatusCodes_PK_StatusCode PRIMARY KEY (StatusCode)
) ON B1Core

GO

CREATE UNIQUE INDEX TaskStatusCodes_UX_StatusName
ON B1.TaskStatusCodes(StatusName) ON B1CoreIdx

GO


ALTER TABLE B1.TaskStatusCodes
ADD CONSTRAINT TaslStatisCpdes_FK_UserMaster
FOREIGN KEY (LastModifiedUserCode)
REFERENCES B1.UserMaster(UserCode)
