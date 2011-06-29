
CREATE TABLE dbo.MemberPurchaseTran(
	UsrCod int NOT NULL,
	MemberPurchaseTranCod smallint NOT NULL,
	MemberPurchaseTranStatCod smallint NOT NULL,
	PaymentTranDate date NOT NULL,
	PaymentTranTim char(6) NOT NULL,
	TotPaymentAmount numeric(9, 4) NOT NULL,
	PaymentMethodCod smallint NOT NULL,
	FinancialServiceEntityCod int NOT NULL,
	FinancialServicePaymentTranId varchar(80) NULL,
	FinancialServiceRejectMsg varchar(500) NULL,
	FinancialServiceProcessFee numeric(9, 4) NOT NULL,
	CreditCardNum varchar(96) NULL,
	CreditCardSecCod smallint NULL,
	CreditCardExpirationDate int NULL,
	CreditCardFirstNam varchar(25) NULL,
	CreditCardLastNam varchar(25) NULL,
	CreditCardAddrLin1 varchar(60) NULL,
	CreditCardCity varchar(40) NULL,
	CreditCardStateId char(2) NULL,
	CreditCardPostalCod char(10) NULL,
	CreditCardCountryId char(2) NULL,
	CreditCardContactTelephone char(15) NULL,
	LastModUsrCod int NOT NULL,
	LastModDate date NOT NULL,
	LastModTim char(12) NOT NULL,
	LastModifiedUserCode	INT,	-- Last User to Modify the record
	LastModifiedDateTime	DATETIME, -- Date/Time of the last modification
 CONSTRAINT MemberPurchaseTran_PK PRIMARY KEY NONCLUSTERED (UsrCod, MemberPurchaseTranCod)
) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX MemberPurchaseTran00 ON dbo.MemberPurchaseTran 
(
	LastModUsrCod ASC,
	LastModDate ASC,
	LastModTim ASC
) ON ContentGalaxyIdx
GO
ALTER TABLE dbo.MemberPurchaseTran  WITH CHECK ADD  CONSTRAINT MemberPurchaseTran_FK01 FOREIGN KEY(UsrCod)
REFERENCES dbo.MemberMast (UsrCod)
GO
ALTER TABLE dbo.MemberPurchaseTran CHECK CONSTRAINT MemberPurchaseTran_FK01
GO
ALTER TABLE dbo.MemberPurchaseTran  WITH CHECK ADD  CONSTRAINT MemberPurchaseTran_FK02 FOREIGN KEY(MemberPurchaseTranStatCod)
REFERENCES dbo.MemberPurchaseTranStat (MemberPurchaseTranStatCod)
GO
ALTER TABLE dbo.MemberPurchaseTran CHECK CONSTRAINT MemberPurchaseTran_FK02
GO
ALTER TABLE dbo.MemberPurchaseTran  WITH CHECK ADD  CONSTRAINT MemberPurchaseTran_FK03 FOREIGN KEY(PaymentMethodCod)
REFERENCES dbo.PaymentMethod (PaymentMethodCod)
GO
ALTER TABLE dbo.MemberPurchaseTran CHECK CONSTRAINT MemberPurchaseTran_FK03
GO
ALTER TABLE dbo.MemberPurchaseTran  WITH CHECK ADD  CONSTRAINT MemberPurchaseTran_FK04 FOREIGN KEY(FinancialServiceEntityCod)
REFERENCES dbo.FinancialServiceMast (FinancialServiceEntityCod)
GO
ALTER TABLE dbo.MemberPurchaseTran CHECK CONSTRAINT MemberPurchaseTran_FK04
GO
ALTER TABLE dbo.MemberPurchaseTran
ADD CONSTRAINT FK_MemberPurchaseTran_UserMaster
FOREIGN KEY (LastModifiedUserCode)
REFERENCES B1.UserMaster(UserCode)
GO

