
CREATE TABLE dbo.PublicationMast(
	PublicationCod smallint NOT NULL,
	PublicationNam varchar(60) NOT NULL,
	PublicationShortNam varchar(30) NOT NULL,
	PublisherEntityCod int NOT NULL,
	PublicationTagLin varchar(150) NULL,
	PublicationImageFilNam varchar(64) NULL,
	PublicationHomeCMPServiceURL varchar(150) NULL,
	PublicationStreamOkFlag smallint NOT NULL,
	PublicationDownloadOkFlag smallint NOT NULL,
	RepeatDownloadClickDurationDayCt smallint NULL,
	PublicationAffiliateOkFlag smallint NOT NULL,
	PurchasePercentShareCMPService smallint NULL,
	PurchasePercentSharePool smallint NULL,
	PurchasePercentSharePublisher smallint NULL,
	PoolPercentShareContentProvider smallint NULL,
	PoolPercentSharePrimaryEditor smallint NULL,
	PoolPercentShareSeniorEditor smallint NULL,
	PublicationContactUsrSortNam varchar(60) NULL,
	PublicationEmailAddr varchar(64) NULL,
	PublicationDescription varchar(1000) NULL,
	PublicationPausedFlag smallint NOT NULL,
	PublicationStatCod smallint NOT NULL,
	PublicationInactiveDate date NULL,
	LastModUsrCod int NOT NULL,
	LastModDate date NOT NULL,
	LastModTim char(12) NOT NULL,
	LastModifiedUserCode	INT,	-- Last User to Modify the record
	LastModifiedDateTime	DATETIME, -- Date/Time of the last modification
 CONSTRAINT PublicationMast_PK PRIMARY KEY NONCLUSTERED (PublicationCod)
) ON [PRIMARY]
GO

CREATE UNIQUE NONCLUSTERED INDEX PublicationMast00 ON dbo.PublicationMast 
(
	LastModUsrCod ASC,
	LastModDate ASC,
	LastModTim ASC
) ON ContentGalaxyIdx
GO
CREATE UNIQUE NONCLUSTERED INDEX PublicationMast02 ON dbo.PublicationMast 
(
	PublicationNam ASC
) ON ContentGalaxyIdx
GO
CREATE UNIQUE NONCLUSTERED INDEX PublicationMast03 ON dbo.PublicationMast 
(
	PublicationShortNam ASC
) ON ContentGalaxyIdx
GO
CREATE UNIQUE NONCLUSTERED INDEX PublicationMast04 ON dbo.PublicationMast 
(
	PublisherEntityCod ASC,
	PublicationCod ASC
) ON ContentGalaxyIdx
GO
ALTER TABLE dbo.PublicationMast  WITH CHECK ADD  CONSTRAINT PublicationMast_FK01 FOREIGN KEY(PublisherEntityCod)
REFERENCES dbo.PublisherMast (PublisherEntityCod)
GO
ALTER TABLE dbo.PublicationMast CHECK CONSTRAINT PublicationMast_FK01
GO
ALTER TABLE dbo.PublicationMast  WITH CHECK ADD  CONSTRAINT PublicationMast_FK03 FOREIGN KEY(PublicationStatCod)
REFERENCES dbo.EntityStat (EntityStatCod)
GO
ALTER TABLE dbo.PublicationMast CHECK CONSTRAINT PublicationMast_FK03
GO
ALTER TABLE dbo.PublicationMast
ADD CONSTRAINT FK_PublicationMast_UserMaster
FOREIGN KEY (LastModifiedUserCode)
REFERENCES B1.UserMaster(UserCode)
GO

