
CREATE TABLE dbo.EntityMast(
	EntityCod int NOT NULL,
	EntityNam varchar(60) NOT NULL,
	EntityTypCod smallint NOT NULL,
	MainContactUsrSortNam varchar(60) NULL,
	EntityEmailAddr varchar(64) NULL,
	DemoEntityFlag smallint NOT NULL,
	EntityIsPublisherFlag smallint NOT NULL,
	EntityIsContentProviderFlag smallint NOT NULL,
	EntityIsEditorFlag smallint NOT NULL,
	EntityIsAffiliateFlag smallint NOT NULL,
	EntityIsAdvertiserFlag smallint NOT NULL,
	EntityIsFinancialServiceFlag smallint NOT NULL,
	EntityIsCMPServiceFlag smallint NOT NULL,
	EntityRegistrationDate date NOT NULL,
	EntityInactiveDate date NULL,
	EntityRem varchar(1000) NULL,
	LastModUsrCod int NOT NULL,
	LastModDate date NOT NULL,
	LastModTim char(12) NOT NULL,
	LastModifiedUserCode	INT,	-- Last User to Modify the record
	LastModifiedDateTime	DATETIME, -- Date/Time of the last modification
 CONSTRAINT EntityMast_PK PRIMARY KEY NONCLUSTERED (EntityCod)
 ) ON [PRIMARY]

GO

ALTER TABLE dbo.EntityMast  WITH CHECK ADD  CONSTRAINT EntityMast_FK01 FOREIGN KEY(EntityTypCod)
REFERENCES dbo.EntityTyp (EntityTypCod)
GO

ALTER TABLE dbo.EntityMast CHECK CONSTRAINT EntityMast_FK01
GO

ALTER TABLE dbo.EntityMast  WITH CHECK ADD  CONSTRAINT EntityMast_FK02 FOREIGN KEY(MainContactUsrSortNam)
REFERENCES dbo.MemberMast (UsrSortNam)
GO

ALTER TABLE dbo.EntityMast CHECK CONSTRAINT EntityMast_FK02
GO

ALTER TABLE dbo.EntityMast ADD  DEFAULT ('0') FOR DemoEntityFlag
GO
ALTER TABLE dbo.EntityMast
ADD CONSTRAINT FK_EntityMast_UserMaster
FOREIGN KEY (LastModifiedUserCode)
REFERENCES B1.UserMaster(UserCode)
GO


