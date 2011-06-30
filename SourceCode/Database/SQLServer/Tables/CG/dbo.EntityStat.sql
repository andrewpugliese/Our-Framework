
CREATE TABLE dbo.EntityStat(
	EntityStatCod smallint NOT NULL,
	EntityStatNam char(13) NOT NULL,
	EntityStatRem varchar(100) NULL,
	LastModUsrCod int NOT NULL,
	LastModDate date NOT NULL,
	LastModTim char(12) NOT NULL,
	LastModifiedUserCode	INT,	-- Last User to Modify the record
	LastModifiedDateTime	DATETIME, -- Date/Time of the last modification
 CONSTRAINT EntityStat_PK PRIMARY KEY NONCLUSTERED (EntityStatCod)
) ON [PRIMARY]

GO

CREATE UNIQUE NONCLUSTERED INDEX [EntityStat00] ON [dbo].[EntityStat] 
(
	[LastModUsrCod] ASC,
	[LastModDate] ASC,
	[LastModTim] ASC
) ON ContentGalaxyIdx
GO

CREATE UNIQUE NONCLUSTERED INDEX [EntityStat02] ON [dbo].[EntityStat] 
(
	[EntityStatNam] ASC
) ON ContentGalaxyIdx
GO

ALTER TABLE dbo.EntityStat
ADD CONSTRAINT FK_EntityStat_UserMaster
FOREIGN KEY (LastModifiedUserCode)
REFERENCES B1.UserMaster(UserCode)
GO

