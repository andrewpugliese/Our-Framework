
CREATE TABLE dbo.EditorMast(
	EditorEntityCod int NOT NULL,
	EditorNam varchar(60) NOT NULL,
	EditorContactUsrSortNam varchar(60) NULL,
	EditorEmailAddr varchar(64) NULL,
	EditorPausedFlag smallint NOT NULL,
	EditorStatCod smallint NOT NULL,
	EditorInactiveDate date NULL,
	LastModUsrCod int NOT NULL,
	LastModDate date NOT NULL,
	LastModTim char(12) NOT NULL,
	LastModifiedUserCode	INT,	-- Last User to Modify the record
	LastModifiedDateTime	DATETIME, -- Date/Time of the last modification
 CONSTRAINT EditorMast_PK PRIMARY KEY NONCLUSTERED (EditorEntityCod)
) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX EditorMastMast00 ON dbo.EditorMast 
(
	LastModUsrCod ASC,
	LastModDate ASC,
	LastModTim ASC
) ON ContentGalaxyIdx
GO
CREATE UNIQUE NONCLUSTERED INDEX EditorMastMast02 ON dbo.EditorMast 
(
	EditorNam ASC
) ON ContentGalaxyIdx
GO

ALTER TABLE dbo.EditorMast  WITH CHECK ADD  CONSTRAINT EditorMast_FK01 FOREIGN KEY(EditorEntityCod)
REFERENCES dbo.EntityMast (EntityCod)
GO
ALTER TABLE dbo.EditorMast CHECK CONSTRAINT EditorMast_FK01
GO
ALTER TABLE dbo.EditorMast  WITH CHECK ADD  CONSTRAINT EditorMast_FK02 FOREIGN KEY(EditorNam)
REFERENCES dbo.EntityMast (EntityNam)
GO
ALTER TABLE dbo.EditorMast CHECK CONSTRAINT EditorMast_FK02
GO
ALTER TABLE dbo.EditorMast  WITH CHECK ADD  CONSTRAINT EditorMast_FK03 FOREIGN KEY(EditorContactUsrSortNam)
REFERENCES dbo.MemberMast (UsrSortNam)
GO
ALTER TABLE dbo.EditorMast CHECK CONSTRAINT EditorMast_FK03
GO
ALTER TABLE dbo.EditorMast  WITH CHECK ADD  CONSTRAINT EditorMast_FK04 FOREIGN KEY(EditorStatCod)
REFERENCES dbo.EntityStat (EntityStatCod)
GO
ALTER TABLE dbo.EditorMast CHECK CONSTRAINT EditorMast_FK04
GO
ALTER TABLE dbo.EditorMast ADD  DEFAULT ('0') FOR EditorPausedFlag
GO
ALTER TABLE dbo.EditorMast ADD  DEFAULT ('300') FOR EditorStatCod
GO

ALTER TABLE dbo.EditorMast
ADD CONSTRAINT FK_EditorMast_UserMaster
FOREIGN KEY (LastModifiedUserCode)
REFERENCES B1.UserMaster(UserCode)
GO
