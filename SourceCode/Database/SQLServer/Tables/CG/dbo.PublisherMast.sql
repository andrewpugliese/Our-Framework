SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE dbo.PublisherMast(
	PublisherEntityCod int NOT NULL,
	PublisherNam varchar(60) NOT NULL,
	PublisherContactUsrSortNam varchar(60) NULL,
	PublisherEmailAddr varchar(64) NULL,
	PublisherPausedFlag smallint NOT NULL,
	PublisherStatCod smallint NOT NULL,
	PublisherInactiveDate date NULL,
	LastModUsrCod int NOT NULL,
	LastModDate date NOT NULL,
	LastModTim char(12) NOT NULL,
 CONSTRAINT PublisherMast_PK PRIMARY KEY NONCLUSTERED (PublisherEntityCod)
) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX PublisherMast00 ON dbo.PublisherMast 
(
	LastModUsrCod ASC,
	LastModDate ASC,
	LastModTim ASC
) ON ContentGalaxyIdx
GO
CREATE UNIQUE NONCLUSTERED INDEX PublisherMast02 ON dbo.PublisherMast 
(
	PublisherNam ASC
) ON ContentGalaxyIdx
GO
ALTER TABLE dbo.PublisherMast  WITH CHECK ADD  CONSTRAINT PublisherMast_FK01 FOREIGN KEY(PublisherEntityCod)
REFERENCES dbo.EntityMast (EntityCod)
GO
ALTER TABLE dbo.PublisherMast CHECK CONSTRAINT PublisherMast_FK01
GO
ALTER TABLE dbo.PublisherMast  WITH CHECK ADD  CONSTRAINT PublisherMast_FK03 FOREIGN KEY(PublisherStatCod)
REFERENCES dbo.EntityStat (EntityStatCod)
GO
ALTER TABLE dbo.PublisherMast CHECK CONSTRAINT PublisherMast_FK03
GO
ALTER TABLE dbo.PublisherMast ADD  DEFAULT ('300') FOR PublisherStatCod
GO
