
CREATE TABLE dbo.ContentItm(
	PublicationCod smallint NOT NULL,
	ContentCod int NOT NULL,
	ContentId varchar(150) NOT NULL,
	ContentLocation varchar(250) NULL,
	ContentTitl varchar(100) NULL,
	ContentItmSiz numeric(10, 0) NOT NULL,
	ContentItmStatCod smallint NOT NULL,
	ContentProviderEntityCod int NULL,
	ContentProviderNam varchar(60) NULL,
	EditorEntityCod int NULL,
	EditorNam varchar(60) NULL,
	ContentDescription varchar(1000) NULL,
	ContentLastModUsrCod int NULL,
	ContentLastModDate date NOT NULL,
	ContentLastModTim char(12) NOT NULL,
	LastModUsrCod int NOT NULL,
	LastModDate date NOT NULL,
	LastModTim char(12) NOT NULL,
	LastModifiedUserCode	INT,	-- Last User to Modify the record
	LastModifiedDateTime	DATETIME, -- Date/Time of the last modification
 CONSTRAINT ContentItm_PK PRIMARY KEY NONCLUSTERED 
(
	PublicationCod ASC,
	ContentCod ASC
)
) ON [PRIMARY]
GO

CREATE UNIQUE NONCLUSTERED INDEX ContentItm00 ON dbo.ContentItm 
(
	LastModUsrCod ASC,
	LastModDate ASC,
	LastModTim ASC
) ON ContentGalaxyIdx
GO

CREATE UNIQUE NONCLUSTERED INDEX [ContentItm02] ON [dbo].[ContentItm] 
(
	[PublicationCod] ASC,
	[ContentId] ASC
)ON ContentGalaxyIdx
GO

ALTER TABLE dbo.ContentItm  WITH CHECK ADD  CONSTRAINT ContentItm_FK01 FOREIGN KEY(PublicationCod)
REFERENCES dbo.PublicationMast (PublicationCod)
GO

ALTER TABLE dbo.ContentItm CHECK CONSTRAINT ContentItm_FK01
GO

ALTER TABLE dbo.ContentItm  WITH CHECK ADD  CONSTRAINT ContentItm_FK02 FOREIGN KEY(ContentItmStatCod)
REFERENCES dbo.ContentItmStat (ContentItmStatCod)
GO

ALTER TABLE dbo.ContentItm CHECK CONSTRAINT ContentItm_FK02
GO

ALTER TABLE dbo.ContentItm  WITH CHECK ADD  CONSTRAINT ContentItm_FK03 FOREIGN KEY(ContentProviderEntityCod)
REFERENCES dbo.ContentProviderMast (ContentProviderEntityCod)
GO

ALTER TABLE dbo.ContentItm CHECK CONSTRAINT ContentItm_FK03
GO

ALTER TABLE dbo.ContentItm  WITH CHECK ADD  CONSTRAINT ContentItm_FK04 FOREIGN KEY(ContentProviderNam)
REFERENCES dbo.ContentProviderMast (ContentProviderNam)
GO

ALTER TABLE dbo.ContentItm CHECK CONSTRAINT ContentItm_FK04
GO

ALTER TABLE dbo.ContentItm  WITH CHECK ADD  CONSTRAINT ContentItm_FK05 FOREIGN KEY(EditorEntityCod)
REFERENCES dbo.EditorMast (EditorEntityCod)
GO

ALTER TABLE dbo.ContentItm CHECK CONSTRAINT ContentItm_FK05
GO

ALTER TABLE dbo.ContentItm  WITH CHECK ADD  CONSTRAINT ContentItm_FK06 FOREIGN KEY(EditorNam)
REFERENCES dbo.EntityMast (EntityNam)
GO

ALTER TABLE dbo.ContentItm CHECK CONSTRAINT ContentItm_FK06
GO

ALTER TABLE dbo.ContentItm  WITH CHECK ADD  CONSTRAINT ContentItm_FK07 FOREIGN KEY(ContentLastModUsrCod)
REFERENCES dbo.MemberMast (UsrCod)
GO

ALTER TABLE dbo.ContentItm CHECK CONSTRAINT ContentItm_FK07
GO

ALTER TABLE dbo.ContentItm
ADD CONSTRAINT FK_ContentItm_UserMaster
FOREIGN KEY (LastModifiedUserCode)
REFERENCES B1.UserMaster(UserCode)
GO

