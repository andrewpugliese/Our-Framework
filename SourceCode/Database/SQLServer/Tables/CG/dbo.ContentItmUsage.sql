
CREATE TABLE dbo.ContentItmUsage(
	PublisherEntityCod int NOT NULL,
	PublicationCod smallint NOT NULL,
	ContentCod int NOT NULL,
	ContentItmUsageCod int NOT NULL,
	EntityCod int NOT NULL,
	EntityRoleCod smallint NOT NULL,
	ClickDate date NOT NULL,
	ContentId varchar(150) NOT NULL,
	ContentURLRemParam varchar(100) NULL,
	PricePerPaidStreamSecond numeric(9, 4) NULL,
	PricePerPaidDownload numeric(9, 4) NULL,
	ContentItmUsagePriceFrozenFlag smallint NOT NULL,
	SubscriptionSaleClickCt int NOT NULL,
	PaidStreamDurationSeconds int NULL,
	EntityPaidStreamDurationSeconds int NULL,
	SubscriptionStreamDurationSeconds int NULL,
	SubscriptionStreamByteCt numeric(12, 0) NULL,
	PaidDownloadCt int NULL,
	EntityPaidDownloadCt numeric(9, 2) NULL,
	SubscriptionDownloadByteCt numeric(12, 0) NULL,
	SubscriptionDownloadCt int NULL,
	LastModUsrCod int NOT NULL,
	LastModDate date NOT NULL,
	LastModTim char(12) NOT NULL,
	LastModifiedUserCode	INT,	-- Last User to Modify the record
	LastModifiedDateTime	DATETIME, -- Date/Time of the last modification
 CONSTRAINT ContentItmUsage_PK PRIMARY KEY NONCLUSTERED 
(
	PublisherEntityCod ASC,
	PublicationCod ASC,
	ContentCod ASC,
	ContentItmUsageCod ASC
)) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX ContentItmUsage00 ON dbo.ContentItmUsage 
(
	LastModUsrCod ASC,
	LastModDate ASC,
	LastModTim ASC
) ON ContentGalaxyIdx
GO
CREATE UNIQUE NONCLUSTERED INDEX ContentItmUsage02 ON dbo.ContentItmUsage 
(
	EntityCod ASC,
	ClickDate ASC,
	PublisherEntityCod ASC,
	PublicationCod ASC,
	EntityRoleCod ASC,
	ContentId ASC,
	ContentURLRemParam ASC
) ON ContentGalaxyIdx
GO
ALTER TABLE dbo.ContentItmUsage  WITH CHECK ADD  CONSTRAINT ContentItmUsage_FK01 FOREIGN KEY(PublisherEntityCod)
REFERENCES dbo.PublisherMast (PublisherEntityCod)
GO
ALTER TABLE dbo.ContentItmUsage CHECK CONSTRAINT ContentItmUsage_FK01
GO
ALTER TABLE dbo.ContentItmUsage  WITH CHECK ADD  CONSTRAINT ContentItmUsage_FK02 FOREIGN KEY(PublicationCod, ContentCod)
REFERENCES dbo.ContentItm (PublicationCod, ContentCod)
GO
ALTER TABLE dbo.ContentItmUsage CHECK CONSTRAINT ContentItmUsage_FK02
GO
ALTER TABLE dbo.ContentItmUsage  WITH CHECK ADD  CONSTRAINT ContentItmUsage_FK03 FOREIGN KEY(EntityCod)
REFERENCES dbo.EntityMast (EntityCod)
GO
ALTER TABLE dbo.ContentItmUsage CHECK CONSTRAINT ContentItmUsage_FK03
GO
ALTER TABLE dbo.ContentItmUsage  WITH CHECK ADD  CONSTRAINT ContentItmUsage_FK04 FOREIGN KEY(EntityRoleCod)
REFERENCES dbo.EntityRole (EntityRoleCod)
GO
ALTER TABLE dbo.ContentItmUsage CHECK CONSTRAINT ContentItmUsage_FK04
GO
ALTER TABLE dbo.ContentItmUsage
ADD CONSTRAINT FK_ContentItmUsage_UserMaster
FOREIGN KEY (LastModifiedUserCode)
REFERENCES B1.UserMaster(UserCode)
GO
