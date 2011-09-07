CREATE TABLE B1.UIControls
(
	UIControlCode			INT NOT NULL,
	UIControlURI			NVARCHAR(128) NOT NULL,
	Description				NVARCHAR(512),
	LastModifiedUserCode	INT,
	LastModifiedDateTime	DATETIME,
	CONSTRAINT UIControls_PK_UIControlCode PRIMARY KEY (UIControlCode)
)
ON B1Core

GO

CREATE UNIQUE INDEX UIControls_UX_UIControlURI 
ON B1.UIControls (UIControlURI)
ON B1CoreIdx

GO

ALTER TABLE B1.UIControls
ADD CONSTRAINT UIControls_FK_UserMaster
FOREIGN KEY (LastModifiedUserCode)
REFERENCES B1.UserMaster(UserCode)

GO