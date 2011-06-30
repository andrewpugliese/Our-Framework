

CREATE TABLE dbo.ContentItmStat(
	ContentItmStatCod smallint NOT NULL,
	ContentItmStatNam char(13) NOT NULL,
	ContentItmStatRem varchar(300) NULL,
	LastModUsrCod int NOT NULL,
	LastModDate date NOT NULL,
	LastModTim char(12) NOT NULL,
	LastModifiedUserCode	INT,	-- Last User to Modify the record
	LastModifiedDateTime	DATETIME, -- Date/Time of the last modification
 CONSTRAINT ContentItmStat_PK PRIMARY KEY NONCLUSTERED (ContentItmStatCod)
) ON [PRIMARY]
GO

ALTER TABLE dbo.ContentItmStat
ADD CONSTRAINT FK_ContentItmStat_UserMaster
FOREIGN KEY (LastModifiedUserCode)
REFERENCES B1.UserMaster(UserCode)
GO