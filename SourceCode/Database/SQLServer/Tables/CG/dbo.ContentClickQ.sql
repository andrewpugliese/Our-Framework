
CREATE TABLE dbo.ContentClickQ(
	PriorityCod smallint NOT NULL,
	DbQDate date NOT NULL,
	DbQTim char(9) NOT NULL,
	PayloadRecTypNam varchar(40) NOT NULL,
	PayloadLastModUsrCod int NOT NULL,
	PayloadLastModDate date NOT NULL,
	PayloadLastModTim char(12) NOT NULL,
	DbQItmStatCod smallint NOT NULL,
	LastModUsrCod int NOT NULL,
	LastModDate date NOT NULL,
	LastModTim char(12) NOT NULL,
	LastModifiedUserCode	INT,	-- Last User to Modify the record
	LastModifiedDateTime	DATETIME, -- Date/Time of the last modification
 CONSTRAINT ContentClickQ_PK PRIMARY KEY CLUSTERED 
(
	PriorityCod ASC,
	DbQDate ASC,
	DbQTim ASC,
	PayloadRecTypNam ASC,
	PayloadLastModUsrCod ASC,
	PayloadLastModDate ASC,
	PayloadLastModTim ASC,
	DbQItmStatCod ASC
)
) ON [PRIMARY]
GO

ALTER TABLE dbo.ContentClickQ
ADD CONSTRAINT FK_ContentClickQ_UserMaster
FOREIGN KEY (LastModifiedUserCode)
REFERENCES B1.UserMaster(UserCode)
GO
