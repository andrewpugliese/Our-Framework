SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE dbo.PaymentTranTyp(
	PaymentTranTypCod smallint NOT NULL,
	PaymentTranTypNam varchar(60) NOT NULL,
	LedgerAccountNum smallint NOT NULL,
	DebitAccountFlag smallint NOT NULL,
	DebitTranFlag smallint NOT NULL,
	OkForMemberFlag smallint NOT NULL,
	OkForPublisherFlag smallint NOT NULL,
	OkForContentProviderFlag smallint NOT NULL,
	OkForEditorFlag smallint NOT NULL,
	OkForAffiliateFlag smallint NOT NULL,
	OkForFinancialServiceFlag smallint NOT NULL,
	PaymentTranTypRem varchar(500) NULL,
	LastModUsrCod int NOT NULL,
	LastModDate date NOT NULL,
	LastModTim char(12) NOT NULL,
	LastModifiedUserCode	INT,	-- Last User to Modify the record
	LastModifiedDateTime	DATETIME, -- Date/Time of the last modification
 CONSTRAINT PaymentTranTyp_PK PRIMARY KEY NONCLUSTERED (PaymentTranTypCod)
) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX PaymentTranTyp00 ON dbo.PaymentTranTyp 
(
	LastModUsrCod ASC,
	LastModDate ASC,
	LastModTim ASC
) on ContentGalaxyIdx
GO
CREATE UNIQUE NONCLUSTERED INDEX PaymentTranTyp02 ON dbo.PaymentTranTyp 
(
	PaymentTranTypNam ASC
) on ContentGalaxyIdx
GO
ALTER TABLE dbo.PaymentTranTyp  WITH CHECK ADD  CONSTRAINT PaymentTranTyp_FK01 FOREIGN KEY(LedgerAccountNum)
REFERENCES dbo.LedgerAccountMast (LedgerAccountNum)
GO
ALTER TABLE dbo.PaymentTranTyp CHECK CONSTRAINT PaymentTranTyp_FK01
GO

ALTER TABLE dbo.PaymentTranTyp
ADD CONSTRAINT FK_PaymentTranTyp_UserMaster
FOREIGN KEY (LastModifiedUserCode)
REFERENCES B1.UserMaster(UserCode)
GO