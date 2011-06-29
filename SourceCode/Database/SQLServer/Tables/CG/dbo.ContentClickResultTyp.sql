
CREATE TABLE dbo.ContentClickResultTyp(
	ContentClickResultTypCod smallint NOT NULL,
	ContentClickResultTypNam varchar(26) NOT NULL,
	ContentClickResultTypRem varchar(1000) NULL,
	LastModUsrCod int NOT NULL,
	LastModDate date NOT NULL,
	LastModTim char(12) NOT NULL,
	LastModifiedUserCode	INT,	-- Last User to Modify the record
	LastModifiedDateTime	DATETIME, -- Date/Time of the last modification
 CONSTRAINT ContentClickResultTyp_PK PRIMARY KEY NONCLUSTERED (ContentClickResultTypCod)
) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX ContentClickResultTyp00 ON dbo.ContentClickResultTyp 
(
	LastModUsrCod ASC,
	LastModDate ASC,
	LastModTim ASC
) ON ContentGalaxyIdx
GO
CREATE UNIQUE NONCLUSTERED INDEX ContentClickResultTyp02 ON dbo.ContentClickResultTyp 
(
	ContentClickResultTypNam ASC
) ON ContentGalaxyIdx

GO
ALTER TABLE dbo.ContentClickResultTyp
ADD CONSTRAINT FK_ContentClickResultTyp_UserMaster
FOREIGN KEY (LastModifiedUserCode)
REFERENCES B1.UserMaster(UserCode)
GO