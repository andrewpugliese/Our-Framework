
CREATE TABLE dbo.AffiliateMast(
	AffiliateEntityCod int NOT NULL,
	AffiliateNam varchar(60) NOT NULL,
	AffiliateId char(15) NOT NULL,
	AffiliateContactUsrSortNam varchar(60) NULL,
	AffiliateEmailAddr varchar(64) NULL,
	AffiliatePausedFlag smallint NOT NULL,
	AffiliateInactiveDate date NULL,
	LastModUsrCod int NOT NULL,
	LastModDate date NOT NULL,
	LastModTim char(12) NOT NULL,
	LastModifiedUserCode	INT,	-- Last User to Modify the record
	LastModifiedDateTime	DATETIME, -- Date/Time of the last modification
 CONSTRAINT AffiliateMast_PK PRIMARY KEY NONCLUSTERED (AffiliateEntityCod)
) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX AffiliateMast00 ON dbo.AffiliateMast 
(
	LastModUsrCod ASC,
	LastModDate ASC,
	LastModTim ASC
) ON ContentGalaxyIdx
GO
CREATE UNIQUE NONCLUSTERED INDEX AffiliateMast02 ON dbo.AffiliateMast 
(
	AffiliateNam ASC
) ON ContentGalaxyIdx
GO
CREATE UNIQUE NONCLUSTERED INDEX AffiliateMast03 ON dbo.AffiliateMast 
(
	AffiliateId ASC
) ON ContentGalaxyIdx
GO
ALTER TABLE dbo.AffiliateMast  WITH CHECK ADD  CONSTRAINT AffiliateMast_FK01 FOREIGN KEY(AffiliateEntityCod)
REFERENCES dbo.EntityMast (EntityCod)
GO
ALTER TABLE dbo.AffiliateMast CHECK CONSTRAINT AffiliateMast_FK01
GO
ALTER TABLE dbo.AffiliateMast  WITH CHECK ADD  CONSTRAINT AffiliateMast_FK02 FOREIGN KEY(AffiliateNam)
REFERENCES dbo.EntityMast (EntityNam)
GO
ALTER TABLE dbo.AffiliateMast CHECK CONSTRAINT AffiliateMast_FK02
GO
ALTER TABLE dbo.AffiliateMast  WITH CHECK ADD  CONSTRAINT AffiliateMast_FK03 FOREIGN KEY(AffiliateContactUsrSortNam)
REFERENCES dbo.MemberMast (UsrSortNam)
GO
ALTER TABLE dbo.AffiliateMast CHECK CONSTRAINT AffiliateMast_FK03
GO
ALTER TABLE dbo.AffiliateMast ADD  DEFAULT ('0') FOR AffiliatePausedFlag
GO
ALTER TABLE dbo.AffiliateMast
ADD CONSTRAINT FK_AffiliateMast_UserMaster
FOREIGN KEY (LastModifiedUserCode)
REFERENCES B1.UserMaster(UserCode)
GO

