SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE dbo.LedgerAccountMast(
	LedgerAccountNum smallint NOT NULL,
	LedgerAccountNam varchar(60) NOT NULL,
	DebitAccountFlag smallint NOT NULL,
	LedgerAccountRem varchar(1000) NULL,
	LastModUsrCod int NOT NULL,
	LastModDate date NOT NULL,
	LastModTim char(12) NOT NULL,
	LastModifiedUserCode	INT,	-- Last User to Modify the record
	LastModifiedDateTime	DATETIME, -- Date/Time of the last modification
 CONSTRAINT LedgerAccountMast_PK PRIMARY KEY NONCLUSTERED (LedgerAccountNum)

) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX LedgerAccountMast00 ON dbo.LedgerAccountMast 
(
	LastModUsrCod ASC,
	LastModDate ASC,
	LastModTim ASC
) ON ContentGalaxyIdx
GO
CREATE UNIQUE NONCLUSTERED INDEX LedgerAccountMast02 ON dbo.LedgerAccountMast 
(
	LedgerAccountNam ASC
) ON ContentGalaxyIdx
GO

ALTER TABLE dbo.LedgerAccountMast
ADD CONSTRAINT FK_LedgerAccountMast_UserMaster
FOREIGN KEY (LastModifiedUserCode)
REFERENCES B1.UserMaster(UserCode)
GO
