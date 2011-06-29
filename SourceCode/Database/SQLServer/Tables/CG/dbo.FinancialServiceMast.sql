SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE dbo.FinancialServiceMast(
	FinancialServiceEntityCod int NOT NULL,
	FinancialServiceNam varchar(60) NOT NULL,
	FinancialServiceContactUsrSortNam varchar(60) NULL,
	FinancialServiceEmailAddr varchar(64) NULL,
	FinancialServicePausedFlag smallint NOT NULL,
	FinancialServiceStatCod smallint NOT NULL,
	FinancialServiceInactiveDate date NULL,
	LastModUsrCod int NOT NULL,
	LastModDate date NOT NULL,
	LastModTim char(12) NOT NULL,
	LastModifiedUserCode	INT,	-- Last User to Modify the record
	LastModifiedDateTime	DATETIME, -- Date/Time of the last modification
 CONSTRAINT FinancialServiceMast_PK PRIMARY KEY NONCLUSTERED (FinancialServiceEntityCod)
) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX FinancialServiceMast00 ON dbo.FinancialServiceMast 
(
	LastModUsrCod ASC,
	LastModDate ASC,
	LastModTim ASC
) ON ContentGalaxyIdx
GO
CREATE UNIQUE NONCLUSTERED INDEX FinancialServiceMast02 ON dbo.FinancialServiceMast 
(
	FinancialServiceNam ASC
) ON ContentGalaxyIdx
GO
ALTER TABLE dbo.FinancialServiceMast  WITH CHECK ADD  CONSTRAINT FinancialServiceMast_FK01 FOREIGN KEY(FinancialServiceEntityCod)
REFERENCES dbo.EntityMast (EntityCod)
GO
ALTER TABLE dbo.FinancialServiceMast CHECK CONSTRAINT FinancialServiceMast_FK01
GO
ALTER TABLE dbo.FinancialServiceMast  WITH CHECK ADD  CONSTRAINT FinancialServiceMast_FK02 FOREIGN KEY(FinancialServiceContactUsrSortNam)
REFERENCES dbo.MemberMast (UsrSortNam)
GO
ALTER TABLE dbo.FinancialServiceMast CHECK CONSTRAINT FinancialServiceMast_FK02
GO
ALTER TABLE dbo.FinancialServiceMast  WITH CHECK ADD  CONSTRAINT FinancialServiceMast_FK03 FOREIGN KEY(FinancialServiceStatCod)
REFERENCES dbo.EntityStat (EntityStatCod)
GO
ALTER TABLE dbo.FinancialServiceMast CHECK CONSTRAINT FinancialServiceMast_FK03
GO
ALTER TABLE dbo.FinancialServiceMast ADD  DEFAULT ('0') FOR FinancialServicePausedFlag
GO
ALTER TABLE dbo.FinancialServiceMast ADD  DEFAULT ('300') FOR FinancialServiceStatCod
GO
ALTER TABLE dbo.FinancialServiceMast
ADD CONSTRAINT FK_FinancialServiceMast_UserMaster
FOREIGN KEY (LastModifiedUserCode)
REFERENCES B1.UserMaster(UserCode)
GO
