
CREATE TABLE dbo.ContentClickIn(
	PublicationCod smallint NOT NULL,
	UsrCod int NOT NULL,
	ContentCod int NOT NULL,
	ClickDate date NOT NULL,
	ClickTim char(6) NOT NULL,
	ContentClickResultTypCod smallint NOT NULL,
	MemberPurchaseDetailCod int NULL,
	ContentClickIsStreamFlag smallint NULL,
	ContentURLRemParam varchar(100) NULL,
	StreamDurationSeconds numeric(12, 0) NULL,
	PaidStreamDurationSeconds numeric(12, 0) NULL,
	DownloadByteCt numeric(12, 0) NULL,
	PaidDownloadByteCt numeric(12, 0) NULL,
	PublisherEntityCod int NOT NULL,
	ContentProviderEntityCod int NULL,
	PrimaryEditorEntityCod int NULL,
	AffiliateEntityCodOrId char(20) NULL,
	UsrIPAddr char(15) NULL,
	LastModUsrCod int NOT NULL,
	LastModDate date NOT NULL,
	LastModTim char(12) NOT NULL,
	LastModifiedUserCode	INT,	-- Last User to Modify the record
	LastModifiedDateTime	DATETIME, -- Date/Time of the last modification
 CONSTRAINT ContentClickIn_PK PRIMARY KEY NONCLUSTERED 
(
	LastModUsrCod ASC,
	LastModDate ASC,
	LastModTim ASC
)) ON [PRIMARY]
GO
ALTER TABLE dbo.ContentClickIn  WITH CHECK ADD  CONSTRAINT ContentClickIn_FK01 FOREIGN KEY(PublicationCod, ContentCod)
REFERENCES dbo.ContentItm (PublicationCod, ContentCod)
GO
ALTER TABLE dbo.ContentClickIn CHECK CONSTRAINT ContentClickIn_FK01
GO
ALTER TABLE dbo.ContentClickIn  WITH CHECK ADD  CONSTRAINT ContentClickIn_FK02 FOREIGN KEY(UsrCod)
REFERENCES B1.USERMASTER (USERCODE)
GO
ALTER TABLE dbo.ContentClickIn CHECK CONSTRAINT ContentClickIn_FK02
GO
ALTER TABLE dbo.ContentClickIn  WITH CHECK ADD  CONSTRAINT ContentClickIn_FK03 FOREIGN KEY(ContentClickResultTypCod)
REFERENCES dbo.ContentClickResultTyp (ContentClickResultTypCod)
GO
ALTER TABLE dbo.ContentClickIn CHECK CONSTRAINT ContentClickIn_FK03
GO
ALTER TABLE dbo.ContentClickIn  WITH CHECK ADD  CONSTRAINT ContentClickIn_FK04 FOREIGN KEY(PublisherEntityCod)
REFERENCES dbo.PublisherMast (PublisherEntityCod)
GO
ALTER TABLE dbo.ContentClickIn CHECK CONSTRAINT ContentClickIn_FK04
GO
ALTER TABLE dbo.ContentClickIn  WITH CHECK ADD  CONSTRAINT ContentClickIn_FK05 FOREIGN KEY(ContentProviderEntityCod)
REFERENCES dbo.ContentProviderMast (ContentProviderEntityCod)
GO
ALTER TABLE dbo.ContentClickIn CHECK CONSTRAINT ContentClickIn_FK05
GO
ALTER TABLE dbo.ContentClickIn  WITH CHECK ADD  CONSTRAINT ContentClickIn_FK06 FOREIGN KEY(PrimaryEditorEntityCod)
REFERENCES dbo.EditorMast (EditorEntityCod)
GO
ALTER TABLE dbo.ContentClickIn CHECK CONSTRAINT ContentClickIn_FK06
GO

ALTER TABLE dbo.ContentClickIn
ADD CONSTRAINT FK_ContentClickIn_UserMaster
FOREIGN KEY (LastModifiedUserCode)
REFERENCES B1.UserMaster(UserCode)
GO
