
CREATE TABLE dbo.SubscriptionPeriodTyp(
	SubscriptionPeriodTypCod smallint NOT NULL,
	SubscriptionPeriodTypNam char(20) NOT NULL,
	SubscriptionPeriodTypRem varchar(300) NULL,
	LastModUsrCod int NOT NULL,
	LastModDate date NOT NULL,
	LastModTim char(12) NOT NULL,
	LastModifiedUserCode	INT,	-- Last User to Modify the record
	LastModifiedDateTime	DATETIME, -- Date/Time of the last modification
 CONSTRAINT SubscriptionPeriodTyp_PK PRIMARY KEY NONCLUSTERED (SubscriptionPeriodTypCod)
) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX SubscriptionPeriodTyp00 ON dbo.SubscriptionPeriodTyp 
(
	LastModUsrCod ASC,
	LastModDate ASC,
	LastModTim ASC
) ON ContentGalaxyIdx
GO
CREATE UNIQUE NONCLUSTERED INDEX SubscriptionPeriodTyp02 ON dbo.SubscriptionPeriodTyp 
(
	SubscriptionPeriodTypNam ASC
) on ContentGalaxyIdx
GO
ALTER TABLE dbo.SubscriptionPeriodTyp
ADD CONSTRAINT FK_SubscriptionPeriodTyp_UserMaster
FOREIGN KEY (LastModifiedUserCode)
REFERENCES B1.UserMaster(UserCode)
GO
