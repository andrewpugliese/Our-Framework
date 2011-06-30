
CREATE TABLE dbo.PublicationUsr(
	PublicationCod smallint NOT NULL,
	UsrCod int NOT NULL,
	UsrSortNam varchar(60) NULL,
	MemberPurchaseTranCod smallint NULL,
	MemberPurchaseDetailCod smallint NULL,
	SubscriptionOfferCod smallint NULL,
	SubscriptionStreamOkFlag smallint NOT NULL,
	SubscriptionDownloadOkFlag smallint NOT NULL,
	SubscriptionUsageExceededFlag smallint NOT NULL,
	SubscriptionExpirationDate date NULL,
	SubscriptionStartDate date NULL,
	UsrIsContentProviderFlag smallint NOT NULL,
	UsrIsEditorFlag smallint NOT NULL,
	UsrIsPublisherFlag smallint NOT NULL,
	UsrIsAdvertiserFlag smallint NOT NULL,
	UsrIsCMPServiceStaffFlag smallint NOT NULL,
	LastModUsrCod int NOT NULL,
	LastModDate date NOT NULL,
	LastModTim char(12) NOT NULL,
	LastModifiedUserCode	INT,	-- Last User to Modify the record
	LastModifiedDateTime	DATETIME, -- Date/Time of the last modification
 CONSTRAINT PublicationUsr_PK PRIMARY KEY NONCLUSTERED (PublicationCod, UsrCod)
) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX PublicationUsr00 ON dbo.PublicationUsr 
(
	LastModUsrCod ASC,
	LastModDate ASC,
	LastModTim ASC
) ON ContentGalaxyIdx
GO
ALTER TABLE dbo.PublicationUsr  WITH CHECK ADD  CONSTRAINT PublicationUsr_FK01 FOREIGN KEY(PublicationCod)
REFERENCES dbo.PublicationMast (PublicationCod)
GO
ALTER TABLE dbo.PublicationUsr CHECK CONSTRAINT PublicationUsr_FK01
GO
ALTER TABLE dbo.PublicationUsr  WITH CHECK ADD  CONSTRAINT PublicationUsr_FK02 FOREIGN KEY(UsrCod)
REFERENCES B1.USERMASTER (USERCODE)
GO
ALTER TABLE dbo.PublicationUsr CHECK CONSTRAINT PublicationUsr_FK02

GO
ALTER TABLE dbo.PublicationUsr
ADD CONSTRAINT FK_PublicationUsr_UserMaster
FOREIGN KEY (LastModifiedUserCode)
REFERENCES B1.UserMaster(UserCode)
GO

