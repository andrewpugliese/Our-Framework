
CREATE TABLE dbo.ContentProviderMast(
	ContentProviderEntityCod int NOT NULL,
	ContentProviderNam varchar(60) NOT NULL,
	ContentProviderContactUsrSortNam varchar(60) NULL,
	ContentProviderEmailAddr varchar(64) NULL,
	ContentProviderPausedFlag smallint NOT NULL,
	ContentProviderStatCod smallint NOT NULL,
	ContentProviderInactiveDate date NULL,
	LastModUsrCod int NOT NULL,
	LastModDate date NOT NULL,
	LastModTim char(12) NOT NULL,
	LastModifiedUserCode	INT,	-- Last User to Modify the record
	LastModifiedDateTime	DATETIME, -- Date/Time of the last modification
 CONSTRAINT ContentProviderMast_PK PRIMARY KEY NONCLUSTERED (ContentProviderEntityCod)

) ON [PRIMARY]

GO

CREATE UNIQUE NONCLUSTERED INDEX ContentProviderMast00 ON dbo.ContentProviderMast 
(
	LastModUsrCod ASC,
	LastModDate ASC,
	LastModTim ASC
) ON ContentGalaxyIdx

CREATE UNIQUE NONCLUSTERED INDEX [ContentProviderMast02] ON [dbo].[ContentProviderMast] 
(
	[ContentProviderNam] ASC
) ON ContentGalaxyIdx
GO

ALTER TABLE dbo.ContentProviderMast  WITH CHECK ADD  CONSTRAINT ContentProviderMast_FK01 FOREIGN KEY(ContentProviderEntityCod)
REFERENCES dbo.EntityMast (EntityCod)
GO

ALTER TABLE dbo.ContentProviderMast CHECK CONSTRAINT ContentProviderMast_FK01
GO

ALTER TABLE dbo.ContentProviderMast  WITH CHECK ADD  CONSTRAINT ContentProviderMast_FK02 FOREIGN KEY(ContentProviderNam)
REFERENCES dbo.EntityMast (EntityNam)
GO

ALTER TABLE dbo.ContentProviderMast CHECK CONSTRAINT ContentProviderMast_FK02
GO

ALTER TABLE dbo.ContentProviderMast  WITH CHECK ADD  CONSTRAINT ContentProviderMast_FK04 FOREIGN KEY(ContentProviderStatCod)
REFERENCES dbo.EntityStat (EntityStatCod)
GO

ALTER TABLE dbo.ContentProviderMast CHECK CONSTRAINT ContentProviderMast_FK04
GO

ALTER TABLE dbo.ContentProviderMast ADD  DEFAULT ('0') FOR ContentProviderPausedFlag
GO

ALTER TABLE dbo.ContentProviderMast
ADD CONSTRAINT FK_ContentProviderMast_UserMaster
FOREIGN KEY (LastModifiedUserCode)
REFERENCES B1.UserMaster(UserCode)
GO

