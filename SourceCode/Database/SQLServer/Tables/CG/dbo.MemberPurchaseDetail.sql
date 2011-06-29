
CREATE TABLE dbo.MemberPurchaseDetail(
	PublicationCod smallint NOT NULL,
	UsrCod int NOT NULL,
	MemberPurchaseTranCod smallint NOT NULL,
	MemberPurchaseDetailCod smallint NOT NULL,
	PaymentTranDate date NOT NULL,
	PaymentTranTypCod smallint NOT NULL,
	PublisherEntityCod int NOT NULL,
	SubscriptionOfferCod smallint NOT NULL,
	DiscountSaleByCrossSellPublisherFlag smallint NOT NULL,
	DiscountSaleByThisPublisherFlag smallint NOT NULL,
	PaymentAmountWithoutTax numeric(9, 4) NOT NULL,
	TaxAmount numeric(9, 4) NOT NULL,
	PurchaseShareAmountFinancialService numeric(9, 4) NOT NULL,
	PurchaseShareAmountPublisher numeric(9, 4) NOT NULL,
	PurchaseShareAmountCompensationPool numeric(9, 4) NOT NULL,
	PurchaseShareAmountCMPService numeric(9, 4) NOT NULL,
	PurchaseShareAmountWebSiteAffiliate numeric(9, 4) NOT NULL,
	PurchaseShareAmountCrossSellPublisher numeric(9, 4) NOT NULL,
	CrossSellPublisherEntityCod int NULL,
	AffiliateEntityCodOrId char(20) NULL,
	AffiliateEntityCod int NULL,
	BadAffiliateEntityCodOrId char(15) NULL,
	SubscriptionStartDate date NULL,
	SubscriptionEndDate date NULL,
	PaidStreamDurationSeconds numeric(12, 0) NULL,
	SubscriptionStreamDurationSeconds numeric(12, 0) NULL,
	SubscriptionStreamByteCt numeric(15, 0) NULL,
	PaidDownloadCt int NULL,
	SubscriptionDownloadByteCt numeric(15, 0) NULL,
	SubscriptionDownloadCt numeric(12, 0) NULL,
	LastModUsrCod int NOT NULL,
	LastModDate date NOT NULL,
	LastModTim char(12) NOT NULL,
	LastModifiedUserCode	INT,	-- Last User to Modify the record
	LastModifiedDateTime	DATETIME, -- Date/Time of the last modification
 CONSTRAINT MemberPurchaseDetail_PK PRIMARY KEY NONCLUSTERED 
(
	PublicationCod ASC,
	UsrCod ASC,
	MemberPurchaseTranCod ASC,
	MemberPurchaseDetailCod ASC
)
) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX MemberPurchaseDetail00 ON dbo.MemberPurchaseDetail 
(
	LastModUsrCod ASC,
	LastModDate ASC,
	LastModTim ASC
) ON ContentGalaxyIdx
GO
CREATE UNIQUE NONCLUSTERED INDEX MemberPurchaseDetail02 ON dbo.MemberPurchaseDetail 
(
	UsrCod ASC,
	PaymentTranDate ASC,
	PublisherEntityCod ASC,
	PublicationCod ASC,
	PaymentTranTypCod ASC,
	MemberPurchaseTranCod ASC,
	MemberPurchaseDetailCod ASC
) ON ContentGalaxyIdx
GO
CREATE UNIQUE NONCLUSTERED INDEX MemberPurchaseDetail03 ON dbo.MemberPurchaseDetail 
(
	UsrCod ASC,
	PaymentTranDate ASC,
	MemberPurchaseTranCod ASC,
	MemberPurchaseDetailCod ASC,
	PublisherEntityCod ASC,
	PublicationCod ASC,
	PaymentTranTypCod ASC
) ON ContentGalaxyIdx
GO
ALTER TABLE dbo.MemberPurchaseDetail  WITH CHECK ADD  CONSTRAINT MemberPurchaseDetail_FK01 FOREIGN KEY(PublicationCod)
REFERENCES dbo.PublicationMast (PublicationCod)
GO
ALTER TABLE dbo.MemberPurchaseDetail CHECK CONSTRAINT MemberPurchaseDetail_FK01
GO
ALTER TABLE dbo.MemberPurchaseDetail  WITH CHECK ADD  CONSTRAINT MemberPurchaseDetail_FK02 FOREIGN KEY(UsrCod, MemberPurchaseTranCod)
REFERENCES dbo.MemberPurchaseTran (UsrCod, MemberPurchaseTranCod)
GO
ALTER TABLE dbo.MemberPurchaseDetail CHECK CONSTRAINT MemberPurchaseDetail_FK02
GO
ALTER TABLE dbo.MemberPurchaseDetail  WITH CHECK ADD  CONSTRAINT MemberPurchaseDetail_FK03 FOREIGN KEY(PaymentTranTypCod)
REFERENCES dbo.PaymentTranTyp (PaymentTranTypCod)
GO
ALTER TABLE dbo.MemberPurchaseDetail CHECK CONSTRAINT MemberPurchaseDetail_FK03
GO
ALTER TABLE dbo.MemberPurchaseDetail  WITH CHECK ADD  CONSTRAINT MemberPurchaseDetail_FK04 FOREIGN KEY(PublisherEntityCod)
REFERENCES dbo.PublisherMast (PublisherEntityCod)
GO
ALTER TABLE dbo.MemberPurchaseDetail CHECK CONSTRAINT MemberPurchaseDetail_FK04
GO
ALTER TABLE dbo.MemberPurchaseDetail  WITH CHECK ADD  CONSTRAINT MemberPurchaseDetail_FK05 FOREIGN KEY(CrossSellPublisherEntityCod)
REFERENCES dbo.PublisherMast (PublisherEntityCod)
GO
ALTER TABLE dbo.MemberPurchaseDetail CHECK CONSTRAINT MemberPurchaseDetail_FK05
GO
ALTER TABLE dbo.MemberPurchaseDetail  WITH CHECK ADD  CONSTRAINT MemberPurchaseDetail_FK06 FOREIGN KEY(AffiliateEntityCod)
REFERENCES dbo.AffiliateMast (AffiliateEntityCod)
GO
ALTER TABLE dbo.MemberPurchaseDetail CHECK CONSTRAINT MemberPurchaseDetail_FK06
GO
ALTER TABLE dbo.MemberPurchaseDetail ADD  DEFAULT ('0') FOR PurchaseShareAmountWebSiteAffiliate
GO
ALTER TABLE dbo.MemberPurchaseDetail ADD  DEFAULT ('0') FOR PurchaseShareAmountCrossSellPublisher
GO
ALTER TABLE dbo.MemberPurchaseDetail
ADD CONSTRAINT FK_MemberPurchaseDetail_UserMaster
FOREIGN KEY (LastModifiedUserCode)
REFERENCES B1.UserMaster(UserCode)
GO
