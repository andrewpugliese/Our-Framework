
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