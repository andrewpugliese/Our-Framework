
CREATE TABLE dbo.MemberPurchaseTranStat(
	MemberPurchaseTranStatCod smallint NOT NULL,
	MemberPurchaseTranStatNam varchar(26) NOT NULL,
	MemberPurchaseTranStatRem varchar(100) NULL,
	LastModUsrCod int NOT NULL,
	LastModDate date NOT NULL,
	LastModTim char(12) NOT NULL,
	LastModifiedUserCode	INT,	-- Last User to Modify the record
	LastModifiedDateTime	DATETIME, -- Date/Time of the last modification
 CONSTRAINT MemberPurchaseTranStat_PK PRIMARY KEY NONCLUSTERED (MemberPurchaseTranStatCod)
) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX MemberPurchaseTranStat00 ON dbo.MemberPurchaseTranStat 
(
	LastModUsrCod ASC,
	LastModDate ASC,
	LastModTim ASC
) ON ContentGalaxyIdx
GO
CREATE UNIQUE NONCLUSTERED INDEX MemberPurchaseTranStat02 ON dbo.MemberPurchaseTranStat 
(
	MemberPurchaseTranStatNam ASC
) ON ContentGalaxyIdx
GO
ALTER TABLE dbo.MemberPurchaseTranStat
ADD CONSTRAINT FK_MemberPurchaseTranStat_UserMaster
FOREIGN KEY (LastModifiedUserCode)
REFERENCES B1.UserMaster(UserCode)
GO
